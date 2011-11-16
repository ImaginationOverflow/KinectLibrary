using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KinectLibrary.Movement.Gestures
{
    public interface IGesture
    {
        void Register();
        void Unregister();
        Matrix GetTransformedMatrix();
    }
}
