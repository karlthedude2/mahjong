using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class HighScoreManager
    {
        List<HighScore> HighScores { get; set; }
        public HighScoreManager()
        {
            HighScores = new List<HighScore>();
            Deserialize(Properties.Settings.Default.HighScores);
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
