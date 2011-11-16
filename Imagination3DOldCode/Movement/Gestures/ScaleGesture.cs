using System;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using KinectLibrary.Movement.EventsArgs;

namespace KinectLibrary.Movement.Gestures
{
    public class ScaleGesture : GameComponent, IGesture
    {
        private readonly MovementTracker _tracker;
        private bool _fullScale;
        private bool _lateralScale;
        private bool _topScale;
        private Matrix _matrix = Matrix.Identity;
        private float _distanceBetweenHands;

        public ScaleGesture(Game game, MovementTracker tracker) : base(game)
        {
            _tracker = tracker;
        }

        public void Register()
        {
            _tracker.AddMovementHandler(MovementType.Any, 0.001f, OnScale, JointID.HandLeft, JointID.HandRight);
        }

        public void Unregister()
        {
            _tracker.RemoveMoventHandler(MovementType.Any,OnScale);
        }

        public Matrix GetTransformedMatrix()
        {
            return GetScaleMatrix();
        }

        private void OnScale(object state, MovementHandlerEventArgs args)
        {
            bool handsAbove = false, handsSided = false;
            if ((args.Joint == JointID.HandRight && args.KinectCoordinates.Y > args.Skeleton.Joints[JointID.HandLeft].Position.Y + 0.1) ||
                (args.Joint == JointID.HandRight && args.KinectCoordinates.Y < args.Skeleton.Joints[JointID.HandLeft].Position.Y - 0.1) ||
                (args.Joint == JointID.HandLeft && args.KinectCoordinates.Y > args.Skeleton.Joints[JointID.HandRight].Position.Y - 0.1) ||
                (args.Joint == JointID.HandLeft && args.KinectCoordinates.Y < args.Skeleton.Joints[JointID.HandRight].Position.Y + 0.1))
            {
                handsAbove = true;
            }

            if ((args.Joint == JointID.HandRight && args.KinectCoordinates.X > args.Skeleton.Joints[JointID.HandLeft].Position.X + 0.1) ||
                (args.Joint == JointID.HandLeft && args.KinectCoordinates.X < args.Skeleton.Joints[JointID.HandRight].Position.X - 0.3))
            {
                handsSided = true;
            }

            if (handsSided && handsAbove)
            {
                _fullScale = true;
                _lateralScale = false;
                _topScale = false;
            }
            else if (handsSided)
            {
                _lateralScale = true;
                _fullScale = false;
                _topScale = false;
            }
            else if (handsAbove)
            {
                _topScale = true;
                _fullScale = false;
                _lateralScale = false;
            }

            if (_fullScale)
                _distanceBetweenHands =
                    (float) Math.Sqrt(
                        Math.Pow(args.Skeleton.Joints[JointID.HandLeft].Position.X - args.Skeleton.Joints[JointID.HandRight].Position.X, 2) +
                        Math.Pow(args.Skeleton.Joints[JointID.HandLeft].Position.Y - args.Skeleton.Joints[JointID.HandRight].Position.Y, 2));
            else if (_lateralScale)
                _distanceBetweenHands = Math.Abs(args.Skeleton.Joints[JointID.HandRight].Position.X - args.Skeleton.Joints[JointID.HandLeft].Position.X);
            else if (_topScale)
                _distanceBetweenHands = Math.Abs(args.Skeleton.Joints[JointID.HandRight].Position.Y - args.Skeleton.Joints[JointID.HandLeft].Position.Y);
        }

        public Matrix GetScaleMatrix()
        {
            return _matrix;
        }

        private TimeSpan _timeElapsed;
        public override void Update(GameTime gameTime)
        {
            float scale;

            if (_timeElapsed.TotalMilliseconds > 10 && _distanceBetweenHands >= 0.6f)
            {
                scale = 1.01f;
                _timeElapsed = TimeSpan.Zero;
            }
            else if (_timeElapsed.TotalMilliseconds > 10 && _distanceBetweenHands <= 0.3f)
            {
                scale = 0.99f;
                _timeElapsed = TimeSpan.Zero;
            }
            else
            {
                scale = 1;
                _timeElapsed += gameTime.ElapsedGameTime;
            }

            
            if (_fullScale)
            {
                _matrix = Matrix.Multiply(_matrix, Matrix.CreateScale(scale));
            }
            else if (_lateralScale)
            {
                _matrix = Matrix.Multiply(_matrix, Matrix.CreateScale(scale, 1, 1));
            }
            else if(_topScale)
            {
                _matrix = Matrix.Multiply(_matrix, Matrix.CreateScale(1, scale, 1));
            }
            
            base.Update(gameTime);
        }
    }
}