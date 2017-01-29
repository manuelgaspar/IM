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

        public static ChessGame game = new ChessGame();
        public static Process p = new Process();
        public static position pi, pf;
        public static bool flag = false;  // flag para saber a posição a actualizar (True-> Pi || False -> pf)
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
            pi.num = 1;
            pi.letra = 1;
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
                Debug.Write("reading terminal");
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
                Debug.Write("go\n");
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
                arguments = string.Join(" ", "3", coordL[pi.letra], coordN[pi.num], "1", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();

            }
            else if (code == 3) // Cancel
            {
                cmd = "1-d1-d1";
                process(cmd);
            }

            if (flag)
            {

                if (code == 8) // swipe up
                {

                    pi.num++;
                    if (pi.num > 8) pi.num = 1;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 2) // swipe down
                {
                    pi.num--;
                    if (pi.num < 1) pi.num = 8;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 4) // swipe left
                {
                    pi.letra--;
                    if (pi.letra < 1) pi.letra = 8;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 6) // swipe right
                {
                    pi.letra++;
                    if (pi.letra > 8) pi.letra = 1;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 5) // select
                {
                    cmd = "4-" + letras[pi.letra] + pi.num + "-" + letras[pf.letra] + pf.num;
                    //process(cmd);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
            }
            else
            {

                if (code == 8) // swipe up
                {
                    pf.num++;
                    if (pf.num > 8) pf.num = 1;
                    arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();
                    Console.WriteLine("up");

                }
                else if (code == 2) // swipe down
                {
                    pf.num--;
                    if (pf.num < 1) pf.num = 8;
                    arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();
                    Console.WriteLine("down");

                }
                else if (code == 4) // swipe left
                {
                    pf.letra--;
                    if (pf.letra < 1) pf.letra = 8;
                    arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();
                    Console.WriteLine("Left");

                }
                else if (code == 6) // swipe right
                {
                    pf.letra++;
                    if (pf.letra > 8) pf.letra = 1;
                    arguments = string.Join(" ", "0", coordL[pf.letra], coordN[pf.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();
                    Console.WriteLine("Right");
                }
                else if (code == 5) // select
                {
                    //flag = !flag;

                    cmd = "3-" + letras[pi.letra] + pi.num + "-" + letras[pf.letra] + pf.num;
                    //process(cmd);
                    Console.WriteLine(cmd);

                }
                
            }

        }
        #endregion

        #region TranslateSP
        public void translateSP(string s, float confidence)
        {
            string[] stArr = s.Split(' ');
            string cmd = string.Empty;
            Tts f = new Tts();
            string[] badRecog = { "Não percebi.", "Pode repetir?", "Não ouvi bem", "Estamos com problemas de comunicação." };
            Random rand = new Random();
            int r;

            if(stArr[1]== "mexer")
            {
                switch (stArr[2])
                {
                    case "peão":
                        //cmd="3-"
                        break;
                    case "rei":
                        //
                        break;
                    case "rainha":
                        break;
                    case "cavalo":
                        break;
                    case "bispo":
                        break;
                    case "torre":
                        break;
                    default:
                        break;
                }
            }
            else if(stArr[1] == "comer")
            {
                switch (stArr[2])
                {
                    case "peão":
                        //cmd=""
                        break;
                    case "rei":
                        //
                        break;
                    case "rainha":
                        break;
                    case "cavalo":
                        break;
                    case "bispo":
                        break;
                    case "torre":
                        break;
                    default:
                        break;
                }
            }
            else if(stArr[0]=="qual")
            {
                cmd = "";
            }
            else if (stArr[0] == "quantas")
            {
                cmd = "";
            }
            else if (stArr[0] == "fazer")
            {
                cmd = "";
            }
            else if (stArr[1] == "desistir" || stArr[0]=="começar")
            {
                cmd = "";
            }




            if (go)
            {
                process(cmd);
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
            }
            else if (str1[0] == 'g' && str1[1] == 'o')
            {
                go = true; // activate gesture recognition

                //activate feedback
                arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
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
            Tts fala = new Tts();
            string move, posf, posi;
            Move movepi; // Move to be done 
            bool valid;  // Valid play?
            string[] aux, aux1;
            List<string> posfL = new List<string>(); //List of possible final positions letters 
            List<string> posfN = new List<string>(); //List of possible final positions numbers 


            aux = cmd.Split('-');

            //posi = aux[1];
            //posf = aux[2];
            validMoves = game.GetValidMoves(game.WhoseTurn); // get valid moves for player

            if(aux[0] == "1")
            {
                fala.Speak("Ok, vou cancelar!!!");
                Cstate = 0;
                flag = false;

            }
            /*else if (aux[0] == "3")//jogada simples
            {

                //jogar
                movepi = new Move(posi, posf, game.WhoseTurn);
                valid = game.IsValidMove(movepi);
                MoveType type = game.ApplyMove(movepi, valid);
                if (valid)
                {
                    Console.WriteLine("bestmove {0}{1}", posi, posf);
                    go = false;
                    p.Kill();

                }
                else
                {
                    // Console.WriteLine("Jogada inválida");
                }

            }*/
            if (aux[0] == "4")  //jogadas possiveis com a peça
            {

                if (game.GetPieceAt(new Position(aux[1])) != null)
                {

                    for (int i = 0; i < validMoves.Count(); i++)
                    {
                        move = validMoves.ElementAt<Move>(i).ToString().ToLower();
                        aux1 = move.Split('-');


                        if (aux1[0] == aux[1])
                        {
                            string auxes = aux1[1];
                            flag = false;

                            for (int j = 1; j < 9; j++)
                            {
                                // Console.WriteLine("if: {0}=={1}", letras[j], auxes[0]);
                                if (letras[j] == auxes[0].ToString())
                                {
                                    posfL.Add(j.ToString());
                                    posfN.Add(auxes[1].ToString());
                                }
                            }



                        }

                    }

                    if (!flag)
                    {
                        arguments = string.Join(" ", "2", coordL[pi.letra], coordN[pi.num], posfL.Count());
                        string argAux;
                        for (int k = 0; k < posfL.Count(); k++)
                        {
                            argAux = " " + coordL[Int32.Parse(posfL.ElementAt(k))] + " " + coordN[Int32.Parse(posfN.ElementAt(k))];
                            arguments = arguments + argAux;
                        }
                        auxArgument = arguments;
                        pf.letra = Int32.Parse(posfL.ElementAt(0));
                        pf.num = Int32.Parse(posfN.ElementAt(0));
                        arguments = arguments + " " + coordL[pf.letra] + " " + coordN[pf.num];
                        //Console.WriteLine(arguments);


                    }

                }



            }
            else if (aux[0] == "6") // help
            {

                move = validMoves.ElementAt<Move>(0).ToString().ToLower();
                aux1 = move.Split('-');

                string auxes = aux1[1];
                string auxes1 = aux1[0];

                for (int j = 1; j < 9; j++)
                {
                    // Console.WriteLine("if: {0}=={1}", letras[j], auxes[0]);
                    if (letras[j] == auxes1[0].ToString())
                    {
                        pi.letra = j;
                        pi.num = Int32.Parse(auxes1[1].ToString());
                    }
                    if (letras[j] == auxes[0].ToString())
                    {
                        pf.letra = j;
                        pf.num = Int32.Parse(auxes[1].ToString());
                    }
                }

                fala.Speak("A melhor jogada é " + move);

                // Console.WriteLine("{0},{1}->{2},{3}", pf.letra, pf.num, pi.letra, pi.num);



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
            Debug.WriteLine("listened\n");

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

