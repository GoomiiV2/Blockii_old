using System;
using System.Collections.Generic;
using System.Text;

namespace MapParser.Common
{
    public partial class Entity
    {
        public List<Brush> Brushes = new List<Brush>();
        public Dictionary<string, string> KeyValues = new Dictionary<string, string>();
    }
}
