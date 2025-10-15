using System.Collections.ObjectModel;
using System.Linq;
using IES.Common;
using GenBOE.DataBridge.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GenBOE.Dtos;
using System.Collections.Generic;

namespace GenBOE.Tests.DAL.DataMappers
{
    [TestClass]
    public class CommonDataMapperTest
    {
		/// <summary>
		/// Mocked Common Data Loader
		/// </summary>
		private Mock<CommonDataLoader> dataLoader { get; set; }

		/// <summary>
		/// Mocked Cache Data Loader
		/// </summary>
		private Mock<ICacheDataLoader> cacheLoader { get; set; }

		/// <summary>
		/// Get System Under Test
		/// </summary>
		/// <returns>Common Data Mapper</returns>
		private CommonDataMapper GetSUT()
		{
			dataLoader = new Mock<CommonDataLoader>();
			cacheLoader = new Mock<ICacheDataLoader>();

			return new CommonDataMapper(dataLoader.Object, cacheLoader.Object);
		}

        [TestMethod]
        public void getRoles()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<RoleModelView> roles = new Mock<RoleModelView>();
            Collection<RoleModelView> toReturn = new Collection<RoleModelView>();
            toReturn.Add(roles.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetRoleDelegate>(), It.IsAny<object[]>(), CacheConstants.ROLES, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getRoles());
            Assert.IsTrue(sut.getRoles().Count > 0);
        }

