using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTSPropricerApi;
using APTSPropricerApi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class AppConfigTest
    {
        [TestMethod]
        public void GetBackupConfig()
        {
            try
            {
                ICollection<PoolManager> instances = PoolManager.Instances;
                Assert.AreEqual("This is the primary RMS", instances.First().FriendlyName);
                int firstInstanceId = instances.First().InstanceId;
                Assert.AreEqual("This is the primary RMS", PoolManager.GetInstance(instances.First().InstanceId).FriendlyName);
                // reload to get the backups
                SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings["UseProPricerBackup"] = "true";
                PoolManager.ResetPoolManagers();
                instances = PoolManager.Instances;

                Assert.AreEqual("This is the backup RMS", instances.First().FriendlyName);
                Assert.AreEqual(firstInstanceId, instances.First().InstanceId);
                Assert.AreEqual("This is the backup RMS", PoolManager.GetInstance(instances.First().InstanceId).FriendlyName);
            }
            finally
            {
                SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings["UseProPricerBackup"] = "false";
                PoolManager.ResetPoolManagers();
            }
        }
    }
}
