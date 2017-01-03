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
                orig.Candidates.Sort(ExposureModeComparer.INSTANCE);
                Assert.IsTrue(orig.Equals(expect));
            }
        }
    }
}
