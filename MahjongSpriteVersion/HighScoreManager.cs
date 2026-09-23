using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class HighScoreManager
    {
        // Scores are stored by layout name, so renamed layouts keep their scores through this map.
        private static readonly Dictionary<string, string> RenamedLayouts = new Dictionary<string, string>
        {
            { "Number One", "Twin Peaks" },
            { "The Runner Up", "Temple" },
        };

        List<HighScore> HighScores { get; set; }
        public HighScoreManager()
        {
            HighScores = new List<HighScore>();
            Deserialize(Properties.Settings.Default.HighScores);

            if (ApplyLayoutRenames())
            {
                Properties.Settings.Default.HighScores = Serialize();
                Properties.Settings.Default.Save();
            }
        }

        private bool ApplyLayoutRenames()
        {
            bool changed = false;
            foreach (HighScore score in HighScores)
            {
                if (RenamedLayouts.TryGetValue(score.LayoutName, out string newName))
                {
                    score.LayoutName = newName;
                    changed = true;
                }
            }

            return changed;
        }

        public HighScore AddScore(int score, string layoutName, string user)
        {
            HighScore highScore = new HighScore(score, layoutName, user);
            HighScores.Add(highScore);

            HighScores = HighScores.OrderByDescending(hs => hs.Score).Take(20).ToList();

            Properties.Settings.Default.HighScores = Serialize();
            Properties.Settings.Default.Save();

            return highScore;
        }


        public List<HighScore> GetListForLayout(string layoutName)
        {
            List<HighScore> list = HighScores.FindAll(hs => hs.LayoutName == layoutName)
                .OrderByDescending(hs => hs.Score).Take(20).ToList();

            int index = 0;
            foreach (HighScore score in list)
            {
                score.DisplayNumber = ++index;
            }

            return list;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            foreach(HighScore score in HighScores)
            {
                sb.AppendLine(score.Serialize());
            }
            return sb.ToString();
        }

        public void Deserialize(string scores)
        {
            string[] scoreArray = scores.Split(new char[] {'\n'},StringSplitOptions.RemoveEmptyEntries);
            foreach(string scoreItem in scoreArray)
            {
                HighScores.Add(new HighScore(scoreItem));
            }
        }

        private void Sort()
        {
        }
    }
}
