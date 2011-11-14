using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace KinectLibrary.Audio
{

    public class SpeechRecognition
    {

        private  SpeechRecognitionEngine _sre;
        private  KinectAudioSource _kinectSource;
        private readonly Dictionary<String, CommandSpeechRecognition> _commands;
        private Stream _stream;

        public SpeechRecognition()
        {
            
            _commands = new Dictionary<string, CommandSpeechRecognition>();
        }


        public void AddCommand(params CommandSpeechRecognition [] commands)
        {
            foreach (CommandSpeechRecognition c in commands)
            {
                if (!_commands.ContainsKey(c.Name))
                {
                    _commands.Add(c.Name, c);
                }
            }
           
        }

        public void InicializeSpeechRecognize()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            if (ri == null)
            {
                throw new RecognizerNotFoundException();
            }

            try
            {
                    _sre = new SpeechRecognitionEngine(ri.Id);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

           var choises = new Choices();
            foreach(CommandSpeechRecognition cmd in _commands.Values)
            {
                choises.Add(cmd.Choise);
            }

            var gb = new GrammarBuilder {Culture = ri.Culture};
            gb.Append(choises);
            var g = new Grammar(gb);

            _sre.LoadGrammar(g);
            _sre.SpeechRecognized += SreSpeechRecognized;
            _sre.SpeechHypothesized += SreSpeechHypothesized;
            _sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;   
        }

        public void Start()
        {
            try
            {
                _kinectSource = new KinectAudioSource
                                    {
                                        SystemMode = SystemMode.OptibeamArrayOnly,
                                        FeatureMode = true,
                                        AutomaticGainControl = false,
                                        MicArrayMode = MicArrayMode.MicArrayExternBeam,
                                        MicArrayBeamAngle = 0.5
                                    };
                _stream = new StreamFilter(_kinectSource.Start());
                _sre.SetInputToAudioStream(_stream, new SpeechAudioFormatInfo(
                                                      EncodingFormat.Pcm, 16000, 16, 1,
                                                      32000, 2, null));
                _sre.RecognizeAsync(RecognizeMode.Multiple);

            }
            catch(Exception e)
            {
               Console.WriteLine(e.Message);
                throw e;
            }
        }

        public void UpdateMicBeamAngle(double angle)
        {
     
            _kinectSource.MicArrayBeamAngle = angle;

        }

        public void Stop()
        {
            _sre.RecognizeAsyncStop();  
            _stream.Close();
            
        }

        private  void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {

            Console.WriteLine("Rejected: {0}", e.Result.Text);
        }

        private  void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Position: {0}", _kinectSource.SoundSourcePositionConfidence);
            string result = e.Result.Text;
            
                if (_commands.ContainsKey(result))
                    _commands[result].Command.Invoke();
            
    
        }

        private  void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.WriteLine("Hypothesized: {0}", e.Result.Text);

        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }
    }


}
