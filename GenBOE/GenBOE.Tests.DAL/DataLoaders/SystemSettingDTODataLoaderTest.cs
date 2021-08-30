// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;

    [TestClass]
    public class SystemSettingDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_GetSystemSetting_EX1()
        {
            var sut = new SystemSettingDTODataLoader();
            sut.GetSystemSetting(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_SaveSystemSetting_EX1()
        {
            var sut = new SystemSettingDTODataLoader();
            sut.SaveSystemSetting(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_ClearSystemSetting_EX1()
        {
            var sut = new SystemSettingDTODataLoader();
            sut.ClearSystemSetting(null);
        }

        [TestMethod]
        public void L_UpsertSystemSetting()
        {
            var sut = new SystemSettingDTODataLoader();
            string systemSettingKey = Guid.NewGuid().ToString().Substring(0, 15);
            string systemSettingValue = Guid.NewGuid().ToString();
            SystemSettingDTO systemSetting = new SystemSettingDTO { Key = systemSettingKey, Value = systemSettingValue};
            string resultKey = sut.SaveSystemSetting(systemSetting);
            Assert.AreEqual(systemSettingKey, resultKey, "Saved System Setting Key does not match original");

            SystemSettingDTO resultSystemSetting = sut.GetSystemSetting(systemSettingKey);
            Assert.AreEqual(systemSettingKey, resultSystemSetting.Key, "System Setting Keys do not match");
            Assert.AreEqual(systemSettingValue, resultSystemSetting.Value, "System Setting Values do not match");

            // now edit the value
            SystemSettingDTO updatedSystemSetting = systemSetting;
            systemSettingValue = Guid.NewGuid().ToString();
            updatedSystemSetting.Value = systemSettingValue;

            sut.SaveSystemSetting(updatedSystemSetting);

            updatedSystemSetting = sut.GetSystemSetting(systemSettingKey);
            Assert.IsTrue(updatedSystemSetting.Key == systemSettingKey, "Updated System Setting Keys didn't match");
            Assert.AreEqual(updatedSystemSetting.Value, systemSettingValue, "Updated System Setting Values do not match");

            // now clear the value
            sut.ClearSystemSetting(systemSettingKey);
            resultSystemSetting = sut.GetSystemSetting(systemSettingKey);
            Assert.IsNull(resultSystemSetting, "System Setting was not cleared");

            ResetTestData();
        }
    }
}
