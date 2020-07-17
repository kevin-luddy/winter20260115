// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ControllerLogic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.DataBridge.DTO;
    using Moq;

    [TestClass]
    public class TravelControllerLogicTest
    {
        /// <summary>
        /// Mock Travel Data Loader
        /// </summary>
        private Mock<ITravelDTODataLoader> inTravelDataLoader = new Mock<ITravelDTODataLoader>();

        /// <summary>
        /// Mock Custom Field Value Loader
        /// </summary>
        private Mock<ICustomFieldValueDTODataLoader> customFieldValueLoader = new Mock<ICustomFieldValueDTODataLoader>();

        /// <summary>
        /// Test the ShowSegmentHelpLink value for the IS&GS configuration
        /// </summary>
        [TestMethod]
        public void ShowSegmentHelpLink()
        {
            TravelControllerLogic sut = new TravelControllerLogic(inTravelDataLoader.Object, customFieldValueLoader.Object);
            Assert.IsTrue(sut.ShowSegmentHelpLink, "The value returned for ShowSegmentHelpLink is incorrect.");
        }

        /// <summary>
        /// Test the ShowSegmentHelpLink value for the SSC configuration
        /// </summary>
        [TestMethod]
        public void ShowSegmentHelpLinkSSC()
        {
            TravelControllerLogicSpaceSystems sut = new TravelControllerLogicSpaceSystems(inTravelDataLoader.Object, customFieldValueLoader.Object);
            Assert.IsFalse(sut.ShowSegmentHelpLink, "The value returned for ShowSegmentHelpLink is incorrect.");
        }

        /// <summary>
        /// Test the ShowSegmentHelpLink value for the MST configuration
        /// </summary>
        [TestMethod]
        public void ShowSegmentHelpLinkMST()
        {
            TravelControllerLogicMST sut = new TravelControllerLogicMST(inTravelDataLoader.Object, customFieldValueLoader.Object);
            Assert.IsFalse(sut.ShowSegmentHelpLink, "The value returned for ShowSegmentHelpLink is incorrect.");
        }

    }
}
