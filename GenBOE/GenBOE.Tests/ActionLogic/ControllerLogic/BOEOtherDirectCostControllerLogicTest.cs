// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;
    using GenBOE.Objects;
    using GenBOE.DataBridge.DTO;
    using Moq;
    using IES.Common.Exceptions;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;

    /// <summary>
    /// Action Logic test for BOEOtherDirectCostControllerLogic class.
    /// </summary>
    [TestClass]
    public class BOEOtherDirectCostControllerLogicTest
    {

        private BOEOtherDirectCostControllerLogic otherDirectCostControllerLogic;
        private Mock<IOtherDirectCostDTODataLoader> otherDirectCostDTOLoader;
        private Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader;
        private Mock<ICommonDataMapper> _commonDataMapper;


        /// <summary>
        /// Initializes data before each test run for this class. 
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.otherDirectCostDTOLoader = new Mock<IOtherDirectCostDTODataLoader>();
            this.otherDirectCostControllerLogic = new BOEOtherDirectCostControllerLogic();
            _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
            _commonDataMapper = new Mock<ICommonDataMapper>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
        }

        /// <summary>
        /// Test the PopulateCompanySpecificProperties method with a valid case
        /// </summary>
        [TestMethod]
        public void PopulateCompanySpecificPropertiesTest()
        {
            //create odcelement model
            ODCElementDetailsModelView ODC = new ODCElementDetailsModelView
            {
                ODCID = 1,
                StartDate = "01/2015",
                EndDate = "02/2015"
            };
            Assert.IsNull(ODC.MOQTextLabel);

            this.otherDirectCostControllerLogic.PopulateCompanySpecificProperties(ODC);
            Assert.IsNotNull(ODC.MOQTextLabel);
        }

        /// <summary>
        /// Test the PopulateCompanySpecificProperties method with a null case
        /// </summary>
        [TestMethod]
        public void PopulateCompanySpecificPropertiesTest_Null()
        {
            //create empty model view
            ODCElementDetailsModelView ODC = null;

            //run method
            this.otherDirectCostControllerLogic.PopulateCompanySpecificProperties(ODC);

            Assert.IsNull(ODC);
        }
    }
}
