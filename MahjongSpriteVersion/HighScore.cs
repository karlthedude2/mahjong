using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class HighScore
    {
        const int UserFieldLength = 13;
        const int DateFieldLength = 10;
        const int ScoreFieldLength = 6;
        const int LayoutNameFieldLength = 16;

        const int UserFieldPosition = 0;
        const int LayoutNameFieldPosition = 13;
        const int ScoreDateFieldPosition = 29;
        const int ScoreFieldPosition = 39;

        public HighScore()
        {
        }
        public HighScore(int score, string layoutName, string user)
        {
            Score = score;
            User = user;
            LayoutName = layoutName;
            ScoreDate = DateTime.Now.ToShortDateString();
        }
        public HighScore(string serializedString)
        {
            User = serializedString.Substring(UserFieldPosition, UserFieldLength).Trim();
            LayoutName = serializedString.Substring(LayoutNameFieldPosition, LayoutNameFieldLength).Trim();
            ScoreDate = serializedString.Substring(ScoreDateFieldPosition, DateFieldLength).Trim();

            int score;
            int.TryParse(serializedString.Substring(ScoreFieldPosition, ScoreFieldLength).Trim(), out score);
            Score = score;
        }

        public int Score { get; set; }
        public string LayoutName { get; set; }
        public string User { get; set; }
        public string ScoreDate { get; set; }
        public int DisplayNumber { get; set; }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            Append(sb, User, UserFieldLength, false);
            Append(sb, LayoutName, LayoutNameFieldLength, false);
            Append(sb, ScoreDate, DateFieldLength, true);
            Append(sb, Score.ToString(), ScoreFieldLength, true);
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(DisplayNumber.ToString());
            sb.Append(".");
            sb.Append(" ");

            //pad extra space
            if (DisplayNumber < 10)
            {
                sb.Append(" ");
            }
            Append(sb, User, UserFieldLength, false);
            Append(sb, ScoreDate, DateFieldLength, true);
            Append(sb, Score.ToString(), ScoreFieldLength, true);
            return sb.ToString();
        }


        private void Append(StringBuilder sb, string data, int fieldLength, bool rightAlign)
        {
            if (rightAlign)
            {
                for (uint i = 0; i < fieldLength - data.Length; i++)
                {
                    sb.Append(" ");
                }
                sb.Append(data);
            }
            else
            {
                sb.Append(data);
                for (uint i = 0; i < fieldLength - data.Length; i++)
                {
                    sb.Append(" ");
                }
            }
        }

    }
}
