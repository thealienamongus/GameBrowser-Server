using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Entities
{
    class Vic20Game : Game
    {
        public override string DisplayMediaType
        {
            get { return "Commodore Vic-20"; }
        }
    }
}
