using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using KinectLibrary.Movement.EventsArgs;
using KinectLibrary.Movement.Extensions;

namespace KinectLibrary.Movement
{
    public delegate void MovementEventHandler(object state, MovementHandlerEventArgs args);

    public delegate void SkeletonOnViewHandler(object state, SkeletonOnViewEventArgs args);

    public delegate void SkeletonDataReadyHandler(object state, SkeletonDataReadyEventArgs args);

    public enum MovementType
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward,
        Any
    }

    class JointNMovementHandlerAggregator
    {
        public MovementEventHandler Handler { get; set; }
        public JointID[] Joints { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (!(obj is JointNMovementHandlerAggregator)) return false;

            var aux = obj as JointNMovementHandlerAggregator;

            return aux.Handler.Equals(this.Handler);
        }

    }

    public class MovementTracker
    {

        #region Status

        private bool _skeletonPresent = false;



        private readonly IDictionary<JointID, Vector> _previousStates = new Dictionary<JointID, Vector>();

        #endregion

        private IDictionary<MovementType, SortedList<float, JointNMovementHandlerAggregator>> _movementHandlers =
            new Dictionary<MovementType, SortedList<float, JointNMovementHandlerAggregator>> {
                { MovementType.Up, new SortedList<float,   JointNMovementHandlerAggregator>() },
                { MovementType.Down, new SortedList<float, JointNMovementHandlerAggregator>() },
                { MovementType.Left, new SortedList<float, JointNMovementHandlerAggregator>() },
                { MovementType.Right, new SortedList<float,JointNMovementHandlerAggregator>() },
                { MovementType.Backward, new SortedList<float,  JointNMovementHandlerAggregator>() },
                { MovementType.Forward, new SortedList<float,  JointNMovementHandlerAggregator>() },
                { MovementType.Any, new SortedList<float, JointNMovementHandlerAggregator>() }
            };


        public event SkeletonOnViewHandler OnSkeletonOnViewChange;

        public event SkeletonDataReadyHandler OnSkeletonDataReceived;

        private readonly Runtime _kinectRuntime;


        public MovementTracker(Runtime kinectRuntime)
        {
            _kinectRuntime = kinectRuntime;
            kinectRuntime.SkeletonFrameReady += OnSkeletonFrameReady;

            kinectRuntime.SkeletonEngine.TransformSmooth = true;
            TransformSmoothParameters parameters = new TransformSmoothParameters();
            parameters.Smoothing = 0.7f;
            parameters.Correction = 0.3f;
            parameters.Prediction = 0.4f;
            parameters.JitterRadius = 1.0f;
            parameters.MaxDeviationRadius = 0.5f;
            kinectRuntime.SkeletonEngine.SmoothParameters = parameters;
        }

        private int _previousSelectedSkeleton = -1;
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            SkeletonData skeletonData = skeletonFrame.Skeletons.Where(sk => sk.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

            if (skeletonData == null) return;

            //
            if(_previousSelectedSkeleton == -1)
            {
                skeletonData = skeletonFrame.Skeletons.Where(sk => sk.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                _previousSelectedSkeleton = skeletonData.UserIndex;
            }
            else
            {
                skeletonData = skeletonFrame.Skeletons.Where(sk => sk.TrackingState == SkeletonTrackingState.Tracked && sk.UserIndex == _previousSelectedSkeleton).FirstOrDefault();
                if (skeletonData == null)
                {
                    _previousSelectedSkeleton = -1;
                    OnSkeletonFrameReady(sender,e);
                }
            }
            //*/
            if (skeletonData.TrackingState == SkeletonTrackingState.Tracked)
            {
                if(OnSkeletonDataReceived != null)
                    OnSkeletonDataReceived.Invoke(this, new SkeletonDataReadyEventArgs { SkeletonData = skeletonData });

                if (_skeletonPresent == false)
                {
                    _skeletonPresent = true;
                    if (OnSkeletonOnViewChange != null)
                        OnSkeletonOnViewChange.Invoke(this, new SkeletonOnViewEventArgs() { State = SkeletonOnViewType.Entered });
                }

                var watchedJoints = _previousStates.Keys;
                var jointsData = watchedJoints.Select(joint => skeletonData.Joints[joint])
                                              .Where(jointData => jointData.TrackingState == JointTrackingState.Tracked).ToArray();

                foreach (var joint in jointsData)
                {
                    var jointVector = _previousStates[joint.ID];
                    var jointMovementsInfo = new Dictionary<MovementType, float>(6);

                    //
                    //  Get the events.
                    //
                    ExtractMovementsFromJoint(joint, jointVector, jointMovementsInfo);

                    //
                    //  Call the handlers, from the events detected.
                    //
                    foreach (var jointInfo in jointMovementsInfo)
                    {
                        //
                        // _movementHandlers[MovementType]
                        //
                        var eventList = _movementHandlers[jointInfo.Key];

                        foreach (var handlerAggregator in eventList.Where(l => l.Value.Joints.Contains(joint.ID)))
                        {
                            //
                            // if (difference >= threshold) 
                            //
                            if (jointInfo.Value >= handlerAggregator.Key)
                                handlerAggregator.Value.Handler(this, new MovementHandlerEventArgs
                                                                          {
                                                                              KinectCoordinates = joint.Position,
                                                                              Differences = new[] { jointInfo.Value },
                                                                              Movements = new[] { jointInfo.Key },
                                                                              Joint = joint.ID,
                                                                              Skeleton = skeletonData
                                                                          });
                        }

                    }






                    //
                    //  Select movement handlers to any movement type where the id have the current joint.
                    //
                    var events = _movementHandlers[MovementType.Any].Where(l => l.Value.Joints.Contains(joint.ID));
                    foreach (var anyEvent in events)
                    {

                        var args = GetAnyEventArgs(anyEvent, joint.Position, jointMovementsInfo);
                        args.Joint = joint.ID;
                        args.Skeleton = skeletonData;
                        anyEvent.Value.Handler(this, args);
                    }

                    //
                    //  Set the new state on the previous state.
                    //
                    _previousStates[joint.ID] = joint.Position;
                }


            }
            else
            {
                if (_skeletonPresent == true)
                {
                    _skeletonPresent = false;
                    OnSkeletonOnViewChange.Invoke(this, new SkeletonOnViewEventArgs() { State = SkeletonOnViewType.Exited });
                }
            }
        }

        private MovementHandlerEventArgs GetAnyEventArgs(
            KeyValuePair<float, JointNMovementHandlerAggregator> anyEvent,
            Vector position,
            Dictionary<MovementType, float> info
            )
        {
            IEnumerable<MovementType> movements = info.Keys.ToArray();
            IEnumerable<float> diffs = info.Values.ToArray();
            float threshold = anyEvent.Key;
            return new MovementHandlerEventArgs
                       {
                           Differences = diffs.Where(t => t >= threshold).ToArray(),
                           Movements = movements.Where(t => info[t] >= threshold).ToArray(),
                           KinectCoordinates = position
                       };


        }

        private static void ExtractMovementsFromJoint(Joint joint, Vector jointVector, Dictionary<MovementType, float> jointMovementsInfo)
        {
            if (joint.HasMovedUp(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Up, joint.MovedUpDifference(jointVector));
            }
            else if (joint.HasMovedDown(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Down, joint.MovedDownDifference(jointVector));
            }

            if (joint.HasMovedRight(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Right, joint.MovedRightDifference(jointVector));
            }
            else if (joint.HasMovedLeft(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Left, joint.MovedLeftDifference(jointVector));
            }

            if (joint.HasMovedForward(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Forward, joint.MovedForwardDifference(jointVector));
            }
            else if (joint.HasMovedBack(jointVector))
            {
                jointMovementsInfo.Add(MovementType.Backward, joint.MovedBackDifference(jointVector));
            }
        }


        public void AddMovementHandler(MovementType type, float threshold, MovementEventHandler handler, params JointID[] joints)
        {


            _movementHandlers[type].Add(threshold, new JointNMovementHandlerAggregator
                {
                    Handler = handler,
                    Joints = joints
                }
            );



            foreach (JointID j in joints)
            {
                if (!_previousStates.ContainsKey(j))
                {
                    _previousStates.Add(j, default(Vector));
                }
            }


        }

        public void RemoveMoventHandler(MovementType type, MovementEventHandler handler)
        {
            var list = _movementHandlers[type];
            list.RemoveAt(list.IndexOfValue(new JointNMovementHandlerAggregator { Handler = handler }));
        }


    }
}