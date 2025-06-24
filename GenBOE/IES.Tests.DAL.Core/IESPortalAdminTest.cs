// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.DataBridge.Core.Picklists;
	using GenTRAC.DataBridge.Core.DTO.PickLists.Common;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.ModelView;
	using IES.ActionLogic.Core.Validation;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.PickList;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// Test class for IES Portal Admin
	/// </summary>
	[TestClass]
	[Ignore("Interfaces for PtmPickListMapper and BoePickListMapper are needed to properly mock them")]
    public class IESPortalAdminTest
    {

        /// <summary>
        /// Pick List Mapper
        /// </summary>
        private Mock<PtmPickListMapper> ptmPickListMapper;

		/// <summary>
		/// Pick List Mapper
		/// </summary>
		private Mock<BoePickListMapper> boePickListMapper;

		/// <summary>
		/// Offline Application Loader
		/// </summary>
		private Mock<IOfflineApplicationLoader> offlineApplicationLoader;

        /// <summary>
        /// Performs setup initialization for the tests.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            this.ptmPickListMapper = new Mock<PtmPickListMapper>();
            this.boePickListMapper = new Mock<BoePickListMapper>();
            this.offlineApplicationLoader = new Mock<IOfflineApplicationLoader>();
        }

        /// <summary>
        /// Creates the system to test.
        /// </summary>
        /// <returns>The system under test.</returns>
        private IESPortalAdminControllerLogic CreateSystem()
        {
            return new IESPortalAdminControllerLogic(this.ptmPickListMapper.Object, this.boePickListMapper.Object, this.offlineApplicationLoader.Object);
        }

        /// <summary>
        /// Test for GetPickListItems
        /// </summary>
        [TestMethod]
        public void C_GetPickListItems()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV grid = new()
			{
                PickLists = data,
                PickListName = PickListEnum.ProposalType.GetDescription()
            };

            this.ptmPickListMapper.Setup(x => x.GetPickListValues(PickListEnum.ProposalType, It.IsAny<bool>())).Returns(grid);

            ICollection<PickListDto> actual = sut.GetPickListItems(PickListEnum.ProposalType).Data.PickLists;

            Assert.AreEqual(actual.First().Id, data.First().Id);
            Assert.AreEqual(actual.First().InUse, data.First().InUse);
            Assert.AreEqual(actual.First().IsActive, data.First().IsActive);
            Assert.AreEqual(actual.First().Text, data.First().Text);
            Assert.IsFalse(actual.First().IsReadOnly);
        }

        /// <summary>
        /// Test Validate Picklist Items
        /// </summary>
        [TestMethod]
        public void C_ValidatePickListItemsTest()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                },
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA"
                },
                new()
                {
                    Id = 444,
                    InUse = false,
                    IsActive = true,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true
                }
            };

            this.ptmPickListMapper.Setup(x => x.GetPickListValues(It.IsAny<PickListEnum>(), It.IsAny<bool>())).Returns(new PickListGridMV { PickLists = data });

            // Delete unused item
            ICollection<PickListModelView> delete1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA",
                    Updateable = UpdateType.Deleted
                }
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalType, delete1);
            Assert.AreEqual(0, validationMessages.Count);

            // Add an item
            ICollection<PickListModelView> update1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 555,
                    InUse = true,
                    IsActive = false,
                    Text = "ImUnique",
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, update1);
            Assert.AreEqual(0, validationMessages.Count);

            // Attempt to delete read-only item
            ICollection<PickListModelView> deleteReadOnly = new List<PickListModelView>()
            {
                new()
                {
                    Id = 444,
                    InUse = true,
                    IsActive = false,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    Updateable = UpdateType.Deleted
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, deleteReadOnly);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(AdminValidationConstants.READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED)));

            // Attempt to update read-only "Forecasted" item
            ICollection<PickListModelView> updateReadOnly = new List<PickListModelView>()
            {
                new()
                {
                    Id = 444,
                    InUse = true,
                    IsActive = true,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.TypeOfRequest, updateReadOnly);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(AdminValidationConstants.READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED)));

            // Attempt to update an item using a duplicate Text value
            ICollection<PickListModelView> duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, duplicate1.First().Text))));

            // Attempt to add an item using a duplicate "FORECASTED" (all upper case) text value
            ICollection<PickListModelView> duplicate2 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 555,
                    InUse = false,
                    IsActive = true,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED.ToUpper(),
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate2);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, CommonConstants.PROPOSAL_CLASS_FORECASTED.ToUpper()))));
        }

        /// <summary>
        /// Test Validate Picklist Items
        /// </summary>
        [TestMethod]
        public void C_ValidatePickListItemsSingleParentTest()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1 }
                },
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA",
                    ParentIds = new int[] {1 }
                },
                new()
                {
                    Id = 444,
                    InUse = false,
                    IsActive = true,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    ParentIds = new int[] { 1 }
                }
            };

            this.ptmPickListMapper.Setup(x => x.GetPickListValues(It.IsAny<PickListEnum>(), It.IsAny<bool>())).Returns(new PickListGridMV { PickLists = data, ContainsParent = true });

            // Delete unused item
            ICollection<PickListModelView> delete1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA",
                    Updateable = UpdateType.Deleted,
                    ParentIds = new int[] {1 }
                }
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalType, delete1);
            Assert.AreEqual(0, validationMessages.Count);

            // Add an item
            ICollection<PickListModelView> update1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 555,
                    InUse = true,
                    IsActive = false,
                    Text = "ImUnique",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, update1);
            Assert.AreEqual(0, validationMessages.Count);

            // Attempt to delete read-only item
            ICollection<PickListModelView> deleteReadOnly = new List<PickListModelView>()
            {
                new()
                {
                    Id = 444,
                    InUse = true,
                    IsActive = false,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    Updateable = UpdateType.Deleted,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, deleteReadOnly);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(AdminValidationConstants.READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED)));

            // Attempt to update an item using a duplicate Text value
            ICollection<PickListModelView> duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, duplicate1.First().Text))));

            // Attempt to update an item using a duplicate Text value in different Parent
            duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {2 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(0, validationMessages.Count);

            // Attempt to update an item without a parent
            duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, duplicate1.First().Text))));
        }

        /// <summary>
        /// Test Validate Picklist Items
        /// </summary>
        [TestMethod]
        public void C_ValidatePickListItemsChildrenTest()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> childrenData = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1 }
                }
            };

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 1,
                    InUse = false,
                    IsActive = false,
                    Text = "Parent"
                }
            };

            this.ptmPickListMapper.Setup(x => x.GetPickListValues(PickListEnum.ProgramArea, It.IsAny<bool>())).Returns(new PickListGridMV { PickLists = childrenData, ContainsParent = true });
            this.ptmPickListMapper.Setup(x => x.GetPickListValues(PickListEnum.LineOfBusiness, It.IsAny<bool>())).Returns(new PickListGridMV { PickLists = data, Children = childrenData, ContainsChildren = true });

            // Delete unused item with a child picklist item
            ICollection<PickListModelView> delete1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 1,
                    InUse = false,
                    IsActive = true,
                    Text = "Parenta",
                    Updateable = UpdateType.Deleted
                }
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidatePickListItems(PickListEnum.LineOfBusiness, delete1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_ITEM_MAY_NOT_BE_DELETED, "Parenta", "Hello World"))));

        }

        /// <summary>
        /// Test Validate Picklist Items
        /// </summary>
        [TestMethod]
        public void C_ValidatePickListItemsMultiParentTest()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                },
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA",
                    ParentIds = new int[] {1 }
                },
                new()
                {
                    Id = 444,
                    InUse = false,
                    IsActive = true,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    ParentIds = new int[] { 2, 3 }
                }
            };

            this.ptmPickListMapper.Setup(x => x.GetPickListValues(It.IsAny<PickListEnum>(), It.IsAny<bool>())).Returns(new PickListGridMV { PickLists = data, ContainsParent = true, AllowsMultipleParents = true });

            // Delete unused item
            ICollection<PickListModelView> delete1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "AaA",
                    Updateable = UpdateType.Deleted,
                    ParentIds = new int[] {1 }
                }
            };

            ICollection<ValidationMessage> validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalType, delete1);
            Assert.AreEqual(0, validationMessages.Count);

            // Add an item
            ICollection<PickListModelView> update1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 555,
                    InUse = true,
                    IsActive = false,
                    Text = "ImUnique",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, update1);
            Assert.AreEqual(0, validationMessages.Count);

            // Attempt to delete read-only item
            ICollection<PickListModelView> deleteReadOnly = new List<PickListModelView>()
            {
                new()
                {
                    Id = 444,
                    InUse = true,
                    IsActive = false,
                    Text = CommonConstants.PROPOSAL_CLASS_FORECASTED,
                    IsReadOnly = true,
                    Updateable = UpdateType.Deleted,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, deleteReadOnly);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(AdminValidationConstants.READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED)));

            // Attempt to update an item using a duplicate Text value
            ICollection<PickListModelView> duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {1 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, duplicate1.First().Text))));

            // Attempt to update an item using a duplicate Text value in different Parent (should still fail)
            duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD",
                    Updateable = UpdateType.Upsert,
                    ParentIds = new int[] {4 }
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, duplicate1.First().Text))));


            // Attempt to update an item without a parent
            duplicate1 = new List<PickListModelView>()
            {
                new()
                {
                    Id = 333,
                    InUse = false,
                    IsActive = true,
                    Text = "hElLo wOrLD3",
                    Updateable = UpdateType.Upsert
                }
            };

            validationMessages = sut.ValidatePickListItems(PickListEnum.ProposalClass, duplicate1);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.Any(x => x.ValidationIssue.Contains(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, duplicate1.First().Text))));
        }

        /// <summary>
        /// Validates the picklists, expecting exception because PickListName is not the same on the Loader
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void ValidatePicklistsAcrossBOEPTM_Exception1()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, ContainsParent = false, PickListName = "Blarg" };

            sut.ValidatePickLists(ptmData, boeData);
        }

        /// <summary>
        /// Validates the picklists, expecting exception because both of them contain Parent Ids which is not supported.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void ValidatePicklistsAcrossBOEPTM_Exception2()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, ContainsParent = true};

            sut.ValidatePickLists(ptmData, boeData);
        }

        /// <summary>
        /// Validates the picklists, expecting exception because Allowing Multiple Parents is not the same for both.
        /// </summary>
        // TODO This is not testable until we allow both PTM and BOE to have Parents at the same time
        //[TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void ValidatePicklistsAcrossBOEPTM_Exception3()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = false };

            sut.ValidatePickLists(ptmData, boeData);
        }

        /// <summary>
        /// Validates the picklists with missing BOE dto.
        /// </summary>
        [TestMethod]
        public void ValidatePicklistsAcrossBOEPTM_Missing1()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = true };

            sut.ValidatePickLists(ptmData, boeData);

            Assert.AreEqual(2, ptmData.Messages.Count);
            Assert.IsTrue(ptmData.Messages.First().ValidationIssue.Contains(CommonConstants.PickListValidation.NUMBER_ITEMS));
            Assert.IsTrue(ptmData.Messages.Last().ValidationIssue.Contains(string.Format(CommonConstants.PickListValidation.MISSING, string.Empty, "BOE" )));
        }

        /// <summary>
        /// Validates the picklists with missing PTM dto.
        /// </summary>
        [TestMethod]
        public void ValidatePicklistsAcrossBOEPTM_Missing2()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = true };

            sut.ValidatePickLists(ptmData, boeData);

            Assert.AreEqual(2, ptmData.Messages.Count);
            Assert.IsTrue(ptmData.Messages.First().ValidationIssue.Contains(CommonConstants.PickListValidation.NUMBER_ITEMS));
            Assert.IsTrue(ptmData.Messages.Last().ValidationIssue.Contains(string.Format(CommonConstants.PickListValidation.MISSING, string.Empty, "PTM")));
        }

        /// <summary>
        /// Validates the picklists with missing Read-Only (no messages)
        /// </summary>
        [TestMethod]
        public void ValidatePicklistsAcrossBOEPTM_Missing3()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 },
                    IsReadOnly = true
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
               
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = true };

            sut.ValidatePickLists(ptmData, boeData);

            Assert.AreEqual(0, ptmData.Messages.Count);
        }

        /// <summary>
        /// Validates the picklists, expecting message for Active not the same.
        /// </summary>
        [TestMethod]
        public void ValidatePicklistsAcrossBOEPTM_Active()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = true,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = true };

            sut.ValidatePickLists(ptmData, boeData);

            Assert.AreEqual(1, ptmData.Messages.Count);
            Assert.IsTrue(ptmData.Messages.First().ValidationIssue.Contains(string.Format(CommonConstants.PickListValidation.ACTIVE, "Hello World")));
        }


        /// <summary>
        /// Validates the picklists, expecting exception because Allowing Multiple Parents is not the same for both.
        /// </summary>
        [TestMethod]
        public void ValidatePicklistsAcrossBOEPTM()
        {
			IESPortalAdminControllerLogic sut = this.CreateSystem();

            ICollection<PickListDto> data = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World",
                    ParentIds = new int[] {1, 2 }
                }
            };

            PickListGridMV ptmData = new() { PickLists = data, ContainsParent = true, AllowsMultipleParents = true };

            ICollection<PickListDto> data2 = new List<PickListDto>()
            {
                new()
                {
                    Id = 222,
                    InUse = true,
                    IsActive = false,
                    Text = "Hello World"
                }
            };

            PickListGridMV boeData = new() { PickLists = data2, AllowsMultipleParents = true };

            sut.ValidatePickLists(ptmData, boeData);
        }

        /// <summary>
        /// Test GetOfflineApplicationData
        /// </summary>
        [TestMethod]
        public void GetOfflineApplicationDataTest()
        {
            IESPortalAdminControllerLogic sut = CreateSystem();

            ICollection<OfflineApplicationModelView> expected = new Collection<OfflineApplicationModelView>();
            expected.Add(new OfflineApplicationModelView() { ApplicationName = "Test App", IsOffline = true, UpdateDate = DateTime.Now });

            this.offlineApplicationLoader.Setup(x => x.GetAll()).Returns(expected);

            ICollection<OfflineApplicationModelView> result = sut.GetOfflineApplicationData();

            Assert.IsTrue(result.Any());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected.First().ApplicationName, result.First().ApplicationName);
            Assert.AreEqual(expected.First().IsOffline, result.First().IsOffline);
            Assert.AreEqual(expected.First().UpdateDate, result.First().UpdateDate);
        }

        /// <summary>
        /// Test SaveOfflineApplicationData
        /// </summary>
        [TestMethod]
        public void SaveOfflineApplicationDataTest()
        {
            IESPortalAdminControllerLogic sut = CreateSystem();

            ICollection<OfflineApplicationModelView> oldValues = new Collection<OfflineApplicationModelView>();
            oldValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 1", IsOffline = true, UpdateDate = DateTime.Now });
            oldValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 2", IsOffline = false, UpdateDate = DateTime.Now });
            oldValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 3", IsOffline = true, UpdateDate = DateTime.Now });

            ICollection<OfflineApplicationModelView> newValues = new Collection<OfflineApplicationModelView>();
            newValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 1", IsOffline = true, UpdateDate = DateTime.Now });
            newValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 2", IsOffline = true, UpdateDate = DateTime.Now });
            newValues.Add(new OfflineApplicationModelView() { ApplicationName = "Test App 3", IsOffline = false, UpdateDate = DateTime.Now });

            this.offlineApplicationLoader.Setup(x => x.GetAll()).Returns(oldValues);

            sut.SaveOfflineApplicationData(newValues);

            // Should only update for Test App 2 and 3 because the status of 1 did not change
            this.offlineApplicationLoader.Verify(x => x.Update(newValues.ElementAt(0)), Times.Never());
            this.offlineApplicationLoader.Verify(x => x.Update(newValues.ElementAt(1)), Times.Once());
            this.offlineApplicationLoader.Verify(x => x.Update(newValues.ElementAt(2)), Times.Once());
        }

        /// <summary>
        /// Test SaveOfflineApplicationData for null parameter exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveOfflineApplicationDataTest_Exception()
        {
            IESPortalAdminControllerLogic sut = CreateSystem();
            sut.SaveOfflineApplicationData(null);
        }
    }
}
