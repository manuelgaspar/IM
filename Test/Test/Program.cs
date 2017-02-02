using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessDotNet;
using System.Threading;


using Microsoft.Speech.Recognition;
using Microsoft.Speech.Recognition.SrgsGrammar;

using System.Media;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.AudioFormat;

using Microsoft.Kinect;

namespace Test
{
    class Program
    {
        public struct position
        {
            public int letra;
            public int num;

        }
        public static Tts fala = new Tts(); // voice feedback
        public static string initp = "",lastSP="";                //inicial piece || last command
        public static List<string> posi = new List<string>(); //list of initial positions
        public static List<string> posf = new List<string>(); //
        public static ChessGame game = new ChessGame();
        public static Process p = new Process();
        public static position pi, pf;
        public static bool flag = false;  // flag for confidence levels
        public static bool go = false;   // flag to detect player turn (True-> Player turn)
        public static int Cstate = 0;   // state machine for complementary command
        public static KinectSensor myKinect;
        public static string[] letras = { " ", "a", "b", "c", "d", "e", "f", "g", "h" };
        public static String[] coordN = { " ", "586", "519", "452", "385", "318", "251", "184", "117" };
        public static String[] coordL = { "", "6", "73", "140", "207", "274", "341", "408", "475" };
        public static string arguments, auxArgument;


        
        static void Main(string[] args)
        {

            // initialization
            int exit=1; // exit code
            string input = ""; // Terminal input
            Gestures gest = new Gestures();
            //listen
            new SpeechRecognizer("grammar.grxml");

            p.StartInfo.FileName = "Feedback.exe";
            p.Start();

            // init feedback coordinates
           
            pf.num = 1;
            pf.letra = 1;

            if (KinectSensor.KinectSensors.Count == 0)
            {
                System.Windows.MessageBox.Show("No Kinects detected");
                return;
            }
            try
            {
                myKinect = KinectSensor.KinectSensors[0];
                myKinect.SkeletonStream.Enable();
                myKinect.DepthStream.Range = DepthRange.Near;
                myKinect.SkeletonStream.EnableTrackingInNearRange = true;
                myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                myKinect.SkeletonFrameReady += gest.MyKinect_SkeletonFrameReady;
                myKinect.Start();


            }
            catch
            {
                System.Windows.MessageBox.Show("Kinect initialise failed");
                return;
            }


            //End of initialization



            gest.recog += new GestureRecognized(gest_recog);


            do
            {
                input = Console.ReadLine();
                exit = uci(input);


            } while (exit != 0);
            
            if(!p.HasExited)
            {
                p.Kill();
            }
            
        }
        #region GestureEvent
        //########### Gesture Event ##########################
        static void gest_recog(object sender, GestureEventArgs e)
        {
            
            if(go)
            {
                TranslateG(e.codeGesture);

            }
            else
            {
                Debug.Write("not go\n");
            }
        }
        #endregion GestureEvent

