using ModelObjective;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFRender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        World world;
        WorldController worldController;
        WorldElement worldElem;
        WorldAdjuster worldAdjuster;
        Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();

            init();
        }

        public void init()
        {
            world = new World();
            world.FoodTick(2);
            worldController = new WorldController(world);
            worldElem = new WorldElement(canvasWorld);            

            Stopwatch sw = new Stopwatch();            
            List<double> buff = new List<double>();
            worldController.OnNeedInvalidate += () =>
            {
                sw.Stop();
                double mills = sw.ElapsedMilliseconds;
                buff.Add(mills);                
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        double total = 0;
                        foreach (double d in buff)
                            total += d;
                        Title = Math.Floor((total / buff.Count)).ToString();                                                

                        if (buff.Count > 4)
                            buff.RemoveAt(0);
                    });

                sw.Restart();
            };

            worldParams.world = world;
            
            canvasWorld.Children.Add(worldElem);
            worldElem.setWorld(world);
            worldElem.setController(worldController);

            for (int i = 0; i < 0; i++)
                worldController.addRandomZoaInArea(rnd, 0, 0, (int)world.rightLifeBorder, (int)world.bottomLifeBorder);            

            worldController.Resume();
            sw.Start();

            MyWindow.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space)
                    addZoa();
            };
        }
        
        private void addZoa()
        {
            worldController.addRandomZoaInArea(rnd, 0, 0, (int)world.rightLifeBorder, (int)world.bottomLifeBorder);
        }
    }
}
