using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Model
{
    public class WorldController
    {
        Random rnd = new Random();

        World world;
        Stopwatch sw = new Stopwatch();
        Timer timer = new Timer(8);

        public WorldController(World world)
        {
            this.world = world;
            timer.Elapsed += (a, b) =>
            {
                sw.Stop();
                timer.Stop();
                //Console.WriteLine("WorldTick Start");
                world.WorldTick(world.multipleTime(sw.ElapsedMilliseconds));
                //Console.WriteLine("WorldTick End");
                timer.Start();
                sw.Restart();
            };            
        }

        public void Resume()
        {
            sw.Start();
            timer.Start();
        }

        public void Pause()
        {
            sw.Stop();
            timer.Stop();
        }

        public void addNewZoa()
        {
            world.addZoa(50);
        }

        public void addSource(SourceType stype, double dist)
        {
            SourcePoint spoint = new SourcePoint(world.surface.getRandomPoint(rnd, (int)dist), stype, rnd);
            world.surface.addSourcePoint(spoint);
        }
    }
}
