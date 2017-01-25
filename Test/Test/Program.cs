using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;


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
        public static KinectSensor myKinect;

        static void Main(string[] args)
        {
            Gestures gest = new Gestures();
            new SpeechRecognizer("grammar.grxml");

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

            gest.recog += new GestureRecognized(gest_recog);
            //v.spev += new SpeechEvent(v_speech);
            while (true) ;
        }
        static void gest_recog(object sender, GestureEventArgs e)
        {
            Console.WriteLine(e.gesture);
        }
        /*static void v_speech(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result);
        }*/
    }
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

            Console.WriteLine(s);

            //sr.RecognizeAsyncStop();


        }


    }
}
