using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointsManager;
using Model;
using System.Collections.Generic;

namespace UnitTestModel
{
    [TestClass]
    public class UnitTestPointsManager
    {
        [TestMethod]
        public void TestMethod1()
        {
            Dictionary<int, AnEntity> entities = new Dictionary<int, AnEntity>()
            {
                {0, new AnEntity(new Pnt(5, 5), 3, 0) },
                {1, new AnEntity(new Pnt(1, 9), 2, 1) },
                {3, new AnEntity(new Pnt(-8, 25), 6, 3) },
            };
            DynamicPointsManager manager = new DynamicPointsManager(new Pnt(0,0), 10);
            foreach (AnEntity enti in entities.Values)
                manager.addPoint(enti.p, enti.interactArea, enti.id);

        }

        struct AnEntity
        {
            public Pnt p;
            public int id;
            public double interactArea;
            public AnEntity(Pnt p, double interactArea, int id)
            {
                this.p = p;
                this.interactArea = interactArea;
                this.id = id;
            }
        }
    }
}
