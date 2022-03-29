using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace CallTrainer.Services
{
    public class Highscore
    {
        public readonly string name;
        public const int tracking = 10;

        private string path;
        private Data data;

        
        public class Data
        {
            public Dictionary<string,int> failes = new Dictionary<string, int>();
            public Dictionary<string,int> correct = new Dictionary<string, int>();
            public List<int> scores = new List<int>();
            public Dictionary<string,Queue<bool>> last = new Dictionary<string, Queue<bool>>();
        }

        public Highscore(string name)
        {
            this.name = name;
            path = Path.Combine(CSD.rootFolder, CSD.HighscoreFolderName);

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, name);
            if(!File.Exists(path))
            {
                data = new Data();
            }
            else
            {
                #if DEBUG
                data = JsonConvert.DeserializeObject<Data>(DecryptString(CSD.HighscoreKey, File.ReadAllText(path)));
                #else
                try
                {
                    data = JsonConvert.DeserializeObject<Data>(DecryptString(CSD.HighscoreKey, File.ReadAllText(path)));
                }
                catch
                {
                    data = new Data();
                }
                    #endif
            }
            
        }

        public void Save()
        {
            #if DEBUG
            File.WriteAllText(path, EncryptString(CSD.HighscoreKey, JsonConvert.SerializeObject(data)));
            #else
            try
            {
                File.WriteAllText(path, EncryptString(CSD.HighscoreKey, JsonConvert.SerializeObject(data)));
            }catch{}
            #endif
        }

        public void New_Score(int score)
        {
            data.scores.Add(score);
        }

        public void New_Call(string name, bool correct)
        {
            Check_Name(name);

            if (correct)
                data.correct[name]++;
            else
                data.failes[name]++;

            data.last[name].Enqueue(correct);
            while (data.last[name].Count > tracking)
                _ = data.last[name].Dequeue();
        }

        public Tuple<int,int> Call_Correct_Failes_Ammount()
        {
            int c = 0, f = 0;
            foreach(var correct in data.correct)
                c += correct.Value;
            foreach(var failse in data.failes)
                f += failse.Value;

            return new Tuple<int, int>(c,f);


        }

        public Tuple<int,int> Call_Correct_Failes_Ammount(string name)
        {
            Check_Name(name);
            return new Tuple<int,int>(data.correct[name], data.failes[name]);
        }
        
        public Tuple<int, int> Call_Last_Correct_Failes_Ammount()
        {
            int c = 0, f = 0;
            foreach(var call in data.last)
                foreach (bool last_correct in call.Value)
                {
                    if (last_correct)
                        c++;
                    else
                        f++;
                }
            return new Tuple<int, int>(c,f);
        }
       
        public Tuple<int, int> Call_Last_Correct_Failes_Ammount(string name)
        {
            Check_Name(name);
            int c = 0, f = 0;
            foreach(bool last_correct in data.last[name])
            {
                if (last_correct)
                    c++;
                else
                    f++;
            }
            return new Tuple<int,int>(c,f);
        }


        public float Call_Percent(string name)
        {
            Check_Name(name);
            return (float)data.correct[name] / (float)(data.correct[name]+data.failes[name]);
        }

        public float Call_Last_Percent(string name)
        {
            var lastCount = Call_Last_Correct_Failes_Ammount(name);
            return (float)lastCount.Item1 / (float)(lastCount.Item2 + lastCount.Item2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Item1: min ; Item2: max ; Item3: avg</returns>
        public Tuple<int, int, int> Score_Last_Infos()
        {
            int min = int.MaxValue, max = int.MinValue, avg = 0;
            for(int i = 0; i < tracking && i < data.scores.Count; i++)
            {
                int score = data.scores[data.scores.Count - 1 - i];
                min = min < score ? min : score;
                max = max > score ? max : score;
                avg += score;                
            }
            avg /= tracking;
            return new Tuple<int, int, int>(min, max, avg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Item1: min ; Item2: max ; Item3: avg</returns>
        public Tuple<int, int, int> Score_Infos()
        {
            int min = int.MaxValue, max = int.MinValue, avg = 0;
            for (int i = 0; i < data.scores.Count; i++)
            {
                int score = data.scores[i];
                min = min < score ? min : score;
                max = max > score ? max : score;
                avg += score;
            }
            avg /= data.scores.Count;
            return new Tuple<int, int, int>(min, max, avg);
        }

        private void Check_Name(string name)
        {
            if (!data.correct.ContainsKey(name))
                data.correct.Add(name, 0);
            if (!data.failes.ContainsKey(name))
                data.failes.Add(name, 0);
            if (!data.last.ContainsKey(name))
                data.last.Add(name, new Queue<bool>());
        }

        #region Crypto
        private static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
