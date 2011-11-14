using System;
using System.Diagnostics;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using KinectLibrary.Movement.EventsArgs;

namespace KinectLibrary.Movement.Gestures
{
    public class RotateGesture : GameComponent, IGesture
    {
        private readonly MovementTracker _tracker;
        private bool _startYRightRotation, _startYLeftRotation, _startXTopRotation, _startXBottomRotation, _startZRightRotation, _startZLeftRotation;
        private Matrix _matrix;

        public RotateGesture(Game game, MovementTracker tracker) : base(game)
        {
            _tracker = tracker;
            _matrix = Matrix.Identity;
        }

        public void Register()
        {
            _tracker.AddMovementHandler(MovementType.Any, 0.0f, OnRotateGesture, JointID.HandRight, JointID.HandLeft);
        }

        public void Unregister()
        {
            _tracker.RemoveMoventHandler(MovementType.Any,OnRotateGesture);
        }

        public Matrix GetTransformedMatrix()
        {
            return GetRotationMatrix();
        }

        private void OnRotateGesture(object state, MovementHandlerEventArgs args)
        {

            const float DistanceBetweenHands = 0.1f;
            //
            if (!(args.Skeleton.Joints[JointID.HandLeft].Position.Y > args.Skeleton.Joints[JointID.HipLeft].Position.Y &&
                    args.Skeleton.Joints[JointID.HandRight].Position.Y > args.Skeleton.Joints[JointID.HipRight].Position.Y))
            {
                _startXBottomRotation =
                    _startXTopRotation =
                    _startYLeftRotation = _startYRightRotation = _startZLeftRotation = _startZRightRotation = false;
                return;
            }
            //*/

            if (args.Joint == JointID.HandRight)
            {
                _startYRightRotation = //args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.ShoulderRight].Position.Z - 0.3 &&
                                       args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.HandLeft].Position.Z - DistanceBetweenHands;
                _startYLeftRotation = false;
            }
            else
            {
                _startYLeftRotation = //args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.ShoulderLeft].Position.Z - 0.3 &&
                                      args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.HandRight].Position.Z - DistanceBetweenHands;
                _startYRightRotation = false;
            }

            if (args.Joint == JointID.HandRight)
            {
                
                _startZRightRotation = args.KinectCoordinates.Y >= args.Skeleton.Joints[JointID.WristRight].Position.Y &&
                                      args.KinectCoordinates.Y > args.Skeleton.Joints[JointID.HandLeft].Position.Y + DistanceBetweenHands;
                _startZLeftRotation = false;
            }
            else
            {
                _startZLeftRotation = args.KinectCoordinates.Y >= args.Skeleton.Joints[JointID.WristLeft].Position.Y &&
                                       args.KinectCoordinates.Y > args.Skeleton.Joints[JointID.HandRight].Position.Y + DistanceBetweenHands;

                _startZRightRotation = false;
            }

        }

        public Matrix GetRotationMatrix()
        {
            return _matrix;
        }

        public override void  Update(GameTime gameTime)
        {
            float yRotation = 0f;

            if (_startYRightRotation)
                yRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;
            else if (_startYLeftRotation)
                yRotation -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;

            _matrix = Matrix.Multiply(_matrix, Matrix.CreateRotationY(yRotation));

            float xRotation = 0f;
            if (_startXTopRotation)
                xRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;
            else if (_startXBottomRotation)
                xRotation -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;

            _matrix = Matrix.Multiply(_matrix, Matrix.CreateRotationX(xRotation));

            float zRotation = 0f;

            if (_startZRightRotation)
                zRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;
            else if (_startZLeftRotation)
                zRotation -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;

            _matrix = Matrix.Multiply(_matrix, Matrix.CreateRotationZ(zRotation));

            base.Update(gameTime);
        }
    }
}