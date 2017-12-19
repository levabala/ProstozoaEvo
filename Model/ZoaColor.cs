using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Model
{
    public class ZoaColor : HSLColor
    {
        public static double scale = 5;
        public ZoaColor(double value)
            : base(value / scale, 1, 0.5)
        {

        }
            
        public Color toColor()
        {
            string rgb = ToRGBString();
            System.Drawing.Color c = this;

            return new Color()
            {
                A = c.A,
                R = c.R,
                G = c.G,
                B = c.B,
            };
        }
    }
}