        #region TranslateG
        static void TranslateG(int code)
        {
            string cmd = " ";
            if (code == 1) // Help
            {
                cmd = "6-d1-d1";
                process(cmd);
                arguments = string.Join(" ", "3", coordL[pf.letra], coordN[pf.num], "1", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();

            }
            else if (code == 3) // Cancel
            {
                cmd = "0-d1-d1";
                process(cmd);
                if (!p.HasExited)
                {
                    p.Kill();
                    arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Start();
                }
               

            }



            if (code == 8) // swipe up
            {
                pf.num++;
                if (pf.num > 8) pf.num = 1;
                arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();
                // Console.WriteLine("up");

            }
            else if (code == 2) // swipe down
            {
                pf.num--;
                if (pf.num < 1) pf.num = 8;
                arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();
                //Console.WriteLine("down");

            }
            else if (code == 4) // swipe left
            {
                pf.letra--;
                if (pf.letra < 1) pf.letra = 8;
                arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();
                //Console.WriteLine("Left");

            }
            else if (code == 6) // swipe right
            {
                pf.letra++;
                if (pf.letra > 8) pf.letra = 1;
                arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();
                //Console.WriteLine("Right");
            }
            else if (code == 5) // select
            {
                if (Cstate > 0)
                {
                    cmd = "3-j-" + letras[pf.letra] + pf.num;
                    process(cmd);
                }


            }

        }
        #endregion

        #region TranslateSP
        public void translateSP(string s, float confidence)
        {
            string[] stArr = s.Split(' ');
            bool validM = false; 
            string cmd = string.Empty;
            string[] badRecog = { "Não percebi.", "Pode repetir?", "Não ouvi bem", "Estamos com problemas de comunicação." };
            Random rand = new Random();
            int r; // to select random voice feedback

            if (stArr.Length > 2)
            {
                validM = true; // valid command
            }
            if (go)
            {
                if (confidence < 0.4)
                {
                    r = rand.Next(0, 4);
                    fala.Speak(badRecog[r]);
                    return;
                }
                if (confidence >= 0.4 && confidence < 0.75 && validM)
                {
                    flag = true;
                    lastSP = s;
                    switch (stArr[1])
                    {
                        case "mexer":
                            fala.Speak("Quer mesmo mexer " + stArr[2] + " ?");
                            break;
                        case "capturar":
                            fala.Speak("Quer mesmo capturar " + stArr[2] + " ?");
                            break;
                        case "jogadas":
                            fala.Speak("Quer saber quantas jogadas pode fazer?");
                            break;
                        case "a":
                            fala.Speak("Quer saber a melhor jogada?");
                            break;
                        case "desistir":
                            fala.Speak("Quer mesmo desistir?");
                            break;
                        case "um":
                            fala.Speak("Quer mesmo desistir?");
                            break;
                        case "castelo":
                            fala.Speak("Quer mesmo fazer castelo?");
                            break;

                    }

                }
                if (confidence >= 0.75)
                {
                    if (s == "sim" && flag)
                    {
                        stArr = lastSP.Split(' ');
                        flag = false;
                        if (stArr.Length > 2)
                        {
                            validM = true; // valid command
                        }
                        fala.Speak("Ok!");
                    }
                    else if (s == "não" && flag)
                    {
                        flag = false;
                        fala.Speak("O que deseja fazer?");
                        return;
                    }

                    if (!flag && validM)
                    {
                        if (stArr[1] == "mexer") // basic move
                        {
                            switch (stArr[2])
                            {
                                case "peão":
                                    cmd = "3-p-a1";
                                    break;
                                case "rei":
                                    cmd = "3-r-a1";
                                    break;
                                case "rainha":
                                    cmd = "3-q-a1";
                                    break;
                                case "cavalo":
                                    cmd = "3-n-a1";
                                    break;
                                case "bispo":
                                    cmd = "3-b-a1";
                                    break;
                                case "torre":
                                    cmd = "3-r-a1";
                                    break;

                            }
                            fala.Speak("Afirmativo");
                        }
                        else if (stArr[1] == "capturar") // capture piece
                        {
                            switch (stArr[2])
                            {
                                case "peão":
                                    cmd = "4-p-a1";
                                    break;
                                case "rei":
                                    cmd = "4-k-a1";
                                    break;
                                case "rainha":
                                    cmd = "4-q-a1";
                                    break;
                                case "cavalo":
                                    cmd = "4-n-a1";
                                    break;
                                case "bispo":
                                    cmd = "4-b-a1";
                                    break;
                                case "torre":
                                    cmd = "4-r-a1";
                                    break;

                            }
                            fala.Speak("Afirmativo");
                        }
                        else if (stArr[0] == "qual")
                        {
                            cmd = "6-d2-d2"; // help
                        }
                        else if (stArr[0] == "quantas")
                        {
                            cmd = "5-f1-f1"; // number of plays
                        }
                        else if (stArr[1] == "fazer")
                        {
                            cmd = "2-c1-c1"; //castle up
                        }
                        else if (stArr[1] == "desistir" || stArr[0] == "começar")
                        {
                            cmd = "1-a1-a1"; //give up
                        }
                        
                        process(cmd);

                    }
                }
            }
        }

        #endregion

        #region UCI
        //########## UCI protocol ################
        private static int uci(string str1)
        {
            int code = 1;
            Tts fala = new Tts();
            string[] play = { "É a sua vez", "Pode jogar", "Qual a sua jogada?" };
            Random rand = new Random();
            int r;

            if (str1.Length < 3)
            {
                return 0; // Error
            }

            if (str1 == "uci")
            {
                Console.WriteLine("id name Test\n");
                Console.WriteLine("id author Test\n");
                Console.WriteLine("uciok");
            }
            else if (str1 == "isready")
            {
                Console.WriteLine("readyok");
            }
            else if (str1 == "ucinewgame")
            {
                game = new ChessGame(); //start a new game
                if (!p.HasExited)
                {
                    p.Kill();
                }
            }
            else if (str1[0] == 'g' && str1[1] == 'o')
            {
                go = true; // activate gesture recognition

                //activate feedback
                arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Start();
                
                
            }
            else if (str1[0] == 'p' && str1[1] == 'o' && str1[2] == 's')
            {
                // Speak feedback
                r = rand.Next(0, 3);
                fala.Speak(play[r]);

                //get moves done by the other player
                int i = str1.Length;
                if (i > 22)
                {
                    string posi = str1[i - 4].ToString() + str1[i - 3].ToString();
                    string posf = str1[i - 2].ToString() + str1[i - 1].ToString();
                    Move xy = new Move(posi, posf, game.WhoseTurn);
                    Move pi = new Move(posi, posf, game.WhoseTurn);
                    MoveType type = game.ApplyMove(xy, true);
                }

            }
            else if (str1 == "quit")
            {
                code = 0;
            }

            return code;

        }
        #endregion

        #region Process
        static void process(string cmd)
        {
            IEnumerable<Move> validMoves;
            string move,movement,piece;
            Move movepi; // Move to be done 
            bool valid, castleK, castleQ; ;  // Valid plays?
            string[] aux, aux1;
            List<string> posfL = new List<string>(); //List of possible final positions letters 
            List<string> posfN = new List<string>(); //List of possible final positions numbers 

            if (cmd.Length<5) //bugs
            {
                return;
            }
            
            aux = cmd.Split('-');
            move = movement = "";
            validMoves = game.GetValidMoves(game.WhoseTurn); // get valid moves for player

            if(aux[0] == "0") // Cancel
            {
                fala.Speak("Ok, vou cancelar!!!");
                Cstate = 0;
                posf.Clear();
                posi.Clear();

            }
            else if (aux[0] == "1") //give up
            {

                Console.WriteLine("bestmove a1a1");
                p.Kill();

            }
            else if (aux[0] == "2") //Castle up
            {

                if ("White" == game.WhoseTurn.ToString())
                {
                    Move e1g1 = new Move("e1", "g1", Player.White);
                    Move e1c1 = new Move("e1", "c1", Player.White);
                    castleK = game.IsValidMove(e1g1);
                    castleQ = game.IsValidMove(e1c1);
                    if (castleK)
                    {
                        game.ApplyMove(e1g1, castleK);
                        Console.WriteLine("bestmove e1g1");
                        p.Kill();
                    }
                    else if (castleQ)
                    {
                        game.ApplyMove(e1c1, castleQ);
                        Console.WriteLine("bestmove e1c1");
                        p.Kill();
                    }
                    else
                    {
                        fala.Speak("Não pode fazer castelo!");
                    }

                }
                else
                {

                    Move e8g8 = new Move("E8", "G8", Player.Black);
                    Move e8c8 = new Move("E8", "C8", Player.Black);
                    castleK = game.IsValidMove(e8g8);
                    castleQ = game.IsValidMove(e8c8);
                    if (castleK)
                    {
                        game.ApplyMove(e8g8, castleK);
                        Console.WriteLine("bestmove e8g8");
                        p.Kill();
                    }
                    else if (castleQ)
                    {
                        game.ApplyMove(e8c8, castleQ);
                        Console.WriteLine("bestmove e8c8");
                        p.Kill();
                    }
                    else
                    {
                        fala.Speak("Não pode fazer castelo");

                    }

                }

            }
            else if (aux[0] == "3") //basic move
            {
                
                if(Cstate==0)       // received voice command
                {
                    Cstate = 1;     // gesture input
                    initp = aux[1];  // store initial piece
                }
                else if( Cstate==1) // received gesture input
                {
                    posf.Add(aux[2]);  // store final position
                    //encontrar posi
                    posi.Clear(); //just in case
                    for (int i = 0; i < validMoves.Count(); i++)
                    {
                        move = validMoves.ElementAt<Move>(i).ToString().ToLower(); // get a valid move
                        aux1 = move.Split('-');
                        if (aux1[1] == posf[0]) // check move final position 
                        {
                            piece = game.GetPieceAt(new Position(aux1[0])).GetFenCharacter().ToString().ToLower();//get piece
                            if (piece == initp)  //check piece
                            {
                                posi.Add(aux1[0]); // store initial position coordinates
                            }

                        }
                    }

                    if(posi.Count>1)
                    {
                        fala.Speak("Existe mais de um movimento possível, selecione a peça que deseja mover!");
                        Cstate = 2; //more than one movement possible
                    }
                    else if(posi.Count == 0)
                    {
                        Cstate = 0;
                        fala.Speak("Não existe nenhuma jogada com a peça escolhida!");
                    }
                    else
                    {
                        Cstate = 0;
                        //play
                        movepi = new Move(posi[0], posf[0], game.WhoseTurn);
                        valid = game.IsValidMove(movepi);
                        MoveType type = game.ApplyMove(movepi, valid);
                        Console.WriteLine("bestmove {0}{1}", posi[0], posf[0]);
                        go = false;
                        p.Kill();
                        posf.Clear();
                        posi.Clear();

                    }

                }
                else if(Cstate==2) // addicional information needed
                {
                    for (int i=0; i<posi.Count;i++)
                    {
                        if(posi[i]==aux[2]) //check possible initial plays
                        {
                            Cstate = 0;
                            //play
                            movepi = new Move(posi[i], posf[0], game.WhoseTurn);
                            valid = game.IsValidMove(movepi);
                            MoveType type = game.ApplyMove(movepi, valid);
                            Console.WriteLine("bestmove {0}{1}", posi[i], posf[0]);
                            go = false;
                            p.Kill();
                            posf.Clear();
                            posi.Clear();

                            break;
                        }
                        
                        
                    }
                    
                }

            }
            else if (aux[0] == "4") //capture piece
            {
                //find piece to capture
                for (int i = 0; i < validMoves.Count(); i++)
                {
                    move = validMoves.ElementAt<Move>(i).ToString().ToLower();
                    aux1 = move.Split('-');
                    if (game.GetPieceAt(new Position(aux1[1])) != null) // is final position a piece?
                    {
                        piece = game.GetPieceAt(new Position(aux1[1])).GetFenCharacter().ToString().ToLower();
                        if (aux[1] == piece) // is it the piece we are looking for
                        {
                            posf.Add(aux1[1]);
                            posi.Add(aux1[0]);
                        }
                    }

                }

                if (posi.Count==1 && posf.Count==1)
                {
                    
                    movepi = new Move(posi[0], posf[0], game.WhoseTurn);
                    valid = game.IsValidMove(movepi);
                    MoveType type = game.ApplyMove(movepi, valid);
                    Console.WriteLine("bestmove {0}{1}", posi[0], posf[0]);
                    go = false;
                    p.Kill();
                    posf.Clear();
                    posi.Clear();

                }
                else if (posi.Count > 1 || posf.Count > 1)
                {
                    fala.Speak("Tem mais de uma possibilidade, use o comando quero mexer!");
                    // ativar cenas para complementar a jogada

                }
                else
                {
                    fala.Speak("Não possui jogadas para capturar a peça escolhida!");
                }

            }
            else if (aux[0] == "5") //number of plays
            {

                movement = validMoves.Count().ToString();
                if (movement == "1")
                {
                    movement = "Tem 1 movimento possível!";
                }
                else
                {
                    movement = "Tem" + movement + "movimentos possíveis!";
                }
                fala.Speak(movement);

            }
            else if (aux[0] == "6") // help
            {

                move = validMoves.ElementAt<Move>(0).ToString().ToLower();
                aux1 = move.Split('-');

                string auxes = aux1[1];
                string auxes1 = aux1[0];

                for (int j = 1; j < 9; j++)
                {
                    if (letras[j] == auxes1[0].ToString())
                    {
                        pf.letra = j;
                        pf.num = Int32.Parse(auxes1[1].ToString());
                    }
                    if (letras[j] == auxes[0].ToString())
                    {
                        pf.letra = j;
                        pf.num = Int32.Parse(auxes[1].ToString());
                    }
                }

                fala.Speak("A melhor jogada é " + move);

            }
           

        }

        #endregion


    } //end class program

