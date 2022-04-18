using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CallTrainer
{
    public static class CSD
    {
        #if DEBUG
        public static readonly string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"CallTrainer");
        #else
        public static readonly string rootFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        #endif

        public const string HighscoreFolderName = "Highscores";
        public const string CallsFolderName = "Calls";
        public const string CallPoolFileName = "CallPool.tsv";
        public const string HighscoreKey = "afdskhgfafdskhgf";
        public const string PercentFormat = "000.##%";
    }
}
