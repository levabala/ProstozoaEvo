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
    /// Interaction logic for WorldParams.xaml
    /// </summary>
    public partial class WorldParams : UserControl
    {
        World w = new World();
        public World world
        {
            get { return w; }
            set { w = value; }
        }
        public WorldParams()
        {            
            InitializeComponent();

            labelSpeedRate.Content = world.SimSpeed;
            sliderSpeedRate.ValueChanged += SliderSpeedRate_ValueChanged;
        }

        private void SliderSpeedRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            world.SimSpeed = Math.Round(Math.Pow(sliderSpeedRate.Value, 4), 1);
            labelSpeedRate.Content = world.SimSpeed;
        }
    }
}
