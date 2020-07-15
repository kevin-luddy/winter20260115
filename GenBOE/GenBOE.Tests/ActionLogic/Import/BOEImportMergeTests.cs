// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEImportMergeTests
    {
        Mock<ICustomFieldValueDTODataLoader> customFieldValueLoader = null;
        BOEImportMerge boeMerger = null;
        WorkofflineImport importedWorkspace = null;
        WorkofflineImportedBoe importBoe = null;
        BoeDTO toMergeBoe = null;
        ClinDTO toMergeClin = null;
        WbsDTO toMergeWbs = null;
        CustomFieldValueContainer cfc = null;

        DateTime boeStartDate;
        DateTime boeEndDate;

        [TestInitialize]
        public void Init()
        {
            Mock<IBoeDTODataLoader> boeLoader = new Mock<IBoeDTODataLoader>();

            boeStartDate = DateTime.Now;
            boeEndDate = boeStartDate.AddMonths(3);

            importedWorkspace = new WorkofflineImport();
            importBoe = new WorkofflineImportedBoe();
            toMergeBoe = new BoeDTO();
            

            importBoe.Id = 538;
            importBoe.Title = "Imported Title";
            importBoe.Description = "Imported Description";
            importBoe.DataSource = "Imported Sources of Data";

            importBoe.ImportedCLINNumber = "12345";
            importBoe.ImportedCLINTitle = "Imported Clin Title";

            importBoe.ImportedWBSNumber = "09876";
            importBoe.ImportedWBSTitle = "Imported WBS Title";

            cfc = new CustomFieldValueContainer();
            cfc.ContainerID = 1;
            cfc.CustomFieldValueID = 1;
            cfc.Updateable = UpdateType.Upsert;
            importBoe.CustomFieldValueContainers.Add(cfc);

            importedWorkspace.ImportedBoes.Add(importBoe);

            toMergeBoe.Id = 538;
            toMergeBoe.Title = "Original Title";
            toMergeBoe.Description = "Original Description";
            toMergeBoe.DataSource = "Original Sources of Data";
            toMergeBoe.CLINID = 12345;
            toMergeBoe.EndDate = boeEndDate;
            toMergeBoe.StartDate = boeStartDate;
            toMergeBoe.State = IES.Common.BOEState.Draft;

            toMergeClin = new ClinDTO();
            toMergeClin.ClinNumber = "12345";
            toMergeClin.ClinTitle = "Original Clin Title";

            toMergeWbs = new WbsDTO();
            toMergeWbs.WbsNumber = "09876";
            toMergeWbs.WbsTitle = "Orignial Wbs Title";
            
            boeLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(toMergeBoe);
            
            customFieldValueLoader = new Mock<ICustomFieldValueDTODataLoader>();
            customFieldValueLoader.Setup(s => s.GetBOECustomFieldContainerFieldValueMappings(It.IsAny<int>())).Returns(new List<CustomFieldContainerFieldValueMapping>());

            boeMerger = new BOEImportMerge(boeLoader.Object, customFieldValueLoader.Object);
        }

        [TestMethod]
        public void BOEImportMerge_ExtractValidBoesFromOfflineImportTest()
        {
            //Arrange

            //Act
            Collection<BoeDTO> result = boeMerger.ExtractValidBoesFromOfflineImport(importedWorkspace);

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Imported Title", result.First().Title);
            Assert.AreEqual("Imported Description", result.First().Description);
            Assert.AreEqual("Imported Sources of Data", result.First().DataSource);
        }

        [TestMethod]
        public void BOEImportMerge_MergeBoeWithDataTest()
        {
            //Arrange
            BoeDTO toImport = (BoeDTO)importBoe;

            //Act
            BoeDTO result = boeMerger.MergeBoeWithData(toImport);

            //Assert
            Assert.AreEqual(toImport.Title, result.Title);
            Assert.AreEqual(toImport.Description, result.Description);
            Assert.AreEqual(toImport.DataSource, result.DataSource);

            Assert.IsTrue(toImport.CustomFieldValueContainers.Contains(cfc));


        }

        [TestMethod]
        public void BOEImportMerge_MergeBoeWithData_IgnoreDuplicateCustomField_Test()
        {
            //Arrange
            BoeDTO toImport = (BoeDTO)importBoe;
            toMergeBoe.CustomFieldValueContainers.Add(cfc);

            //Act
            BoeDTO result = boeMerger.MergeBoeWithData(toImport);

            //Assert
            Assert.AreEqual(toImport.Title, result.Title);
            Assert.AreEqual(toImport.Description, result.Description);
            Assert.AreEqual(toImport.DataSource, result.DataSource);

            Assert.IsTrue(toImport.CustomFieldValueContainers.Contains(cfc));
            Assert.AreEqual(1, toImport.CustomFieldValueContainers.Count);

        }
    }
}
