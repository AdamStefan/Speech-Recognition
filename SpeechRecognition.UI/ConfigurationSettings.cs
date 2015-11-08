using System;
using System.Collections.Generic;
using System.Configuration;

namespace SpeechRecognition.UI
{
    public static class ConfigurationSettings
    {
        public static IEnumerable<string> LearningsFolders
        {
            get
            {
                return ConfigurationManager.AppSettings["LearningFolders"].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string RecordingFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["RecordingFolder"];
            }
        }
    }
}
