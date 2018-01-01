using MathAssembly;
using PointsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Model
{
    public class WorldRenderer
    {
        public World world;

        public double foodScale = 3;
        public double foodMaxDensity = Math.Pow(3, 3);
        public double zoaScale = 1;
        public double surfaceOpacity = 0.5;

        public int maxPartiesRendered = 1000;

        public Dictionary<SourceType, double> sourcesCoeffs = new Dictionary<SourceType, double>()
        {
            {SourceType.Toxicity, 1},
            {SourceType.Fertility, 0.3 },
            {SourceType.Viscosity, 1 },
            {SourceType.Fire, 0},
            {SourceType.Grass, 0 },
            {SourceType.Ocean, 0 }
        };

        //my brushes, pens
        Dictionary<SourceType, RadialGradientBrush> sourceBrushes = new Dictionary<SourceType, RadialGradientBrush>()
        {
            {SourceType.Toxicity, new RadialGradientBrush(Colors.LightPink, Colors.Transparent) },
            {SourceType.Fertility, new RadialGradientBrush(Colors.DarkGreen, Colors.Transparent) },
            {SourceType.Viscosity, new RadialGradientBrush(Colors.SandyBrown, Colors.Transparent) },
            {SourceType.Fire, new RadialGradientBrush(Colors.Firebrick, Colors.Transparent) },
            {SourceType.Grass, new RadialGradientBrush(Colors.DarkSeaGreen, Colors.Transparent) },
            {SourceType.Ocean, new RadialGradientBrush(Colors.DeepSkyBlue, Colors.Transparent) }
        };        

        public WorldRenderer(World world)
        {
            this.world = world;            
        }

        int checkTimes = 50;
        Stopwatch calcWatch = new Stopwatch();
        List<double> calcTimes = new List<double>();
        Stopwatch renderWatch = new Stopwatch();
        List<double> renderTimes = new List<double>();
        public ColoredGeometry[] GetGeometries(Point leftTopView, Point rightBottomView, Window window)//, Matrix m)
        {						
            calcWatch.Restart();
            lock (world.tickLocker)
            {                
                DinamicPointsSet[] setsToDraw =
                        world.pointsManager.getPointsSets(
                            leftTopView.X, rightBottomView.X, leftTopView.Y, rightBottomView.Y,
                            maxPartiesRendered);
                int clustersDrawed = 0;
                int totalElements = 0;
                foreach (Cluster c in world.pointsManager.clusters)                
                    if (c.idX >= world.pointsManager.li && c.idX <= world.pointsManager.ri && c.idY >= world.pointsManager.ti && c.idY <= world.pointsManager.bi)
                    {
                        clustersDrawed++;
                        totalElements += c.points.Count;
                        /*Geometry g = new RectangleGeometry(
                                    new Rect(
                                        new Point(c.x, c.y),
                                        new Point(c.x + c.size, c.y + c.size))
                                );
                        g.Freeze();
                        Color color = Colors.DarkGreen;
                        ColoredGeometry cg = new ColoredGeometry(g, null, color);
                        coloredGeometry.Add(cg);*/
                    }				
                //List<Protozoa> zoasToDraw = new List<Protozoa>();
                //List<SourcePoint> sourcePointsToDraw = new List<SourcePoint>();

				ColoredGeometry[] coloredGeometry = new ColoredGeometry[setsToDraw.Length];
				for (int i = 0; i < setsToDraw.Length; i++)
				{
					DinamicPointsSet set = setsToDraw[i];
					switch (set.type)
					{
						case World.ZoaType:
							break;
						case World.FoodType:							
							double size = (set.joinDist) / 2;
							if (size < foodScale)
								size = foodScale;
							double alpha =
								(foodScale * foodScale * set.points.Count) / //total food area 
								(Math.PI * (set.joinDist / 2) * (set.joinDist / 2));
							if (alpha > 1)
								alpha = 1;
							double fire = 0;
							double grass = 0;
							double ocean = 0;
							double toxicity = 0;
							foreach (DinamicPoint p in set.points)
							{
								Food f = world.food[p.id];
								fire += f.fire;
								grass += f.grass;
								ocean += f.ocean;
								toxicity += f.toxicity;
							}
							toxicity /= set.points.Count;
							//FoodDraw fd = new FoodDraw(set.x, set.y, alpha, size, fire, grass, ocean);
							double sum = fire + grass + ocean;

							Geometry g = new RectangleGeometry(
									new Rect(
										new Point(set.x - size / 2, set.y - size / 2),
										new Point(set.x + size / 2, set.y + size / 2))
								);
							g.Freeze();

							Color c = Color.FromArgb(
										(byte)(alpha * 255),//coeff * 255),
										(byte)(fire / sum * 255),
										(byte)(grass / sum * 255),
										(byte)(ocean / sum * 255));
							ColoredGeometry cg = new ColoredGeometry(g, c, null);
							coloredGeometry[i] = cg;
							break;
					}
				}

                calcTimes.Add(calcWatch.ElapsedMilliseconds);
                if (calcTimes.Count > checkTimes)
                    calcTimes.RemoveAt(0);

                int calcTime = 0;
                foreach (double d in calcTimes)
                    calcTime += (int)d;
                calcTime /= calcTimes.Count;

                int renderTime = 0;
				lock (renderTimes)
					foreach (double d in renderTimes)
						renderTime += (int)d;
                if (renderTimes.Count > 0)
                    renderTime /= renderTimes.Count;

                try
                {
                    window.Dispatcher.Invoke(() =>
                    {
                        if (window.Title != null)
                            window.Title = String.Format(
                                "ClustersDrawed: {0}/{1}, Elements(Rendered/MaxRendered/InViewedClusters/Total): {2}/{3}/{4}/{5} " + 
								"ElapsedTime(Calc/Render/Total): {6}/{7}/{8}ms",
                                clustersDrawed, world.pointsManager.clusters.Length,
                                coloredGeometry.Length, maxPartiesRendered, totalElements, world.pointsManager.points.Count,
                                calcTime, renderTime, calcTime + renderTime);
                    });
                }
                catch (Exception e) { }

				return coloredGeometry;
            }            
        }
        

        public void Render(DrawingContext drawingContext, ColoredGeometry[] coloredGeometry, Matrix m)
        {
			//Window mainWindow = Application.Current.Windows[0];

            renderWatch.Restart();
            DrawingGroup group = new DrawingGroup();
            group.Transform = new MatrixTransform(m);            
            foreach (ColoredGeometry cg in coloredGeometry)
                group.Children.Add(
                    new GeometryDrawing(
                        (cg.brushColor == null) ? null : new SolidColorBrush((Color)cg.brushColor),
                        (cg.penColor == null) ? null : new Pen(new SolidColorBrush((Color)cg.penColor), 1), 
                        cg.g)
                    );
            group.Freeze();
            drawingContext.DrawDrawing(group);

			lock (renderTimes)
            renderTimes.Add(renderWatch.ElapsedMilliseconds);
            if (renderTimes.Count > checkTimes)
                renderTimes.RemoveAt(0);       
        }

        /*Stopwatch renderWatch = new Stopwatch();
        public void Render(DrawingContext drawingContext, Matrix matrix, Drawing[] drawings)
        {         
            
            renderWatch.Restart();
            Point zeroPoint = matrix.Transform(new Point(0, 0));            
            Action drawAll = new Action(() =>
            {                
                                                

                //firstly, let's draw the surface
                /*foreach (SourcePoint spoint in sourcePointsToDraw)
                {
                    RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                    double coeff = sourcesCoeffs[spoint.sourceType];
                    brush.Opacity = surfaceOpacity * coeff;
                    double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(spoint.distance, 0)).X);
                    drawingContext.DrawEllipse(brush, null, matrix.Transform(spoint.location.toPoint()), radius * 2, radius * 2);
                    //drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.3 }, 1), matrix.Transform(spoint.location.toPoint()), radius, radius);
                }//

                //now food                  
                foreach (FoodDraw f in foodToDraw)
                {                    
                    double radius = f.size;
                    Point center = new Point(f.x - radius, f.y - radius);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double width = Math.Abs(zeroX - matrix.Transform(new Point(radius * 2, 0)).X);

                    Pen pen = new Pen(Brushes.Black, 1);                   
                    SolidColorBrush brush = new SolidColorBrush(
                        Color.FromArgb(
                            (byte)(f.alpha * 255),//coeff * 255),
                            (byte)(f.fire * 255), 
                            (byte)(f.grass * 255), 
                            (byte)(f.ocean * 255)));
                    drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
                }
                
                Window mainWindow = Application.Current.Windows[0];
                mainWindow.Title = String.Format(
                    "ClustersDrawed: {0}/{1}, ZoaDrawed: {2}/{3}, FoodDrawed: {4}/{5}, " +
                    "SourcePointsDrawed: {6}/{7}, MaxParties: {8}, MinLayerId: {9}, RenderTime: {10}ms", 
                    clustersDrawed, world.pointsManager.clusters.Length,
                    0,0,//zoasToDraw.Count, world.protozoas.Count,
                    foodToDraw.Count, world.food.Count,
                    0,0,//sourcePointsToDraw.Count, world.surface.sourcePoints.Count,
                    maxPartiesRendered,
                    world.pointsManager.minLayerId,
                    renderWatch.ElapsedMilliseconds);
            });

            lock (world.tickLocker)
                drawAll();            
        }*/

        /*private struct FoodDraw
        {
            public double alpha, size, fire, grass, ocean, x, y;                        
            public FoodDraw(Food f, double alpha)
            {
                this.alpha = alpha;
                x = f.point.x;
                y = f.point.y;
                size = f.size;
                fire = f.fireRate;
                grass = f.grassRate;
                ocean = f.oceanRate;
            }

            public FoodDraw(double x, double y, double alpha, double size, double fire, double grass, double ocean)
            {
                this.x = x;
                this.y = y;
                this.alpha = alpha;
                this.size = size;
                double sum = fire + grass + ocean;
                this.fire = fire / sum;
                this.grass = grass / sum;
                this.ocean = ocean / sum;
            }
        }*/        

        private struct FoodDraw
        {
            public double alpha, size, fire, grass, ocean, x, y;
            public FoodDraw(Food f, double alpha)
            {
                this.alpha = alpha;
                x = f.point.x;
                y = f.point.y;
                size = f.size;
                fire = f.fireRate;
                grass = f.grassRate;
                ocean = f.oceanRate;
            }

            public FoodDraw(double x, double y, double alpha, double size, double fire, double grass, double ocean)
            {
                this.x = x;
                this.y = y;
                this.alpha = alpha;
                this.size = size;
                double sum = fire + grass + ocean;
                this.fire = fire / sum;
                this.grass = grass / sum;
                this.ocean = ocean / sum;
            }
        }

        private struct ZoaDraw
        {

        }

        private struct SourceDraw
        {

        }

        //without-clustering render
        /*public void Render(DrawingContext drawingContext, Matrix matrix)
        {
            Point zeroPoint = matrix.Transform(new Point(0, 0));

            //firstly, let's draw the surface
            foreach (SourcePoint spoint in world.surface.sourcePoints)
            {
                RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                double coeff = sourcesCoeffs[spoint.sourceType];
                brush.Opacity = surfaceOpacity * coeff;                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(spoint.distance, 0)).X);
                drawingContext.DrawEllipse(brush, null, matrix.Transform(spoint.location.toPoint()), radius * 2, radius * 2);                
                drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.3 }, 1), matrix.Transform(spoint.location.toPoint()), radius, radius);

                double penWidth = 30;
                double penRadius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(penWidth, 0)).X);
                //drawingContext.DrawEllipse(null, new Pen(brush, penWidth), matrix.Transform(spoint.location.toPoint()), radius, radius);
            }

            //now food
            lock(world.food)
                foreach (Food f in world.food.Values)
                {                    
                    Point center = new Point(f.point.x, f.point.y);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double width = Math.Abs(zeroX - matrix.Transform(new Point(foodScale, 0)).X);

                    Pen pen = new Pen(Brushes.Black, 1);
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)(f.fireRate * 255), (byte)(f.grassRate * 255), (byte)(f.oceanRate * 255)));
                    drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
                }

            //now zoas' view fields
            foreach (Protozoa zoa in world.protozoas.Values)
            {
                StreamGeometry viewZoneGeometry = new StreamGeometry();
                using (StreamGeometryContext ctx = viewZoneGeometry.Open())
                {
                    ctx.BeginFigure(matrix.Transform(zoa.centerP.toPoint()), true, true);
                    ctx.LineTo(matrix.Transform(zoa.leftViewP.toPoint()), true, false);
                    ctx.LineTo(matrix.Transform(zoa.rightViewP.toPoint()), true, false);
                }
                viewZoneGeometry.Freeze();
                Brush viewBrush = new SolidColorBrush(Colors.Azure)
                {
                    Opacity = 0
                };
                viewBrush.Freeze();
                Pen pen = new Pen(new SolidColorBrush(Colors.Black)
                {
                    Opacity = 0.1
                }, 1);
                pen.Freeze();
                drawingContext.DrawGeometry(viewBrush, pen, viewZoneGeometry);
            }

            //now zoas
            foreach (Protozoa zoa in world.protozoas.Values)
            {
                Color color = zoa.zoaColor.toColor();
                SolidColorBrush brush = new SolidColorBrush(color);
                Pen pen = new Pen(brush, 1);
                brush.Opacity = zoa.energy / zoa.energyCapacity;
                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(zoa.radius, 0)).X);
                Point translatedCenter = matrix.Transform(zoa.centerP.toPoint());
                drawingContext.DrawEllipse(brush, pen, translatedCenter, radius, radius);
            }
        }*/
    }

    public struct ColoredGeometry
    {
        public Geometry g;
        public Color? brushColor, penColor;
        public ColoredGeometry(Geometry g, Color? brushColor, Color? penColor)
        {
            this.g = g;
            this.brushColor = brushColor;
            this.penColor = penColor;
        }
    }
}
