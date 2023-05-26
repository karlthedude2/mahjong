using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class Action
    {
        public Layout Layout { get; set; }
        public TileStyles TileStyles { get; set; }

        public int ScoreFromAction { get; set; }
        public int BonusFromAction { get; set; }

    }
}
