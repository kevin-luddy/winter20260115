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
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.Mediator;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Security;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	[TestClass]
    public class BurdenPoolControllerLogicTest
    {
        /// <summary>
        /// Mock Revision Loader
        /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<IActiveDirectoryService> adUtils = new();

        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        public BurdenPoolControllerLogic CreateSut()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null, null);
            return new BurdenPoolControllerLogic(this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBurdenPools_EX1()
        {
            BurdenPoolControllerLogic sut = this.CreateSut();
            sut.ValidateBurdenPools(null);
        }

        [TestMethod]
        public void ValidateBurdenPools_NoErrors()
        {
            BurdenPoolControllerLogic sut = this.CreateSut();
            Collection<BurdenPoolDetailModelView> burdenPools = new();
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool1",
                Description = "Test1"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool2",
                Description = "Test2"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool3",
                Description = "Test3"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool4",
                IsDeleted = true
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool5",
                Description = "Test5",
                IsDeleted = true
            });
            ICollection<ValidationMessage> messages = sut.ValidateBurdenPools(burdenPools);

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void ValidateBurdenPools_RequiredFields()
        {
            BurdenPoolControllerLogic sut = this.CreateSut();
            Collection<BurdenPoolDetailModelView> burdenPools = new();
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool1",
                Description = "Test1"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool2"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool3"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                Description = "Test4"
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool4",
                IsDeleted = true
            });
            burdenPools.Add(new BurdenPoolDetailModelView
            {
                BurdenPool = "BurdenPool5",
                Description = "Test5",
                IsDeleted = true
            });
            ICollection<ValidationMessage> messages = sut.ValidateBurdenPools(burdenPools);

            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages.ElementAt(0).ValidationIssue.Equals("One or more rows are missing the required Burden Pool field."));
            Assert.IsTrue(messages.ElementAt(1).ValidationIssue.Equals("The Description field is required for the following Burden Pool(s): BurdenPool2, BurdenPool3."));
        }
    }
}
