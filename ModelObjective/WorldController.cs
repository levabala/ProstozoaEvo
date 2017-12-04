﻿using System;
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
        public static double DEFAULT_MOVE_TIMER_INTERVAL = 8;        
        public static double DEFAULT_CONTROL_TIMER_INTERVAL = 5;
        public static double DEFAULT_FOOD_TIMER_INTERVAL = 16;        

        World world;
        Stopwatch moveWatch = new Stopwatch();
        Stopwatch foodWatch = new Stopwatch();
        Stopwatch controlWatch = new Stopwatch();
        Timer moveTimer = new Timer();
        Timer controlTimer = new Timer();
        Timer foodTimer = new Timer();

        public double MoveTickLength
        {
            get { return moveTimer.Interval; }
            set
            {
                moveTimer.Interval = value;
            }
        }

        public double ControlTickLength
        {
            get { return controlTimer.Interval; }
            set
            {
                controlTimer.Interval = value;
            }
        }

        public double FoodSpawnTickLength
        {
            get { return foodTimer.Interval; }
            set
            {
                foodTimer.Interval = value;
            }
        }

        object locker = new object();
        object locker2 = new object();
        public WorldController(World world)
        {
            this.world = world;

            moveTimer.Interval = DEFAULT_MOVE_TIMER_INTERVAL;
            controlTimer.Interval = DEFAULT_CONTROL_TIMER_INTERVAL;
            foodTimer.Interval = DEFAULT_FOOD_TIMER_INTERVAL;

            moveTimer.Elapsed += (a, b) =>
            {
                moveWatch.Stop();
                double time = (double)moveWatch.Elapsed.Milliseconds / 1000;

                time = world.multiplyTimeDueToSimSpeed(time);

                lock (locker)
                    world.MoveTick(time);

                moveWatch.Restart();
            };

            controlTimer.Elapsed += (a, b) =>
            {

                moveWatch.Stop();
                double time = (double)moveWatch.Elapsed.Milliseconds / 1000;

                time = world.multiplyTimeDueToSimSpeed(time);

                lock (locker)
                    world.ControlTick(time);

                moveWatch.Restart();                
            };

            foodTimer.Elapsed += (a, b) =>
            {
                foodWatch.Stop();
                double time = (double)foodWatch.Elapsed.Milliseconds / 1000;

                time = world.multiplyTimeDueToSimSpeed(time);

                lock (locker2)
                    world.FoodTick(time);

                foodWatch.Restart();
            };
        }

        public void Resume()
        {
            moveTimer.Start();
            //controlTimer.Start();
            foodTimer.Start();

            moveWatch.Start();
            foodWatch.Start();
        }

        public void Pause()
        {
            moveTimer.Stop();
            controlTimer.Stop();
            foodTimer.Stop();

            moveWatch.Stop();
            foodWatch.Stop();
        }

        public void addRandomZoaInArea(Random rnd, int left, int top, int right, int bottom)
        {
            Prostozoa zoa = World.generateRandomZoa(rnd, left, top, right, bottom);            
            world.addZoa(zoa);
        }
    }
}
