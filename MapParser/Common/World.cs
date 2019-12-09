using System;
using System.Collections.Generic;
using System.Text;

namespace MapParser.Common
{
    public class World
    {
        public Game GameType;
        public List<Entity> Entitys = new List<Entity>();
        public List<string> Wads = new List<string>();
    }

    public enum Game
    {
        Quake1,
        Halflife
    }
}
