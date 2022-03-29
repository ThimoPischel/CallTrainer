using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CallTrainer.Services;
using CallTrainer.Views;

namespace CallTrainer
{
    public partial class Main : Form
    {


        public Main()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            SA.menu = new Views.GameMenu();
            BackToMenu();
        }

        public void StartGame()
        {
            SA.game = new Game();
            ShowPanel(SA.game);
        }

        public void EndGame()
        {
            if (SA.game == null)
                return;

            ShowPanel(new EndScreen());
        }

        public void BackToMenu()
        {
            SA.game = null;
            ShowPanel(SA.menu);    
        }

        public void ShowStatistiks()
        {
            ShowPanel(new Statistik());
        }

        private void ShowPanel(Control control)
        {
            mainpanel.Controls.Clear();
            mainpanel.Controls.Add(control);
        }



    }
}
