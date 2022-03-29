using CallTrainer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CallTrainer.Views
{
    public partial class EndScreen : UserControl
    {
        public EndScreen()
        {
            InitializeComponent();
            groupBox_result.Text = "Result " + SA.currentName;
            Highscore hs = new Highscore(SA.currentName);
            groupBox_cl.Text = groupBox_pl.Text  = "LAST " + Highscore.tracking.ToString();

            FillHighscoreWithNewResults(hs);
            SetupThisRun();
            SetupLastX(hs);
            SetupOverAll(hs);
        }

        private void SetupOverAll(Highscore hs)
        {
            var correctAndFalse = hs.Call_Correct_Failes_Ammount();
            float percent = (float)correctAndFalse.Item1 / (float)(correctAndFalse.Item1 + correctAndFalse.Item2);
            var minMaxAvg = hs.Score_Infos();

            coc.Text = correctAndFalse.Item1.ToString();
            cof.Text = correctAndFalse.Item2.ToString();
            cop.Text = percent.ToString(CSD.PercentFormat);

            pomin.Text = minMaxAvg.Item1.ToString();
            pomax.Text = minMaxAvg.Item2.ToString();
            poavg.Text = minMaxAvg.Item3.ToString();

        }

        private void SetupLastX(Highscore hs)
        {
            var correctAndFalse = hs.Call_Last_Correct_Failes_Ammount();
            float percent = (float)correctAndFalse.Item1 / (float)(correctAndFalse.Item1 + correctAndFalse.Item2);
            var minMaxAvg = hs.Score_Last_Infos();

            clc.Text = correctAndFalse.Item1.ToString();
            clf.Text = correctAndFalse.Item2.ToString();
            clp.Text = percent.ToString(CSD.PercentFormat);
            
            plmin.Text = minMaxAvg.Item1.ToString();
            plmax.Text = minMaxAvg.Item2.ToString();
            plavg.Text = minMaxAvg.Item3.ToString();

        }

        private void SetupThisRun()
        {
            Game game = SA.game;

            int correct = 0,
                fails   = 0,
                points  = game.pointStack;
            float percent = 0f;

            foreach(var result in game.answers)
            {
                correct += Convert.ToInt32(result);
                fails += Convert.ToInt32(!result);
            }
            
            percent = (float)correct / (float)(correct + fails);

            ctc.Text = correct.ToString();
            ctf.Text = fails.ToString();
            ctp.Text = percent.ToString(CSD.PercentFormat);
            pgc.Text = points.ToString();
        }


        private void FillHighscoreWithNewResults(Highscore hs)
        {
            Game game = SA.game;
            hs.New_Score(game.pointStack);
            for(int i = 0; i < game.answers.Count; i++)
            {
                hs.New_Call(CallNameBuilder(game.cards[game.cardIterationToAccess[i]]), game.answers[i]);
            }
            hs.Save();
        }

        private string CallNameBuilder(Game.Card card)
        {
            string result = "";
            for(int i = 0; i < card.correct.Length; i++)
            {
                result += card.calls[i,card.correct[i]] + " ";
            }
            return result.Trim();
        }

        private void button_restart_Click(object sender, EventArgs e)
        {
            SA.main.StartGame();
        }

        private void button_menu_Click(object sender, EventArgs e)
        {
            SA.main.BackToMenu();
        }
    }
}
