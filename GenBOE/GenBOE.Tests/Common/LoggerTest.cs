using Microsoft.VisualStudio.TestTools.UnitTesting;
using IES.Common;

namespace GenBOE.Tests.Common
{
    [TestClass]
    public class LoggerTest
    {
        private Logger logger = new Logger(typeof(LoggerTest));

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void TestDebug()
        {
            logger.Debug("debug entry");
        }

        [TestMethod]
        public void TestInfo()
        {
            logger.Info("info entry");
        }

        [TestMethod]
        public void TestWarning()
        {
            logger.Warn("warn entry");
        }

        [TestMethod]
        public void TestError()
        {
            logger.Error("error entry");
        }
    }
}
