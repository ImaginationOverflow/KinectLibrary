using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace KinectLibrary.Movement.Extensions
{
    public static class JointExtensions
    {
        public static bool HasMovedDown(this Joint instance, Vector previousState){return instance.Position.Y < previousState.Y;}

        public static bool HasMovedUp(this Joint instance, Vector previousState){return instance.Position.Y > previousState.Y;}

        public static bool HasMovedRight(this Joint instance, Vector previousState){return instance.Position.X > previousState.X;}

        public static bool HasMovedLeft(this Joint instance, Vector previousState){return instance.Position.X < previousState.X;}

        public static bool HasMovedForward(this Joint instance, Vector previousState){return instance.Position.Z < previousState.Z;}

        public static bool HasMovedBack(this Joint instance, Vector previousState){return instance.Position.Z > previousState.Z;}



        public static float MovedDownDifference(this Joint instance, Vector previousState) { return previousState.Y - instance.Position.Y; }

        public static float MovedUpDifference(this Joint instance, Vector previousState) { return instance.Position.Y - previousState.Y; }

        public static float MovedRightDifference(this Joint instance, Vector previousState) { return instance.Position.X - previousState.X; }

        public static float MovedLeftDifference(this Joint instance, Vector previousState) { return previousState.X - instance.Position.X; }

        public static float MovedForwardDifference(this Joint instance, Vector previousState) { return previousState.Z - instance.Position.X; }

        public static float MovedBackDifference(this Joint instance, Vector previousState) { return instance.Position.Z - previousState.Z; }



    }
}
