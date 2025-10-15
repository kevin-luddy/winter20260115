// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.Mediator;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Security;
	using IES.DataBridge.ModelViews;
    using IES.DataBridge.Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
	using System.Linq;
	using IES.Common.Core.Enums;

	[TestClass]
    public class PPRDControllerLogicTest
    {
        /// <summary>
        /// Controller logic sut
        /// </summary>
        private PPRDControllerLogic sut;

        /// <summary>
        /// Test section data
        /// </summary>
        private ICollection<SectionModelView> sections = new Collection<SectionModelView>();

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

        /// <summary>
        /// Initialize test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null, null);

            this.sut = new PPRDControllerLogic(null, this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object, null);

            this.sections.Add(new SectionModelView()
            {
                Title = "Section",
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1",
                ChildNodes = new List<SectionModelView>
                {
                    new() {
                        Title = "Sub Section1",
                        DisplayOrder = 1,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.1",
                        ChildNodes = new List<SectionModelView>
                        {
                            new() {
                                Title = "Text1",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>()
                            },
                            new() {
                                Title = "Table1",
                                DisplayOrder = 2,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new() {
                        Title = "Sub Section2 - Too many tables!",
                        DisplayOrder = 2,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.2",
                        ChildNodes = new List<SectionModelView>
                        {
                            new() {
                                Title = "Table2",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            },
                            new() {
                                Title = "Table3",
                                DisplayOrder = 2,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new() {
                        Title = "Sub Section3",
                        DisplayOrder = 3,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.3",
                        ChildNodes = new List<SectionModelView>
                        {
                            new() {
                                Title = "Text2",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new() {
                        Title = "Sub Section4 - bad content",
                        DisplayOrder = 4,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.4",
                        ChildNodes = new List<SectionModelView>
                        {
                            new() {
                                Title = "Text3",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>
                                {
                                    new() {
                                        Title = "Text4",
                                        DisplayOrder = 1,
                                        ContentType = SectionContentType.Text,
                                        TextContent = "This is bad.  Text node can't have children!",
                                        ChildNodes = new List<SectionModelView>()
                                    }
                                }
                            },
                            new() {
                                Title = "Bad Content Type",
                                DisplayOrder = 2,
                                ContentType = SectionContentType.None,
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    }
                }
            });
        }

		/// <summary>
		/// Test Tables In Sections
		/// Only 1 table is allowed per section
		/// Validate section content types
		/// Validate content nodes can't have child nodes
		/// Casb is invalid (core & service)
		/// NonCompliance is invalid (core & service)
		/// Test that there is at least one "included in cover sheet selection" in the address section invalid
		/// Total 8 errors
		/// </summary>
		[TestMethod]
        public void TestValidateSections()
        {
            Collection<ValidationMessage> validationErrors = new();
            this.sut.ValidateSections(this.sections, validationErrors);
            Assert.AreEqual(8, validationErrors.Count);
        }

        /// <summary>
        /// Only 1 table is allowed per section
        /// Validate section content types
        /// Validate content nodes can't have child nodes
		/// CASB valid, non compliance causes an issue (core & service)
		/// Test that there is at least one "included in cover sheet selection" in the address section invalid
		/// Total 6 errors
        /// </summary>
        [TestMethod]
        public void TestValidateSections_2()
        {
            Collection<ValidationMessage> validationErrors = new();
            this.sections.First().SectionContainsCasbDisclosureCore = true;
			this.sections.First().SectionContainsCasbDisclosureService = true;
			this.sut.ValidateSections(this.sections, validationErrors);
            Assert.AreEqual(6, validationErrors.Count);
        }

		/// <summary>
		/// Only 1 table is allowed per section
		/// Validate section content types
		/// Validate content nodes can't have child nodes        
		/// Non compliance and CASB sections are valid (no errors)
		/// Test that there is at least one "included in cover sheet selection" in the address section invalid
		/// Total 4 errors
		/// </summary>
		[TestMethod]
        public void TestValidateSections_3()
        {
            Collection<ValidationMessage> validationErrors = new();
            this.sections.First().SectionContainsCasbDisclosureCore = true;
			this.sections.First().SectionContainsCasbDisclosureService = true;
			this.sections.Last().SectionContainsNonComplianceCore = true;
			this.sections.Last().SectionContainsNonComplianceService = true;
			this.sut.ValidateSections(this.sections, validationErrors);
            Assert.AreEqual(4, validationErrors.Count);
        }

		/// <summary>
		/// Only 1 table is allowed per section
		/// Validate section content types
		/// Validate content nodes can't have child nodes
		/// Test that 2 sections marked as non-compliance cause an error (core & service)
		/// Test that there is at least one "included in cover sheet selection" in the address section invalid
		/// Total 6 errors
		/// </summary>
		[TestMethod]
        public void TestValidateSections_4()
        {
            Collection<ValidationMessage> validationErrors = new();
			this.sections.First().SectionContainsCasbDisclosureCore = true;
			this.sections.First().SectionContainsCasbDisclosureService = true;
			this.sections.First().SectionContainsNonComplianceCore = true;
			this.sections.First().SectionContainsNonComplianceService = true;
			this.sections.First().ChildNodes.First().SectionContainsNonComplianceCore = true;
			this.sections.First().ChildNodes.First().SectionContainsNonComplianceService = true;
			this.sut.ValidateSections(this.sections, validationErrors);
            Assert.AreEqual(6, validationErrors.Count);
        }

		/// <summary>
		/// Only 1 table is allowed per section
		/// Validate content nodes can't have child nodes
		/// Validate section content types
		/// Non compliance and CASB disclosure sections are invalid (core and service)
		/// Test that there is at least one "included in cover sheet selection" in the address section
		/// Total 8 errors
		/// </summary>
		[TestMethod]
		public void TestValidateSections_5()
		{
			Collection<ValidationMessage> validationErrors = new();
			this.sut.ValidateSections(this.sections, validationErrors);
			Assert.AreEqual(8, validationErrors.Count);

			//Add IncludeInCoversheet
			this.sections.First().ChildNodes.Add(new SectionModelView
			{
				Title = null,
				DisplayOrder = 5,
				ContentType = SectionContentType.Address,
				IncludeInCoversheet = true
			});

			validationErrors.Clear();
			this.sut.ValidateSections(this.sections, validationErrors);
			Assert.AreEqual(7, validationErrors.Count);
		}
	}
}
