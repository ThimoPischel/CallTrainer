using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CallTrainer.Services;

namespace CallTrainer.Views
{
    public partial class Game : UserControl
    {
        public int pointStack { get; private set; }
        public int[] cardIterationToAccess { get; private set; }
        public List<bool> answers { get; private set; }
        public List<Card> cards { get; private set; }

        private const int startPoints = 3000;
        private const int pointReduktion = 75;
        private const int pointMinus = -500;
        private const int pointFail = 1000;
        private const int pauseTime = 800;

        public class Card
        {
            public string imageName;
            public int[] correct;
            public string[,] calls;
        }

        private Dictionary<string, string> pictureNameToPath = new Dictionary<string, string>();
        private int callIndex;
        private int cardIteration;
        private Color buttonDefColor, buttonFalseColor, buttonCorrectColor;
        private int AccessCardIndex
        {
            get
            {
                return cardIterationToAccess[cardIteration];
            }
        }

        private Action<Button>[] buttonActions;
        private bool reducePoints = false;
        private int points;
        private Button[] buttons;
        private DateTime? nextInvokeTime = null;
        private bool instantClose = false;
        private bool finished = false;

        private bool ReadyForNext
        {
            get
            {
                return nextInvokeTime.HasValue;
            }
            set
            {
                if(value == nextInvokeTime.HasValue)
                    return;

                if(value)
                    nextInvokeTime = DateTime.Now + TimeSpan.FromMilliseconds(pauseTime);
                else
                    nextInvokeTime = null;
            }
        }

        public Game()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            cards = new List<Card>();
            
            string folderPath = Path.Combine(CSD.rootFolder, CSD.CallsFolderName, SA.currentName);
            string filePath = Path.Combine(folderPath, SA.currentName + ".tsv");
            if(!Directory.Exists(folderPath) || !File.Exists(filePath))
            {
                Console.WriteLine($"Folder or File not found:\n  {folderPath}\n  {filePath}");
                instantClose = true;
                return;
            }

#if !DEBUG
            try
            {
#endif            
            foreach(var file in Directory.GetFiles(folderPath))
            {
                if(file == filePath)
                    continue;
                
                pictureNameToPath.Add(Path.GetFileNameWithoutExtension(file),file);
            }
            
            List<string> callPool = new List<string>();
            string callPoolFile = Path.Combine(folderPath, CSD.CallPoolFileName);
            if (File.Exists(callPoolFile))
            {
                foreach(var line in File.ReadAllLines(callPoolFile))
                {
                    callPool.Add(line.Trim());
                }
            }

            string[] lines = File.ReadAllLines(filePath);
            foreach(var line in lines)
            {
                string[] calls = line.Split('\t');
                for(int i = 1; i< calls.Length; i++)
                    callPool.Add(calls[i].Trim());
            }
            Random rnd = new Random();
            foreach(var line in lines)
            {
                Card tmp = new Card();
                string[] values = line.Trim().Split('\t');
                tmp.imageName = values[0];

                int blocks = values.Length - 1;
                tmp.correct = new int[blocks];
                tmp.calls = new string[blocks,4];

                for(int b = 0; b < blocks; b++)
                {
                    int[] random = RandomMap(4);
                    for(int i = 0; i < 4; i++)
                        if(random[i] == 0)
                        {
                            tmp.correct[b] = i;
                            tmp.calls[b,i] = values[b+1].Trim();
                            break;
                        }
                }                
                cards.Add(tmp);
            }

            foreach(var c in cards)
            {
                string[] correct = new string[c.correct.Length];
                for(int i = 0; i < correct.Length; i++)
                {
                    correct[i] = c.calls[i,c.correct[i]].ToLower();
                }

                for(int i = 0; i < c.correct.Length; i++)
                {
                    for(int j = 0; j < 4; j++)
                    {
                        if(j == c.correct[i])
                            continue;

                        string randomCall = "";
                        do
                        {
                            randomCall = callPool[rnd.Next(0, callPool.Count)];
                        } while (correct.Contains(randomCall.ToLower()));
                        c.calls[i,j] = randomCall;
                    }
                }
            }
#if !DEBUG
            }
            catch
            {
                SA.main.BackToMenu();
            }
#endif

