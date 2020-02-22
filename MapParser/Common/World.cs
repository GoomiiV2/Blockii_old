using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapParser.Common
{
    public class World
    {
        public Game GameType;
        public List<Entity> Entitys = new List<Entity>();
        public List<string> Wads = new List<string>();

        public Entity WorldspawnGetter => Entitys.FirstOrDefault(x => x.KeyValues["CLASSNAME"].ToUpper() == "WORLDSPAWN");
    }

    public enum Game
    {
        Quake1,
        Halflife
    }
}
