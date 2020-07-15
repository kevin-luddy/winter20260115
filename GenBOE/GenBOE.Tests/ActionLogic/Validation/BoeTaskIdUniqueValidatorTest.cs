// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DataBridge.Common;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BoeTaskIdUniqueValidatorTest
    {
        [TestMethod]
        public void BoeTaskIdUniqueValidator_IsValid()
        {
            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), new Mock<ICommonDataMapper>().Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), new Mock<IPermissionsDTODataLoader>().Object);
            retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(new FullWorkspace());


            int boeid = 99;
            BoeTaskIDUniqueValidator sut = new BoeTaskIDUniqueValidator(factory.Object);
            Collection<Dictionary<string, string>> boetaskDict = new Collection<Dictionary<string, string>>();
            boetaskDict.Add(new Dictionary<string, string>());
            boetaskDict.First<Dictionary<string, string>>().Add("BOEID", boeid.ToString());
            boetaskDict.First<Dictionary<string, string>>().Add("TaskID", "333");
            boetaskDict.First<Dictionary<string, string>>().Add("TaskElementDetailID", "1");

            // setup BOE
            BoeDTO boeDto = new BoeDTO { Id = boeid };
            FullBoe boe = new FullBoe(boeDto);

            // Setup Method
            factory.Setup(x => x.CreateFullBoe(boeid)).Returns(boe);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeid, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { new BoeTaskElementDTO { BOETaskID = "456", Id = 1 }, new BoeTaskElementDTO { BOETaskID = "457", Id = 2 } });
            retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int>() { boeid }, false)).Returns(new Collection<OtherDirectCostDTO> { });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boeid, false)).Returns(new Collection<TravelDTO> { });
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetMaterialCollectionByBoeID(boeid, false)).Returns(new Collection<MaterialDTO> { });

            // Act and Assert

            bool returnValue = sut.isValid("456", boetaskDict);
            Assert.IsTrue(returnValue);

            boetaskDict.Clear();
            boetaskDict.Add(new Dictionary<string, string>());
            boetaskDict.First<Dictionary<string, string>>().Add("BOEID", boeid.ToString());
            boetaskDict.First<Dictionary<string, string>>().Add("TaskElementDetailID", "2");
            returnValue = sut.isValid("457", boetaskDict);
            Assert.IsTrue(returnValue);

        }

        [TestMethod]
        public void BoeTaskIdUniqueValidator_InvalidParameterTest()
        {
            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            BoeTaskIDUniqueValidator sut = new BoeTaskIDUniqueValidator(factory.Object);

            int boeid = 99;

            // setup BOE with bad data, 1 new task IDs with the value of 456
            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>{ new BoeTaskElementDTO { BOETaskID = "456", Id = 1 } };
            BoeDTO boeDto = new BoeDTO { Id = boeid };
            FullBoe boe = new FullBoe(boeDto); 
           
            // Setup Method

            retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int>() { boeid }, false)).Returns(new Collection<OtherDirectCostDTO> { });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boeid, false)).Returns(new Collection<TravelDTO> { });
            retriever.Setup(x => x.GetMaterialCollectionByBoeID(boeid, false)).Returns(new Collection<MaterialDTO> { });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElements);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), new Mock<ICommonDataMapper>().Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), new Mock<IPermissionsDTODataLoader>().Object);
            retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(new FullWorkspace());
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(boe);

            Collection<Dictionary<string, string>> boetaskDict = new Collection<Dictionary<string, string>>();
            boetaskDict.Add(new Dictionary<string, string>());
            boetaskDict.First<Dictionary<string, string>>().Add("BOEID", boeid.ToString());
            boetaskDict.First<Dictionary<string, string>>().Add("TaskID", "456"); // user 'entered' this taskid, but since TaskElementDetailID is the same it is an 'edit' and OK
            boetaskDict.First<Dictionary<string, string>>().Add("TaskElementDetailID", "1");

            bool returnValue = sut.isValid("456", boetaskDict);
            Assert.AreEqual(true, returnValue, "The return value was 'false' but this test should have been ensuring an 'edit' to a task element was allowed");

            boetaskDict.First<Dictionary<string, string>>().Remove("TaskElementDetailID"); // get rid of old TaskElementDetailID
            boetaskDict.First<Dictionary<string, string>>().Add("TaskElementDetailID", "2"); // new task element ... so the TaskID is now a duplicate
            returnValue = sut.isValid("456", boetaskDict);
            Assert.AreEqual(false, returnValue, "The return value was 'true' but this test should have simulated a new TaskElement with a duplicate ID");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BoeTaskIdUniqueValidator_NullIsValidTest()
        {
            var factory = new Mock<IFullObjectFactory>();

            BoeTaskIDUniqueValidator sut = new BoeTaskIDUniqueValidator(factory.Object);
            sut.isValid(null, new Collection<Dictionary<string, string>>());
        }
    }
}
