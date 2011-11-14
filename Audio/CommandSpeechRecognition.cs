using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;


namespace KinectLibrary.Audio
{
    public class CommandSpeechRecognition
    {
        public String Name { get; internal set; }
        public Choices Choise { get; internal set; }
        public Action Command { get; internal set; }

        public CommandSpeechRecognition(string command, Action action)
        {
            Name = command;
            Command = action;
            Choise = new Choices(new[]{command});
        }
    }
}
