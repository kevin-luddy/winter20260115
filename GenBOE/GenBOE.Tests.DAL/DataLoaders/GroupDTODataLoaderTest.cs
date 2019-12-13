using System;
using System.Collections.ObjectModel;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class GroupDTODataLoaderTest
    {
        [TestMethod]
        public void L_TestSaveAndGetGroup()
        {
            GroupDTO dto = new GroupDTO
            {
                Domain = "moqgrp " + Guid.NewGuid().ToString().Substring(0, 3),
                ID = -1,
                Name = "moqname " + Guid.NewGuid().ToString().Substring(0, 16),
                Updateable = GenBOE.DataBridge.Common.UpdateType.Upsert
            };

            GroupDTODataLoader sut = new GroupDTODataLoader();
            int newId = sut.InsertGroup(dto);
            Assert.IsTrue(newId > 0, "New GroupDTO save did not produce new ID > 0.  ID is " + newId);

            GroupDTO savedDTO = sut.GetGroupById(newId);

            Assert.AreEqual(newId, savedDTO.ID);
            Assert.AreEqual(dto.Domain, savedDTO.Domain);
            Assert.AreEqual(dto.Name, savedDTO.Name);

            int? groupId = null;
            Assert.IsTrue(sut.GroupExists(dto.Domain, dto.Name, out groupId));
            Assert.IsNotNull(groupId, "group existed, but Id was not returned correctly");
            Assert.AreEqual(newId, groupId.Value);

            Assert.IsFalse(sut.GroupExists("domabc00", "gibberish.flibberish", out groupId));
            Assert.AreEqual(0, groupId, "group should not have existed, but Id returned was " + groupId.Value);

            Collection<int> groups = sut.GetAllGroupIds();
            Assert.IsTrue(groups.Contains(newId), "Could not locate group id " + newId + " in collection of groups <" + string.Join(",", groups) + ">");
        }

    }
}
