using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ModelObjective
{
    public class WorldAdjuster
    {
        public delegate void RefreshHandler();
        public event RefreshHandler OnRefresh;

        public double lowPopulation = 10;
        public double highPopulation = 14;
        public double foodAdjusting = 0.1;

        double period = 4000; //4sec
        Timer timer = new Timer(4000);
        Timer refreshTimer = new Timer(300);

        public WorldAdjuster(World world)
        {
            timer.Elapsed += (a, b) =>
            {
                timer.Interval = period / world.simSpeed;
                if (world.prostozoas.Count < lowPopulation)
                    world.FoodWeight /= 1 + foodAdjusting;
                else
                if (world.prostozoas.Count > highPopulation)
                    world.FoodWeight *= 1 - foodAdjusting;
            };
            timer.Start();

            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh();
        }
    }
}
