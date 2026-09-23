using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Mahjong.Core;

namespace MahjongSpriteVersion
{
    public partial class GameForm : Form
    {
        private const string PlayerName = "Karl";
        private const string DefaultLayoutName = "Twin Peaks";

        // Start the game with this switch to also list hidden (debugging) layouts such as Test.
        private const string ShowHiddenLayoutsSwitch = "--show-hidden-layouts";

        private readonly Game game = new Game();
        private readonly Timer timer = new Timer { Interval = 1000 };
        private bool gameOver;

        public GameForm()
        {
            InitializeComponent();

            pbLayoutPanel.BackColor = Color.Transparent;
            foreach (var label in panel1.Controls.OfType<Label>())
            {
                label.BackColor = Color.Transparent;
            }

            panel1.BackColor = Color.FromArgb(100, 255, 255, 255);

            LoadLayoutChoices();
            FitPlayAreaToLayouts();

            game.TilesRemoved += Game_TilesRemoved;
            game.Sound = chkSound.Checked;
            timer.Tick += timer_Tick;
            FormClosed += (s, e) => game.Dispose();
        }

        private static LayoutDefinition DefaultLayout =>
            LayoutCatalog.Find(DefaultLayoutName) ?? LayoutCatalog.Default;

        private LayoutDefinition SelectedLayout =>
            cboLayout.SelectedItem as LayoutDefinition ?? DefaultLayout;

        private void LoadLayoutChoices()
        {
            // Extra layouts can be dropped into a Layouts folder next to the exe.
            var problems = LayoutCatalog.AddFromDirectory(Path.Combine(Application.StartupPath, "Layouts"));
            if (problems.Count > 0)
            {
                MessageBox.Show("Some layouts could not be loaded:\n\n" + string.Join("\n", problems), "Layouts");
            }

            cboLayout.DropDownStyle = ComboBoxStyle.DropDownList;
            bool showHidden = Environment.GetCommandLineArgs()
                .Any(a => string.Equals(a, ShowHiddenLayoutsSwitch, StringComparison.OrdinalIgnoreCase));
            var layouts = showHidden ? LayoutCatalog.All : LayoutCatalog.Visible;
            cboLayout.Items.AddRange(layouts.ToArray<object>());
            cboLayout.SelectedItem = DefaultLayout;
        }

        /// <summary>
        /// Grows the play area (and the window) so the largest layout fits at full tile size,
        /// as far as the screen allows. Layouts are centred in it when drawn.
        /// </summary>
        private void FitPlayAreaToLayouts()
        {
            const int margin = 12;

            var largest = LayoutCatalog.All
                .Select(l => TileGeometry.Bounds(l.Positions).Size)
                .Aggregate((a, b) => new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height)));

            // Leave room for the window frame and the score panel; anything bigger than the
            // screen is shrunk to fit by the game when drawn.
            var screen = Screen.PrimaryScreen.WorkingArea;
            var frame = Size - ClientSize;
            int maxWidth = screen.Width - frame.Width - pbLayoutPanel.Left - margin;
            int maxHeight = screen.Height - frame.Height - pbLayoutPanel.Top - margin;

            pbLayoutPanel.Size = new Size(
                Math.Min(maxWidth, Math.Max(pbLayoutPanel.Width, largest.Width + 2 * margin)),
                Math.Min(maxHeight, Math.Max(pbLayoutPanel.Height, largest.Height + 2 * margin)));

            ClientSize = new Size(
                pbLayoutPanel.Right + margin,
                Math.Max(ClientSize.Height, pbLayoutPanel.Bottom + margin));

            StartPosition = FormStartPosition.CenterScreen;
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            game.Initialize(pbLayoutPanel.ClientSize);
            StartNewGame();
        }

        private void StartNewGame()
        {
            timer.Stop();
            game.New(SelectedLayout);
            gameOver = false;
            pbLayoutPanel.Enabled = true;
            btnPause.Text = "Pause";

            lblBonusQty.Text = string.Empty;
            lblBonusTotal.Text = string.Empty;
            lblMinuteBonus.Text = string.Empty;
            lblShuffleBonus.Text = string.Empty;
            lblFinalBonus.Text = "0";
            lblClock.Text = game.Scoring.Clock.ClockText;
            ShowHighScores();

            timer.Start();
            Render();
        }

        private void Render()
        {
            pbLayoutPanel.Image = game.Render();
            UpdateScoreLabels();
        }

        private void UpdateScoreLabels()
        {
            var card = game.Scoring.Card;
            lblBonusTotal.Text = card.SuperBonusScore.ToString();
            lblBonusQty.Text = card.SuperBonusQuantity.ToString();
            lblScore.Text = card.Score.ToString();
            lblSpeedBonusTimeLeft.Text = game.Scoring.Clock.SuperBonusSeconds.ToString();
            lblSpeedBonusTilesCollected.Text = card.SuperBonusTilesRemoved.ToString();
        }

        private void ShowHighScores(HighScore selected = null)
        {
            lstHighScores.Items.Clear();
            lstHighScores.Items.AddRange(game.HighScoreManager.GetListForLayout(SelectedLayout.Name).ToArray());
            lstHighScores.SelectedItem = selected;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            game.Tick();
            lblClock.Text = game.Scoring.Clock.ClockText;
            UpdateScoreLabels();
        }

        private void Game_TilesRemoved(object sender, EventArgs e)
        {
            // Let the board repaint before any message box appears.
            BeginInvoke(new Action(CheckStatus));
        }

        private void CheckStatus()
        {
            switch (game.Current.Status)
            {
                case GameStatus.NoMovesLeft:
                    MessageBox.Show("There are no more solvable pairs. Try Shuffle or Undo.");
                    break;

                case GameStatus.Complete:
                    FinishGame();
                    break;
            }
        }

        private void FinishGame()
        {
            timer.Stop();
            gameOver = true;

            var scoring = game.Scoring;
            if (scoring.EarnsFinalBonus)
            {
                lblMinuteBonus.Text = scoring.Clock.MinuteBonus.ToString();
                lblShuffleBonus.Text = scoring.Card.ShuffleBonus.ToString();
                lblFinalBonus.Text = scoring.FinalTimeBonus.ToString();
            }

            game.Current.Finish();
            lblScore.Text = scoring.Card.Score.ToString();

            var newScore = game.HighScoreManager.AddScore(scoring.Card.Score, SelectedLayout.Name, PlayerName);
            ShowHighScores(newScore);

            MessageBox.Show("You Win!!");
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (gameOver)
            {
                return;
            }

            bool pausing = btnPause.Text == "Pause";
            timer.Enabled = !pausing;
            pbLayoutPanel.Enabled = !pausing;
            btnPause.Text = pausing ? "Play" : "Pause";
        }

        private void btnShuffle_Click(object sender, EventArgs e)
        {
            if (gameOver)
            {
                return;
            }

            game.Shuffle();
            Render();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (gameOver)
            {
                return;
            }

            game.Undo();
            Render();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (gameOver)
            {
                return;
            }

            game.Redo();
            Render();
            CheckStatus();
        }

        private void cboLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Before the form has loaded (while the list is being filled) there is no game to replace yet.
            if (game.Current == null)
            {
                ShowHighScores();
                return;
            }

            StartNewGame();
        }

        private void pbLayoutPanel_MouseClick(object sender, MouseEventArgs e)
        {
            game.HandleClick(e.Location);
            Render();
        }

        private void chkSound_CheckedChanged(object sender, EventArgs e)
        {
            game.Sound = chkSound.Checked;
        }
    }
}
