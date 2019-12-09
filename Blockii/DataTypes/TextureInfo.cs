using System;
using System.Collections.Generic;
using System.Text;

namespace Blockii.DataTypes
{
    public class TextureInfo
    {
        public int Width;
        public int Height;
        public string FileName;
        public ushort Id;
        public bool Exclude; // if true faces with this texture won't be exported
    }
}
