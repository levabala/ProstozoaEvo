using BillionPointsManager;
using MathAssembly;
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

        //Dictionary<Guid, uint> setsHashes = new Dictionary<Guid, uint>();
        //Dictionary<Guid, ColoredGeometry> geometriesCache = new Dictionary<Guid, ColoredGeometry>();
        //Dictionary<ColoredGeometry, GeometryDrawing> drawingsCache = new Dictionary<ColoredGeometry, GeometryDrawing>();

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

        int checkTimes = 10;
        Stopwatch calcWatch = new Stopwatch();
        Stopwatch calcOtherWatch = new Stopwatch();
        Stopwatch calcOtherOtherWatch = new Stopwatch();
        Stopwatch renderWatch = new Stopwatch();        
        List<double> calcTimes = new List<double>();
        List<double> calcGetSetsTimes = new List<double>();
        List<double> calcIterateClustersTimes = new List<double>();
        List<double> calcGeometryTimes = new List<double>();        

        List<double> foodGetTimes = new List<double>();
        List<double> createGeometryTimes = new List<double>();
        List<double> renderTimes = new List<double>();

        
        public PointSet[] GetSetsToDraw(Point leftTopView, Point rightBottomView, Window window)//, Matrix m)
        {						            
            lock (world.tickLocker)
            {
                calcOtherWatch.Restart();
                calcWatch.Restart();                
                PointSet[] setsToDraw =
                        world.pointsManager.getPointsSets(
                            leftTopView.X, rightBottomView.X, leftTopView.Y, rightBottomView.Y,
                            maxPartiesRendered);
                calcGetSetsTimes.Add(calcOtherWatch.ElapsedMilliseconds);

                calcOtherWatch.Restart();
                int clustersDrawed = 0;
                int totalElements = 0;
                foreach (Cluster c in world.pointsManager.clusters)                
                    if (c != null && c.idX >= world.pointsManager.li && c.idX <= world.pointsManager.ri && c.idY >= world.pointsManager.ti && c.idY <= world.pointsManager.bi)
                    {
                        clustersDrawed++;
                        totalElements += c.container.Values.Count;                        
                    }
                calcIterateClustersTimes.Add(calcOtherWatch.ElapsedMilliseconds);

                calcOtherWatch.Restart();                
                int coloredGeometrySetLength = 0;
                double foodGetTime = 0;
                int foodGetCount = 0;
                double createGeometryTime = 0;
                int geometriesCreated = 0;
                int geometriesRestored = 0;
                //Parallel.For(0, coloredGeometry.Length - 1, (i) =>                
                for (int i = 0; i < setsToDraw.Length; i++)
                {
                    PointSet set = setsToDraw[i];                    
                    ColoredGeometry cg = new ColoredGeometry();

                    Object val = set.linkedObjects[CacheLastHashGeometryType];
                    uint oldHash = 0;
                    if (val != null)
                        oldHash = (uint)val;
                    uint nowHash = set.hash;
                    if (oldHash == nowHash)
                    {
                        Object val2 = set.linkedObjects[CacheColoredGeometryType];                        
                        if (val2 != null)
                        {
                            geometriesRestored++;
                            continue;
                        }                                                 
                    }
                    geometriesCreated++;
                    set.linkedObjects[CacheLastHashGeometryType] = nowHash;

                    switch (set.type)
                    {
                        case World.ZoaType:
                            break;
                        case World.FoodType:                            
                            double size = (set.joinDist * 2);
                            double alpha =
                                (world.pointsManager.lowestPointSize * world.pointsManager.lowestPointSize * set.points.Count * 4) / //total food area 
                                (Math.PI * size * size / 4);
                            if (alpha > 1)
                                alpha = 1;                            
                            coloredGeometrySetLength++;

                            double fire = 0;
                            double grass = 0;
                            double ocean = 0;
                            double toxicity = 0;
                            foreach (StaticPoint p in set.points)
                            {
                                Food f = (Food)p.linkedObjects[World.WorldObjectType];//world.food[p.id];
                                fire += f.fire;
                                grass += f.grass;
                                ocean += f.ocean;
                                toxicity += f.toxicity;
                            }                            
                            toxicity /= set.points.Count;

                            /*Geometry g = new RectangleGeometry(
                                    new Rect(
                                        new Point(set.x - size / 2, set.y - size / 2),
                                        new Point(set.x + size / 2, set.y + size / 2))
                                );*/
                            Geometry g = new EllipseGeometry(
                                new Rect(set.x - size / 2, set.y - size / 2, size, size)                                    
                            );
                            g.Freeze();

                            double sum = fire + grass + ocean;
                            Color c = Color.FromArgb(
                                        (byte)(alpha * 255),//coeff * 255),
                                        (byte)(fire / sum * 255),
                                        (byte)(grass / sum * 255),
                                        (byte)(ocean / sum * 255));
                            cg = new ColoredGeometry(g, c, null);                            
                            break;
                    }
                    set.linkedObjects[CacheColoredGeometryType] = cg;
                }//);
                createGeometryTime /= coloredGeometrySetLength;
                foodGetTime /= foodGetCount;
                foodGetTimes.Add(foodGetTime);
                createGeometryTimes.Add(createGeometryTime);
                calcGeometryTimes.Add(calcOtherWatch.ElapsedMilliseconds);

                calcTimes.Add(calcWatch.ElapsedMilliseconds);

                if (calcTimes.Count > checkTimes)
                    calcTimes.RemoveAt(0);
                if (calcGetSetsTimes.Count > checkTimes)
                    calcGetSetsTimes.RemoveAt(0);
                if (calcIterateClustersTimes.Count > checkTimes)
                    calcIterateClustersTimes.RemoveAt(0);
                if (calcGeometryTimes.Count > checkTimes)
                    calcGeometryTimes.RemoveAt(0);                

                int calcTime = 0;
                foreach (double d in calcTimes)
                    calcTime += (int)d;
                calcTime /= calcTimes.Count;

                int calcGetSetsTime = 0;
                foreach (double d in calcGetSetsTimes)
                    calcGetSetsTime += (int)d;
                calcGetSetsTime /= calcGetSetsTimes.Count;

                int calcIterateClustersTime = 0;
                foreach (double d in calcIterateClustersTimes)
                    calcIterateClustersTime += (int)d;
                calcIterateClustersTime /= calcIterateClustersTimes.Count;

                int calcGeometryTime = 0;
                foreach (double d in calcGeometryTimes)
                    calcGeometryTime += (int)d;
                calcGeometryTime /= calcGeometryTimes.Count;

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
                                "Clusters: {14}x{15}x{16} isGeneratingLayer: {17}|{18}%" + 
                                " ClustersDrawed: {0}/{1}, Elements(Rendered/MaxRendered/InViewedClusters/Total): {2}/{3}/{4}/{5} " +
                                //"ElapsedTime: CalcSets/CalcClusters/CalcGeometry/CalcAll {6}/{7}/{8}/{9}ms" + 
                                " Render {10}/Total {11}ms Geometries(Restored/Created): {12}/{13} FixedLayer: {19}",
                                //" CacheSize(CG/Drawings): {14}/{15}",
                                clustersDrawed, world.pointsManager.clusters.Length,
                                setsToDraw.Length, maxPartiesRendered, totalElements, world.pointsManager.pointsCount,
                                calcGetSetsTime, calcIterateClustersTime, calcGeometryTime,
                                calcTime, renderTime, calcTime + renderTime,
                                geometriesRestored, geometriesCreated,
                                world.pointsManager.clusters.GetLength(0), world.pointsManager.clusters.GetLength(1), world.pointsManager.clusters.GetLength(2),
                                world.pointsManager.isGeneratingLayer, (int)(world.pointsManager.layerGeneratingProgress * 100),
                                world.pointsManager.fixedLayerId);//,
                                //geometriesCache.Count, drawingsCache.Count);
                    });
                }
                catch (Exception e) { }

				return setsToDraw;
            }            
        }
        
        public void Render(DrawingContext drawingContext, PointSet[] setsToDraw, Matrix m)
        {			
            renderWatch.Restart();
            DrawingGroup group = new DrawingGroup();
            group.Transform = new MatrixTransform(m);            

            foreach (PointSet set in setsToDraw)
            {
                Object drawingObj = set.linkedObjects[CacheDrawingsType];
                GeometryDrawing drawing;

                Object val = set.linkedObjects[CacheLastHashDrawingType];
                uint oldHash = 0;
                if (val != null)
                    oldHash = (uint)val;
                uint nowHash = set.hash;

                if (drawingObj != null && oldHash == nowHash)
                    drawing = drawingObj as GeometryDrawing;
                else
                {
                    ColoredGeometry cg = (ColoredGeometry)set.linkedObjects[CacheColoredGeometryType];
                    drawing = new GeometryDrawing(
                            (cg.brushColor == null) ? null : new SolidColorBrush((Color)cg.brushColor),
                            (cg.penColor == null) ? null : new Pen(new SolidColorBrush((Color)cg.penColor), 1),
                            cg.g);                    
                    set.linkedObjects[CacheDrawingsType] = drawing;
                }
                set.linkedObjects[CacheLastHashDrawingType] = nowHash;

                drawing.Freeze();                
                group.Children.Add(drawing);                
            }
            //group.Freeze();


            foreach (Cluster c in world.pointsManager.clusters)
            {
                if (c != null && c.idZ == -1)// && c.idX >= world.pointsManager.li && c.idX <= world.pointsManager.ri && c.idY >= world.pointsManager.ti && c.idY <= world.pointsManager.bi)
                {
                    Geometry g = new RectangleGeometry(
                                new Rect(
                                    new Point(c.x, c.y),
                                    new Point(c.x + c.size, c.y + c.size))
                            );
                    g.Freeze();
                    Color color = Colors.DarkGreen;
                    group.Children.Add(
                        new GeometryDrawing(null, new Pen(new SolidColorBrush(color), c.idZ * 10 + 1), g)
                        );
                }
            }

            group.Children.Add(
                new GeometryDrawing(
                    Brushes.Black, null, new EllipseGeometry(new Point(0, 0), 20, 20)
                    )
                    );

            drawingContext.DrawDrawing(group);

            lock (renderTimes)
            {
                renderTimes.Add(renderWatch.ElapsedMilliseconds);
                if (renderTimes.Count > checkTimes)
                    renderTimes.RemoveAt(0);
            }
        }

        public const int CacheColoredGeometryType = 10;
        public const int CacheLastHashGeometryType = 11;
        public const int CacheLastHashDrawingType = 12;
        public const int CacheDrawingsType = 13;

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
        public int set;
        public ColoredGeometry(Geometry g, Color? brushColor, Color? penColor)
        {
            this.g = g;
            this.brushColor = brushColor;
            this.penColor = penColor;
            set = 1;
        }
    }
}