    #region SpeechRecognizer
    class SpeechRecognizer
    {
        private SpeechRecognitionEngine sr;

        public SpeechRecognizer(string GName)
        {

            //creates the speech recognizer engine
            sr = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            sr.SetInputToDefaultAudioDevice();

            Grammar g = null;

            //verifies if file exist, and loads the Grammar file, else load defualt grammar
            if (System.IO.File.Exists(GName))
            {

                g = new Grammar(GName);
                g.Enabled = true;

            }

            // Create a Grammar object and load it to the recognizer.
            g = new Grammar(GName);
            g.Name = ("Chess");

            //load Grammar to speech engine
            sr.LoadGrammar(g);
            //assigns a method, to execute when speech is recognized
            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);


            // Start asynchronous, continuous speech recognition.
            sr.RecognizeAsync(RecognizeMode.Multiple);


        }

        //##### Speech Event ############
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Program main = new Program();

            //gets recognized text.

            main.translateSP(e.Result.Text, e.Result.Confidence);



            //sr.RecognizeAsyncStop();


        }


    }
    #endregion

    #region Speech Syntgesizer
    /* ############################
            Speech Synthesizer
       ###########################*/
    class Tts
    {
        SpeechSynthesizer tts = null;
        static SoundPlayer player = new SoundPlayer();

        /*
         * Text to Speech
         */
        public Tts()
        {


            //create speech synthesizer
            tts = new SpeechSynthesizer();

            //set voice
            tts.SelectVoiceByHints(VoiceGender.Female, VoiceAge.NotSet, 0, new System.Globalization.CultureInfo("pt-PT"));

            //set function to play audio after synthesis is complete
            tts.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(tts_SpeakCompleted);

        }

        /*
         * Speak
         * 
         * @param text - text to convert
         */
        public void Speak(string text)
        {
            while (player.Stream != null)
            {
            }

            //create audio stream with speech
            player.Stream = new System.IO.MemoryStream();
            tts.SetOutputToWaveStream(player.Stream);
            tts.SpeakAsync(text);
        }

        //################## Speak event ##################### 
        void tts_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            if (player.Stream != null)
            {
                player.Stream.Position = 0;
                player.Play();
                player.Stream = null;
            }
        }
    }
    #endregion

}

