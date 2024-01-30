// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Reporting
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEStatusReportTest : MOQObject
    {
        private Mock<ICommonDataMapper> _ICommonDataMapper = null;
        private Mock<IUserDTODataLoader> _IUserDTODataLoader = null;
        private Mock<IVariableSelectBOEtoSumCalculation> _IVariableSelectBOEtoSumCalculation = null;
        private Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = null;
        private Mock<TravelTripCostCalculation> _TravelTripCostCalculator = null;
        
        private BOEStatusReport CreateSystem()
        {
            _ICommonDataMapper = new Mock<ICommonDataMapper>();
            _IUserDTODataLoader = new Mock<IUserDTODataLoader>();
            _IVariableSelectBOEtoSumCalculation = new Mock<IVariableSelectBOEtoSumCalculation>();
            _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            
            _TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();


            return new BOEStatusReport(
                _ICommonDataMapper.Object,
                _IUserDTODataLoader.Object,
                _IVariableSelectBOEtoSumCalculation.Object,
                _IPermissionsDTOLoader.Object,
                _TravelTripCostCalculator.Object);
        }

        private BOEStatusReport CreateSystemSpaceSystems()
        {
            _ICommonDataMapper = new Mock<ICommonDataMapper>();
            _IUserDTODataLoader = new Mock<IUserDTODataLoader>();
            _IVariableSelectBOEtoSumCalculation = new Mock<IVariableSelectBOEtoSumCalculation>();
            _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            _TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();
            

            return new BOEStatusReportSpaceSystems(
                _ICommonDataMapper.Object,
                _IUserDTODataLoader.Object,
                _IVariableSelectBOEtoSumCalculation.Object,
                _IPermissionsDTOLoader.Object,
                _TravelTripCostCalculator.Object);
        }

        [TestMethod]
        public void GetResourceTypesToBeSummedTest()
        {
			BOEStatusReport boeStatusReport = CreateSystem();
            Collection<int> resourceTypes = boeStatusReport.GetResourceTypesToBeSummed();
            Assert.AreEqual(6, resourceTypes.Count);
        }

        [TestMethod]
        public void GetResourceTypesToBeSummedTestSpaceSystems()
        {
			BOEStatusReport boeStatusReport = CreateSystemSpaceSystems();
            Collection<int> resourceTypes = boeStatusReport.GetResourceTypesToBeSummed();
            Assert.AreEqual(3, resourceTypes.Count);
        }
    }
}