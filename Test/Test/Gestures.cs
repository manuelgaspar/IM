using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;


using Microsoft.Speech.Recognition;
using Microsoft.Speech.Recognition.SrgsGrammar;

using System.Media;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.AudioFormat;

using Microsoft.Kinect;

namespace Test
{

    class Gestures
    {
        public static int state = 0; // state machine
        public static int frame = 0; // frame number

        public event GestureRecognized recog;

        //##################################   Kinect  Event ############################################## 
        public void MyKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            int code = 0;
            int maxframes = 30;
            string message = " ";

            GestureEventArgs ev = new GestureEventArgs();
            
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
                            ev.gesture = "Make a move";
                            recog(this, ev);
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
                            ev.gesture = "SwipeLeft";
                            recog(this, ev);
                            //Console.WriteLine(message);
                            code = 4;
                        }
                        else if (SwipeRight(skeleton))
                        {
                            state = 0;
                            ev.gesture = "SwipeRight";
                            recog(this, ev);
                            code = 6;
                        }
                        else if (Select(skeleton))
                        {
                            state = 0;
                            ev.gesture = "Selected";
                            recog(this, ev);
                            code = 5;
                        }
                        else if (SwipeUp(skeleton))
                        {
                            state = 0;
                            ev.gesture = "SwipeUp";
                            recog(this, ev);
                            code = 8;
                        }
                        else if (SwipeDown(skeleton))
                        {
                            state = 0;
                            ev.gesture = "SwipeDown";
                            recog(this, ev);
                            code = 2;
                        }
                        else if (Clap(skeleton))
                        {
                            state = 0;
                            ev.gesture = "Clap";
                            recog(this, ev);
                            code = 1;
                        }
                        else if (Anular(skeleton))
                        {
                            state = 0;
                            ev.gesture = "Cancelar";
                            recog(this, ev);
                            code = 3;
                        }
                        

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
    }

    public delegate void GestureRecognized(object sender, GestureEventArgs e);

    public class GestureEventArgs : EventArgs
    {
        public string gesture;
    }


}
