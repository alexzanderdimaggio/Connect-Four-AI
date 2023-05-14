using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConnectFourAITesting;
using System.Diagnostics;

namespace FinalProject
{
    public partial class ConnectFour : Form
    {
        CircularButton[,] btn = new CircularButton[7, 6];
        private Position position = new Position();
        private ConnectAI ai = new ConnectAI();

        public ConnectFour()
        {
            InitializeComponent();
            
            for(int x = 0; x < Position.Width; x++)
            {
                for (int y = 0; y < Position.Height; y++)
                {
                    btn[x, y] = new CircularButton();
                    btn[x, y].SetBounds(90 * x, 90 * y, 90, 90);
                    btn[x, y].Name = "btn" + x.ToString() + y.ToString();
                    btn[x, y].Click += new EventHandler(this.CircularButton_Click);
                    Controls.Add(btn[x, y]);
                    btn[x, y].BackColor = Color.Gray;
                } //end for loop

            }//end for loop

        }//end ConnectFour Constructor

        private void CircularButton_Click(object sender, EventArgs e)
        {
            CircularButton c = (CircularButton)sender;

            if (position.CanPlay(int.Parse(c.Name.Substring(3, 1))))
            {
                bool win = position.IsWinningMove(int.Parse(c.Name.Substring(3, 1)));
                position.Play(int.Parse(c.Name.Substring(3, 1)));
                updateAllButtons();

                if (win || position.nbMoves() == Position.Width * Position.Height)
                {
                    GameOver();
                }//end if statement

                else
                {
                    int aicol = ai.AITurn(position);
                    win = position.IsWinningMove(aicol);
                    position.Play(aicol);
                    updateAllButtons();
                    if (win || position.nbMoves() == Position.Width * Position.Height)
                    {
                        GameOver();
                    }//end if statement

                }//end else statement

            }//end if statement

            /*
            Control ctrl = ((Control)sender);
            switch (ctrl.BackColor.Name)
            {
                case "Yellow":
                    ctrl.BackColor = Color.Yellow;
                    break;
            }//end switch
            */

        }//end CircularButton_Click Event

        private void updateAllButtons()
        {
            for (int i = 0; i < Position.Width; i++)
            {
                for (int j = 0; j < Position.Height; j++)
                {
                    btn[i, j].BackColor = Color.Gray;
                }//end for loop

                //heightPlayable[i] = bHeight - 1;

            }//end for loop

            ulong key = position.key() + position.bottom();


            for (int i = 0; i < 7; i++)
            {
                bool hitTop = false;
                uint col = (uint)(key >> (i * 7));
                for (int j = 6; j >= 0; j--)
                {
                    uint v = col >> j;

                    if (hitTop)
                    {
                        if (position.nbMoves() % 2 == 0 && v % 2 == 1
                            || position.nbMoves() % 2 == 1 && v % 2 == 0)
                            btn[i, 5-j].BackColor = Color.Red;
                        else btn[i, 5-j].BackColor = Color.Yellow;
                    }//end if statement

                    if (v % 2 == 1)
                    {
                        hitTop = true;
                    }//end if statement

                }//end for loop

            }//end for loop

        }//end updateAllButtons

        private void GameOver()
        {
            MessageBox.Show("Game is now over", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }//end GameOver Method

        private void ConnectFour_Load(object sender, EventArgs e)
        {

        }//end ConnectFour_Load

        private void btnUndo_Click(object sender, EventArgs e)
        {
            position.Undo();
            updateAllButtons();
        }//end OnDue

        private void btnReset_Click(object sender, EventArgs e)
        {
            position = new Position();
            updateAllButtons();
        }//end Reset

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int score = ai.Solve(position, sw);
            lblBoardScore.Text = "Score: " + score.ToString();

            sw.Stop();
        }//end Evaluate

    }//end ConnectFour Class

}//end namespace
