using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii.DataTypes
{
    public class BrushModel
    {
        public Vector3 Origin;
        public List<Poly> Polys = new List<Poly>();

        // Set the origin for this model
        // TODO: Add suport for other orign points
        public void SetOrigin()
        {
            var tempOrigin = Vector3.Zero;
            var verts      = Polys.SelectMany(x => x.Verts);
            foreach (var vert in verts)
            {
                switch (Config.Conversion.ModelOrignPosition)
                {
                    case ModelOriginPos.Center:
                        tempOrigin += vert.Pos;
                        break;
                    default:
                        break;
                }
            }

            switch (Config.Conversion.ModelOrignPosition)
            {
                case ModelOriginPos.Center:
                    Origin = tempOrigin / verts.Count();
                    break;
                default:
                    break;
            }
        }
    }
}
