using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class TraceReportControllerTest
    {
        [TestMethod]
        public void Get_Trace_Report()
        {
            string prid = "65e1eb88-ca1a-e711-82a5-847beb323d31";
            ICollection<TraceReportDto> tracerep;
            using (TraceReportController tcontroller = new TraceReportController())
            {

                /// Act
                tracerep = tcontroller.Get(TestConstants.InstanceId, prid).ToList();
            }

            /// Assert
            int count = 0;
            using (IEnumerator<TraceReportDto> enumerator = tracerep.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    count++;
                }
            }

            Assert.IsTrue(count > 0);
        }
    }
}