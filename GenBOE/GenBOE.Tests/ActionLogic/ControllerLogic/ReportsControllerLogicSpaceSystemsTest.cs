// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using DataBridge.Reference;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReportsControllerLogicSpaceSystemsTest
    {
        Mock<IBOEExporter> boeExporter = new Mock<IBOEExporter>();
        
        Mock<BOESummary> boeSummary;
        Mock<IBOECustomExporter> boeCustomeExporter = new Mock<IBOECustomExporter>();
        Mock<IWorkspaceExportFormatDTODataLoader> workspaceExportFormatDTOLoader = new Mock<IWorkspaceExportFormatDTODataLoader>();

        Mock<IFullObjectFactory> Factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> _retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();

        Mock<TravelTripCostCalculation> _TravelTripCostCalculator;
        Mock<RMSZoneTravelRatesFeesDataLoader> _RMSZoneTravelRatesFeesDataLoader;
        Mock<BOEDiscrepancyReport> boeDiscrepancyReport = new Mock<BOEDiscrepancyReport>(new Mock<IFullWorkspaceRecalculation>().Object, new Mock<IUserDTODataLoader>().Object);


        Mock<IResourceDTODataLoader> resourceDTODataLoader = new Mock<IResourceDTODataLoader>();
        Mock<IBOEFormIBOEDTODataLoader> iboeFormDataLoader = new Mock<IBOEFormIBOEDTODataLoader>();
        Mock<IBOEFormPBOEDTODataLoader> pboeFormDataLoader = new Mock<IBOEFormPBOEDTODataLoader>();
        Mock<IInUseDataLoader> iInUseDataLoader = new Mock<IInUseDataLoader>();

        Mock<GenTRAC.DataBridge.DTO.IProposalLoader> proposalLoader = new Mock<GenTRAC.DataBridge.DTO.IProposalLoader>();
        Mock<IWorkspaceControllerLogic> workspaceControllerLogic = new Mock<IWorkspaceControllerLogic>();

        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);

            _TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();
            _RMSZoneTravelRatesFeesDataLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();

            boeSummary = new Mock<BOESummary>(_TravelTripCostCalculator.Object, _RMSZoneTravelRatesFeesDataLoader.Object);
        }

        /// <summary>
        /// This test checks GetMetricNameTaskElementMappingDTO. It will verify that when
        /// FullWorkspace is passed through that it will throw a ArgumentNullException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMetricNameTaskElementMappingDTOSpaceSystemsTestWorkspaceNull()
        {
            //Value Declaration
            FullWorkspace workspace = null;

            ReportsControllerLogicSpaceSystems sut = new ReportsControllerLogicSpaceSystems(boeExporter.Object,
                boeSummary.Object, boeCustomeExporter.Object, workspaceExportFormatDTOLoader.Object,
                boeDiscrepancyReport.Object, resourceDTODataLoader.Object, iboeFormDataLoader.Object,
                pboeFormDataLoader.Object, iInUseDataLoader.Object, this.proposalLoader.Object,
                this.workspaceControllerLogic.Object);
            //ACt
            sut.GetMetricNameTaskElementMappingDTO(workspace);
        }
    }
}
