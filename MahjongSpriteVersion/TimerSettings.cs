using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class TimerSettings
    {
        public TimerSettings()
        {
            Seconds = 0;
            BonusSeconds = 300;
            ResetSuperBonusSeconds();
        }
        public int Seconds { get; set; }
        public int BonusSeconds { get; set; }
        public int SuperBonusSeconds { get; set; }

        public void ResetSuperBonusSeconds()
        {
            SuperBonusSeconds = 30;
        }

        public string ClockText
        {
            get
            {
                string hours = Seconds / 3600 > 1 ? (Seconds / 3600).ToString() + ":" : string.Empty;
                string minutes = (Seconds / 60) % 60 > 1 ? ((Seconds / 60) % 60).ToString() : "0" + ((Seconds / 60) % 60).ToString();
                string secs = (Seconds % 60).ToString().Length == 1 ? "0" + (Seconds % 60).ToString() : (Seconds % 60).ToString();
                return hours.ToString() + minutes.ToString() + ":" + secs.ToString();
            }
        }
    }
}
