using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace ModelObjective
{
    public class WorldController
    {        
        World world;        
        Stopwatch worldWatch = new Stopwatch();
        Timer worldTimer = new Timer(8);        

        object locker = new object();        
        public WorldController(World world)
        {
            this.world = world;
            
            worldTimer.Elapsed += (a, b) =>
            {
                lock (locker)
                {
                    worldWatch.Stop();
                    double time = worldWatch.Elapsed.TotalMilliseconds / 1000;
                    time = world.multiplyTimeDueToSimSpeed(time);

                    world.WorldTick(time);
                    worldWatch.Restart();
                }                
            };            
        }

        public void Resume()
        {            
            worldTimer.Start();
            worldWatch.Start();            
        }

        public void Pause()
        {            
            worldTimer.Stop();            
            worldWatch.Stop();
        }

        public void addRandomZoaInArea(Random rnd, int left, int top, int right, int bottom)
        {
            Prostozoa zoa = World.generateRandomZoa(rnd, left, top, right, bottom);            
            world.addZoa(zoa);
        }
    }
}
