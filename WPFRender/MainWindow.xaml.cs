using ModelObjective;
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
        Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();

            init();
        }

        public void init()
        {
            world = new World();
            worldController = new WorldController(world);

            worldParams.world = world;

            worldElem = new WorldElement(canvasWorld);
            canvasWorld.Children.Add(worldElem);
            worldElem.setWorld(world);
            
            for (int i = 0; i < 1; i++)
                worldController.addRandomZoaInArea(rnd, 0, 0, (int)world.rightLifeBorder, (int)world.bottomLifeBorder);

            worldController.Resume();

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
