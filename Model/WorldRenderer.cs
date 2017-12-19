using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Model
{
    public class WorldRenderer
    {
        public World world;

        public double foodScale = 3;
        public double zoaScale = 1;
        public double surfaceOpacity = 0.2;

        public Dictionary<SourceType, double> sourcesCoeffs = new Dictionary<SourceType, double>()
        {
            {SourceType.Toxicity, 10},
            {SourceType.Fertility, 10 },
            {SourceType.Viscosity, 10 }
        };

        //my brushes, pens
        Dictionary<SourceType, RadialGradientBrush> sourceBrushes = new Dictionary<SourceType, RadialGradientBrush>()
        {
            {SourceType.Toxicity, new RadialGradientBrush(Colors.LightPink, Colors.Transparent) },
            {SourceType.Fertility, new RadialGradientBrush(Colors.DarkGreen, Colors.Transparent) },
            {SourceType.Viscosity, new RadialGradientBrush(Colors.SandyBrown, Colors.Transparent) }
        };        

        public WorldRenderer(World world)
        {
            this.world = world;            
        }
        public void Render(DrawingContext drawingContext)
        {
            //firstly, let's draw the surface
            foreach (SourcePoint spoint in world.surface.sourcePoints)
            {
                RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                double coeff = sourcesCoeffs[spoint.sourceType];
                brush.Opacity = surfaceOpacity;// * spoint.strength;
                double radius = spoint.strength * coeff;
                drawingContext.DrawEllipse(brush, null, spoint.location.toPoint(), radius, radius);
            }

            //now food

            //now zoas' view fields

            //now zoas
        }
    }
}
