// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.IO.Export;
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

	/// <summary>
	/// Test Class for TraceTableExporter
	/// </summary>
	[TestClass]
	public class TraceTableExporterTest
	{
		private Mock<IRetriever> retriever;
		private Mock<IPermissionsDTODataLoader> permissionsLoader;
		private Mock<ICommonDataMapper> dataMapper;
		private Mock<IFullObjectFactory> factory;

		/// <summary>
		/// init
		/// </summary>
		[TestInitialize]
		public void Setup()
		{
			retriever = new Mock<IRetriever>();
			permissionsLoader = new Mock<IPermissionsDTODataLoader>();
			dataMapper = new Mock<ICommonDataMapper>();
			factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), dataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
		}

		/// <summary>
		/// Test ExportTraceTableData for all summary fields
		/// </summary>
		[TestMethod]
		public void TestExportTraceTableData()
		{
			ITraceTableExporter sut = new TraceTableExporter();
			FullWorkspace ws = new FullWorkspace() { Id = 1 };

			ResourceTypeDto resourceType1 = new ResourceTypeDto()
			{
				Id = 1,
				BoeID = 1,
				TaskElementId = 1,
				CLINID = 1,
				WBSID = 1,
				ResourceID = 1,
				PerformingOrgID = 1,
				SpreadType = SpreadType.Hours,
				StartDate = new DateTime(2021, 1, 1),
				EndDate = new DateTime(2023, 12, 31),
				SpreadCurveID = SpreadCurves.SpreadCurve3,
				ValueSpread = 36
			};
			resourceType1.LaborSpreads = CreateResourceSpreads(resourceType1.ValueSpread.Value, resourceType1.Id, resourceType1.StartDate.Value, resourceType1.EndDate.Value, resourceType1.BoeID);

			ResourceTypeDto resourceType2 = new ResourceTypeDto()
			{
				Id = 1,
				BoeID = 1,
				TaskElementId = 1,
				CLINID = 1,
				WBSID = 1,
				ResourceID = 2,
				PerformingOrgID = 2,
				SpreadType = SpreadType.Hours,
				StartDate = new DateTime(2021, 1, 1),
				EndDate = new DateTime(2023, 12, 31),
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				ValueSpread = 360
			};
			resourceType2.LaborSpreads = CreateResourceSpreads(resourceType2.ValueSpread.Value, resourceType2.Id, resourceType2.StartDate.Value, resourceType2.EndDate.Value, resourceType2.BoeID);

			ResourceTypeDto resourceType3 = new ResourceTypeDto()
			{
				Id = 1,
				BoeID = 1,
				TaskElementId = 2,
				CLINID = 1,
				WBSID = 1,
				ResourceID = 1,
				PerformingOrgID = 1,
				SpreadType = SpreadType.Hours,
				StartDate = new DateTime(2021, 1, 1),
				EndDate = new DateTime(2023, 12, 31),
				SpreadCurveID = SpreadCurves.SpreadCurve2,
				ValueSpread = 3600
			};
			resourceType3.LaborSpreads = CreateResourceSpreads(resourceType3.ValueSpread.Value, resourceType3.Id, resourceType3.StartDate.Value, resourceType3.EndDate.Value, resourceType3.BoeID);

			ResourceTypeDto resourceType4 = new ResourceTypeDto()
			{
				Id = 1,
				BoeID = 2,
				TaskElementId = 3,
				CLINID = 1,
				WBSID = 2,
				ResourceID = 1,
				PerformingOrgID = 1,
				SpreadType = SpreadType.Hours,
				StartDate = new DateTime(2021, 1, 1),
				EndDate = new DateTime(2023, 12, 31),
				SpreadCurveID = SpreadCurves.SpreadCurve5,
				ValueSpread = 36000
			};
			resourceType4.LaborSpreads = CreateResourceSpreads(resourceType4.ValueSpread.Value, resourceType4.Id, resourceType4.StartDate.Value, resourceType4.EndDate.Value, resourceType4.BoeID);

			ResourceTypeDto resourceType5 = new ResourceTypeDto()
			{
				Id = 1,
				BoeID = 2,
				TaskElementId = 3,
				CLINID = 2,
				WBSID = 2,
				ResourceID = 3,
				PerformingOrgID = 1,
				SpreadType = SpreadType.Hours,
				StartDate = new DateTime(2021, 1, 1),
				EndDate = new DateTime(2023, 12, 31),
				SpreadCurveID = SpreadCurves.SpreadCurve6,
				ValueSpread = 360000
			};
			resourceType5.LaborSpreads = CreateResourceSpreads(resourceType5.ValueSpread.Value, resourceType5.Id, resourceType5.StartDate.Value, resourceType5.EndDate.Value, resourceType5.BoeID);

			CustomFieldValueContainer cfvc1 = new CustomFieldValueContainer() { Id = 1, ContainerID = 1, CustomFieldID = 1, CustomFieldValueID = 1, OpenEndedValue = "testing 1" };
			CustomFieldValueContainer cfvc2 = new CustomFieldValueContainer() { Id = 2, ContainerID = 2, CustomFieldID = 2, CustomFieldValueID = 2, OpenEndedValue = "testing 2" };
			CustomFieldValueContainer cfvc3 = new CustomFieldValueContainer() { Id = 3, ContainerID = 3, CustomFieldID = 1, CustomFieldValueID = 1, OpenEndedValue = "testing 1" };
			CustomFieldValueContainer cfvc4 = new CustomFieldValueContainer() { Id = 4, ContainerID = 4, CustomFieldID = 2, CustomFieldValueID = 3, OpenEndedValue = "testing 3" };
			CustomFieldValueContainer cfvc5 = new CustomFieldValueContainer() { Id = 5, ContainerID = 5, CustomFieldID = 1, CustomFieldValueID = 4, OpenEndedValue = "testing 4" };
			CustomFieldValueContainer cfvc6 = new CustomFieldValueContainer() { Id = 6, ContainerID = 6, CustomFieldID = 2, CustomFieldValueID = 5, OpenEndedValue = "testing 1" };
			CustomFieldValueContainer cfvc7 = new CustomFieldValueContainer() { Id = 7, ContainerID = 7, CustomFieldID = 1, CustomFieldValueID = 1, OpenEndedValue = "testing 1" };
			CustomFieldValueContainer cfvc8 = new CustomFieldValueContainer() { Id = 8, ContainerID = 8, CustomFieldID = 2, CustomFieldValueID = 2, OpenEndedValue = "testing 2" };

			resourceType1.CustomFieldValueContainers.Add(cfvc1);
			resourceType1.CustomFieldValueContainers.Add(cfvc2);
			resourceType2.CustomFieldValueContainers.Add(cfvc3);
			resourceType2.CustomFieldValueContainers.Add(cfvc4);
			resourceType3.CustomFieldValueContainers.Add(cfvc5);
			resourceType3.CustomFieldValueContainers.Add(cfvc6);
			resourceType4.CustomFieldValueContainers.Add(cfvc7);
			resourceType4.CustomFieldValueContainers.Add(cfvc8);

			BoeTaskElementDTO task1 = new BoeTaskElementDTO() { Id = 1, TaskTitle = "Task 1",  taskElementLabors = new Collection<ResourceTypeDto>() { resourceType1, resourceType2 } };
			BoeTaskElementDTO task2 = new BoeTaskElementDTO() { Id = 2, TaskTitle = "Task 2", taskElementLabors = new Collection<ResourceTypeDto>() { resourceType3 } };
			BoeTaskElementDTO task3 = new BoeTaskElementDTO() { Id = 3, TaskTitle = "Task 3", taskElementLabors = new Collection<ResourceTypeDto>() { resourceType4, resourceType5 } };

			FullBoe boe1 = new FullBoe() { Id = 1, Title = "BOE1", Workspace = ws };
			FullBoe boe2 = new FullBoe() { Id = 2, Title = "BOE2", Workspace = ws };
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { task1, task2, task3 });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { task1, task2 });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { task3 });
			retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<ICollection<int>>(), false)).Returns(new Collection<OtherDirectCostDTO>());
			retriever.Setup(x => x.GetTravelByWorkspaceId(It.IsAny<int>(), false)).Returns(new Collection<TravelDTO>());
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe1, boe2 });

			ResourceDTO resource1 = new ResourceDTO() { Id = 1, ResourceName = "TEST1", ResourceDesc = "TEST1 - TESTING ONE", ElementOfCost = ElementOfCostType.LMLabor };
			ResourceDTO resource2 = new ResourceDTO() { Id = 2, ResourceName = "TEST2", ResourceDesc = "TEST2 - TESTING TWO", ElementOfCost = ElementOfCostType.LMLabor };
			ResourceDTO resource3 = new ResourceDTO() { Id = 3, ResourceName = "TEST3", ResourceDesc = "TEST2 - TESTING THREE", ElementOfCost = ElementOfCostType.LMLabor };
			retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>() { resource1, resource2, resource3 });

			PerformingOrgDTO perfOrg1 = new PerformingOrgDTO() { Id = 1, PerformingOrgName = "ORG 1" };
			PerformingOrgDTO perfOrg2 = new PerformingOrgDTO() { Id = 2, PerformingOrgName = "ORG 2" };
			retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg1, perfOrg2 });

			FullClin clin1 = new FullClin() { Id = 1, ClinNumber = "1" };
			FullClin clin2 = new FullClin() { Id = 2, ClinNumber = "2" };
			retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, clin2 });

			FullWbs wbs1 = new FullWbs() { Id = 1, WbsNumber = "1" };
			FullWbs wbs2 = new FullWbs() { Id = 2, WbsNumber = "2" };
			retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, wbs2 });

			CustomFieldDTO customField1 = new CustomFieldDTO() { Id = 1, CustomFieldName = "TEST 1" };
			CustomFieldDTO customField2 = new CustomFieldDTO() { Id = 2, CustomFieldName = "TEST 2" };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(new Collection<CustomFieldDTO>() { customField1, customField2 });

			TraceTableSettingsData settings = new TraceTableSettingsData()
			{
				ElementsOfCost = new Collection<int> { (int)ElementOfCostType.LMLabor },
				RateType = (int)RateType.Hours,
				ShowYears = true,
				SummaryFields = new Collection<string>() { SummaryFieldType.CLINNum.GetDescription(), SummaryFieldType.WBSNum.GetDescription(), SummaryFieldType.ResourceOrActivityId.GetDescription() }
			};

			ICollection<TraceTableBoeData> result = sut.ExportTraceTableData(ws, settings);

			Assert.IsTrue(result.Any());

			// First level should be CLIN
			TraceTableBoeData clinField = result.FirstOrDefault();
			Assert.IsNotNull(clinField);
			Assert.AreEqual(SummaryFieldType.CLINNum.GetDescription(), clinField.SummaryField);
			Assert.AreEqual(clin1.ClinNumber, clinField.SummaryFieldValue);
			Assert.AreEqual(0, clinField.TotalValue);
			Assert.IsFalse(clinField.SpreadValuesForYear.Any());
			Assert.IsTrue(clinField.ChildData.Any());

			// Then WBS
			TraceTableBoeData wbsField = clinField.ChildData.FirstOrDefault();
			Assert.IsNotNull(wbsField);
			Assert.AreEqual(SummaryFieldType.WBSNum.GetDescription(), wbsField.SummaryField);
			Assert.AreEqual(wbs1.WbsNumber, wbsField.SummaryFieldValue);
			Assert.AreEqual(0, wbsField.TotalValue);
			Assert.IsFalse(wbsField.SpreadValuesForYear.Any());
			Assert.IsTrue(wbsField.ChildData.Any());

			// Then Resource along with the value/spreads
			// For this combination of CLIN, WBS, and Resource ID we will have resourceType1 and resourceType3 from above
			TraceTableBoeData resIdField = wbsField.ChildData.FirstOrDefault();
			Assert.IsNotNull(resIdField);
			Assert.AreEqual(SummaryFieldType.ResourceOrActivityId.GetDescription(), resIdField.SummaryField);
			Assert.AreEqual(resource1.ResourceName, resIdField.SummaryFieldValue);
			Assert.AreEqual(resourceType1.ValueSpread + resourceType3.ValueSpread, resIdField.TotalValue);
			Assert.IsTrue(resIdField.SpreadValuesForYear.Any());
			Assert.IsFalse(resIdField.ChildData.Any());

			// Test for other fields
			settings.SummaryFields = new Collection<string>()
			{
				SummaryFieldType.BOETitle.GetDescription(),
				SummaryFieldType.TaskDescription.GetDescription(), 
				SummaryFieldType.ResourceDescription.GetDescription(),
				SummaryFieldType.PerformingOrgId.GetDescription()
			};

			result = sut.ExportTraceTableData(ws, settings);

			Assert.IsTrue(result.Any());

			// First level should be BOE Title
			TraceTableBoeData boeField = result.FirstOrDefault();
			Assert.IsNotNull(boeField);
			Assert.AreEqual(SummaryFieldType.BOETitle.GetDescription(), boeField.SummaryField);
			Assert.AreEqual(boe1.Title, boeField.SummaryFieldValue);
			Assert.AreEqual(0, boeField.TotalValue);
			Assert.IsFalse(boeField.SpreadValuesForYear.Any());
			Assert.IsTrue(boeField.ChildData.Any());

			// Then Task Title
			TraceTableBoeData taskField = boeField.ChildData.FirstOrDefault();
			Assert.IsNotNull(taskField);
			Assert.AreEqual(SummaryFieldType.TaskDescription.GetDescription(), taskField.SummaryField);
			Assert.AreEqual(task1.TaskTitle, taskField.SummaryFieldValue);
			Assert.AreEqual(0, taskField.TotalValue);
			Assert.IsFalse(taskField.SpreadValuesForYear.Any());
			Assert.IsTrue(taskField.ChildData.Any());

			// Then Resource Description
			TraceTableBoeData resDescField = taskField.ChildData.FirstOrDefault();
			Assert.IsNotNull(wbsField);
			Assert.AreEqual(SummaryFieldType.ResourceDescription.GetDescription(), resDescField.SummaryField);
			Assert.AreEqual(resource1.ResourceDesc, resDescField.SummaryFieldValue);
			Assert.AreEqual(0, resDescField.TotalValue);
			Assert.IsFalse(resDescField.SpreadValuesForYear.Any());
			Assert.IsTrue(resDescField.ChildData.Any());

			// Then Perf Org along with the value/spreads
			// For this combination will have only resourceType1
			TraceTableBoeData perfOrgField = resDescField.ChildData.FirstOrDefault();
			Assert.IsNotNull(perfOrgField);
			Assert.AreEqual(SummaryFieldType.PerformingOrgId.GetDescription(), perfOrgField.SummaryField);
			Assert.AreEqual(perfOrg1.PerformingOrgName, perfOrgField.SummaryFieldValue);
			Assert.AreEqual(resourceType1.ValueSpread, perfOrgField.TotalValue);
			Assert.IsTrue(perfOrgField.SpreadValuesForYear.Any());
			Assert.IsFalse(perfOrgField.ChildData.Any());

			// Test for custom fields
			settings.SummaryFields = new Collection<string>()
			{
				customField1.CustomFieldName,
				customField2.CustomFieldName
			};

			result = sut.ExportTraceTableData(ws, settings);

			Assert.IsTrue(result.Any());

			// First level should be custom field 1
			TraceTableBoeData cf1Field = result.FirstOrDefault();
			Assert.IsNotNull(cf1Field);
			Assert.AreEqual(customField1.CustomFieldName, cf1Field.SummaryField);
			Assert.AreEqual(cfvc1.OpenEndedValue, cf1Field.SummaryFieldValue);
			Assert.AreEqual(0, cf1Field.TotalValue);
			Assert.IsFalse(cf1Field.SpreadValuesForYear.Any());
			Assert.IsTrue(cf1Field.ChildData.Any());

			// Then custom field 2 along with the value/spreads
			// For this combination will have only resourceType1 and resourceType4
			TraceTableBoeData cf2Field = cf1Field.ChildData.FirstOrDefault();
			Assert.IsNotNull(cf2Field);
			Assert.AreEqual(customField2.CustomFieldName, cf2Field.SummaryField);
			Assert.AreEqual(cfvc2.OpenEndedValue, cf2Field.SummaryFieldValue);
			Assert.AreEqual(resourceType1.ValueSpread + resourceType4.ValueSpread, cf2Field.TotalValue);
			Assert.IsTrue(cf2Field.SpreadValuesForYear.Any());
			Assert.IsFalse(cf2Field.ChildData.Any());
		}

		/// <summary>
		/// Create resource spreads
		/// </summary>
		/// <param name="resourceTypeValue">Resource type total value</param>
		/// <param name="resourceTypeId">Resource Type ID</param>
		/// <param name="startDate">start date</param>
		/// <param name="endDate">end date</param>
		/// <param name="boeId">boe ID</param>
		/// <returns>collection of resource spreads for the resource type</returns>
		private Collection<ResourceSpreadDto> CreateResourceSpreads(decimal resourceTypeValue, int resourceTypeId, DateTime startDate, DateTime endDate, int boeId)
		{
			Collection<ResourceSpreadDto> toReturn = new Collection<ResourceSpreadDto>();
			int totalMonths = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
			decimal spreadValue = resourceTypeValue / totalMonths;

			for (int i = 0; i < totalMonths; i++)
			{
				toReturn.Add(new ResourceSpreadDto()
				{
					LaborTypeId = resourceTypeId,
					BoeID = boeId,
					LaborSpreadValue = spreadValue,
					LaborSpreadDate = startDate.AddMonths(i)
				});
			}

			return toReturn;
		}
	}
}
