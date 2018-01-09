using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFRendererModel2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            init();
        }

        private void init()
        {            
            Random rnd = new Random();
            World world = new World();
            WorldRenderer worldRenderer = new WorldRenderer(world);
            WorldElement worldElement = new WorldElement(mainCanvas);
            WorldController worldController = new WorldController(world);

            myWindow.KeyUp += (a, b) =>
            {
                switch (b.Key)
                {
                    case Key.OemPlus:
                        worldRenderer.maxPartiesRendered = (int)(worldRenderer.maxPartiesRendered * 1.5);
                        break;                   
                    case Key.OemMinus:
                        worldRenderer.maxPartiesRendered = (int)(worldRenderer.maxPartiesRendered / 1.5);
                        break;
                    case Key.O:
                        world.pointsManager.fixedLayerId++;
                        break;
                    case Key.L:
                        if (world.pointsManager.fixedLayerId >= 0)
                            world.pointsManager.fixedLayerId--;
                        break;
                }
            };

            worldElement.setWorldRenderer(worldRenderer);
            mainCanvas.Children.Add(worldElement);

            for (int i = 0; i < 100; i++)
            {
                worldController.addSource(SourceType.Fire, 700);
                worldController.addSource(SourceType.Fertility, 100);
                worldController.addSource(SourceType.Grass, 700);
                worldController.addSource(SourceType.Fertility, 100);
                worldController.addSource(SourceType.Ocean, 700);
                worldController.addSource(SourceType.Fertility, 100);
            }

            int count = 500000; //one million points! (no)
            mainProgressBar.Value = 0;
            new Task(() =>
            {        
                while (world.food.Count < count)
                {
                    lock (world.tickLocker)
                        world.FoodTick(100);
                    onUI(() => {
                        mainProgressBar.Value = (double)world.food.Count / count;                        
                    });
                }                
                onUI(() => mainProgressBar.Value = 1);
            }).Start();

            //worldController.Resume();
        }

        private void onUI(Action act)
        {
            try
            {
                Dispatcher.Invoke(act);
            }
            catch(Exception e)
            {

            }
        }

        private delegate void NoArgDelegate();    
        public static void Refresh(DependencyObject obj)
        {
            obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                (NoArgDelegate)delegate { });
        }
    }
}
