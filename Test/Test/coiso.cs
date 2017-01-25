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
    class coiso
    {
        private SpeechRecognitionEngine sr;
        public event SpeechEvent spev;
        public string s;
        public coiso(string GName)
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

        public void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {


            //gets recognized text.
            s = e.Result.Text;

            spev(this, e);
            sr.RecognizeAsyncStop();


        }


    }


    public delegate void SpeechEvent(object sender, SpeechRecognizedEventArgs e);



}
