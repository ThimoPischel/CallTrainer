using CallTrainer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CallTrainer.Views
{
    public partial class Statistik : UserControl
    {
        private Dictionary<string,Highscore> highscores;
        private string name;
        private string[] calls;

        public Statistik()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            highscores = new Dictionary<string, Highscore>();
            string folderPath = Path.Combine(CSD.rootFolder, CSD.HighscoreFolderName);

            foreach(var file in Directory.GetFiles(folderPath))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                highscores.Add(name, new Highscore(name));
            }
            
            comboBox_CallGroup.Items.Clear();
            foreach(var callgroup in highscores)
            {
                comboBox_CallGroup.Items.Add(callgroup.Key);
            }
            if(comboBox_CallGroup.Items.Count > 0)
            {
                comboBox_CallGroup.SelectedIndex = 0;
                comboBox_CallGroup_SelectionChangeCommitted(null, null);
            }


        }
            

        private void comboBox_CallGroup_SelectionChangeCommitted(object sender, EventArgs e)
        {
            name = comboBox_CallGroup.Text;
            if (!highscores.ContainsKey(name))
                return;
            Highscore hs = highscores[name];

            var minMaxAvg = hs.Score_Last_Infos();
            lsmin.Text = minMaxAvg.Item1.ToString();
            lsmax.Text = minMaxAvg.Item2.ToString();
            lsavg.Text = minMaxAvg.Item3.ToString();
            minMaxAvg = hs.Score_Infos();
            osmin.Text = minMaxAvg.Item1.ToString();
            osmax.Text = minMaxAvg.Item2.ToString();
            osavg.Text = minMaxAvg.Item3.ToString();

            var correctFails = hs.Call_Last_Correct_Failes_Ammount();
            lcc.Text = correctFails.Item1.ToString();
            lcf.Text = correctFails.Item2.ToString();
            lcp.Text = ((float)correctFails.Item1 / (float)(correctFails.Item1 + correctFails.Item2)).ToString(CSD.PercentFormat);
            correctFails = hs.Call_Correct_Failes_Ammount();
            occ.Text = correctFails.Item1.ToString();
            ocf.Text = correctFails.Item2.ToString();
            ocp.Text = ((float)correctFails.Item1 / (float)(correctFails.Item1 + correctFails.Item2)).ToString(CSD.PercentFormat);

            calls = hs.AllCalls();
            
            comboBox_LastSingleCall.Items.Clear();
            comboBox_LastSingleCall.Items.AddRange(calls);
            comboBox_LastSingleCall.SelectedIndex = 0;
            comboBox_OverallSingleCall.Items.Clear();
            comboBox_OverallSingleCall.Items.AddRange(calls);
            comboBox_OverallSingleCall.SelectedIndex = 0;
                        
            List<Tuple<string,float>> callTraining = new List<Tuple<string, float>>();

            foreach(var call in calls)
            {
                float f = 0f;
                bool[] c = hs.Call_Last_Correct(call);
                for(int i = 0; i < c.Length; i++)
                {
                    if(!c[i])
                    {
                        f += ((float)c.Length - (float)i) * (float)c.Length; 
                    }
                }
                callTraining.Add(new Tuple<string, float>(call, f));
            }

            perfekt.Items.Clear();
            need.Items.Clear();
            bad.Items.Clear();
            for(int i = 0; i < callTraining.Count(); i++)
            {
                if(callTraining[i].Item2 == 0f)
                {
                    perfekt.Items.Add(callTraining[i].Item1);
                    callTraining.RemoveAt(i);
                }
                else
                {
                    need.Items.Add(callTraining[i].Item1);
                }
            }
            callTraining.Sort((x,y) => { return Convert.ToInt32(x.Item2 < y.Item2);});
            
            foreach(var call in callTraining)
            {
                bad.Items.Add(call.Item1);
            }
                      
            comboBox_LastSingleCall_SelectedIndexChanged(null,null);
            comboBox_OverallSingleCall_SelectedIndexChanged(null,null);
        }

        private void comboBox_LastSingleCall_SelectedIndexChanged(object sender, EventArgs e)
        {
            string call = comboBox_LastSingleCall.Text;
            if (call != null && calls.Contains(call))
            {
                var cf = highscores[name].Call_Last_Correct_Failes_Ammount(call);
                lsinglec.Text = cf.Item1.ToString();
                lsinglef.Text = cf.Item2.ToString();
                lsinglep.Text = ((float)cf.Item1 / (float)(cf.Item1 + cf.Item2)).ToString();

                bool[] order = highscores[name].Call_Last_Correct(call);
                string o = "";
                for(int i = 0; i < order.Length; i++)
                {
                    o += order[i] ? "1 " : "0 ";
                }
                lsingleo.Text = o;
            }
        }

        private void comboBox_OverallSingleCall_SelectedIndexChanged(object sender, EventArgs e)
        {
            string call = comboBox_OverallSingleCall.Text;
            if (call != null && calls.Contains(call))
            {
                var cf = highscores[name].Call_Correct_Failes_Ammount(call);
                osinglec.Text = cf.Item1.ToString();
                osinglef.Text = cf.Item2.ToString();
                osinglep.Text = ((float)cf.Item1 / (float)(cf.Item1 + cf.Item2)).ToString();
            }
        }

        private void backToMenu_Click(object sender, EventArgs e)
        {
            SA.main.BackToMenu();
        }
    }
}
