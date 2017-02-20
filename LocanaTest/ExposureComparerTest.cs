using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using System.Collections.Generic;
using Locana.Utility;
using System.Diagnostics;

namespace LocanaTest
{
    [TestClass]
    public class UnitTest1
    {
        // Tuple(BeforeSort, Expected)
        List<Tuple<Capability<string>, Capability<string>>> ExposureTestCapabilities = new List<Tuple<Capability<string>, Capability<string>>>()
        {
            new Tuple<Capability<string>, Capability<string>>(
                new Capability<string>()
                {
                    Current =ExposureMode.Manual,
                    Candidates = new List<string>()
                    {
                        ExposureMode.Program,
                        ExposureMode.Superior,
                        ExposureMode.Aperture,
                        ExposureMode.SS,
                    } },
                new Capability<string>() {
                    Current=ExposureMode.Manual,
                    Candidates=new List<string>() {
                        ExposureMode.Program,
                        ExposureMode.Aperture,
                        ExposureMode.SS,
                        ExposureMode.Superior,
                    }
                }),
            new Tuple<Capability<string>, Capability<string>>(
                new Capability<string>()
                {
                    Current=ExposureMode.Superior,
                    Candidates=new List<string>()
                    {
                        ExposureMode.Program,
                        "Unknown",
                        ExposureMode.Superior
                    }
                },
                new Capability<string>() {
                    Current=ExposureMode.Superior,
                    Candidates=new List<string>() {
                        ExposureMode.Program,
                        ExposureMode.Superior,
                        "Unknown",
                    }
                }),
        };

        [TestMethod]
        public void TestExposureComparer()
        {
            foreach (var t in ExposureTestCapabilities)
            {
                var orig = t.Item1;
                var expect = t.Item2;
                Assert.IsFalse(orig.Equals(expect));
                Assert.IsTrue(orig != expect);
                Assert.IsFalse(orig == expect);
                orig.Candidates.Sort(ExposureModeComparer.INSTANCE);
                Assert.IsTrue(orig.Equals(expect));
                Assert.IsTrue(orig == expect);
                Assert.IsFalse(orig != expect);
            }
        }

        [TestMethod]
        public void TestCapabilityOperators()
        {
            var c1 = new Capability<string>
            {
                Current = "a",
                Candidates = new List<string>() { "a", "b", "c" }
            };
            var c2 = new Capability<string>()
            {
                Current = "a",
                Candidates = new List<string>() { "a", "b", "c", "d" }
            };

            Assert.IsTrue(c1 != null);
            Assert.IsFalse(c1 == null);
            Assert.IsTrue(null != c1);
            Assert.IsFalse(null == c1);
            Assert.IsTrue(c1 == c1);
            Assert.IsFalse(c1 != c1);
            Assert.IsFalse(c1 == c2);
            Assert.IsTrue(c1 != c2);

            Assert.IsFalse(c1.Equals(null));
        }
    }
}
