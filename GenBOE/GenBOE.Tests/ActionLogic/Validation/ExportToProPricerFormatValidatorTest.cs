// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ExportToProPricerFormatValidatorTest
    {
        Mock<IFullObjectFactory> _factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();

        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), _factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
        }

        /// <summary>
        /// Checks that the ExportToProPricerFormatValidator checks 
        /// to make sure that there is no Duplicated Format Names and 
        /// if there is then to report back with a response
        /// </summary>
        [TestMethod]
        public void ExportToProPricerFormatvalidationTest()
        {
            //Variable Declarations
            ProPricerDTO value1 = new ProPricerDTO() { Id = 1, WorkspaceID = 2, ExportID = 1, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name" };
            ProPricerDTO value2 = new ProPricerDTO() { Id = 1, WorkspaceID = 3, ExportID = 1, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name" };
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<string, string>>();
            
            FullWorkspace workspace1 = new FullWorkspace() { Id = 3 };
            FullWorkspace workspace2 = new FullWorkspace() { Id = 4 };

            ICollection<ProPricerDTO> existingFormats = new Collection<ProPricerDTO>(){
                new ProPricerDTO() { Id = 1, ExportID = 1, Scope = IES.Common.ProPricerScope.System, FormatName = "Format Name" },
                new ProPricerDTO() { Id = 2, ExportID = 1, Scope = IES.Common.ProPricerScope.System, FormatName = "Different Name" },
                new ProPricerDTO() { Id = 3, ExportID = 1, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name" },
                new ProPricerDTO() { Id = 4, ExportID = 1, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Different Name" }};
            ICollection<ProPricerDTO> existingFormatsBad = new Collection<ProPricerDTO>(){
                new ProPricerDTO() { Id = 1, ExportID = 1, Scope = IES.Common.ProPricerScope.System, FormatName = "Format Name" },
                new ProPricerDTO() { Id = 2, ExportID = 2, Scope = IES.Common.ProPricerScope.System, FormatName = "Different Name" },
                new ProPricerDTO() { Id = 3, ExportID = 2, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name" },
                new ProPricerDTO() { Id = 4, ExportID = 1, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Different Name" }};

            _factory.Setup(x => x.CreateFullWorkspace(value1.WorkspaceID.Value)).Returns(workspace1);
            _factory.Setup(x => x.CreateFullWorkspace(value2.WorkspaceID.Value)).Returns(workspace2);
            
            retriever.Setup(x => x.GetProPricerExportsByWorkspaceId(workspace1.Id)).Returns(existingFormats);
            retriever.Setup(x => x.GetProPricerExportsByWorkspaceId(workspace2.Id)).Returns(existingFormatsBad);

            ExportToProPricerFormatValidator sut = new ExportToProPricerFormatValidator(_factory.Object);

            //Act
            Collection<String> response1 = sut.validation(value1, inData);
            Collection<String> response2 = sut.validation(value2, inData);

            Assert.AreEqual(0, response1.Count);
            Assert.AreEqual(1, response2.Count);
            Assert.AreEqual("The format name must be unique within a Workspace scope.", response2[0]);
        }

        /// <summary>
        /// Checks that the ExportToProPricerFormatValidator checks 
        /// to make sure that there is no Duplicated Format Names and 
        /// if there is then to report back with a response
        /// </summary>
        [TestMethod]
        public void SystemExportToProPricerFormatvalidationTest()
        {
            //Variable Declarations
            ProPricerDTO value1 = new ProPricerDTO() { Id = 1, ExportID = 3, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name" };
            ProPricerDTO value2 = new ProPricerDTO() { Id = 1, ExportID = 4, Scope = IES.Common.ProPricerScope.Workspace, FormatName = "Format Name Unique" };
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<string, string>>();

            Mock<IProPricerDTODataLoader> loader = new Mock<IProPricerDTODataLoader>();

            ICollection<ProPricerDTO> existingFormats = new Collection<ProPricerDTO>(){
                new ProPricerDTO() { Id = 1, ExportID = 1, Scope = IES.Common.ProPricerScope.System, FormatName = "Format Name" },
                new ProPricerDTO() { Id = 2, ExportID = 2, Scope = IES.Common.ProPricerScope.System, FormatName = "Different Name" }};
            
            loader.Setup(x => x.GetAllSystemExports()).Returns(existingFormats);

            SystemExportToProPricerFormatValidator sut = new SystemExportToProPricerFormatValidator(loader.Object);

            //Act
            Collection<String> response1 = sut.validation(value1, inData);
            Collection<String> response2 = sut.validation(value2, inData);

            Assert.AreEqual(1, response1.Count);
            Assert.AreEqual(0, response2.Count);
            Assert.AreEqual("The format name must be unique within System scope.", response1[0]);
        }


        /// <summary>
        /// This checks to make sure that the ArgumentException
        /// is thrown when WorkspaceID is false.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "WorkspaceID was not NULL")]
        public void ExportToProPricerFormatvalidationTestWorkspaceIDNull()
        {
            //Variable Declarations
            object value = new ProPricerDTO() { Id = 1, WorkspaceID = null };
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>();

            ExportToProPricerFormatValidator sut = new ExportToProPricerFormatValidator(_factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This checks to make sure that the InvalidCastException 
        /// is thrown when value is not of type ProPricerDTO.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Value type is ProPricerDTO")]
        public void ExportToProPricerFormatvalidationTestCastException()
        {
            //Variable Declarations
            object value = new ProPricerTasks();
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>();

            ExportToProPricerFormatValidator sut = new ExportToProPricerFormatValidator(_factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This checks to make sure that the ArgumentNullException
        /// is thrown when value is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Value was not NULL")]
        public void ExportToProPricerFormatvalidationTestValueNull()
        {
            //Variable Declarations
            object value = null;
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>();

            ExportToProPricerFormatValidator sut = new ExportToProPricerFormatValidator(_factory.Object);
    
            //Act
            sut.validation(value, inData);
        }
    }
}
