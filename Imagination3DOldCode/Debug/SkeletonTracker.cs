using System;
using System.Collections.Generic;
using KinectLibrary.Movement;
using KinectLibrary.Movement.EventsArgs;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using UI.Geometry;
using UI.Kinect.Movement;
using UI.Kinect.Movement.EventsArgs;

namespace UI.Kinect.Debug
{
    /*
    public class SkeletonTracker : DrawableGameComponent
    {
        private readonly MovementTracker _tracker;

        private IList<Joint> _joinsToDraw = new List<Joint>();
        private SpherePrimitive _jointGeometry;
            
        public SkeletonTracker(Imaginect3D game, MovementTracker tracker)
            : base(game)
        {
            _tracker = tracker;

            _tracker.OnSkeletonDataReceived += OnSkeletonData;
            _jointGeometry = new SpherePrimitive(Game.GraphicsDevice);
        }

        private void OnSkeletonData(object state, SkeletonDataReadyEventArgs args)
        {
            SkeletonData data = args.SkeletonData;

            _joinsToDraw.Clear();

            foreach (Joint joint in data.Joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    _joinsToDraw.Add(joint);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Imaginect3D game = (Game as Imaginect3D);

            foreach (var joint in _joinsToDraw)
            {
                if (game != null)
                    _jointGeometry.Draw(Matrix.Multiply(Matrix.CreateScale(0.1f) ,Matrix.CreateTranslation(joint.Position.X, joint.Position.Y, joint.Position.Z)), game.View, game.Projection, Color.White);
            }
            
            base.Draw(gameTime);
        }
    }*/
}