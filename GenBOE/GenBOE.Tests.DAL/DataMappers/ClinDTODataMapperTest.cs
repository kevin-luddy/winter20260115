using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.Common;
using GenBOE.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;

namespace GenBOE.Tests.DAL.DataMappers
{
    [TestClass]
    public class ClinDTODataMapperTest
    {
        [TestMethod]
        public void M_SaveClinDTOs()
        {
            var clinDataLoader = new Mock<IClinDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var sut = new ClinDTODataMapper(clinDataLoader.Object, cacheDataLoader.Object);

            int clinid = 1;
            var clinModel = new ClinDTO { ClinID = clinid };
            Collection<ClinDTO> tosave = new Collection<ClinDTO> { clinModel };

            clinDataLoader.Setup(a => a.SaveCLINs(It.IsAny<Collection<ClinDTO>>()));
            sut.SaveClins(tosave);

            clinDataLoader.Verify(x => x.SaveCLINs(tosave), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.CLIN_DTO + "_" + clinid), Times.Once());
        }


        [TestMethod]
        public void M_getClinDTO()
        {

            var clinDataLoader = new Mock<IClinDTODataLoader>();

            ClinDTO Clin1 = new ClinDTO { ClinID = 1, ClinNumber = "1.1", ClinTitle = "mock clin1 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000001??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = 1000 };
 
            // if caching is turned on, need to test this by adding memorycache parameters
            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());

            int clinid = 1;
            int wsid = 1;

            int[] clinids = { 1 };
            clinDataLoader.Setup(x => x.GetCLINIdsByWorkspaceID(wsid)).Returns(new Collection<int>(clinids));

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetClinDTODelegate>(), It.IsAny<object[]>(), CacheConstants.CLIN_DTO + "_" + clinid,true))
                .Returns(Clin1);

            var sut = new ClinDTODataMapper(clinDataLoader.Object, cacheDataLoader.Object);

            sut.GetCLINsByWorkspaceID(wsid);
            Assert.IsNotNull(sut.GetCLINsByWorkspaceID(wsid));
            Assert.IsTrue(sut.GetCLINsByWorkspaceID(wsid).Count > 0);
        }

        [TestMethod]
        public void GetCLINsByWorkspaceID()
        {
            ClinDTO Clin1 = new ClinDTO { ClinID = 1, ClinNumber = "1.1", ClinTitle = "mock clin1 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000001??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = 1000 };
            ClinDTO Clin2 = new ClinDTO { ClinID = 2, ClinNumber = "1.2", ClinTitle = "mock clin2 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000002??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = 1000 };
            ClinDTO Clin3 = new ClinDTO { ClinID = 3, ClinNumber = "1.3", ClinTitle = "mock clin3 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000003??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = 1000 };

            var clinRetrieveDataLoader = new Mock<ClinDTODataLoader>();

            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());

            int[] clinids = { 1, 2, 3 };
            clinRetrieveDataLoader.Setup(y => y.GetCLINIdsByWorkspaceID(1)).Returns(new Collection<int>(clinids));

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetClinDTODelegate>(), It.IsAny<object[]>(), CacheConstants.CLIN_DTO + "_" + clinids[0], true))
                            .Returns(Clin1);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetClinDTODelegate>(), It.IsAny<object[]>(), CacheConstants.CLIN_DTO + "_" + clinids[1], true))
                            .Returns(Clin2);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetClinDTODelegate>(), It.IsAny<object[]>(), CacheConstants.CLIN_DTO + "_" + clinids[2], true))
                            .Returns(Clin3);

            ClinDTODataMapper sut = new ClinDTODataMapper(clinRetrieveDataLoader.Object, cacheDataLoader.Object);

            //sut
            Collection<ClinDTO> Clins = sut.GetCLINsByWorkspaceID(1);

            //Assert
            Assert.IsNotNull(Clins);
            Assert.IsTrue(Clins.Count == 3);
            Assert.AreEqual(Clins[0].ClinID.ToString(), "1");
            Assert.AreEqual(Clins[0].ClinNumber.ToString(), "1.1");
            Assert.AreEqual(Clins[0].ClinPaddedNumber.ToString(), "000000000000000000001??.000000000000000000001??");
            Assert.AreEqual(Clins[0].StartDate, Convert.ToDateTime("02/01/2000"));
            Assert.AreEqual(Clins[0].EndDate, Convert.ToDateTime("10/31/2010"));
            Assert.AreEqual(Clins[0].ClinTitle, "mock clin1 title");
        }

        [TestMethod]
        public void M_GetBoeCountByClinID()
        {
            var clinRetrieveDataLoader = new Mock<ClinDTODataLoader>();

            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());

            var clinModelDomain = new Mock<ClinDTO>();

            Collection<ClinDTO> toReturn = new Collection<ClinDTO>();
            toReturn.Add(clinModelDomain.Object);

            Collection<int> boeIDs = new Collection<int> { 1, 2, 3 };
            int clinID = 1;
            clinRetrieveDataLoader.Setup(y => y.GetBOECountByCLINID(clinID)).Returns(boeIDs.Count);

            ClinDTODataMapper sut = new ClinDTODataMapper(clinRetrieveDataLoader.Object, cacheDataLoader.Object);

            //sut
            int BoeCount = sut.GetBOECountByCLINID(clinID);
            Assert.IsTrue(BoeCount > 0, "No Boes tied to a clin");
            Assert.IsTrue(BoeCount ==3, "3 boes were not tied to the clin");
        }

        [TestMethod]
        public void M_WarmCacheClin()
        {
            var _ClinLoader = new Mock<IClinDTODataLoader>();
            var cacheProxy = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(cacheProxy.Object, -1, new GenBOEUtilities());
            Collection<ClinDTO> Clins = new Collection<ClinDTO> { new ClinDTO { ClinID = 73 } };
            _ClinLoader.Setup(x => x.GetAllClins()).Returns(Clins);
            Collection<int> taskVarIDs = new Collection<int>() { 1, 2 };
            Dictionary<int, Collection<int>> taskVarDictionary = new Dictionary<int, Collection<int>>();
            taskVarDictionary.Add(73, taskVarIDs);
            _ClinLoader.Setup(x => x.GetTaskVariableIDsByClinIDs(new Collection<int>() { 73 })).Returns(taskVarDictionary);
            Collection<int> workspaceVarIDs = new Collection<int>() { 3, 4 };
            Dictionary<int, Collection<int>> workspaceVarDictionary = new Dictionary<int, Collection<int>>();
            workspaceVarDictionary.Add(73, workspaceVarIDs);
            _ClinLoader.Setup(x => x.GetWorkspaceVariableIDsByClinIDs(new Collection<int>() { 73 })).Returns(workspaceVarDictionary);

            var sut = new CacheWarmingClinDataMapper(_ClinLoader.Object, cacheDataLoader.Object, cacheProxy.Object);
            sut.WarmClinCache();

            //verify the item was added to cache
            cacheProxy.Verify(x => x.Add(CacheConstants.CLIN_DTO + "_" + 73, Clins[0], -1), Times.Once());
            cacheProxy.Verify(x => x.Add(CacheConstants.TASK_VARIABLE_IDS_BY_CLIN_ID + "_" + 73, taskVarIDs, -1), Times.Once());
            cacheProxy.Verify(x => x.Add(CacheConstants.WORKSPACE_VARIABLE_IDS_BY_CLIN_ID + "_" + 73,workspaceVarIDs, -1), Times.Once());
        }

        [TestMethod]
        public void M_GetBoeIDsByClinID()
        {
            var clinRetrieveDataLoader = new Mock<ClinDTODataLoader>();

            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());

            var clinModelDomain = new Mock<ClinDTO>();

            Collection<ClinDTO> toReturn = new Collection<ClinDTO>();
            toReturn.Add(clinModelDomain.Object);

            Collection<int> boeIDs = new Collection<int> { 1, 2, 3 };
            int clinID = 1;
            clinRetrieveDataLoader.Setup(y => y.GetBOEIDsByCLINID(clinID)).Returns(boeIDs);

            ClinDTODataMapper sut = new ClinDTODataMapper(clinRetrieveDataLoader.Object, cacheDataLoader.Object);

            //sut
            Collection<int> returnedBoeIDs = sut.GetBoeIDsByClinID(clinID);
            Assert.IsTrue(returnedBoeIDs.Count == 3, "3 boes were not tied to the clin");
        }


        [TestMethod]
        public void M_GetTaskVariableIDsByClinID()
        {
            var _ClinLoader = new Mock<ClinDTODataLoader>();
            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());
            Collection<int> taskVarIds = new Collection<int> { 1, 2, 3 };
            _ClinLoader.Setup(x => x.GetTaskVariableIDsByClinID(1)).Returns(taskVarIds);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetTaskVariableIDsByClinIDDelegate>(), It.IsAny<object[]>(), CacheConstants.TASK_VARIABLE_IDS_BY_CLIN_ID + "_" +1, false))
            .Returns(taskVarIds);

            ClinDTODataMapper sut = new ClinDTODataMapper(_ClinLoader.Object, cacheDataLoader.Object);
           Collection<int> getTaskVarIDs = sut.GetTaskVariableIDsByClinID(1);
           Assert.AreEqual(getTaskVarIDs.Count, taskVarIds.Count, "ID counts were not equal");
        }

        [TestMethod]
        public void M_GetWorkspaceVariableIDsByClinID()
        {
            var _ClinLoader = new Mock<ClinDTODataLoader>();
            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());
            Collection<int> workspaceVarIds = new Collection<int> { 1, 2, 3 };
            _ClinLoader.Setup(x => x.GetWorkspaceVariableIDsByClinID(1)).Returns(workspaceVarIds);
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspaceVariableIDsByClinIDDelegate>(), It.IsAny<object[]>(), CacheConstants.WORKSPACE_VARIABLE_IDS_BY_CLIN_ID + "_" + 1, false))
          .Returns(workspaceVarIds);
            ClinDTODataMapper sut = new ClinDTODataMapper(_ClinLoader.Object, cacheDataLoader.Object);
            Collection<int> getWorkspaceVarIDs = sut.GetWorkspaceVariableIDsByClinID(1);
            Assert.AreEqual(getWorkspaceVarIDs.Count, workspaceVarIds.Count, "ID counts were not equal");
        }

        [TestMethod]
        public void M_ClearCacheKey()
        {
            var clinDataLoader = new Mock<IClinDTODataLoader>();

            // if caching is turned on, need to test this by adding memorycache parameters
            var cacheDataLoader = new Mock<ICacheDataLoader>();

            int clinid = 1;

            var clinModel = new ClinDTO { ClinID = clinid };
            Collection<ClinDTO> toReturn = new Collection<ClinDTO>();
            toReturn.Add(clinModel);

           
            var sut = new ClinDTODataMapper(clinDataLoader.Object, cacheDataLoader.Object);

            sut.ClearCacheKeys(new Collection<int> { clinid });
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.CLIN_DTO + "_" + clinid), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.TASK_VARIABLE_IDS_BY_CLIN_ID + "_" + clinid), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.WORKSPACE_VARIABLE_IDS_BY_CLIN_ID + "_" + clinid), Times.Once());
        }

        /// <summary>
        /// Tests the ClearCacheKeysAfterBorOrWsTaskElementSave method
        /// </summary>
        [TestMethod]
        public void M_ClearCacheKeysAfterBorOrWsTaskElementSave()
        {
            var clinDataLoader = new Mock<IClinDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();

            int clinid = 111;

            var sut = new ClinDTODataMapper(clinDataLoader.Object, cacheDataLoader.Object);
            sut.ClearCacheKeysAfterBoeOrWsTaskElementSave(new Collection<int>() { clinid });

            cacheDataLoader.Verify(x => x.Remove(CacheConstants.TASK_VARIABLE_IDS_BY_CLIN_ID + "_" + clinid), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.WORKSPACE_VARIABLE_IDS_BY_CLIN_ID + "_" + clinid), Times.Once());
        }

        [TestMethod]
        public void M_GetWBSIdsByClinID()
        {
            var _ClinLoader = new Mock<ClinDTODataLoader>();
            var memoryCache = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(memoryCache.Object, -1, new GenBOEUtilities());
            Collection<int> wbsIDs = new Collection<int> { 1, 2, 3 };
            int clinID = 1;
            _ClinLoader.Setup(x => x.GetWBSIDByCLINID(clinID)).Returns(wbsIDs);

            ClinDTODataMapper sut = new ClinDTODataMapper(_ClinLoader.Object, cacheDataLoader.Object);
            Collection<int> getWbsIDs = sut.GetWBSIdsByClinID(clinID);
            Assert.AreEqual(getWbsIDs.Count, wbsIDs.Count, "ID counts were not equal");
        }


        [TestMethod]
        public void M_DeleteCLIN()
        {
            var clinDataLoader = new Mock<IClinDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var sut = new ClinDTODataMapper(clinDataLoader.Object, cacheDataLoader.Object);

            int clinid = 1;
            var clinModel = new ClinDTO { ClinID = clinid, Updateable = GenBOE.DataBridge.Common.UpdateType.Deleted };
            Collection<ClinDTO> tosave = new Collection<ClinDTO> { clinModel };
             Collection<int> wbsIDs = new Collection<int> { 1, 2, 3 };
            clinDataLoader.Setup(x => x.GetWBSIDByCLINID(clinid)).Returns(wbsIDs);


            clinDataLoader.Setup(a => a.SaveCLINs(It.IsAny<Collection<ClinDTO>>()));
            sut.SaveClins(tosave);

            clinDataLoader.Verify(x => x.SaveCLINs(tosave), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.CLIN_DTO + "_" + clinid), Times.Once());
            cacheDataLoader.Verify(x=>x.Remove(CacheConstants.WBS_DTO + "_" + wbsIDs[0]), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.WBS_DTO + "_" + wbsIDs[1]), Times.Once());
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.WBS_DTO + "_" + wbsIDs[2]), Times.Once());
        }


    }
}
