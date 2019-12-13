using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using APTSPropricerApi.Connection;

namespace UnitTestProject
{
    [TestClass]
    public class ResourceSummaryDefinitionsControllerTest
    {
        static PoolManager poolManager = PoolManager.Instance;
        static IProPricerConnection ppc;

        [ClassInitialize]
        static public void ClassInit(TestContext testContext)
        {
        }

        [TestInitialize]
        public void TestInit()
        {
            ppc = new ProPricerConnection();
            if (ppc.workspace != null)
            {
                poolManager.AddObject(ppc);
                System.Diagnostics.Debug.WriteLine("Success");
            }
        }

        [TestMethod]
        public void Get_Resource_Summary_Definitions()
        {
            /// Arrange
            ResourceSummaryDefinitionsController controller = new ResourceSummaryDefinitionsController();

            /// Act
            IEnumerable<ResourceSummaryDefinitionsDto> resSumDefs = controller.Get();

            /// Assert
            int count = 0;
            using (IEnumerator<ResourceSummaryDefinitionsDto> enumerator = resSumDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    count++;
            }

            Assert.IsTrue(count > 0);
        }

        [TestCleanup]
        public void Dispose()
        {
            poolManager.ReleaseObject(ppc);
        }

    }
}
