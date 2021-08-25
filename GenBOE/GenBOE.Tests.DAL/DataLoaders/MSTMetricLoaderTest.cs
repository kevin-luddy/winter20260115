// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    /// <summary>
    /// Tests for MSTMetricLoader class.  
    /// </summary>
    [TestClass]
    public class MSTMetricLoaderTest
    {
        /// <summary>
        /// Test getting search qualilfier data.
        /// </summary>
        [TestMethod]
        public void L_GetMSTSearchCriteria()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchCriteriaDTO dto = loader.GetMSTMetricSearchCriteria();
            Assert.IsTrue(dto.DataSources.Any());
            Assert.IsTrue(dto.MeasureFunctions.Any());
            Assert.IsTrue(dto.MeasureNames.Any());
            Assert.IsTrue(dto.Programs.Any());
            Assert.IsTrue(dto.MeasureQualifiers.Any());
        }

        /// <summary>
        /// Test getting metrics from program id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsByProgramId()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { ProgramId = 11 };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.ProgramId == search.ProgramId);
            
            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics from measure id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsByMeasureId()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { MeasureId = 1 };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.MeasureId == search.MeasureId);

            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics from measure function id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsByMeasureFunctionId()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { MeasureFunctionId = "SE" };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.MeasureFunctionId == search.MeasureFunctionId);

            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics from data source id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsByDataSourceId()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { DataSourceId = 1 };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.DataSourceId == search.DataSourceId);

            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics from Measure qualifier id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsByMeasureQualifier()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { MeasureQualifierId = "CDR; Critical Design Review" };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.MeasureQualifierId == search.MeasureQualifierId);

            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics from measure id in search criteria.
        /// </summary>
        [TestMethod]
        public void L_SearchMSTMetricsBySearchString()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { SearchFor = "JSF" };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            int totalFound = dtos.Count(x => x.ProgramDescription.Contains(search.SearchFor) || x.MeasureDescription.Contains(search.SearchFor) || x.Comment.Contains(search.SearchFor));

            Assert.IsTrue(totalFound == dtos.Count);
        }

        /// <summary>
        /// Test getting metrics by PMM metric Ids.
        /// </summary>
        [TestMethod]
        public void L_GetMSTMetricsByIds()
        {
            MSTMetricLoader loader = new MSTMetricLoader();
            MSTMetricSearchDTO search = new MSTMetricSearchDTO() { SearchFor = "JSF" };
            ICollection<MSTMetricDetailsDTO> dtos = loader.SearchMSTMetrics(search);

            // Get Ids from search results and try to get the lot by Id.
            ICollection<int> ids = dtos.Select(i => i.Id).ToList<int>();
            ICollection<MSTMetricDetailsDTO> dtosById = loader.GetByIds(ids);

            Assert.IsTrue(dtosById.Count == dtos.Count);
        }  
    }
}
