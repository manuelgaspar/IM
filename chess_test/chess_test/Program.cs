using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using ChessDotNet;
using System.Windows;
using System.Windows.Forms;


using Microsoft.Speech.Recognition;
using Microsoft.Speech.Recognition.SrgsGrammar;

using System.Media;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.AudioFormat;

using Microsoft.Kinect;



namespace chess_test
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
        public static bool flag = true;  // flag para saber a posição a actualizar
        public static bool go = false;   // flag to detect player turn
        public static KinectSensor myKinect;
        public static string[] letras = { " ", "a", "b", "c", "d", "e", "f", "g", "h" };
        public static String[] coordN = { " ", "586", "519", "452", "385", "318", "251", "184", "117" };
        public static String[] coordL = { "", "6", "73", "140", "207", "274", "341", "408", "475" };
        public static int state = 0; // state machine
        public static int frame = 0; // frame number
        public static string arguments, auxArgument;

        [STAThread]
        static void Main(string[] args)
        {
            string str1;
            int code;
            p.StartInfo.FileName = "Feedback.exe";

            /*System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.Run(new Form());
            */




            // ##################  init Kinect  ##############
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
                //myKinect.SkeletonFrameReady += MyKinect_SkeletonFrameReady;
                //myKinect.SkeletonFrameReady -= MyKinect_SkeletonFrameReady;
                myKinect.Start();
                
                
            }
            catch
            {
                System.Windows.MessageBox.Show("Kinect initialise failed");
                return;
            }


            pi.num = 1;
            pi.letra = 1;
            pf.num = 1;
            pf.letra = 1;

            using (FileStream fs = System.IO.File.Create("file.txt"))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                
                do
                {
                    str1 = Console.ReadLine();
                    code = uci(str1);

                    writer.WriteLine(str1);


                } while (code != 0);
                p.Kill();
                writer.Close();
            }
               

        }

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
                str1 = "error";
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
                go = true; // activate gesture
               

                arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                p.StartInfo.Arguments = arguments;
                p.Start();

                //input voice command
                //SpeechRecognizer voz= new SpeechRecognizer("grammar.grxml");
            }
            else if (str1[0] == 'p' && str1[1] == 'o' && str1[2] == 's')
            {
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

        //########## Process Speech ################
        private static void processSP(string cmd)
        {

            IEnumerable<Move> validMoves;
            string move, posf, posi, piece, movement;
            Move xy;
            bool castleK, castleQ, valid;
            string[] aux, aux1;
            Tts fala = new Tts();

            aux = cmd.Split('-');

            move = movement = "";
            posi = "a1";
            validMoves = game.GetValidMoves(game.WhoseTurn);



            if (aux[0] == "1")//Desisto
            {

                Console.WriteLine("bestmove a1a1");

            }
            else if (aux[0] == "2") //Castelo
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
                    }
                    else if (castleQ)
                    {
                        game.ApplyMove(e1c1, castleQ);
                        Console.WriteLine("bestmove e1c1");
                    }
                    else
                    {
                        //Console.WriteLine("Não pode fazer castelo");
                        fala.Speak("Não pode fazer castelo!");
                        new SpeechRecognizer("grammar.grxml");
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
                    }
                    else if (castleQ)
                    {
                        game.ApplyMove(e8c8, castleQ);
                        Console.WriteLine("bestmove e8c8");
                    }
                    else
                    {
                        //Console.WriteLine("Não pode fazer castelo");
                        fala.Speak("Não pode fazer castelo");
                        new SpeechRecognizer("grammar.grxml");

                    }

                }

            }
            else if (aux[0] == "3")//jogada simples
            {

                //encontrar posi
                posf = aux[2];
                for (int i = 0; i < validMoves.Count(); i++)
                {
                    move = validMoves.ElementAt<Move>(i).ToString().ToLower();
                    aux1 = move.Split('-');
                    if (aux1[1] == posf)
                    {

                        piece = game.GetPieceAt(new Position(aux1[0])).GetFenCharacter().ToString().ToLower();
                        if (piece == aux[1])
                        {
                            posi = aux1[0];
                        }

                    }
                }

                //jogar
                xy = new Move(posi, posf, game.WhoseTurn);
                valid = game.IsValidMove(xy);
                MoveType type = game.ApplyMove(xy, valid);
                if (valid == true)
                {
                    Console.WriteLine("bestmove {0}{1}", posi, posf);

                }
                else
                {
                    //Console.WriteLine("Jogada inválida");
                    fala.Speak("Jogada Inválida");
                    new SpeechRecognizer("grammar.grxml");

                }

            }
            else if (aux[0] == "4") //comer peça (4-peça)
            {
                //encontrar peça a comer
                for (int i = 0; i < validMoves.Count(); i++)
                {
                    move = validMoves.ElementAt<Move>(i).ToString().ToLower();
                    aux1 = move.Split('-');
                    if (game.GetPieceAt(new Position(aux1[1])) != null)
                    {
                        piece = game.GetPieceAt(new Position(aux1[1])).GetFenCharacter().ToString().ToLower();
                        if (aux[1] == piece)
                        {
                            movement = move;

                        }
                    }

                }
                if (movement != "")
                {
                    aux1 = movement.Split('-');
                    Console.WriteLine("bestmove {0}{1}", aux1[0], aux1[1]);
                    xy = new Move(aux1[0], aux1[1], game.WhoseTurn);
                    valid = game.IsValidMove(xy);
                    MoveType type = game.ApplyMove(xy, valid);

                }
                else
                {
                    // Console.WriteLine("Jogada inválida");
                    fala.Speak("Jogada Inválida");
                    new SpeechRecognizer("grammar.grxml");

                }

            }
            else if (aux[0] == "5") //numero de jogadas
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
                new SpeechRecognizer("grammar.grxml");

            }
            else if (aux[0] == "6") //Qual a melhor jogada
            {
                for (int i = 0; i < validMoves.Count(); i++)
                {
                    move = validMoves.ElementAt<Move>(i).ToString().ToLower();
                    aux1 = move.Split('-');
                    if (game.GetPieceAt(new Position(aux1[1])) != null)
                    {
                        movement = move;
                        i = validMoves.Count();
                    }

                }
                if (movement != "")
                {
                    //Console.WriteLine("A melhor jogada é {0}", movement);
                    fala.Speak("A melhor jogada é " + movement);
                    new SpeechRecognizer("grammar.grxml");

                }
                else
                {
                    //Console.WriteLine("A melhor jogada é {0}", move);
                    fala.Speak("A melhor jogada é " + move);
                    new SpeechRecognizer("grammar.grxml");

                }

            }
            else if (aux[0] == "7") //é uma boa jogada peça xy?
            {

                if (game.GetPieceAt(new Position(aux[2])) != null)
                {
                    // Console.WriteLine("Sim é.");
                    fala.Speak("Sim é.");
                    new SpeechRecognizer("grammar.grxml");

                }
                else
                {
                    //Console.WriteLine("Não.");
                    fala.Speak("Não, não é.");
                    System.Threading.Thread.Sleep(2000);
                    processSP("6-" + aux[1] + "-" + aux[2]);
                }
            }
            else
            {
                fala.Speak("Não percebi o seu comando, por favor repita.");
                new SpeechRecognizer("grammar.grxml");
            }


        } // end of process


        //########## Translate Speech ################
        /* This function translates the recognized speech into a notation used only in this program */

        public void translateSP(string s, double confidence)
        {
            string[] stArr = s.Split(' ');
            string cmd = string.Empty;
            Tts f = new Tts();
            string[] badRecog = { "Não percebi.", "Pode repetir?", "Não ouvi bem", "Estamos com problemas de comunicação." };
            Random rand = new Random();
            int r;

            if (stArr[0] == "sim")
            {
                string saveCommand = System.IO.File.ReadLines("cmdsave.txt").First();
                processSP(saveCommand);
                if(saveCommand[0] == '5')
                {
                    new SpeechRecognizer("grammar.grxml");
                }
            }
            else if(stArr[0] == "não")
            {
                f.Speak("Por favor repita o comando.");
                new SpeechRecognizer("grammar.grxml");
            }
            else
            {
            for (int i = 0; i < stArr.Length; i++)
            {
                switch (stArr[i])
                {
                    case "desistir":
                        cmd = "1";
                        break;
                        break;
                    case "começar":
                        cmd = "1";
                        break;
                        break;
                    case "castelo":
                        cmd = "2";
                        break;
                        break;
                    case "mexer":
                        cmd = "3";
                        break;
                    case "peão":
                        cmd = cmd + "-p";
                        break;
                    case "torre":
                        cmd = cmd + "-r";
                        break;
                    case "cavalo":
                        cmd = cmd + "-n";
                        break;
                    case "bispo":
                        cmd = cmd + "-b";
                        break;
                    case "rainha":
                        cmd = cmd + "-q";
                        break;
                    case "rei":
                        cmd = cmd + "-k";
                        break;
                    case "alfa":
                        cmd = cmd + "-a";
                        break;
                    case "beta":
                        cmd = cmd + "-b";
                        break;
                    case "charlie":
                        cmd = cmd + "-c";
                        break;
                    case "delta":
                        cmd = cmd + "-d";
                        break;
                    case "eco":
                        cmd = cmd + "-e";
                        break;
                    case "foxtrote":
                        cmd = cmd + "-f";
                        break;
                    case "golfe":
                        cmd = cmd + "-g";
                        break;
                    case "hotel":
                        cmd = cmd + "-h";
                        break;
                    case "1":
                        cmd = cmd + "1";
                        break;
                    case "2":
                        cmd = cmd + "2";
                        break;
                    case "3":
                        cmd = cmd + "3";
                        break;
                    case "4":
                        cmd = cmd + "4";
                        break;
                    case "5":
                        cmd = cmd + "5";
                        break;
                    case "6":
                        cmd = cmd + "6";
                        break;
                    case "7":
                        cmd = cmd + "7";
                        break;
                    case "8":
                        cmd = cmd + "8";
                        break;
                    case "comer":
                        cmd = "4";
                        break;
                    case "jogadas":
                        cmd = "5";
                        break;
                        break;
                    case "qual":
                        cmd = "6";
                        break;
                        break;
                    case "boa":
                        string aux = cmd;
                        cmd = "7" + aux;
                        break;
                        break;
                }
            }

            if ((cmd[0] == '3' || cmd[0] == '7') && cmd.Length != 6)
            {
               f.Speak("Jogada Inválida");
               new SpeechRecognizer("grammar.grxml");
            }

                /* Confidence level check */

            else if (confidence < 0.4)
            {
                r = rand.Next(0, 4);
                f.Speak(badRecog[r]);
                new SpeechRecognizer("grammar.grxml");

            }
            else if (confidence >= 0.1 && confidence <= 0.75)
                {
                    string[] confPieces = { "peão", "torre", "cavalo", "bispo", "rainha", "rei" };
                    string[] confCoordinates = { "alfa", "beta", "charli", "delta", "eco", "foxtrote", "golfe", "hotel" };
                    using (FileStream fs = System.IO.File.Create("cmdsave.txt"))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(cmd);
                        writer.Close();
                    }
                    if (cmd[0] == '1')
                    {
                        f.Speak("Quer mesmo terminar este jogo?");
                    }
                    else if(cmd[0] == '2')
                    {
                        f.Speak("Quer mesmo fazer castelo?");
                    }
                    else if (cmd[0] == '3')
                    {
                        int auxP = 0;
                        int auxC = 0;
                        switch (cmd[2])
                        {
                            case 'p':
                                auxP = 0;
                                break;
                            case 'r':
                                auxP = 1;
                                break;
                            case 'n':
                                auxP = 2;
                                break;
                            case 'b':
                                auxP = 3;
                                break;
                            case 'q':
                                auxP = 4;
                                break;
                            case 'k':
                                auxP = 5;
                                break;
                        }
                        switch (cmd[4])
                        {
                            case 'a':
                                auxC = 0;
                                break;
                            case 'b':
                                auxC = 1;
                                break;
                            case 'c':
                                auxC = 2;
                                break;
                            case 'd':
                                auxC = 3;
                                break;
                            case 'e':
                                auxC = 4;
                                break;
                            case 'f':
                                auxC = 5;
                                break;
                            case 'g':
                                auxC = 6;
                                break;
                            case 'h':
                                auxC = 7;
                                break;
                        }
                        f.Speak("Quer mexer " + confPieces[auxP] + " para " + confCoordinates[auxC] + cmd[5] + "?");
                    }
                    else if (cmd[0] == '4')
                    {
                        int auxE = 0;
                        switch (cmd[2])
                        {
                            case 'p':
                                auxE = 0;
                                break;
                            case 'r':
                                auxE = 1;
                                break;
                            case 'n':
                                auxE = 2;
                                break;
                            case 'b':
                                auxE = 3;
                                break;
                            case 'q':
                                auxE = 4;
                                break;
                            case 'k':
                                auxE = 5;
                                break;
                        }
                        f.Speak("Quer comer " + confPieces[auxE] + "?");
                    }
                    else if (cmd[0] == '5')
                    {
                        f.Speak("Quer saber quantas jogadas pode fazer?");
                    }
                    else if(cmd[0] == '6')
                    {
                        f.Speak("Quer saber qual a melhor jogada?");
                    }
                    else if(cmd[0] == '7')
                    {
                        int auxB = 0;
                        int auxD = 0;
                        switch(cmd[2])
                        {
                            case 'p':
                                auxB = 0;
                                break;
                            case 'r':
                                auxB = 1;
                                break;
                            case 'n':
                                auxB = 2;
                                break;
                            case 'b':
                                auxB = 3;
                                break;
                            case 'q':
                                auxB = 4;
                                break;
                            case 'k':
                                auxB = 5;
                                break;
                        }
                        switch (cmd[4])
                        {
                            case 'a':
                                auxD = 0;
                                break;
                            case 'b':
                                auxD = 1;
                                break;
                            case 'c':
                                auxD = 2;
                                break;
                            case 'd':
                                auxD = 3;
                                break;
                            case 'e':
                                auxD = 4;
                                break;
                            case 'f':
                                auxD = 5;
                                break;
                            case 'g':
                                auxD = 6;
                                break;
                            case 'h':
                                auxD = 7;
                                break;
                        }
                        f.Speak("Quer saber se mexer " + confPieces[auxB] + " para " + confCoordinates[auxD] + cmd[5] + " é uma boa jogada?");
                    }
                    new SpeechRecognizer("confirmationgrammar.grxml");
                }
                else if (confidence > 0.75)
                {
                    processSP(cmd);
                }

            }
        }

        //##################################   Kinect  Event ############################################## 
        private static void MyKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            int code = 0;
            int maxframes = 30;
            //string message = " ";

            if(myKinect.AudioSource==null)
            {
                Console.WriteLine("merda");
            }

            Skeleton[] skeletons = null;
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                    //message = "Frame";
                }
            }
            if (skeletons == null) return;
            foreach (Skeleton skeleton in skeletons)
            {

                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if (state == 0)
                    {

                        if (ValidStartPos(skeleton))
                        {
                            state = 1;
                            //message = "Make a move";
                            //  Console.WriteLine(message);
                            frame = 0;
                        }

                    }
                    else if (state == 1)
                    {
                        frame++;
                        if (frame > maxframes)
                        {

                            state = 0;
                        }

                        if (SwipeLeft(skeleton))
                        {
                            state = 0;
                            //message = "SwipeLeft";
                            // Console.WriteLine(message);
                            code = 4;
                        }
                        else if (SwipeRight(skeleton))
                        {
                            state = 0;
                            //message = "SwipeRight";
                            // Console.WriteLine(message);
                            code = 6;
                        }
                        else if (Select(skeleton))
                        {
                            state = 0;
                            //message = "Selected";
                            //Console.WriteLine(message);
                            code = 5;
                        }
                        else if (SwipeUp(skeleton))
                        {
                            state = 0;
                            //message = "SwipeUp";
                            // Console.WriteLine(message);
                            code = 8;
                        }
                        else if (SwipeDown(skeleton))
                        {
                            state = 0;
                            //message = "SwipeDown";
                            // Console.WriteLine(message);
                            code = 2;
                        }
                        else if (Clap(skeleton))
                        {
                            state = 0;
                            //message = "Clap";
                            // Console.WriteLine(message);
                            code = 1;
                        }
                        else if (Anular(skeleton))
                        {
                            state = 0;
                            //message = "Cancelar";
                            // Console.WriteLine(message);
                            code = 3;
                        }

                        if (go)
                            translateG(code);

                    }
                }
            }


        }

        // ########### Gesture Functions ###############################
        private static bool ValidStartPos(Skeleton skeleton)
        {

            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint ERight = skeleton.Joints[JointType.ElbowRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((SRight.Position.Y - ERight.Position.Y > 0.2f) &&
                (Math.Abs(SRight.Position.Y - HRight.Position.Y) < 0.3f) &&
                (ELeft.Position.Y > HLeft.Position.Y) &&
                (HRight.Position.X - SRight.Position.X > 0.1f) &&
                (Math.Abs(HLeft.Position.X - HRight.Position.X) > 0.5f))
            {

                return true;
            }

            return false;
        }
        private static bool SwipeLeft(Skeleton skeleton)
        {
            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((Math.Abs(SRight.Position.Y - HRight.Position.Y) < 0.3f) &&
                (SRight.Position.X > HRight.Position.X) &&
                (Math.Abs(SRight.Position.X - HRight.Position.X) > 0.2f) &&
                (ELeft.Position.Y - HLeft.Position.Y > 0.2f))
            {
                return true;
            }


            return false;
        }
        private static bool SwipeRight(Skeleton skeleton)
        {
            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint ERight = skeleton.Joints[JointType.ElbowRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((SRight.Position.Y - ERight.Position.Y < 0.2f) &&
                (Math.Abs(SRight.Position.Y - HRight.Position.Y) < 0.2f) &&
                (Math.Abs(SRight.Position.X - HRight.Position.X) > 0.3f) &&
                (ELeft.Position.Y - HLeft.Position.Y > 0.2f))
            {
                return true;
            }


            return false;

        }
        private static bool SwipeUp(Skeleton skeleton)
        {
            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((Math.Abs(HRight.Position.Y - SRight.Position.Y) > 0.2f) &&
                (HRight.Position.Y > SRight.Position.Y) &&
                (Math.Abs(HRight.Position.X - SRight.Position.X) < 0.3f) &&
                (ELeft.Position.Y - HLeft.Position.Y > 0.2f))
            {
                return true;
            }


            return false;

        }
        private static bool SwipeDown(Skeleton skeleton)
        {
            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((Math.Abs(HRight.Position.Y - SRight.Position.Y) > 0.3f) &&
                (HRight.Position.Y < SRight.Position.Y) &&
                (Math.Abs(HRight.Position.X - SRight.Position.X) < 0.3f) &&
                (ELeft.Position.Y - HLeft.Position.Y > 0.2f))
            {
                return true;
            }


            return false;

        }


        private static bool Select(Skeleton skeleton)
        {

            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((Math.Abs(SRight.Position.Y - HRight.Position.Y) < 0.3f) &&
                (Math.Abs(HRight.Position.Z - SRight.Position.Z) > 0.5f) &&
                (ELeft.Position.Y - HLeft.Position.Y > 0.2f))
            {
                return true;
            }

            return false;
        }

        private static bool Clap(Skeleton skeleton)
        {

            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((Math.Abs(HLeft.Position.Y - HRight.Position.Y) < 0.1f) &&
                (Math.Abs(HRight.Position.Z - HLeft.Position.Z) < 0.1f) &&
                (Math.Abs(HLeft.Position.X - HRight.Position.X) < 0.1f))
            {
                return true;
            }

            return false;
        }

        private static bool Anular(Skeleton skeleton)
        {

            Joint SRight = skeleton.Joints[JointType.ShoulderRight];
            Joint ERight = skeleton.Joints[JointType.ElbowRight];
            Joint HRight = skeleton.Joints[JointType.HandRight];
            Joint ELeft = skeleton.Joints[JointType.ElbowLeft];
            Joint HLeft = skeleton.Joints[JointType.HandLeft];

            if ((SRight.Position.Y - ERight.Position.Y > 0.2f) &&
                (Math.Abs(SRight.Position.Y - HRight.Position.Y) < 0.3f) &&
                (ELeft.Position.Y < HLeft.Position.Y) &&
                (HRight.Position.X - SRight.Position.X > 0.1f) &&
                (Math.Abs(HLeft.Position.X - HRight.Position.X) > 0.5f) &&
                (Math.Abs(HLeft.Position.Y - SRight.Position.Y) < 0.2f))
            {
                return true;
            }

            return false;
        }

        //################### Translate Gesture ########################
        private static void translateG(int code)
        {
            string cmd = " ";
            if (code == 1)
            {
                cmd = "6-d1-d1";
                processG(cmd);
                arguments = string.Join(" ", "3", coordL[pi.letra], coordN[pi.num], "1", coordL[pf.letra], coordN[pf.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();

            }
            else if (code == 3)
            {
                arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                p.StartInfo.Arguments = arguments;
                p.Kill();
                p.Start();
                flag = true;
            }
            if (flag)
            {

                if (code == 8)
                {

                    pi.num++;
                    if (pi.num > 8) pi.num = 1;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 2)
                {
                    pi.num--;
                    if (pi.num < 1) pi.num = 8;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 4)
                {
                    pi.letra--;
                    if (pi.letra < 1) pi.letra = 8;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 6)
                {
                    pi.letra++;
                    if (pi.letra > 8) pi.letra = 1;
                    arguments = string.Join(" ", "0", coordL[pi.letra], coordN[pi.num]);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 5)
                {
                    cmd = "4-" + letras[pi.letra] + pi.num + "-" + letras[pf.letra] + pf.num;
                    processG(cmd);
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
            }
            else
            {

                if (code == 8)
                {
                    pf.num++;
                    if (pf.num > 8) pf.num = 1;
                    arguments = auxArgument + " " + coordL[pf.letra] + " " + coordN[pf.num];
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 2)
                {
                    pf.num--;
                    if (pf.num < 1) pf.num = 8;
                    arguments = auxArgument + " " + coordL[pf.letra] + " " + coordN[pf.num];
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 4)
                {
                    pf.letra--;
                    if (pf.letra < 1) pf.letra = 8;
                    arguments = auxArgument + " " + coordL[pf.letra] + " " + coordN[pf.num];
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 6)
                {
                    pf.letra++;
                    if (pf.letra > 8) pf.letra = 1;
                    arguments = auxArgument + " " + coordL[pf.letra] + " " + coordN[pf.num];
                    p.StartInfo.Arguments = arguments;
                    p.Kill();
                    p.Start();

                }
                else if (code == 5)
                {
                    flag = !flag;

                    cmd = "3-" + letras[pi.letra] + pi.num + "-" + letras[pf.letra] + pf.num;
                    processG(cmd);
                    //Console.WriteLine(cmd);


                }

            }



        }
        // ################# Process gesture #############################
        private static void processG(string cmd)
        {

            IEnumerable<Move> validMoves;
            string move, posf, posi;
            Move pis;
            bool valid;
            string[] aux, aux1;
            List<string> posfL = new List<string>();
            List<string> posfN = new List<string>();
            Tts fala = new Tts();

            aux = cmd.Split('-');

            posi = aux[1];
            posf = aux[2];
            validMoves = game.GetValidMoves(game.WhoseTurn);



            if (aux[0] == "3")//jogada simples
            {

                //jogar
                pis = new Move(posi, posf, game.WhoseTurn);
                valid = game.IsValidMove(pis);
                MoveType type = game.ApplyMove(pis, valid);
                if (valid)
                {
                    Console.WriteLine("bestmove {0}{1}", posi, posf);
                    go = false;
                    p.Kill();

                }
                else
                {
                    fala.Speak("Jogada Inválida");
                    // Console.WriteLine("Jogada inválida");
                }

            }
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
                else
                {
                    fala.Speak("Jogada Inválida.");
                }
                



            }
            else if (aux[0] == "6") // Ajuda
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

                // Console.WriteLine("{0},{1}->{2},{3}", pf.letra, pf.num, pi.letra, pi.num);



            }

        } // end of process

    }
    //######### Speech Class #############
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

        /*
         * SpeechRecognized
         * 
         * EventHandler
         * 
         * 
        */

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            

            //gets recognized text.
            string s = e.Result.Text;

            Program x = new Program();
          
            x.translateSP(s,e.Result.Confidence);

            sr.RecognizeAsyncStop();


        }


    }


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

        /*
         * tts_SpeakCompleted
         */

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

}
