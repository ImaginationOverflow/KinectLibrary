using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace KinectLibrary.Movement.EventsArgs
{
    public class MovementHandlerEventArgs : EventArgs
    {
        public MovementType[] Movements { get; internal set; }
        public float[] Differences { get; internal set; }
        public Vector KinectCoordinates { get; internal set; }
        public JointID Joint { get; internal set; }
        public SkeletonData Skeleton { get; internal set; }
    }
}
