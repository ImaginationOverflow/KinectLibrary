using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KinectLibrary.Movement.EventsArgs;

namespace KinectLibrary.Movement.Gestures
{
    public class TranslationGesture : GameComponent, IGesture 
    {
        private readonly JointID _cursor;
        private readonly MovementTracker _tracker;
        private Matrix _matrix = Matrix.Identity;
        private Vector _vector;

        public TranslationGesture(Game game, MovementTracker tracker, JointID cursor = JointID.HandRight)
            : base(game)
        {
            _cursor = cursor;
            _tracker = tracker;
        }


        public void Register()
        {
            _tracker.AddMovementHandler(MovementType.Any, 0.1f, OnTranslationGesture, _cursor);
        }

        public void Unregister()
        {
            _tracker.RemoveMoventHandler(MovementType.Any,OnTranslationGesture);
        }

        public Matrix GetTransformedMatrix()
        {
            return GetTranslationMatrix();
        }

        private void OnTranslationGesture(object state, MovementHandlerEventArgs args)
        {
            _vector = args.KinectCoordinates;
            /*/
                Game.GraphicsDevice.Viewport.Project(ConvertRealWorldPoint(args.KinectCoordinates),
                                                     Matrix.CreatePerspectiveFieldOfView(1,Game.GraphicsDevice.Viewport.AspectRatio, 1, 10),
                                                     Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up),
                                                     Matrix.Identity); //*/
        }

        public Matrix GetTranslationMatrix()
        {
            return _matrix;
        }

        public override void Update(GameTime gameTime)
        {
            _matrix = Matrix.CreateTranslation(ConvertRealWorldPoint(_vector));
            //_matrix = Matrix.CreateTranslation(_vector);
        }

        private static Vector3 ConvertRealWorldPoint(Vector position)
        {
            var returnVector = new Vector3();
            returnVector.X = position.X * 10;
            returnVector.Y = position.Y * 10;
            returnVector.Z = position.Z;
            return returnVector;
        }
    }
}
