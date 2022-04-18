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
    public partial class GameMenu : UserControl
    {
        private string callsFolder = Path.Combine(CSD.rootFolder, CSD.CallsFolderName);
        private List<string> maps;

        public GameMenu()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            try
            {
                comboBox.Items.Clear();
                maps = new List<string>();
                foreach(var folder in Directory.GetDirectories(callsFolder))
                {
                    maps.Add(Path.GetFileName(folder));
                }
                comboBox.Items.AddRange(maps.ToArray());
                comboBox.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("No \"Calls\" Folder @:\n"+callsFolder+"\n");
                Application.Exit();
            }
        }

        private void button_Play_Click(object sender, EventArgs e)
        {
            SA.main.StartGame();
        }

        private void button_Statistiks_Click(object sender, EventArgs e)
        {
            SA.main.ShowStatistiks();
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SA.currentName = comboBox.Text;
        }
    }
}
