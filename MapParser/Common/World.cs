using System;
using System.Collections.Generic;
using System.Text;

namespace MapParser.Common
{
    public class World
    {
        public Game GameType;
        public List<Entity> Entitys = new List<Entity>();
    }

    public enum Game
    {
        Quake1,
        Halflife
    }
}