            cardIterationToAccess = RandomMap(cards.Count);
            answers = new List<bool>(cards.Count);
            for(int i = 0; i < cards.Count; i++)
                answers.Add(true);

            buttonActions = new Action<Button>[]
            {
                DoNothing,
                DoNothing,
                DoNothing,
                DoNothing
            };
            buttons = new Button[]
            {
                button1,
                button2,
                button3,
                button4
            };
            
            buttonDefColor = button1.BackColor;
            buttonCorrectColor = button2.BackColor;
            buttonFalseColor = button3.BackColor;

            cardIteration = 0;
            callIndex = -1;
            ReadyForNext = true;
        }


        private void Next()
        {
            if(finished)
            {
                timer1.Stop();
                return;
            }
            callIndex++;
            if(callIndex >= cards[AccessCardIndex].calls.Length / 4)
            {
                callIndex = 0;
                cardIteration++;
                if(cardIteration >= cards.Count)
                {
                    finished = true;
                    Finished();
                }
            }
            SetupForm();
            points = startPoints;
            reducePoints = true;
        }

        private void SetupForm()
        {
            if(finished)
            {
                Unbind();
                return;
            }
            Card card = cards[AccessCardIndex];
            button1.Text = card.calls[callIndex,0];
            button2.Text = card.calls[callIndex,1];
            button3.Text = card.calls[callIndex,2];
            button4.Text = card.calls[callIndex,3];
            button1.BackColor = button2.BackColor = button3.BackColor = button4.BackColor = buttonDefColor;
            buttonActions[0]  = buttonActions[1]  = buttonActions[2]  = buttonActions[3]  = FailButton;
            buttonActions[card.correct[callIndex]] = RightButton;
            if(callIndex == 0)
                pictureBox.Image = Image.FromFile(pictureNameToPath[card.imageName]);
            pictureBox.Show();
        }


        private void Finished()
        {
            timer1.Stop();
            SA.main.EndGame();
        }
        private void DoNothing(Button button) { }
        private void FailButton(Button button)
        {
            answers[cardIteration] = false;
            button.BackColor = buttonFalseColor;
            buttons[cards[AccessCardIndex].correct[callIndex]].BackColor = buttonCorrectColor;
            pointStack -= pointFail;
            Unbind();
            ReadyForNext = true;
        }
        private void RightButton(Button button)
        {
            reducePoints = false;
            pointStack += points;
            button.BackColor = buttonCorrectColor;
            Unbind();
            ReadyForNext = true;
        }

        private void Unbind()
        {
            buttonActions[0]  = buttonActions[1]  = buttonActions[2]  = buttonActions[3]  = DoNothing;
        }



        private int[] RandomMap(int size)
        {
            List<int> rest = new List<int>(size);
            for(int i =  0; i < size; i++)
            {
                rest.Add(i);
            }

            Random rnd = new Random();
            int[] result = new int[size];
            for(int i = 0; i<size; i++)
            {
                int index = rnd.Next(0,rest.Count);
                result[i] = rest[index];
                rest.RemoveAt(index);
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            buttonActions[0](button1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttonActions[1](button2);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            buttonActions[2](button3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            buttonActions[3](button4);

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if(instantClose)
            {
                timer1.Stop();
                SA.main.BackToMenu();
            }
            if(reducePoints)
            {
                points -= pointReduktion;
                if(points < 0)
                {
                    points = pointMinus;
                    reducePoints = false;
                }    
            }
            if(ReadyForNext)
            {
                pictureBox.Hide();
                if(nextInvokeTime.Value<DateTime.Now)
                {
                    ReadyForNext = false;
                    Next();
                }
                
            }

            label_current.Text = points.ToString();
            label_total.Text = pointStack.ToString();
        }
    }
}
