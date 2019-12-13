using System.Collections.ObjectModel;
using GenBOE.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GenBOE.Tests.DAL.DataMappers
{
    [TestClass]
    public class GroupDTODataMapperTest
    {
        [TestMethod]
        public void M_GetAllGroups()
        {
            var groupLoader = new Mock<IGroupDTODataLoader>();
            var cacheLoader = new Mock<ICacheDataLoader>();

            groupLoader.Setup(x => x.GetAllGroupIds()).Returns(new Collection<int> { 1, 2, 3 });

            // groups are returned in reverse name order from their IDs (i.e. id 1 = name "A", id 2 = name "B",...
            cacheLoader.Setup(x => x.GetData(It.IsAny<GetGroupDTODelegate>(),
                                            new object[]{1},
                                            CacheConstants.GROUP_DTO + "_" + 1))
                .Returns(new GroupDTO { ID = 1, Name = "C" });

            cacheLoader.Setup(x => x.GetData(It.IsAny<GetGroupDTODelegate>(),
                                            new object[] { 2 },
                                            CacheConstants.GROUP_DTO + "_" + 2))
                .Returns(new GroupDTO { ID = 1, Name = "B" });
            
            cacheLoader.Setup(x => x.GetData(It.IsAny<GetGroupDTODelegate>(),
                                            new object[] { 3 },
                                            CacheConstants.GROUP_DTO + "_" + 3))
                .Returns(new GroupDTO { ID = 1, Name = "A" });

            GroupDTODataMapper sut = new GroupDTODataMapper(groupLoader.Object, cacheLoader.Object);

            Collection<GroupDTO> returned = sut.GetAllGroups();

            // check that the groups were reordered according to their names .. in addition to just returning the right groups
            Assert.AreEqual("A", returned[0].Name);
            Assert.AreEqual("B", returned[1].Name);
            Assert.AreEqual("C", returned[2].Name);
        }

        [TestMethod]
        public void M_GetGroupById()
        {
            var groupLoader = new Mock<IGroupDTODataLoader>();
            var cacheLoader = new Mock<ICacheDataLoader>();

            int groupid = 1;
            GroupDTO toReturn = new GroupDTO { ID = groupid };
            cacheLoader.Setup(x => x.GetData(It.IsAny<GetGroupDTODelegate>(), 
                                            It.IsAny<object[]>(),
                                            CacheConstants.GROUP_DTO + "_" + groupid))
                .Returns(toReturn);

            GroupDTODataMapper sut = new GroupDTODataMapper(groupLoader.Object, cacheLoader.Object);

            GroupDTO returned = sut.GetGroupById(groupid);

            Assert.IsNotNull(returned);
            Assert.AreEqual(groupid, returned.ID);  
        }

        [TestMethod]
        public void M_InsertGroup()
        {
            var groupLoader = new Mock<IGroupDTODataLoader>();
            var cacheLoader = new Mock<ICacheDataLoader>();

            int groupid = 1;
            GroupDTO tosave = new GroupDTO { ID = -1 };
            groupLoader.Setup(x => x.InsertGroup(tosave)).Returns(groupid);

            GroupDTODataMapper sut = new GroupDTODataMapper(groupLoader.Object, cacheLoader.Object);

            int returned = sut.InsertGroup(tosave);

            Assert.AreEqual(groupid, returned);  
        }

        [TestMethod]
        public void M_ClearCacheKeys()
        {
            var groupLoader = new Mock<IGroupDTODataLoader>();
            var cacheLoader = new Mock<ICacheDataLoader>();

            Collection<int> groupIds = new Collection<int>();
            int groupid = 1;
            int groupid2 = 2;

            groupIds.Add(groupid);
            groupIds.Add(groupid2);
           

            GroupDTODataMapper sut = new GroupDTODataMapper(groupLoader.Object, cacheLoader.Object);

            sut.ClearCacheKeys(groupIds);

            cacheLoader.Verify(x => x.Remove(CacheConstants.GROUP_DTO + "_" + groupid), Times.Once());
            cacheLoader.Verify(x => x.Remove(CacheConstants.GROUP_DTO + "_" + groupid2), Times.Once());
        }
    }
}
