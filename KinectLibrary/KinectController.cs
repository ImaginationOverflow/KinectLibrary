using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace KinectLibrary
{


    public delegate void KinectConnectedHandler(object sender, Runtime kinect);
    public delegate void KinectDisconnectedHandler(object sender, Runtime kinect);

    public class KinectController
    {
        public static readonly KinectController Instance = new KinectController();

        private event KinectConnectedHandler _onKinectConnect;
        private event KinectDisconnectedHandler _onKinectDisconnect;

        private bool _initialized;
        private readonly IList<Runtime> _connectedKinects = new List<Runtime>();

        public IList<Runtime> ConnectedKinects { get { return _connectedKinects; } }


        public event KinectConnectedHandler OnKinectConnect
        {
            add
            {
                ThrowIfNotInitialzed();
                _onKinectConnect += value;
            }
            remove
            {
                ThrowIfNotInitialzed();
                _onKinectConnect -= value;
            }
        }


        public event KinectDisconnectedHandler OnKinectDisconnect
        {
            add
            {
                ThrowIfNotInitialzed();
                _onKinectDisconnect += value;
            }
            remove
            {
                ThrowIfNotInitialzed();
                _onKinectDisconnect -= value;
            }
        }

        
        private KinectController() { }

        public void Initialize()
        {

            Runtime.Kinects.StatusChanged += OnKinectStatusChanged;
            foreach (var kinect in Runtime.Kinects)
            {
                if (kinect.Status == KinectStatus.Connected)
                    _connectedKinects.Add(kinect);
            }
            _initialized = true;
        }

        private void OnKinectStatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == KinectStatus.Connected)
            {
                _connectedKinects.Add(e.KinectRuntime);
                _onKinectConnect(this, e.KinectRuntime);
            }

            if (_connectedKinects.Contains(e.KinectRuntime))
            {
                _connectedKinects.Remove(e.KinectRuntime);
                _onKinectDisconnect(this, e.KinectRuntime);
            }
        }


        public void Shutdown()
        {
            Runtime.Kinects.StatusChanged -= OnKinectStatusChanged;
            foreach (var connectedKinect in _connectedKinects)
            {
                _onKinectDisconnect(this, connectedKinect);
            }
            _initialized = false;
        }


        private void ThrowIfNotInitialzed()
        {
            if (!_initialized)
                throw new KinectControllerNotInitializedException();
        }
    }


}
