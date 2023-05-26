using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MahjongSpriteVersion
{
    public partial class GameForm : Form
    {
        private Game Game = new Game();
        Timer timer = new Timer();
        public GameForm()
        {
            InitializeComponent();

            pbLayoutPanel.BackColor = Color.Transparent;
            lblClock.BackColor = Color.Transparent;
            lblScore.BackColor = Color.Transparent;
            label2.BackColor = Color.Transparent;
            label3.BackColor = Color.Transparent;
            label4.BackColor = Color.Transparent;
            label5.BackColor = Color.Transparent;
            label6.BackColor = Color.Transparent;
            label7.BackColor = Color.Transparent;
            label8.BackColor = Color.Transparent;
            label9.BackColor = Color.Transparent;
            label10.BackColor = Color.Transparent;
            label11.BackColor = Color.Transparent;

            lblSpeedBonusTimeLeft.BackColor = Color.Transparent;
            lblSpeedBonusTilesCollected.BackColor = Color.Transparent;
            lblBonusQty.BackColor = Color.Transparent;
            lblBonusTotal.BackColor = Color.Transparent;
            lblFinalBonus.BackColor = Color.Transparent;
            lblShuffleBonus.BackColor = Color.Transparent;
            lblMinuteBonus.BackColor = Color.Transparent;

            panel1.BackColor = Color.FromArgb(100, 255, 255, 255);

            Game.OnTilesRemoved += Game_OnTilesRemoved;
            Game.New(GetLayoutName());
            Game.Sound = chkSound.Checked;

            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();
        }


        void Game_OnTilesRemoved(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            lblBonusTotal.Text = Game.SuperBonusScore.ToString();
            lblBonusQty.Text = Game.SuperBonusQuantity.ToString();
            lblScore.Text = Game.Score.ToString();

            StringBuilder sb = new StringBuilder();
            foreach(string score in Game.SuperBonusHistory)
            {
                sb.AppendLine(score);
            }
            //txtBonusRecord.Text = sb.ToString();
        }


        public event EventHandler OnGameOver = null;

        private void timer_Tick(object sender, EventArgs e)
        {
            Game.BeatHeart();

            lblClock.Text = Game.TimerSettings.ClockText;
            DisplayStatus();
            lblSpeedBonusTimeLeft.Text = Game.TimerSettings.SuperBonusSeconds.ToString();
            lblSpeedBonusTilesCollected.Text = Game.SuperBonusTilesRemoved.ToString();
        }

        private void Render()
        {
            pbLayoutPanel.Image = Game.Render();
            UpdateControls();
        }

        private void DisplayStatus()
        {
            if (Game.TimesClicked == 0)
                return;

            if (Game.TimesClicked == 1)
                Game.TimesClicked++;

            if (Game.TimesClicked == 2)
            {
                Game.TimesClicked = 0;
                TileStatus status = Game.GetStatus();
                if (status == TileStatus.Blocked)
                    MessageBox.Show("There are no more solvable pairs");
                else if (status == TileStatus.GameComplete)
                {
                    if (OnGameOver != null)
                    {
                        OnGameOver(this, new EventArgs());
                    }


                    if (Game.TimerSettings.BonusSeconds > 0)
                    {
                        Game.CalculateFinalScore();

                        lblMinuteBonus.Text = Game.MinuteBonus.ToString();
                        lblShuffleBonus.Text = Game.ShuffleBonus.ToString();
               
                        lblScore.Text = Game.Score.ToString();
                        lblFinalBonus.Text = (Game.TimerSettings.BonusSeconds * 30).ToString();
                    }
                    timer.Stop();

                    HighScore newScore =  Game.HighScoreManager.AddScore(Game.Score, GetLayoutName(), "Karl");
                    lstHighScores.Items.Clear();
                    lstHighScores.Items.AddRange(Game.HighScoreManager.GetListForLayout(GetLayoutName()).ToArray());
                    lstHighScores.SelectedItem = newScore;

                    MessageBox.Show("You Win!!");
                }
            }
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            lblSpeedBonusTilesCollected.Text = Game.SuperBonusTilesRemoved.ToString();
            lblSpeedBonusTilesCollected.Text = "0";

            List<HighScore> list = Game.HighScoreManager.GetListForLayout(GetLayoutName());
            lstHighScores.Items.AddRange(Game.HighScoreManager.GetListForLayout(GetLayoutName()).ToArray());

            Game.Initialize(this.Size);
            Render();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            timer.Stop();
            Game.New(GetLayoutName());
            lblBonusQty.Text = string.Empty;
            lblBonusTotal.Text = string.Empty;
            lblMinuteBonus.Text = string.Empty;
            lblShuffleBonus.Text = string.Empty;
            lblFinalBonus.Text = "0";
            Game.LoadLayout();
            timer.Start();
            Render();
        }

        private string GetLayoutName()
        {
            string name;

            name = cboLayout.Text;
            if(name == string.Empty)
            {
                name = "Number One";
            }

            return name;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (btnPause.Text == "Pause")
            {
                timer.Stop();
                pbLayoutPanel.Enabled = false;
                btnPause.Text = "Play";
            }
            else
            {
                timer.Start();
                pbLayoutPanel.Enabled = true;
                btnPause.Text = "Pause";
            }
        }

        private void btnShuffle_Click(object sender, EventArgs e)
        {
            Game.Shuffle();
            Render();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            Game.UndoAction();
            Render();
        }

        private void cboLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstHighScores.Items.Clear();
            lstHighScores.Items.AddRange(Game.HighScoreManager.GetListForLayout(GetLayoutName()).ToArray());
        }
        
        private void pbLayoutPanel_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Game.HandleClick(e.Location);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Render();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            Game.RedoAction();
            Render();
        }

        private void chkSound_CheckedChanged(object sender, EventArgs e)
        {
            Game.Sound = chkSound.Checked;
        }
    }
}
