using System;
using Microsoft.Research.Kinect.Nui;
using KinectLibrary.Movement.EventsArgs;

namespace KinectLibrary.Movement
{
    
    public class KinectMouse : 
    {
        private readonly MovementTracker _tracker;
        private Texture2D _mouseIcon;
        public static Vector2 MouseCoordinates { get; set; }
        private SpriteBatch _spriteBatch;
        private bool _isEnable = false;

        public KinectMouse(Game game, MovementTracker tracker, JointID mouseJoint) : base(game)
        {
            _tracker = tracker;

            _tracker.AddMovementHandler(MovementType.Any, 0f, HandleMouseEvent, mouseJoint);
        }

        protected override void LoadContent()
        {
            _mouseIcon = Game.Content.Load<Texture2D>("mouse");
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        private void HandleMouseEvent(object state, MovementHandlerEventArgs args)
        {
           MouseCoordinates = new Vector2((args.KinectCoordinates.X * 1000) + 400, (args.KinectCoordinates.Y * -1000) + 320);

        }

        public override void Draw(GameTime gameTime)
        {
            if(!_isEnable)
                return;
            
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            _spriteBatch.Draw(_mouseIcon, MouseCoordinates, Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public void Enable()
        {
            _isEnable = true;
        }

        public void Disable()
        {
            _isEnable = false;
        }


        public bool IsEnable()
        {
            return _isEnable;
        }
    }

    public class KinectMouseSelection : DrawableGameComponent
    {
        private const int NumberOfImagesToCompleteSelection = 8;
        private static Texture2D[] _textures = new Texture2D[NumberOfImagesToCompleteSelection];
        private SpriteBatch _spriteBatch;
        
        private int _textureIndex = -1;
        private int _tokenBase = 1;

        private int _currentToken = 0;

        private bool _feeded = false;

        private TimeSpan? _nextStateChange;

        public static KinectMouseSelection Instance { get; private set; }
        public KinectMouseSelection(Game game) : base(game)
        {
            Instance = this;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            for (int i = 1; i <= _textures.Length; i++)
            {
                _textures[i - 1] = Game.Content.Load<Texture2D>(String.Format("mouseselected{0}", i));
            }

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!_feeded)
            {
                _textureIndex = -1;
                _currentToken = 0;
                return;
            }

            if (_nextStateChange == null)
            {
                _nextStateChange = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, 250);
            }

            if (gameTime.TotalGameTime > _nextStateChange)
            {
                _textureIndex++;
                if (_textureIndex == _textures.Length - 1)
                    _nextStateChange = TimeSpan.MaxValue;
                else
                    _nextStateChange = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, 250);

            }



            _feeded = false;

        }

        public bool IsSelected()
        {
            return _nextStateChange == TimeSpan.MaxValue && _textureIndex == _textures.Length - 1;
        }

        public int GetMouseToken()
        {
            return _tokenBase++;
        }

        public void FeedSelection(int token)
        {
            if (_currentToken != token)
            {
                _textureIndex = 0;
                _currentToken = token;
                _nextStateChange = null;
            }

            _feeded = true;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _spriteBatch.Begin();

            if (_textureIndex != -1)
            {
                _spriteBatch.Draw(_textures[_textureIndex],KinectMouse.MouseCoordinates,Color.White);
            }

            _spriteBatch.End();
        }
    }
}