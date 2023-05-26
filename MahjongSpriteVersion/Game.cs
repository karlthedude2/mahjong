using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.IO;

namespace MahjongSpriteVersion
{

    public class Game
    {

        TileSprite selectedTile = null;
        Graphics device;
        SoundPlayer clickSound = null; 

        Stack<Layout> UndoLayout = new Stack<Layout>();
        Stack<Layout> RedoLayout = new Stack<Layout>();
        Stack<TileStyles> UndoTileStyles = new Stack<TileStyles>();
        Stack<TileStyles> RedoTileStyles = new Stack<TileStyles>();

        Stack<Action> RedoActions = new Stack<Action>();
        Stack<Action> UndoActions = new Stack<Action>();

        int tilesRemoved = 0;
        string layoutName;

        public Game()
        {
            TimerSettings = new TimerSettings();
            SuperBonusHistory = new List<string>();
            HighScoreManager = new HighScoreManager();
        }


        public event EventHandler OnTilesRemoved = null;
        public event EventHandler OnGameOver = null;

        public TimerSettings TimerSettings { get; set; }
        public TileStyles TileStyles { get; set; }
        public Layout TileLayout { get; set; }
        public Bitmap Surface { get; set; }
        public int Score { get; set; }
        public int TimesClicked { get; set; }
        public int SuperBonusTilesRemoved { get; set; }
        public List<string> SuperBonusHistory { get; set; }
        public int SuperBonusScore { get; set; }
        public int MinuteBonus { get; set; }
        public int ShuffleBonus { get; set; }
        public int SuperBonusQuantity { get; set; }
        public bool Sound { get; set; }
        public HighScoreManager HighScoreManager { get; private set; }


        public void Initialize(Size size)
        {
            Surface = new Bitmap(size.Width, size.Height);
            device = Graphics.FromImage(Surface);

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Stream soundStream = thisAssembly.GetManifestResourceStream("MahjongSpriteVersion.176893__mickmon__click-lighter.wav");
            clickSound = new SoundPlayer(soundStream);

            LoadLayout();
        }

        
        public void New(string layout)
        {
            Score = 0;
            layoutName = layout;

            TimerSettings = new TimerSettings();
            TileLayout = LayoutFactory.CreateLayout(layout);
            TileStyles = StyleFactory.CreateStyle(layout);

            SuperBonusScore = 0;
            SuperBonusQuantity = 0;
            SuperBonusTilesRemoved = 0;
            MinuteBonus = 5000;
            ShuffleBonus = 3000;

            UndoLayout.Clear();
            RedoLayout.Clear();
            UndoTileStyles.Clear();
            RedoTileStyles.Clear();

            RedoActions.Clear();
            UndoActions.Clear();

            selectedTile = null;

        }

        public Bitmap Render()
        {
            device.Clear(Color.Transparent);
            foreach (TileSprite tile in TileLayout.Tiles)
            {
                tile.Draw(device);
            }

            return Surface;
        }

        public void LoadLayout()
        {
            int index = 0;
            //bool sameObject = false;
            foreach (Coordinate coordinate in TileLayout.Locations)
            {
                TileSprite tile = new TileSprite(TileLayout, coordinate, TileStyles.Styles[index++]);
                tile.Location = new Point(coordinate.GetPixelX(), coordinate.GetPixelY());
                //sameObject = this.TileLayout.Equals(tile.TileLayout);
                TileLayout.Tiles.Add(tile);
            }
        }

        public void BeatHeart()
        {
            TimerSettings.Seconds++;

            if (--TimerSettings.SuperBonusSeconds == 0)
            {
                TimerSettings.ResetSuperBonusSeconds();
                SuperBonusTilesRemoved = 0;
            }

            if (TimerSettings.BonusSeconds != 0)
            {
                TimerSettings.BonusSeconds--;
            }

            switch(TimerSettings.Seconds) 
            {
                case 160:
                    MinuteBonus -= 1000;
                    break;
                case 170:
                    MinuteBonus -= 1000;
                    break;
                case 180:
                    MinuteBonus -= 500;
                    break;
                case 195:
                    MinuteBonus -= 500;
                    break;
                case 210:
                    MinuteBonus -= 500;
                    break;
                case 225:
                    MinuteBonus -= 500;
                    break;
                case 240:
                    MinuteBonus -= 500;
                    break;
                case 255:
                    MinuteBonus -= 250;
                    break;
                case 270:
                    MinuteBonus -= 250;
                    break;

            }

        }

        public void HandleClick(Point location)
        {
            TileSprite clickedTile = TileLayout.FindTile(location);
            if (clickedTile != null)
            {
                if (clickSound != null && Sound)
                {
                    clickSound.Play();
                }
                clickedTile.OnClick();

                if (selectedTile == null)
                {
                    selectedTile = clickedTile;
                }
                else if (selectedTile.Style.Name == clickedTile.Style.Name &&
                    selectedTile.Id != clickedTile.Id)
                {
                    CalculateScores(selectedTile);
                    CalculateScores(clickedTile);

                    SaveActionStateForUndo(selectedTile);

                    //UndoLayout.Push(TileLayout.GetCopy());
                    //UndoTileStyles.Push(TileStyles.GetCopy());

                    //RedoLayout.Clear();
                    //RedoTileStyles.Clear();

                    RemoveTile(selectedTile);
                    RemoveTile(clickedTile);
                    selectedTile = null;

                    if (OnTilesRemoved != null)
                    {
                        OnTilesRemoved(this, new EventArgs());
                    }

                    TimesClicked = 1;
                }
                else if (selectedTile != clickedTile)
                {
                    selectedTile.Deselect();
                    selectedTile = clickedTile;
                }
                else
                {
                    //selectedTile.Deselect();
                    selectedTile = null;
                }
            }

        }

