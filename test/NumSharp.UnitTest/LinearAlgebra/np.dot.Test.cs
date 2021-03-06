using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NumSharp.UnitTest.LinearAlgebra
{
    /// <summary>
    /// Test concolve with standard example from 
    /// https://www.numpy.org/devdocs/reference/generated/numpy.convolve.html
    /// </summary>
    [TestClass]
    public class NdArrayDotTest 
    {
        [TestMethod]
        public void Dot0X0()
        {
            int x = 2;
            int y = 3;
            int z = np.dot(x, y);

            Assert.AreEqual(z, 6);
        }

        [TestMethod]
        public void Dot1x1()
        {
            var x = np.arange(3);
            var y = np.arange(3, 6);

            int nd3 = np.dot(x, y);
            Assert.IsTrue(nd3 == 14);
        }

        [TestMethod]
        public void Dot2x1()
        {
            var x = np.array(new int[,]
            {
                { 1, 1 },
                { 1, 2 },
                { 2, 2 },
                { 2, 3 }
            });

            var y = np.array(new int[] { 2, 3 });

            var z = np.dot(x, y);

            Assert.AreEqual(z.Data<int>(0), 5);
            Assert.AreEqual(z.Data<int>(1), 8);
            Assert.AreEqual(z.Data<int>(2), 10);
            Assert.AreEqual(z.Data<int>(3), 13);
        }

        [TestMethod]
        public void Dot2x2()
        {
            var x = np.array(new int[,]
            {
                { 3, 1 },
                { 1, 2 }
            });

            var y = np.array(new int[,]
            {
                { 2, 3 },
                { 1, 2 }
            });

            var z = np.dot(x, y);

            Assert.AreEqual(z.Data<int>(0), 7);
            Assert.AreEqual(z.Data<int>(1), 11);
            Assert.AreEqual(z.Data<int>(2), 4);
            Assert.AreEqual(z.Data<int>(3), 7);
        }
    }
}