        [TestMethod]
        public void getProposalStatus()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<ProposalStatusTypeModelView> zero = new Mock<ProposalStatusTypeModelView>();
            Collection<ProposalStatusTypeModelView> toReturn = new Collection<ProposalStatusTypeModelView>();
            toReturn.Add(zero.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetProposalStatusTypesDelegate>(), It.IsAny<object[]>(), CacheConstants.PROPOSAL_STATE_TYPE, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getProposalStatusTypes());
            Assert.IsTrue(sut.getProposalStatusTypes().Count > 0);
        }

        [TestMethod]
        public void getSpreadCurve()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<SpreadCurveModelView> spread = new Mock<SpreadCurveModelView>();
            Collection<SpreadCurveModelView> toReturn = new Collection<SpreadCurveModelView>();
            toReturn.Add(spread.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetSpreadCurveDelegate>(), It.IsAny<object[]>(), CacheConstants.SPREAD_CURVE, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getSpreadCurve());
            Assert.IsTrue(sut.getSpreadCurve().Count > 0);
        }

        [TestMethod]
        public void getProjectMapSpreadCurve()
        {
            Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

            Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Mock<SpreadCurveModelView> spread = new Mock<SpreadCurveModelView>();
            Collection<SpreadCurveModelView> toReturn = new Collection<SpreadCurveModelView>();
            toReturn.Add(spread.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetSpreadCurveDelegate>(), It.IsAny<object[]>(), CacheConstants.PROJECT_MAP_SPREAD_CURVE, false))
                .Returns(toReturn);

            CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Collection<SpreadCurveModelView> result = sut.getProjectMapSpreadCurve();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void getMOQType()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<MOQTypeModelView> moqType = new Mock<MOQTypeModelView>();
            Collection<MOQTypeModelView> toReturn = new Collection<MOQTypeModelView>();
            toReturn.Add(moqType.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetMOQTypeDelegate>(), It.IsAny<object[]>(), CacheConstants.MOQ_TYPE, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getMOQType());
            Assert.IsTrue(sut.getMOQType().Count > 0);
        }

        [TestMethod]
        public void getMOQTypeName()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            MOQTypeModelView moqType = new MOQTypeModelView { MOQTypeID = (int)MOQType.Comparison, MOQTypeName = "blah" };
            Collection<MOQTypeModelView> toReturn = new Collection<MOQTypeModelView>();
            toReturn.Add(moqType);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetMOQTypeDelegate>(), It.IsAny<object[]>(), CacheConstants.MOQ_TYPE, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.AreEqual("blah", sut.getMOQTypeName(MOQType.Comparison));
        }

        [TestMethod]
        public void GetEmails()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<EmailModelDomain> emailType = new Mock<EmailModelDomain>();
            Collection<EmailModelDomain> toReturn = new Collection<EmailModelDomain>();
            toReturn.Add(emailType.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetEmailDelegate>(), It.IsAny<object[]>(), CacheConstants.EMAILS, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.GetEmails());
            Assert.IsTrue(sut.GetEmails().Count > 0);
        }

        [TestMethod]
        public void GetReportTypes()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<ReportDTO> reportType = new Mock<ReportDTO>();
            Collection<ReportDTO> toReturn = new Collection<ReportDTO>();
            toReturn.Add(reportType.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetReportsDelegate>(), It.IsAny<object[]>(), CacheConstants.REPORTS, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getReports());
            Assert.IsTrue(sut.getReports().Count > 0);
        }

        [TestMethod]
        public void getBOEStates()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<BOEStateModelView> states = new Collection<BOEStateModelView>();
            states.Add(new BOEStateModelView { BOEState = "one", BOEStateID = (int)BOEState.Approved });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOEStateDelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_STATE, false))
                .Returns(states);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

			Collection<BOEStateModelView> returned = sut.getBOEStates();
            Assert.IsTrue(returned.Count == 1);
            Assert.AreEqual("one", returned[0].BOEState);
        }


        [TestMethod]
        public void getBOEStateName()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<BOEStateModelView> states = new Collection<BOEStateModelView>();
            states.Add(new BOEStateModelView { BOEState = "one", BOEStateID = (int)BOEState.Approved });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOEStateDelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_STATE, false))
                .Returns(states);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.AreEqual("one", sut.getBOEStateName(BOEState.Approved));
        }


        [TestMethod]
        public void getWorkspaceStates()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<WorkspaceStateModelView> states = new Collection<WorkspaceStateModelView>();
            states.Add(new WorkspaceStateModelView { WorkspaceStateID = (int)WorkspaceState.Closed, WorkspaceState = "state" });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspaceStateDelegate>(), It.IsAny<object[]>(), CacheConstants.WORKSPACE_STATE, false))
                .Returns(states);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

			Collection<WorkspaceStateModelView> returned = sut.getWorkspaceStates();
            Assert.IsTrue(returned.Count == 1);
            Assert.AreEqual("state", returned[0].WorkspaceState);
        }


        [TestMethod]
        public void getWorkspaceStateName()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<WorkspaceStateModelView> states = new Collection<WorkspaceStateModelView>();
            states.Add(new WorkspaceStateModelView { WorkspaceStateID = (int)WorkspaceState.Closed, WorkspaceState = "state" });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspaceStateDelegate>(), It.IsAny<object[]>(), CacheConstants.WORKSPACE_STATE, false))
                .Returns(states);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

			string returned = sut.getWorkspaceStateName(WorkspaceState.Closed);
            Assert.AreEqual(returned, "state");
        }

        [TestMethod]
        public void getFieldTypes()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<FieldTypeModelView> fieldtypes = new Collection<FieldTypeModelView>();
            fieldtypes.Add(new FieldTypeModelView { FieldTypeID = (int)FieldType.ApproverResponse, FieldTypeName = "type" });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetFieldTypeDelegate>(), It.IsAny<object[]>(), CacheConstants.FIELD_TYPE, false))
                .Returns(fieldtypes);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

			Collection<FieldTypeModelView> returned = sut.getFieldTypes();
            Assert.IsTrue(returned.Count == 1);
            Assert.AreEqual("type", returned[0].FieldTypeName);
        }
        
        [TestMethod]
        public void getOdcSpreadCurve()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();

			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

			Mock<OtherDirectCostSpreadCurveModelView> spread = new Mock<OtherDirectCostSpreadCurveModelView>();
            Collection<OtherDirectCostSpreadCurveModelView> toReturn = new Collection<OtherDirectCostSpreadCurveModelView>();
            toReturn.Add(spread.Object);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetODCSpreadCurveDelegate>(), It.IsAny<object[]>(), CacheConstants.ODC_SPREAD_CURVE, false))
                .Returns(toReturn);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

            Assert.IsNotNull(sut.getOdcSpreadCurve());
            Assert.IsTrue(sut.getOdcSpreadCurve().Count > 0);
        }

        [TestMethod]
        public void getSumVariableResourceTypes()
        {
			Mock<CommonDataLoader> commonDataLoader = new Mock<CommonDataLoader>();
			Mock<ICacheDataLoader> cacheDataLoader = new Mock<ICacheDataLoader>();

            Collection<SumVariableResourceTypeModelView> sumVariableResourceTypes = new Collection<SumVariableResourceTypeModelView>();
            sumVariableResourceTypes.Add(new SumVariableResourceTypeModelView { SumVariableResourceTypeID = (int)SumVariableResourceType.LOESub, SumVariableResourceTypeName = "LOE Sub" });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetSumVariableResourceTypesDelegate>(), It.IsAny<object[]>(), CacheConstants.SUM_VARIABLE_RESOURCE_TYPE, false))
                .Returns(sumVariableResourceTypes);

			CommonDataMapper sut = new CommonDataMapper(commonDataLoader.Object, cacheDataLoader.Object);

			Collection<SumVariableResourceTypeModelView> returned = sut.GetSumVariableResourceTypes();
            Assert.IsTrue(returned.Count == 1);
            Assert.AreEqual("LOE Sub", returned[0].SumVariableResourceTypeName);
        }

		/// <summary>
		/// Test GetProPricerFieldsDictionary when Task Author is enabled
		/// </summary>
		[TestMethod]
		public void GetProPricerFieldsDictionaryTest_TaskAuthorEnabled()
		{
			CommonDataMapper sut = GetSUT();

			ICollection<EnumTypeModelView> cachedFields = new Collection<EnumTypeModelView>()
			{
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Task.TaskTitle, EnumTypeName = ProPricerField_Task.TaskTitle.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Task.TaskAuthor, EnumTypeName = ProPricerField_Task.TaskAuthor.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Resources.ResourceID, EnumTypeName = ProPricerField_Resources.ResourceID.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Resources.TaskAuthor, EnumTypeName = ProPricerField_Resources.TaskAuthor.GetDescription() },
			};

			cacheLoader.Setup(x => x.GetData(It.IsAny<GetProPricerFieldsDelegate>(), It.IsAny<object[]>(), CacheConstants.PROPRICER_FIELDS, false)).Returns(cachedFields);

			IDictionary<int, EnumTypeModelView> result = sut.GetProPricerFieldsDictionary(false, true);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Task.TaskTitle));
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Task.TaskAuthor));
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Resources.ResourceID));
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Resources.TaskAuthor));
		}

		/// <summary>
		/// Test GetProPricerFieldsDictionary when Task Author is disabled
		/// </summary>
		[TestMethod]
		public void GetProPricerFieldsDictionaryTest_TaskAuthorDisabled()
		{
			CommonDataMapper sut = GetSUT();

			ICollection<EnumTypeModelView> cachedFields = new Collection<EnumTypeModelView>()
			{
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Task.TaskTitle, EnumTypeName = ProPricerField_Task.TaskTitle.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Task.TaskAuthor, EnumTypeName = ProPricerField_Task.TaskAuthor.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Resources.ResourceID, EnumTypeName = ProPricerField_Resources.ResourceID.GetDescription() },
				new EnumTypeModelView() { EnumTypeID = (int)ProPricerField_Resources.TaskAuthor, EnumTypeName = ProPricerField_Resources.TaskAuthor.GetDescription() },
			};

			cacheLoader.Setup(x => x.GetData(It.IsAny<GetProPricerFieldsDelegate>(), It.IsAny<object[]>(), CacheConstants.PROPRICER_FIELDS, false)).Returns(cachedFields);

			IDictionary<int, EnumTypeModelView> result = sut.GetProPricerFieldsDictionary(false, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Task.TaskTitle));
			Assert.IsFalse(result.ContainsKey((int)ProPricerField_Task.TaskAuthor));
			Assert.IsTrue(result.ContainsKey((int)ProPricerField_Resources.ResourceID));
			Assert.IsFalse(result.ContainsKey((int)ProPricerField_Resources.TaskAuthor));
		}
	}
}
