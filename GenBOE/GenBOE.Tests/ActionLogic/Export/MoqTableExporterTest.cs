// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MoqTableExporterTest
    {
        private Mock<IRetriever> retriever = null;
        private Mock<IFullObjectFactory> factory = null;
        private Mock<ICommonDataMapper> commonDataMapper = null;
        private Mock<IPermissionsDTODataLoader> _PermissionDataLoader = null;
        private string templatePath;

        /// <summary>
        /// initialize
        /// </summary>
        [TestInitialize]
        public void init()
        {
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            _PermissionDataLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _PermissionDataLoader.Object);
        }

        /// <summary>
        /// Create SUT
        /// </summary>
        /// <returns>SUT for SSC</returns>
        private MoqTableExporter CreateSystem()
        {
            // save a copy of the Excel template file to template Path
            templatePath = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName() + ".xlsx");
            File.WriteAllBytes(templatePath, Properties.Resources.MOQTableSSC);
            return new MoqTableExporter();
        }

        /// <summary>
        /// Create SUT for RMS
        /// </summary>
        /// <returns>SUT for RMS</returns>
        private MoqTableExporterRMS CreateSystemRMS()
        {
            // save a copy of the Excel template file to template Path
            templatePath = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName() + ".xlsx");
            File.WriteAllBytes(templatePath, Properties.Resources.MOQTableRMS);
            return new MoqTableExporterRMS();
        }

        /// <summary>
        /// Gets the data expected to return from MOQTableSSC_Valid
        /// </summary>
        /// <returns>Expected data for SSC</returns>
        private ICollection<MoqTableData> CreateTableDataSSC()
        {
            ICollection<MoqTableData> toReturn = new Collection<MoqTableData>();

            toReturn.Add(new MoqTableData()
            {
                TableName = "Table 1",
                RepositoryName = "Rep1",
                QueryType = MoqTableData.WEEKLY,
                DateOfReport = new DateTime(2021, 7, 1),
                HistoricalProgramName = "Historical Name 1",
                WbsElement = "WBS 1",
                PoPStartWeek = 5,
                PoPStartYear = 2021,
                PoPEndWeek = 8,
                PoPEndYear = 2021,
                AdditionalQueryFilters = "Filter 1",
                TotalRelevantHours = 100,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
                {
                    new CustomFieldValueContainer()
                    {
                        CustomFieldID = 1,
                        OpenEndedValue = "cf test 1"
                    }, new CustomFieldValueContainer ()
                    {
                        CustomFieldID = 2,
                        OpenEndedValue = "required cf 1"
                    }
                }
            });

            toReturn.Add(new MoqTableData()
            {
                TableName = "Table 2",
                RepositoryName = "Rep2",
                QueryType = MoqTableData.MONTHLY,
                DateOfReport = new DateTime(2021, 7, 1),
                HistoricalProgramName = "Historical Name 2",
                WbsElement = "WBS 2",
                PoPStart = new DateTime(2021, 1, 1),
                PoPEnd = new DateTime(2021, 6, 1),
                AdditionalQueryFilters = "Filter 2",
                TotalRelevantHours = 200,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
                {
                    new CustomFieldValueContainer()
                    {
                        CustomFieldID = 1,
                        OpenEndedValue = "cf test 2"
                    }, new CustomFieldValueContainer ()
                    {
                        CustomFieldID = 2,
                        OpenEndedValue = "required cf 2"
                    }
                }
            });

            return toReturn;
        }

        /// <summary>
        /// Gets the data expected to return from MOQTableRMS_Valid
        /// </summary>
        /// <returns>Expected data for SSC</returns>
        private ICollection<MoqTableData> CreateTableDataRMS()
        {
            ICollection<MoqTableData> toReturn = new Collection<MoqTableData>();

            toReturn.Add(new MoqTableData()
            {
                TableName = "Table 1",
                DateOfReport = new DateTime(2021, 7, 1),
                HistoricalProgramName = "Historical Name 1",
                ContractNumber = "Contract 1",
                WbsElement = "WBS 1",
                PoPStart = new DateTime(2021, 1, 1),
                PoPEnd = new DateTime(2021, 6, 1),
                TotalWbsHours = 100,
                AdditionalQueryFilters = "filter 1",
                TotalRelevantHours = 50,
                QueryType = MoqTableData.MONTHLY, // Not typically needed, but setting as monthly so date strings will be properly formatted while company config is set to SSC for testing
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
                {
                    new CustomFieldValueContainer()
                    {
                        CustomFieldID = 1,
                        OpenEndedValue = "cf test 1"
                    }, new CustomFieldValueContainer ()
                    {
                        CustomFieldID = 2,
                        OpenEndedValue = "required cf 1"
                    }
                }
            });

            toReturn.Add(new MoqTableData()
            {
                TableName = "Table 2",
                DateOfReport = new DateTime(2021, 7, 1),
                HistoricalProgramName = "Historical Name 2",
                ContractNumber = "Contract 2",
                WbsElement = "WBS 2",
                PoPStart = new DateTime(2021, 3, 1),
                PoPEnd = new DateTime(2021, 6, 1),
                TotalWbsHours = 200,
                AdditionalQueryFilters = "filter 2",
                TotalRelevantHours = 125,
                QueryType = MoqTableData.MONTHLY,
                CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
                {
                    new CustomFieldValueContainer()
                    {
                        CustomFieldID = 1,
                        OpenEndedValue = "cf test 2"
                    }, new CustomFieldValueContainer ()
                    {
                        CustomFieldID = 2,
                        OpenEndedValue = "required cf 2"
                    }
                }
            });

            return toReturn;
        }

        /// <summary>
        /// Get custom fields
        /// </summary>
        /// <returns></returns>
        private ICollection<CustomFieldDTO> GetCustomFields()
        {
            return new Collection<CustomFieldDTO>()
            {
                new CustomFieldDTO()
                {
                    Id = 1,
                    CustomFieldName = "TEST",
                    CustomFieldRequired = false,
                    CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay
                },
                new CustomFieldDTO()
                {
                    Id = 2,
                    CustomFieldName = "TEST REQ",
                    CustomFieldRequired = true,
                    CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay
                }
            };
        }

        /// <summary>
        /// Test ExportToExcelFile for SSC
        /// </summary>
        [TestMethod]
        public void ExportToExcelFile_SSC()
        {
            MoqTableExporter sut = this.CreateSystem();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<MoqTableData> tablesToExport = this.CreateTableDataSSC();

            string resultFile = sut.ExportToExcelFile(this.templatePath, tablesToExport, ws);

            Assert.IsTrue(File.Exists(resultFile));

            // Use the importer to read the file and test it
            // It has its own tests to verify it's working properly
            MoqTableImporter importer = new MoqTableImporter();
            ICollection<ImportedMoqTable> importResults;

            using (MemoryStream file = new MemoryStream(File.ReadAllBytes(resultFile)))
            {
                importResults = importer.ImportMoqTableFromExcelFile(file, ws);
            }

            this.ValidateExportTableData(tablesToExport, importResults);
        }

        /// <summary>
        /// Test ExportToExcelFile for RMS
        /// </summary>
        [TestMethod]
        public void ExportToExcelFile_RMS()
        {
            MoqTableExporterRMS sut = this.CreateSystemRMS();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<MoqTableData> tablesToExport = this.CreateTableDataRMS();
            string resultFile = sut.ExportToExcelFile(this.templatePath, tablesToExport, ws);

            Assert.IsTrue(File.Exists(resultFile));

            // Use the importer to read the file and test it
            // It has its own tests to verify it's working properly
            MoqTableImporterRMS importer = new MoqTableImporterRMS();
            ICollection<ImportedMoqTable> importResults;

            using (MemoryStream file = new MemoryStream(File.ReadAllBytes(resultFile)))
            {
                importResults = importer.ImportMoqTableFromExcelFile(file, ws);
            }

            // remove query type from the original values
            foreach(MoqTableData table in tablesToExport)
            {
                table.QueryType = null;
            }

            this.ValidateExportTableData(tablesToExport, importResults);
        }

        /// <summary>
        /// Validate the data from the export matches the data that was exported
        /// </summary>
        /// <param name="expected">data that was exported</param>
        /// <param name="results">data from the export</param>
        private void ValidateExportTableData(ICollection<MoqTableData> expected, ICollection<ImportedMoqTable> results)
        {
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(expected.Count(), results.Count());
            for (int i = 0; i < results.Count(); i++)
            {
                Assert.AreEqual(expected.ElementAt(i).TableName, results.ElementAt(i).TableName);
                Assert.AreEqual(expected.ElementAt(i).RepositoryName, results.ElementAt(i).RepositoryName);
                Assert.AreEqual(expected.ElementAt(i).QueryType, results.ElementAt(i).QueryType);
                Assert.AreEqual(expected.ElementAt(i).DateOfReport.Normalize(DateTimePrecision.Day), results.ElementAt(i).DateOfReport);
                Assert.AreEqual(expected.ElementAt(i).HistoricalProgramName, results.ElementAt(i).HistoricalProgramName);
                Assert.AreEqual(expected.ElementAt(i).ContractNumber, results.ElementAt(i).ContractNumber);
                Assert.AreEqual(expected.ElementAt(i).WbsElement, results.ElementAt(i).WbsElement);
				Assert.AreEqual(expected.ElementAt(i).QueryType == MoqTableData.WEEKLY ? expected.ElementAt(i).PoPStart : expected.ElementAt(i).PoPStart.Normalize(DateTimePrecision.Day), results.ElementAt(i).PoPStart);
				Assert.AreEqual(expected.ElementAt(i).QueryType == MoqTableData.WEEKLY ? expected.ElementAt(i).PoPEnd : expected.ElementAt(i).PoPEnd.Normalize(DateTimePrecision.Day), results.ElementAt(i).PoPEnd);
				// TotalWbsHours will be overridden with a call to SAP Api
				Assert.AreEqual(expected.ElementAt(i).AdditionalQueryFilters, results.ElementAt(i).AdditionalQueryFilters);
                Assert.AreEqual(expected.ElementAt(i).TotalRelevantHours, results.ElementAt(i).TotalRelevantHours);
                Assert.AreEqual(expected.ElementAt(i).MOQTypeSelectionId, results.ElementAt(i).MOQTypeSelectionId);
                Assert.AreEqual(expected.ElementAt(i).CustomFieldValueContainers.Count(), results.ElementAt(i).CustomFieldValueContainers.Count());
                for (int j = 0; j < results.ElementAt(i).CustomFieldValueContainers.Count(); j++)
                {
                    Assert.AreEqual(expected.ElementAt(i).CustomFieldValueContainers.ElementAt(j).OpenEndedValue, results.ElementAt(i).CustomFieldValueContainers.ElementAt(j).OpenEndedValue);
                }
            }
        }
    }
}
