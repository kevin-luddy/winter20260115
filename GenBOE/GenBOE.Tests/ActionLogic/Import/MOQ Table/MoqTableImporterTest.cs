// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
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
    public class MoqTableImporterTest
    {
        private Mock<IRetriever> retriever = null;
        private Mock<IFullObjectFactory> factory = null;
        private Mock<ICommonDataMapper> commonDataMapper = null;
        private Mock<IPermissionsDTODataLoader> _PermissionDataLoader = null;

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
        private MoqTableImporter CreateSystem()
        {
            return new MoqTableImporter();
        }

        /// <summary>
        /// Create SUT for RMS
        /// </summary>
        /// <returns>SUT for RMS</returns>
        private MoqTableImporterRMS CreateSystemRMS()
        {
            return new MoqTableImporterRMS();
        }
        
        /// <summary>
        /// Gets the data expected to return from MOQTableSSC_Valid
        /// </summary>
        /// <returns>Expected data for SSC</returns>
        private ICollection<MoqTableData> GetExpectedDataSSC()
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
        private ICollection<MoqTableData> GetExpectedDataRMS()
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
                PoPStart = new DateTime(2021, 3, 3),
                PoPEnd = new DateTime(2021, 6, 15),
                TotalWbsHours = 200,
                AdditionalQueryFilters = "filter 2",
                TotalRelevantHours = 125,
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
        /// Assert the results of the valid MOQ Table import
        /// </summary>
        /// <param name="expected">expected MOQ Table values</param>
        /// <param name="result">result MOQ Table values</param>
        private void AssertValidMoqTables(MoqTableData expected, ImportedMoqTable result)
        {
            Assert.AreEqual(1, result.ImportTypes.Count());
            Assert.AreEqual(MoqTableImportType.CreateMoqTable, result.ImportTypes.First());
            Assert.AreEqual(expected.TableName, result.TableName);
            Assert.AreEqual(expected.RepositoryName, result.RepositoryName);
            Assert.AreEqual(expected.QueryType, result.QueryType);
            Assert.AreEqual(expected.DateOfReport, result.DateOfReport);
            Assert.AreEqual(expected.HistoricalProgramName, result.HistoricalProgramName);
            Assert.AreEqual(expected.WbsElement, result.WbsElement);
            Assert.AreEqual(expected.PoPStart, result.PoPStart);
            Assert.AreEqual(expected.PoPEnd, result.PoPEnd);
            Assert.AreEqual(expected.AdditionalQueryFilters, result.AdditionalQueryFilters);
            Assert.AreEqual(expected.TotalRelevantHours, result.TotalRelevantHours);
            Assert.AreEqual(expected.ContractNumber, result.ContractNumber);
            Assert.AreEqual(expected.TotalWbsHours, result.TotalWbsHours);
            Assert.AreEqual(expected.CustomFieldValueContainers.Count(), result.CustomFieldValueContainers.Count());

            foreach(CustomFieldValueContainer expectedCfvc in expected.CustomFieldValueContainers)
            {
                CustomFieldValueContainer resultCfvc = result.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == expectedCfvc.CustomFieldID);
                Assert.IsNotNull(resultCfvc);
                Assert.AreEqual(expectedCfvc.OpenEndedValue, resultCfvc.OpenEndedValue);
            }
        }

        /// <summary>
        /// Test ImportMoqTableFromExcelFile for SSC
        /// </summary>
        [TestMethod]
        public void ImportMoqTableFromExcelFileTest_SSC()
        {
            MoqTableImporter sut = CreateSystem();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<ImportedMoqTable> results;

            using (MemoryStream file = new MemoryStream(Properties.Resources.MoqTableSscValid))
            {
                results = sut.ImportMoqTableFromExcelFile(file, ws);
            }

            ICollection<MoqTableData> expected = this.GetExpectedDataSSC();

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());

            for(int i = 0; i < results.Count(); i ++)
            {
                this.AssertValidMoqTables(expected.ElementAt(i), results.ElementAt(i));
            }
        }

        /// <summary>
        /// Test ImportMoqTableFromExcelFile for RMS
        /// </summary>
        [TestMethod]
        public void ImportMoqTableFromExcelFileTest_RMS()
        {
            MoqTableImporter sut = CreateSystemRMS();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<ImportedMoqTable> results;

            using (MemoryStream file = new MemoryStream(Properties.Resources.MoqTableRmsValid))
            {
                results = sut.ImportMoqTableFromExcelFile(file, ws);
            }

            ICollection<MoqTableData> expected = this.GetExpectedDataRMS();

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());

            for (int i = 0; i < results.Count(); i++)
            {
                this.AssertValidMoqTables(expected.ElementAt(i), results.ElementAt(i));
            }
        }

        /// <summary>
        /// Test ImportMoqTableFromExcelFile for invalid inputs to make sure proper import types are returned for SSC
        /// </summary>
        [TestMethod]
        public void ImportMoqTableFromExcelFileTest_SSC_Invalid()
        {
            MoqTableImporter sut = CreateSystem();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<ImportedMoqTable> results;

            using (MemoryStream file = new MemoryStream(Properties.Resources.MOQTableSSCInvalid))
            {
                results = sut.ImportMoqTableFromExcelFile(file, ws);
            }

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());

            // Assert all import types returned
            ICollection<MoqTableImportType> importTypes = results.SelectMany(x => x.ImportTypes).ToCollection();
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.MissingTableName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeTableName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.MissingRequiredField));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidDateOfReport));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeHistoricalProgramName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeWBSElement));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopStart));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopStartFW));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopEnd));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopEndFW));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopRange));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeRepositoryName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidQueryType));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidTotalRelevantHours));
            Assert.IsFalse(importTypes.Contains(MoqTableImportType.CreateMoqTable));
            Assert.IsFalse(importTypes.Contains(MoqTableImportType.None));
        }

        /// <summary>
        /// Test ImportMoqTableFromExcelFile for invalid inputs to make sure proper import types are returned for RMS
        /// </summary>
        [TestMethod]
        public void ImportMoqTableFromExcelFileTest_RMS_Invalid()
        {
            MoqTableImporter sut = CreateSystemRMS();

            WorkspaceDTO workspace = new WorkspaceDTO() { Id = 1 };
            FullWorkspace ws = new FullWorkspace(workspace);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(this.GetCustomFields());

            ICollection<ImportedMoqTable> results;

            using (MemoryStream file = new MemoryStream(Properties.Resources.MOQTableRMSInvalid))
            {
                results = sut.ImportMoqTableFromExcelFile(file, ws);
            }

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());

            // Assert all import types returned
            ICollection<MoqTableImportType> importTypes = results.SelectMany(x => x.ImportTypes).ToCollection();
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.MissingTableName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeTableName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.MissingRequiredField));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidDateOfReport));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeHistoricalProgramName));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeWBSElement));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopStart));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopEnd));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidPopRange));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.LargeContractNumber));
            Assert.IsTrue(importTypes.Contains(MoqTableImportType.InvalidTotalRelevantHours));
            Assert.IsFalse(importTypes.Contains(MoqTableImportType.CreateMoqTable));
            Assert.IsFalse(importTypes.Contains(MoqTableImportType.None));
        }
    }
}