        private void CalculateScores(TileSprite tile)
        {
            Score += tile.Style.Value;
            if (++tilesRemoved == 20)
            {
                Score += TimerSettings.BonusSeconds;

                tilesRemoved = 0;
            }

            if (TimerSettings.SuperBonusSeconds > 0 && ++SuperBonusTilesRemoved == 20)
            {

                Score += 100 * TimerSettings.SuperBonusSeconds;

                SuperBonusHistory.Add((SuperBonusHistory.Count + 1).ToString() + "  " + (100 * TimerSettings.SuperBonusSeconds).ToString());

                SuperBonusScore += 100 * TimerSettings.SuperBonusSeconds;
                SuperBonusQuantity += 1;

                TimerSettings.ResetSuperBonusSeconds();
                SuperBonusTilesRemoved = 0;
            }
        }

        public void CalculateFinalScore()
        {

            Score += TimerSettings.BonusSeconds * 30;
            Score += ShuffleBonus;
            Score += MinuteBonus;
        }

        private void RemoveTile(TileSprite tile)
        {
            tile.Deselect();
            //bool sameObject = this.TileLayout.Equals(tile.TileLayout);
            this.TileLayout.Tiles.Remove(tile);
            this.TileLayout.Locations.Remove(tile.Coordinate);
            this.TileStyles.Styles.Remove(tile.Style);
        }

        public TileStatus GetStatus()
        {

            List<TileStyle> styles = new List<TileStyle>();
            foreach (TileSprite tile in TileLayout.Tiles)
            {
                if (!TileLayout.IsBlocked(tile.Coordinate))
                {
                    styles.Add(tile.Style);
                }
            }

            foreach (TileStyle style in styles)
            {
                var items = styles.FindAll(s => s.Name == style.Name);
                if (items.Count > 1)
                {
                    return TileStatus.Available;
                }
            }

            if (this.TileStyles.Styles.Count == 0)
            {
                return TileStatus.GameComplete;
            }

            return TileStatus.Blocked;
        }

        public void Shuffle()
        {
            if(ShuffleBonus > 0)
            {
                ShuffleBonus -= 1000;
            }

            foreach (TileSprite tile in TileLayout.Tiles)
            {
                tile.ShuffleIndex = TileStyles.Styles.IndexOf(tile.Style);
            }

            TileStyles.Shuffle();

            foreach (TileSprite tile in TileLayout.Tiles)
            {
                tile.Style = TileStyles.Styles[tile.ShuffleIndex];
                tile.Shuffled = true;
                tile.Deselect();
            }
        }

        //public void Undo()
        //{
        //    if (UndoTileStyles.Count > 0)
        //    {
        //        RedoTileStyles.Push(TileStyles.GetCopy());
        //        RedoLayout.Push(TileLayout.GetCopy());

        //        TileStyles = UndoTileStyles.Pop();
        //        TileLayout = UndoLayout.Pop();
        //    }
        //}
        public void UndoAction()
        {
            if (UndoActions.Count > 0)
            {
                Action undoAction = UndoActions.Pop();
                RedoActions.Push(undoAction);

                TileStyles = undoAction.TileStyles;
                TileLayout = undoAction.Layout;
                UpdateTilesLayoutReference();

                Score -= undoAction.ScoreFromAction;
                //SuperBonusScore -= undoAction.BonusFromAction;
                //SuperBonusTilesRemoved -= undoAction.BonusFromAction == 0 ? 2 : 0;
                //SuperBonusQuantity -= undoAction.BonusFromAction == 0 ? 0 : 1;
            }
        }

        //public void Redo()
        //{
        //    if (RedoTileStyles.Count > 0)
        //    {
        //        UndoLayout.Push(TileLayout.GetCopy());
        //        UndoTileStyles.Push(TileStyles.GetCopy());

        //        TileStyles = RedoTileStyles.Pop();
        //        TileLayout = RedoLayout.Pop();

        //    }
        //}
        public void RedoAction()
        {
            if (RedoActions.Count > 0)
            {
                Action redoAction = RedoActions.Pop();
                UndoActions.Push(redoAction);

                TileStyles = redoAction.TileStyles;
                TileLayout = redoAction.Layout;
                UpdateTilesLayoutReference();

                Score += redoAction.ScoreFromAction;
                SuperBonusScore += redoAction.BonusFromAction;
                SuperBonusQuantity += redoAction.BonusFromAction == 0 ? 2 : 0;
                SuperBonusQuantity += redoAction.BonusFromAction == 0 ? 0 : 1;
            }
        }

        private void SaveActionStateForUndo(TileSprite tile)
        {
            Action action = new Action();
            action.Layout = TileLayout.GetCopy();
            action.TileStyles = TileStyles.GetCopy();
            action.BonusFromAction = 0;
            action.ScoreFromAction = tile.Style.Value * 2;

            if(UndoActions.Count > 0)
            {
                Action previousAction = UndoActions.Peek();

                if (SuperBonusScore < previousAction.BonusFromAction)
                {
                    action.BonusFromAction = SuperBonusScore - previousAction.BonusFromAction;
                }
            }

            UndoActions.Push(action);
            RedoActions.Clear();
        }

        private void UpdateTilesLayoutReference()
        {
            foreach(TileSprite tile in TileLayout.Tiles)
            {
                tile.TileLayout = TileLayout;
            }
        }

    }

}
