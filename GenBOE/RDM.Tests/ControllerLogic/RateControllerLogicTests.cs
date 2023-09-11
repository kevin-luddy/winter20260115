// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.ActionLogic.Common;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.Mediator;
    using IES.ActionLogic.Validation;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.DataBridge.Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using IES.Tests;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RateControllerLogicTest
    {
        /// <summary>
        /// Mock Rate Config Loader
        /// </summary>
        private Mock<IRateConfigLoader> rateConfigLoader = new Mock<IRateConfigLoader>();

        /// <summary>
        /// Mock Rate Detail Loader
        /// </summary>
        private Mock<IRateDetailLoader> rateDetailLoader = new Mock<IRateDetailLoader>();

        /// <summary>
        /// Mock Common Data Mapper
        /// </summary>
        private Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();

        /// <summary>
        /// Mock Home Controller Logic
        /// </summary>
        private Mock<IHomeControllerLogic> homeControllerLogic = new Mock<IHomeControllerLogic>();

        /// <summary>
        /// Mock Burden Pool Loader
        /// </summary>
        private Mock<IBurdenPoolLoader> burdenPoolLoader = new Mock<IBurdenPoolLoader>();

        /// <summary>
        /// Mock Section Loader
        /// </summary>
        private Mock<ISectionLoader> sectionLoader = new Mock<ISectionLoader>();

        /// <summary>
       /// Mock Revision Loader
       /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new Mock<IRevisionMediator>();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new Mock<IAreaLockingLoader>();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<ActiveDirectoryUtilities> adUtils = new Mock<ActiveDirectoryUtilities>();

        /// <summary>
        /// The replication loader
        /// </summary>
        private Mock<IRateCodeReplicationLoader> replicationLoader = new Mock<IRateCodeReplicationLoader>();
        
        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        /// <summary>
        /// Mock existing set of RateCode MVs from DB
        /// </summary>
        private RateDetailModelView[] originals;

        /// <summary>
        /// Set of test RateCode MVs for use by tests
        /// </summary>
        private Dictionary<string, RateDetailModelView> testRateCodes = new Dictionary<string, RateDetailModelView>();

        public RateControllerLogic CreateSut()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);
            return new RateControllerLogic(this.rateDetailLoader.Object, this.commonDataMapper.Object,
                this.homeControllerLogic.Object, this.burdenPoolLoader.Object, this.sectionLoader.Object, 
                this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object,
                this.replicationLoader.Object);
        }

        /// <summary>
        /// Initializes the test data.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            List<RateCodeModelView> replications = new List<RateCodeModelView>
            {
                new RateCodeModelView
                {
                    From = "FROM",
                    To = "TO"
                }
            };

            this.replicationLoader.Setup(x => x.GetAll()).Returns(replications);

            this.originals = new RateDetailModelView[]
            {
                new RateDetailModelView
                {
                    Id = 1,
                    RateCode = "FXEDAA",
                    RateCategoryDescription = "Direct Labor",
                    RateCategory = RateCategory.DirectLabor,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Section = 27208,
                    RateType = RateType.Hours,
                    Description = "Titusville Development Hourly & NES Straight Time Rate",
                    CommercialBurdenPool = "OHDEVNET-G",
                    CommercialBurdenPoolId = 3830,
                    GovernmentBurdenPool = "OHDEVNET",
                    GovernmentBurdenPoolId = 3829,
                    ProPricerMappings = null,
                    Values = new Collection<RateYearModelView>(),
                    GenerateAdditionalDirectLaborRates = true,
                    RateDescription = string.Empty,
                    ResourceClass = string.Empty,
                    ResourceClassId = 0,
                    RateDescription1 = "ATLO Titusville Development Hourly & NES Straight Time Rate",
                    ResourceClass1 = "Labor-Core-ATLO(1)",
                    ResourceClassId1 = 19,
                    RateDescription2 = "ENG Titusville Development Hourly & NES Straight Time Rate",
                    ResourceClass2 = "Labor-Core-Engineering(2)",
                    ResourceClassId2 = 27,
                    RateDescription3 = "LABS Titusville Development Hourly & NES Straight Time Rate",
                    ResourceClass3 = "Labor-Core-Labs(3)",
                    ResourceClassId3 = 28,
                    RateDescription4 = "OTHER Titusville Development Hourly & NES Straight Time Rate",
                    ResourceClass4 = "Labor-Core-Other(4)",
                    ResourceClassId4 = 42,
                    RateDescription5 = "QUAL Titusville Development Hourly & NES Straight Time Rate",
                    ResourceClass5 = "Labor-Core-Quality(5)",
                    ResourceClassId5 = 43,
                    RateDescription6 = string.Empty,
                    ResourceClass6 = string.Empty,
                    ResourceClassId6 = 0,
                    RateDescription7 = string.Empty,
                    ResourceClass7 = string.Empty,
                    ResourceClassId7 = 0,
                    RateDescription8 = string.Empty,
                    ResourceClass8 = string.Empty,
                    ResourceClassId8 = 0,
                    RateDescription9 = string.Empty,
                    ResourceClass9 = string.Empty,
                    ResourceClassId9 = 0
                },
                new RateDetailModelView
                {
                    Id = 2,
                    RateCode = "XXJDAB",
                    RateCategoryDescription = "Direct Labor",
                    RateCategory = RateCategory.DirectLabor,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Section = 27208,
                    RateType = RateType.Hours,
                    Description = "Billerica Development (Nantero) Lvl 1 & 2",
                    CommercialBurdenPool = "OHDEVNET-G",
                    CommercialBurdenPoolId = 3830,
                    GovernmentBurdenPool = "OHDEVNET",
                    GovernmentBurdenPoolId = 3829,
                    ProPricerMappings = null,
                    Values = new Collection<RateYearModelView>(),
                    GenerateAdditionalDirectLaborRates = true,
                    RateDescription = string.Empty,
                    ResourceClass = string.Empty,
                    ResourceClassId = 0,
                    RateDescription1 = string.Empty,
                    ResourceClass1 = string.Empty,
                    ResourceClassId1 = 0,
                    RateDescription2 = string.Empty,
                    ResourceClass2 = string.Empty,
                    ResourceClassId2 = 0,
                    RateDescription3 = string.Empty,
                    ResourceClass3 = string.Empty,
                    ResourceClassId3 = 0,
                    RateDescription4 = "Billerica (MA) Development Lvl 1 & 2",
                    ResourceClass4 = "Labor-Core-ATC-Other(4)",
                    ResourceClassId4 = 16,
                    RateDescription5 = string.Empty,
                    ResourceClass5 = string.Empty,
                    ResourceClassId5 = 0,
                    RateDescription6 = string.Empty,
                    ResourceClass6 = string.Empty,
                    ResourceClassId6 = 0,
                    RateDescription7 = string.Empty,
                    ResourceClass7 = string.Empty,
                    ResourceClassId7 = 0,
                    RateDescription8 = string.Empty,
                    ResourceClass8 = string.Empty,
                    ResourceClassId8 = 0,
					RateDescription9 = string.Empty,
					ResourceClass9 = string.Empty,
					ResourceClassId9 = 0
				},
                new RateDetailModelView
                {
                    Id = 3,
                    RateCode = "541976LB",
                    RateCategoryDescription = "Service Center",
                    RateCategory = RateCategory.ServiceCenter,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Section = 27245,
                    RateType = RateType.Cost,
                    Description = "BUS Center of Excellence Service Center - Labor",
                    CommercialBurdenPool = "OHDEVNET-G",
                    CommercialBurdenPoolId = 3830,
                    GovernmentBurdenPool = "OHDEVNET",
                    GovernmentBurdenPoolId = 3829,
                    ProPricerMappings = null,
                    Values = new Collection<RateYearModelView>(),
                    GenerateAdditionalDirectLaborRates = false,
                    RateDescription = "BUS Center of Excellence Service Center",
                    ResourceClass = "Service_Center-Labor",
                    ResourceClassId = 50,
                    RateDescription1 = string.Empty,
                    ResourceClass1 = string.Empty,
                    ResourceClassId1 = 0,
                    RateDescription2 = string.Empty,
                    ResourceClass2 = string.Empty,
                    ResourceClassId2 = 0,
                    RateDescription3 = string.Empty,
                    ResourceClass3 = string.Empty,
                    ResourceClassId3 = 0,
                    RateDescription4 = string.Empty,
                    ResourceClass4 = string.Empty,
                    ResourceClassId4 = 0,
                    RateDescription5 = string.Empty,
                    ResourceClass5 = string.Empty,
                    ResourceClassId5 = 0,
                    RateDescription6 = string.Empty,
                    ResourceClass6 = string.Empty,
                    ResourceClassId6 = 0,
                    RateDescription7 = string.Empty,
                    ResourceClass7 = string.Empty,
                    ResourceClassId7 = 0,
                    RateDescription8 = string.Empty,
                    ResourceClass8 = string.Empty,
                    ResourceClassId8 = 0,
					RateDescription9 = string.Empty,
					ResourceClass9 = string.Empty,
					ResourceClassId9 = 0
				},
                new RateDetailModelView
                {
                    Id = 4,
                    RateCode = "TRAVLOTC",
                    RateCategoryDescription = "Travel OTC",
                    RateCategory = RateCategory.TravelOtc,
                    ResourceType = DirectRateMappingResourceType.None,
                    Section = 0,
                    RateType = RateType.NotSet,
                    Description = "Other travel costs ",
                    CommercialBurdenPool = string.Empty,
                    CommercialBurdenPoolId = 0,
                    GovernmentBurdenPool = string.Empty,
                    GovernmentBurdenPoolId = 0,
                    ProPricerMappings = null,
                    Values = new Collection<RateYearModelView>(),
                    GenerateAdditionalDirectLaborRates = false,
                    RateDescription = string.Empty,
                    ResourceClass = string.Empty,
                    ResourceClassId = 0,
                    RateDescription1 = string.Empty,
                    ResourceClass1 = string.Empty,
                    ResourceClassId1 = 0,
                    RateDescription2 = string.Empty,
                    ResourceClass2 = string.Empty,
                    ResourceClassId2 = 0,
                    RateDescription3 = string.Empty,
                    ResourceClass3 = string.Empty,
                    ResourceClassId3 = 0,
                    RateDescription4 = string.Empty,
                    ResourceClass4 = string.Empty,
                    ResourceClassId4 = 0,
                    RateDescription5 = string.Empty,
                    ResourceClass5 = string.Empty,
                    ResourceClassId5 = 0,
                    RateDescription6 = string.Empty,
                    ResourceClass6 = string.Empty,
                    ResourceClassId6 = 0,
                    RateDescription7 = string.Empty,
                    ResourceClass7 = string.Empty,
                    ResourceClassId7 = 0,
                    RateDescription8 = string.Empty,
                    ResourceClass8 = string.Empty,
                    ResourceClassId8 = 0,
					RateDescription9 = string.Empty,
					ResourceClass9 = string.Empty,
					ResourceClassId9 = 0
				}
            };

            testRateCodes["FXDDAB"] = new RateDetailModelView
            {
                Id = 11,
                RateCode = "FXDDAB",
                RateCategoryDescription = "Direct Labor",
                RateCategory = RateCategory.DirectLabor,
                ResourceType = DirectRateMappingResourceType.Labor,
                Section = 27208,
                RateType = RateType.Hours,
                Description = "Denver FBM Development Lvl 1 & 2",
                CommercialBurdenPool = "OHDEVNET-G",
                CommercialBurdenPoolId = 3830,
                GovernmentBurdenPool = "OHDEVNET",
                GovernmentBurdenPoolId = 3829,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = true,
                RateDescription = string.Empty,
                ResourceClass = string.Empty,
                ResourceClassId = 0,
                RateDescription1 = "ATLO Denver FBM Development Lvl 1 & 2",
                ResourceClass1 = "Labor-Core-ATLO(1)",
                ResourceClassId1 = 19,
                RateDescription2 = "ENG Denver FBM Development Lvl 1 & 2",
                ResourceClass2 = "Labor-Core-Engineering(2)",
                ResourceClassId2 = 27,
                RateDescription3 = "LABS Denver FBM Development Lvl 1 & 2",
                ResourceClass3 = "Labor-Core-Labs(3)",
                ResourceClassId3 = 28,
                RateDescription4 = "OTHER Denver FBM Development Lvl 1 & 2",
                ResourceClass4 = "Labor-Core-Other(4)",
                ResourceClassId4 = 42,
                RateDescription5 = "QUAL Denver FBM Development Lvl 1 & 2",
                ResourceClass5 = "Labor-Core-Quality(5)",
                ResourceClassId5 = 43,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};

            testRateCodes["C1NDAB"] = new RateDetailModelView
            {
                Id = 12,
                RateCode = "C1NDAB",
                RateCategoryDescription = "Direct Labor",
                RateCategory = RateCategory.DirectLabor,
                ResourceType = DirectRateMappingResourceType.Labor,
                Section = 27208,
                RateType = RateType.Hours,
                Description = "Core NorthEast Dev Lvl 1&2 ",
                CommercialBurdenPool = "OHDEVNET-G",
                CommercialBurdenPoolId = 3830,
                GovernmentBurdenPool = "OHDEVNET",
                GovernmentBurdenPoolId = 3829,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = true,
                RateDescription = string.Empty,
                ResourceClass = string.Empty,
                ResourceClassId = 0,
                RateDescription1 = "ATLO Core NorthEast Dev Lvl 1&2",
                ResourceClass1 = "Labor-Core-Mission_Solutions-ATLO(1)",
                ResourceClassId1 = 30,
                RateDescription2 = "ENG Core NorthEast Dev Lvl 1&2",
                ResourceClass2 = "Labor-Core-Mission_Solutions-Engineering(2)",
                ResourceClassId2 = 37,
                RateDescription3 = "LABS Core NorthEast Dev Lvl 1&2",
                ResourceClass3 = "Labor-Core-Mission_Solutions-Labs(3)",
                ResourceClassId3 = 38,
                RateDescription4 = "OTHER Core NorthEast Dev Lvl 1&2",
                ResourceClass4 = "Labor-Core-Mission_Solutions-Other(4)",
                ResourceClassId4 = 39,
                RateDescription5 = "QUAL Core NorthEast Dev Lvl 1&2",
                ResourceClass5 = "Labor-Core-Mission_Solutions-Quality(5)",
                ResourceClassId5 = 40,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};

            testRateCodes["CASPRNET"] = new RateDetailModelView
            {
                Id = 13,
                RateCode = "CASPRNET",
                RateCategoryDescription = "FCCOM",
                RateCategory = RateCategory.Fccom,
                ResourceType = DirectRateMappingResourceType.None,
                Section = 27196,
                RateType = RateType.NotSet,
                Description = "Production FCCOM",
                CommercialBurdenPool = string.Empty,
                CommercialBurdenPoolId = 0,
                GovernmentBurdenPool = string.Empty,
                GovernmentBurdenPoolId = 0,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = string.Empty,
                ResourceClass = string.Empty,
                ResourceClassId = 0,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};
            testRateCodes["Travel SERV Esc"] = new RateDetailModelView
            {
                Id = 14,
                RateCode = "Travel SERV Esc",
                RateCategoryDescription = "Non-Labor Escalation Factor",
                RateCategory = RateCategory.NonLaborEscalationFactor,
                ResourceType = DirectRateMappingResourceType.Travel,
                Section = 0,
                RateType = RateType.Cost,
                Description = "Services Autogroup Travel Escalation only ***",
                CommercialBurdenPool = "SERVNLG&APOOL-G",
                CommercialBurdenPoolId = 3874,
                GovernmentBurdenPool = "SERVNLG&APOOL",
                GovernmentBurdenPoolId = 3873,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = "Services Autogroup Travel Escalation only ***",
                ResourceClass = "Travel-Services",
                ResourceClassId = 55,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};
            testRateCodes["NLBESCCH"] = new RateDetailModelView
            {
                Id = 15,
                RateCode = "NLBESCCH",
                RateCategoryDescription = "Non-Labor Escalation Percentage",
                RateCategory = RateCategory.NonLaborEscalationPercentage,
                ResourceType = DirectRateMappingResourceType.Other,
                Section = 0,
                RateType = RateType.Cost,
                Description = "Non-Labor Escalation (IHS Global Insight)",
                CommercialBurdenPool = string.Empty,
                CommercialBurdenPoolId = 0,
                GovernmentBurdenPool = string.Empty,
                GovernmentBurdenPoolId = 0,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = "Autogroup NonLabor Escalation only ***",
                ResourceClass = "Other-Core",
                ResourceClassId = 48,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};
            testRateCodes["OHDEVNET"] = new RateDetailModelView
            {
                Id = 16,
                RateCode = "OHDEVNET",
                RateCategoryDescription = "Overhead",
                RateCategory = RateCategory.Overhead,
                ResourceType = DirectRateMappingResourceType.None,
                Section = 27174,
                RateType = RateType.NotSet,
                Description = "Development OH",
                CommercialBurdenPool = string.Empty,
                CommercialBurdenPoolId = 0,
                GovernmentBurdenPool = string.Empty,
                GovernmentBurdenPoolId = 0,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = string.Empty,
                ResourceClass = string.Empty,
                ResourceClassId = 0,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};
            testRateCodes["541760NL"] = new RateDetailModelView
            {
                Id = 17,
                RateCode = "541760NL",
                RateCategoryDescription = "Service Center",
                RateCategory = RateCategory.ServiceCenter,
                ResourceType = DirectRateMappingResourceType.Other,
                Section = 27231,
                RateType = RateType.Cost,
                Description = "Commercial Civil Space MSC Non-Labor",
                CommercialBurdenPool = string.Empty,
                CommercialBurdenPoolId = 0,
                GovernmentBurdenPool = string.Empty,
                GovernmentBurdenPoolId = 0,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = "Commercial Civil Space Service Center",
                ResourceClass = "Service_Center-Non-Labor",
                ResourceClassId = 51,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};
            testRateCodes["Mileage Serv"] = new RateDetailModelView
            {
                Id = 18,
                RateCode = "Mileage Serv",
                RateCategoryDescription = "Travel Mlge",
                RateCategory = RateCategory.TravelMlge,
                ResourceType = DirectRateMappingResourceType.Travel,
                Section = 0,
                RateType = RateType.Cost,
                Description = "Services Personal Car Travel $/Mile",
                CommercialBurdenPool = "SERVNLG&APOOL-G",
                CommercialBurdenPoolId = 3874,
                GovernmentBurdenPool = "SERVNLG&APOOL",
                GovernmentBurdenPoolId = 3873,
                ProPricerMappings = null,
                Values = new Collection<RateYearModelView>(),
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = "Services Personal Car Travel $/Mile",
                ResourceClass = "Other-Services",
                ResourceClassId = 49,
                RateDescription1 = string.Empty,
                ResourceClass1 = string.Empty,
                ResourceClassId1 = 0,
                RateDescription2 = string.Empty,
                ResourceClass2 = string.Empty,
                ResourceClassId2 = 0,
                RateDescription3 = string.Empty,
                ResourceClass3 = string.Empty,
                ResourceClassId3 = 0,
                RateDescription4 = string.Empty,
                ResourceClass4 = string.Empty,
                ResourceClassId4 = 0,
                RateDescription5 = string.Empty,
                ResourceClass5 = string.Empty,
                ResourceClassId5 = 0,
                RateDescription6 = string.Empty,
                ResourceClass6 = string.Empty,
                ResourceClassId6 = 0,
                RateDescription7 = string.Empty,
                ResourceClass7 = string.Empty,
                ResourceClassId7 = 0,
                RateDescription8 = string.Empty,
                ResourceClass8 = string.Empty,
                ResourceClassId8 = 0,
				RateDescription9 = string.Empty,
				ResourceClass9 = string.Empty,
				ResourceClassId9 = 0
			};

            rateConfigLoader = new Mock<IRateConfigLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRateConfigLoader), rateConfigLoader.Object);
            this.rateConfigLoader.Setup(x => x.GetAll()).Returns(TestData.GetRateConfigTestData());
        }

        /// <summary>
        /// Tests Replication of the rate codes.
        /// </summary>
        [TestMethod]
        public void ReplicateRates()
        {

            RateControllerLogic sut = this.CreateSut();

            List<RateDetailModelView> importedRates = new List<RateDetailModelView>
            {
                new RateDetailModelView
                {
                    RateCode = "FROM",
                    Values = new List<RateYearModelView>
                    {
                        new RateYearModelView
                        {
                            Value = 10,
                            Year = 2018
                        }
                    }
                }
            };

            sut.ReplicateRateCodes(importedRates);

            Assert.AreEqual(2, importedRates.Count);
            RateDetailModelView replicated = importedRates.FirstOrDefault(r => r.RateCode == "TO");
            Assert.IsNotNull(replicated);
            Assert.AreEqual(1, replicated.Values.Count);
            Assert.AreEqual(10, replicated.Values.First().Value);
        }

        [TestMethod]
        public void ValidateRateDetailModelViewsPrecisionAndYears()
        {
            RateControllerLogic sut = this.CreateSut();
            Collection<RateDetailModelView> validRateDetails = new Collection<RateDetailModelView>();
            Collection<RateDetailModelView> invalidRateDetails = new Collection<RateDetailModelView>();

            int id = 1;
            string rateCode = "TESTRC";
            decimal rate = 0m;
            int ratePrecision = 0;
            foreach (RateCategory category in Enum.GetValues(typeof(RateCategory)))
            {
                rate = 1.123456789m;      // Make rate invalid.
                invalidRateDetails.Add(new RateDetailModelView() { Id = id++, RateCode = rateCode, RateCategory = category, Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate, Dirty = true } } });

                // Round the rate to the correct precision.
                ratePrecision = RateFormatter.GetRatePrecision(RateTarget.Rate, category.GetDescription());
                rate = decimal.Round((decimal)rate, ratePrecision);

                validRateDetails.Add(new RateDetailModelView() { Id = id++, RateCode = rateCode, RateCategory = category, Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate, Dirty = true } } });
            }

            ICollection<ValidationMessage> validMessages = sut.ValidateRateDetailModelViews(validRateDetails, 2012, 2012);
            ICollection<ValidationMessage> invalidMessages = sut.ValidateRateDetailModelViews(invalidRateDetails, 2012, 2012);
            ICollection<ValidationMessage> invalidYearTooLateMessages = sut.ValidateRateDetailModelViews(validRateDetails, 2010, 2011);   // year 2012 is after range
            ICollection<ValidationMessage> invalidYearTooEarlyMessages = sut.ValidateRateDetailModelViews(validRateDetails, 2013, 2014);   // year 2012 is before range

            // No errors on valid rates.
            Assert.AreEqual(0, validMessages.Count);

            // All RateCategories errored for invalid rates and years.
            Assert.AreEqual(Enum.GetNames(typeof(RateCategory)).Length, invalidMessages.Count);

            foreach (RateCategory category in Enum.GetValues(typeof(RateCategory)))
            {
                // Confirm specific error messages for invalid rates and years
                ratePrecision = RateFormatter.GetRatePrecision(RateTarget.Rate, category.GetDescription());
                string precisionError = string.Format(category == RateCategory.NonLaborEscalationFactor || category == RateCategory.NonLaborEscalationPercentage ?
                    RateMappingValidationConstants.RATEDETAILS_RATEPRECISION_ERROR_ALLOW_NEGATIVE : RateMappingValidationConstants.RATEDETAILS_RATEPRECISION_ERROR,
                    rateCode, 2012, 1.123456789m, category.GetDescription(), ratePrecision);
                Assert.AreEqual(1, invalidMessages.Count(x => x.ValidationIssue.Equals(precisionError)));
            }

            Assert.AreEqual(Enum.GetNames(typeof(RateCategory)).Length, invalidYearTooLateMessages.Count(x => x.ValidationIssue.Equals(string.Format(RateMappingValidationConstants.RATEDETAILS_RATEYEAR_ERROR, rateCode, 2012, 2010, 2011))));
            Assert.AreEqual(Enum.GetNames(typeof(RateCategory)).Length, invalidYearTooEarlyMessages.Count(x => x.ValidationIssue.Equals(string.Format(RateMappingValidationConstants.RATEDETAILS_RATEYEAR_ERROR, rateCode, 2012, 2013, 2014))));
        }

        /// <summary>
        /// Test ValidateRateProPricerMappings for valid rate details, as called through ValidateRateDetailModelViews
        /// </summary>
        [TestMethod]
        public void TestValidateRateProPricerMappings_Valid()
        {
            RateControllerLogic sut = this.CreateSut();

            ICollection<RateYearModelView> values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = 1.1m, Dirty = true } };

            RateDetailModelView rateDetails = new RateDetailModelView()
            {
                RateCategory = RateCategory.DirectLabor,
                RatePrecision = 1,
                Values = values,
                RateCode = "TEST",
                CommercialBurdenPoolId = 1,
                GovernmentBurdenPoolId = 1,
                RateType = RateType.Hours,
                ResourceType = DirectRateMappingResourceType.Labor,
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = "Test Rate"
            };
            
            ICollection<ValidationMessage> result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should not return any validation messages
            Assert.IsFalse(result.Any());

            // Testing that a rate code ending in "NL" is allowed to not have burden pools populated
            rateDetails.RateCode = "TESTNL";
            rateDetails.CommercialBurdenPoolId = 0;
            rateDetails.GovernmentBurdenPoolId = 0;

            // And test when GenerateAdditionalDirectLaborRates is true
            rateDetails.GenerateAdditionalDirectLaborRates = true;
            rateDetails.ResourceType = DirectRateMappingResourceType.Labor;
            rateDetails.RateDescription = string.Empty;
            rateDetails.RateDescription1 = "Test1";

            result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should not return any validation messages
            Assert.IsFalse(result.Any());

            // Similar to above, test for unpopulated burden pools for approved rate code
            // Approved rate codes include Mileage, NLBESCCH, Travel Escalation, Travel Factor, TRAVLESC
            rateDetails.RateCode = "TESTMileage";

            result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should not return any validation messages
            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Test ValidateRateProPricerMappings for invalid rate details, as called through ValidateRateDetailModelViews
        /// </summary>
        [TestMethod]
        public void TestValidateRateProPricerMappings_Invalid()
        {
            RateControllerLogic sut = this.CreateSut();

            ICollection<RateYearModelView> values =
                new Collection<RateYearModelView>() {new RateYearModelView() {Year = 2012, Value = 1.1m, Dirty = true}};

            RateDetailModelView rateDetails = new RateDetailModelView()
            {
                RateCategory = RateCategory.DirectLabor,
                RatePrecision = 1,
                Values = values,
                RateCode = "TEST",
                CommercialBurdenPoolId = 1,
                GovernmentBurdenPoolId = 0,
                RateType = RateType.NotSet,
                ResourceType = DirectRateMappingResourceType.None,
                GenerateAdditionalDirectLaborRates = false,
                RateDescription = string.Empty
            };

            ICollection<ValidationMessage> result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should have validation messages for gov't burden pool not set (when commerical is), rate type not set, resource type not set, and description not set
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.ElementAt(0).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_GOVERNMENT_BURDENPOOL_REQUIRED));
            Assert.IsTrue(result.ElementAt(1).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_RATETYPE_REQUIRED));
            Assert.IsTrue(result.ElementAt(2).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_RESOURCETYPE_REQUIRED));
            Assert.IsTrue(result.ElementAt(3).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_DESCRIPTION_REQUIRED));

            // Fix above issues and set to test commerical burden pool not set (when gov't is) and addition descriptions not set
            rateDetails.CommercialBurdenPoolId = 0;
            rateDetails.GovernmentBurdenPoolId = 1;
            rateDetails.RateType = RateType.Hours;
            rateDetails.ResourceType = DirectRateMappingResourceType.Labor;
            rateDetails.GenerateAdditionalDirectLaborRates = true;

            result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should have validation messages for commercial burden pool not set (when gov't is) and additional descriptions not set
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ElementAt(0).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_BURDENPOOL_REQUIRED));
            Assert.IsTrue(result.ElementAt(1).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED));

            // Fix above issues and test both burden pools not set (for unapproved rate code)
            rateDetails.GovernmentBurdenPoolId = 0;
            rateDetails.RateDescription1 = "Test1";

            result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);

            // Should have validation messages for commercial burden pool not set (when gov't is) and additional descriptions not set
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ElementAt(0).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_AND_GOVERNMENT_BURDENPOOL_REQUIRED));

            // Fix above issues and test case where RateType and ResourceType are populated, but no descriptions are supplied
            rateDetails.CommercialBurdenPoolId = 1;
            rateDetails.GovernmentBurdenPoolId = 1;
            rateDetails.RateDescription1 = string.Empty;

            result = sut.ValidateRateDetailModelViews(new Collection<RateDetailModelView>() { rateDetails }, 2012, 2012);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ElementAt(0).ValidationIssue.Contains(RateMappingValidationConstants.RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED));
        }

        /// <summary>
        /// Tests validating imported rate codes with null existing rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateImportedRatesTest_EX1()
        {
            var sut = CreateSut();
            sut.ValidateImportedRates(null, new Collection<RateDetailModelView>());
        }

        /// <summary>
        /// Tests validating imported rate codes with null imported rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateImportedRatesTest_EX2()
        {
            var sut = CreateSut();
            sut.ValidateImportedRates(new Collection<RateDetailModelView>(), null);
        }

        /// <summary>
        /// Tests validating imported rate codes with missing or duplicate rate codes.
        /// </summary>
        [TestMethod]
        public void ValidateImportedRatesTest()
        {
            var sut = CreateSut();
            Collection<RateDetailModelView> originals = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView { RateCode= "RateCode1", RateCategoryDescription = "Direct Labor"},
                new RateDetailModelView { RateCode= "RateCode2", RateCategoryDescription = "Direct Labor" },
                new RateDetailModelView { RateCode= "RateCode2", RateCategoryDescription = "Escalation A" } // duplicate rate code with different category (not supported)
            };

            Collection<RateDetailModelView> imported = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView { RateCode= "RateCode1" },
                new RateDetailModelView { RateCode= "RateCode2" },
                new RateDetailModelView { RateCode= "RateCode3" }   // imported rate code not in original set of rate codes
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRates(originals, imported);
            Assert.AreEqual(2, validationMessages.Count);
            string expectedMessage1 = string.Format(RateMappingValidationConstants.RATEDETAILS_DUPLICATE_RATECODE, "RateCode2");
            string expectedMessage2 = string.Format(RateMappingValidationConstants.RATEDETAILS_RATECODE_MISSING, "RateCode3");
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Equals(expectedMessage1)));
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Equals(expectedMessage2)));

            // Validate the RateCategoryDescription was updated as expected in the imported rate details 
            Assert.IsTrue(imported.First(x => x.RateCode.Equals("RateCode1")).RateCategoryDescription.Equals("Direct Labor"));
            Assert.IsTrue(string.IsNullOrEmpty(imported.First(x => x.RateCode.Equals("RateCode2")).RateCategoryDescription));
            Assert.IsTrue(string.IsNullOrEmpty(imported.First(x => x.RateCode.Equals("RateCode3")).RateCategoryDescription));
        }

        /// <summary>
        /// Tests importing rates with null existing rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadImportedRatesTest_EX1()
        {
            var sut = CreateSut();
            sut.LoadImportedRates(null, new Collection<RateDetailModelView>());
        }

        /// <summary>
        /// Tests importing rates with null imported rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void LoadImportedRatesTest_EX2()
        {
            var sut = CreateSut();
            sut.LoadImportedRates(new Collection<RateDetailModelView>(), null);
        }

        /// <summary>
        /// Tests importing rates with wrong count of imported rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void LoadImportedRatesTest_EX3()
        {
            var sut = CreateSut();
            Collection<RateDetailModelView> originals = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView { RateCode= "RateCode1" }
            };

            Collection<RateDetailModelView> imported = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView { RateCode= "RateCode1" },
                new RateDetailModelView { RateCode= "RateCode2" }
            };

            ICollection <RateDetailModelView> updated = sut.LoadImportedRates(originals, imported);
        }

        /// <summary>
        /// Tests Import Rate Loader.
        /// </summary>
        [TestMethod]
        public void LoadImportedRatesTest()
        {
            var sut = CreateSut();
            Collection<RateDetailModelView> originals = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView()
                {
                    RateCode = "RateCode1",
                    Id = 1,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 1,
                            Year = 2017,
                            Value = 1,
                            Dirty = false
                        }
                    }
                },
                new RateDetailModelView()
                {
                    RateCode = "RateCode2",
                    Id = 2,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 2,
                            Year = 2017,
                            Value = 2,
                            Dirty = false
                        }
                    }
                },
                new RateDetailModelView()
                {
                    RateCode = "RateCode3",
                    Id = 3,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 3,
                            Year = 2017,
                            Value = 3,
                            Dirty = false
                        }
                    }
                }
            };

            Collection<RateDetailModelView> imported = new Collection<RateDetailModelView>()
            {
                new RateDetailModelView()
                {
                    RateCode = "RateCode1",
                    Id = 1,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 1,
                            Year = 2017,
                            Value = 1,
                            Dirty = false
                        }
                    }
                },
                new RateDetailModelView()
                {
                    RateCode = "RateCode2",
                    Id = 2,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 2,
                            Year = 2017,
                            Value = 2,
                            Dirty = false
                        },
                        new RateYearModelView()
                        {
                            Id = 4,
                            Year = 2018,
                            Value = 20,
                            Dirty = true
                        }
                    }
                },
                new RateDetailModelView()
                {
                    RateCode = "RateCode3",
                    Id = 3,
                    Values = new Collection<RateYearModelView> {
                        new RateYearModelView()
                        {
                            Id = 3,
                            Year = 2017,
                            Value = 4,
                            Dirty = true
                        }
                    }
                }
            };

            ICollection<RateDetailModelView> updated = sut.LoadImportedRates(originals, imported);

            Assert.AreEqual(2, updated.Count);
            Assert.AreEqual(2, updated.Where(u => u.Updateable == UpdateType.Upsert).Count());
            Assert.IsTrue(updated.Any(u => u.Id == 2));
            Assert.IsTrue(updated.Any(u => u.Id == 3));
        }

        /// <summary>
        /// Tests validating imported rate codes with null existing rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateImportedRateCodesTest_EX1()
        {
            var sut = CreateSut();
            RateDetailModelView[] imported = new RateDetailModelView[] { };
            sut.ValidateImportedRates(null, imported);
        }

        /// <summary>
        /// Tests validating imported rate codes with null imported rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateImportedRateCodesTest_EX2()
        {
            var sut = CreateSut();
            RateDetailModelView[] existing = new RateDetailModelView[] { };
            sut.ValidateImportedRates(existing, null);
        }

        /// <summary>
        /// Tests validating imported rate codes without any errors.
        /// </summary>
        [TestMethod]
        public void ValidateImportedRateCodes_HappyPath()
        {
            RateDetailModelView[] imported = testRateCodes.Values.ToArray();

            var sut = CreateSut();
            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(0, validationMessages.Count,
                "Validator should generate no errors since the Rate Codes all valid");
        }

        /// <summary>
        /// Test required fields validation
        /// </summary>
        [TestMethod]
        public void ValidateImportedRateCodes_RequiredFields()
        {
            RateDetailModelView[] imported = new RateDetailModelView[]
            {
                new RateDetailModelView { RateCode= "RateCode1", RateCategoryDescription = "Direct Labor" },
                new RateDetailModelView { RateCode= "RateCode2", RateCategoryDescription = "Direct Labor" },
                new RateDetailModelView { }
            };

            var sut = CreateSut();
            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(11, validationMessages.Count,
                "Validator should generate 11 errors since 2 rows have 3 missing fields each, and the last row has 5 missing fields");
        }

        /// <summary>
        /// Tests validating imported rate codes with duplicates.
        /// </summary>
        [TestMethod]
        public void ValidateImportedRateCodes_RateCode_NotUnique()
        {
            var sut = CreateSut();

            // Test duplicate rate codes (within imported rate codes)
            RateDetailModelView[] imported = new RateDetailModelView[]
            {
                testRateCodes["FXDDAB"],
                testRateCodes["C1NDAB"],
                testRateCodes["C1NDAB"]     // duplicate rate code
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(2, validationMessages.Count,
                "Validator should generate 2 errors one for each row where C1NDAB is not unique");
        }

        /// <summary>
        /// Tests validating imported rate codes with invalid ProPricer mappings.
        /// </summary>
        [TestMethod]
        public void ValidateImportedRateCodes_ProPricerMappings_Invalid()
        {
            var sut = CreateSut();

            // test case where both ProPricer base and extended descriptions are populated 
            RateDetailModelView[] imported = new RateDetailModelView[] { testRateCodes["FXDDAB"] };
            imported[0].RateDescription = "Invalid";    // not allowed since extensions 1-7 are also populated
            imported[0].ResourceClass = "Invalid";
            imported[0].ResourceClassId = 999;

            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(1, validationMessages.Count,
                "Validator should generate errors for invalid ProPricer mappings");

            // test case where a ProPricer Resource Class is specified without a corresponding description
            imported[0].RateDescription = string.Empty;
            imported[0].ResourceClass = string.Empty;
            imported[0].ResourceClassId = 0;
            imported[0].RateDescription1 = string.Empty;    // not allowed whenever the corresponding ResourceClass is populated
            imported[0].RateDescription3 = string.Empty;    // not allowed whenever the corresponding ResourceClass is populated

            validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(1, validationMessages.Count,
                "Validator should generate errors for invalid ProPricer mappings");
        }

        /// <summary>
        /// Tests validating imported rate codes where a ProPricer additional resource class is specified but ResourceType is not Labor.
        /// </summary>
        [TestMethod]
        public void ValidateImportedRateCodes_ProPricerMappings_InvalidResourceType()
        {
            var sut = CreateSut();

            // test case where ProPricer extended descriptions are populated, but ResourceType is something other than Labor.
            RateDetailModelView[] imported = new RateDetailModelView[] { testRateCodes["FXDDAB"] };
            imported[0].ResourceType = DirectRateMappingResourceType.Travel;    // not allowed when extensions 1-7 are also populated

            ICollection<ValidationMessage> validationMessages = sut.ValidateImportedRateCodes(this.originals, imported);
            Assert.AreEqual(1, validationMessages.Count,
                "Validator should generate errors for ProPricer additional mappings with Resource Type not equal to Labor");
        }

        /// <summary>
        /// Tests importing rate codes with null existing rates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void LoadImportedRateCodesTest_EX1()
        {
            var sut = CreateSut();
            sut.LoadImportedRateCodes(null);
        }

        /// <summary>
        /// Tests getting updated rate codes when there are duplicate (existing) rate codes.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void GetUpdatedRateCodesTest_EX1()
        {
            RateDetailModelView[] imported = this.originals.DeepClone();

            var sut = CreateSut();
            RateDetailModelView[] existing = this.originals.ToArray();
            existing[existing.Length - 1].RateCode = existing[0].RateCode;  // duplicate rate codes

            sut.GetUpdatedRateCodes(existing, imported);
        }

        /// <summary>
        /// Tests get updated rate codes - changes to existing.
        /// </summary>
        [TestMethod]
        public void GetUpdatedRateCodesTest_UpdateExistingOnly()
        {
            RateDetailModelView[] imported = this.originals.DeepClone();

            // make some changes to imported rate codes
            foreach (RateDetailModelView rateCode in imported)
            {
                switch (rateCode.RateCode)
                {
                    case "FXEDAA":
                        rateCode.Description = "Modified " + rateCode.Description;
                        break;
                    case "541976LB":
                        rateCode.RateType = RateType.Hours; // originally RateType.Cost
                        break;
                    case "TRAVLOTC":
                        rateCode.GovernmentBurdenPoolId = 11111;
                        rateCode.CommercialBurdenPoolId = 22222;
                        break;
                }
            }

            var sut = CreateSut();
            ICollection<RateDetailModelView> updated = sut.GetUpdatedRateCodes(this.originals, imported);
            Assert.AreEqual(3, updated.Count);
            Assert.AreEqual(3, updated.Where(u => u.Updateable == UpdateType.Upsert).Count());
            Assert.IsTrue(updated.Any(u => u.Id == this.originals[0].Id));
            Assert.IsTrue(updated.Any(u => u.Id == this.originals[2].Id));
            Assert.IsTrue(updated.Any(u => u.Id == this.originals[3].Id));
        }

        /// <summary>
        /// Tests get updated rate codes - inserts only.
        /// </summary>
        [TestMethod]
        public void GetUpdatedRateCodesTest_InsertNewOnly()
        {
            RateDetailModelView[] imported = testRateCodes.Values.ToArray();

            var sut = CreateSut();
            ICollection<RateDetailModelView> updated = sut.GetUpdatedRateCodes(this.originals, imported);
            Assert.AreEqual(testRateCodes.Count, updated.Count);
        }

        /// <summary>
        /// Tests get updated rate codes - no changes.
        /// </summary>
        [TestMethod]
        public void GetUpdatedRateCodesTest_NoChanges()
        {
            RateDetailModelView[] imported = this.originals.ToArray();

            var sut = CreateSut();
            ICollection<RateDetailModelView> updated = sut.GetUpdatedRateCodes(this.originals, imported);
            Assert.AreEqual(0, updated.Count);
        }

        /// <summary>
        /// Tests get updated rate codes - inserts and updates.
        /// </summary>
        [TestMethod]
        public void GetUpdatedRateCodesTest_InsertAndUpdate()
        {
            // combine originals + testRateCodes into a single collection for import
            Collection<RateDetailModelView> importedCollection = this.originals.DeepClone().ToCollection();
            importedCollection.AddRange(testRateCodes.Values.DeepClone().ToCollection());
            RateDetailModelView[] imported = importedCollection.ToArray();

            // make some changes to original rate codes
            foreach (RateDetailModelView rateCode in imported)
            {
                switch (rateCode.RateCode)
                {
                    case "FXEDAA":
                        rateCode.RateDescription = "Testing";
                        rateCode.ResourceClass = "Testing-Core-ATLO(1)";
                        rateCode.ResourceClassId = 222;
                        rateCode.RateDescription1 = string.Empty;
                        rateCode.ResourceClass1 = string.Empty;
                        rateCode.ResourceClassId1 = null;
                        rateCode.RateDescription2 = string.Empty;
                        rateCode.ResourceClass2 = string.Empty;
                        rateCode.ResourceClassId2 = null;
                        rateCode.RateDescription3 = string.Empty;
                        rateCode.ResourceClass3 = string.Empty;
                        rateCode.ResourceClassId3 = null;
                        rateCode.RateDescription4 = string.Empty;
                        rateCode.ResourceClass4 = string.Empty;
                        rateCode.ResourceClassId4 = null;
                        rateCode.RateDescription5 = string.Empty;
                        rateCode.ResourceClass5 = string.Empty;
                        rateCode.ResourceClassId5 = null;
                        break;
                    case "XXJDAB":
                        rateCode.RateCategory = RateCategory.Fccom; // originally RateCategory.DirectLabor
                        break;
                    case "541976LB":
                        rateCode.RateDescription = "Hello world";
                        rateCode.ResourceClassId = 111;
                        break;
                }
            }

            var sut = CreateSut();
            ICollection<RateDetailModelView> updated = sut.GetUpdatedRateCodes(this.originals, imported);
            Assert.AreEqual(testRateCodes.Count + 3, updated.Count);
            Assert.AreEqual(testRateCodes.Count + 3, updated.Where(u => u.Updateable == UpdateType.Upsert).Count());
        }

        /// <summary>
        /// Test GetRatesVersionDifferences
        /// </summary>
        [TestMethod]
        public void GetRatesVersionDifferencesTest()
        {
            RateControllerLogic sut = this.CreateSut();

            RevisionOptionModelView revision1 = new RevisionOptionModelView() { Id = 5, Label = "WIP", Revision = "5", StartYear = 2010, EndYear = 2040 };
            RevisionOptionModelView revision2 = new RevisionOptionModelView() { Id = 2, Label = "2", Revision = "2", StartYear = 2010, EndYear = 2040 };

            ICollection<RateDetailModelView> expected = new Collection<RateDetailModelView>() { new RateDetailModelView() { Id = 1, RateCode = "TEST",  } };

            rateDetailLoader.Setup(x => x.GetComparableRates(revision1, revision2)).Returns(expected);

            ICollection<RateDetailModelView> result = sut.GetRatesVersionDifferences(new Collection<RevisionOptionModelView>() { revision1, revision2 }, revision1.Id, revision2.Id);

            Assert.IsTrue(result.Any());
            Assert.AreEqual(expected.Count, result.Count);
            Assert.AreEqual(expected.First().Id, result.First().Id);
            Assert.AreEqual(expected.First().RateCode, result.First().RateCode);
        }
    }
}
