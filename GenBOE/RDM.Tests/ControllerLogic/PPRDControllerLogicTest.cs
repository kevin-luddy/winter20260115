// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.ModelViews;
    using IES.DataBridge.Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        /// <summary>
        /// Initialize test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);

            this.sut = new PPRDControllerLogic(null, this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object);

            this.sections.Add(new SectionModelView()
            {
                Title = "Section",
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1",
                ChildNodes = new List<SectionModelView>
                {
                    new SectionModelView
                    {
                        Title = "Sub Section1",
                        DisplayOrder = 1,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.1",
                        ChildNodes = new List<SectionModelView>
                        {
                            new SectionModelView
                            {
                                Title = "Text1",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>()
                            },
                            new SectionModelView
                            {
                                Title = "Table1",
                                DisplayOrder = 2,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new SectionModelView
                    {
                        Title = "Sub Section2 - Too many tables!",
                        DisplayOrder = 2,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.2",
                        ChildNodes = new List<SectionModelView>
                        {
                            new SectionModelView
                            {
                                Title = "Table2",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            },
                            new SectionModelView
                            {
                                Title = "Table3",
                                DisplayOrder = 2,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new SectionModelView
                    {
                        Title = "Sub Section3",
                        DisplayOrder = 3,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.3",
                        ChildNodes = new List<SectionModelView>
                        {
                            new SectionModelView
                            {
                                Title = "Text2",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>()
                            }
                        }
                    },
                    new SectionModelView
                    {
                        Title = "Sub Section4 - bad content",
                        DisplayOrder = 4,
                        ContentType = SectionContentType.Section,
                        ReferenceNumber = "1.4",
                        ChildNodes = new List<SectionModelView>
                        {
                            new SectionModelView
                            {
                                Title = "Text3",
                                DisplayOrder = 1,
                                ContentType = SectionContentType.Text,
                                TextContent = "Hello World",
                                ChildNodes = new List<SectionModelView>
                                {
                                    new SectionModelView
                                    {
                                        Title = "Text4",
                                        DisplayOrder = 1,
                                        ContentType = SectionContentType.Text,
                                        TextContent = "This is bad.  Text node can't have children!",
                                        ChildNodes = new List<SectionModelView>()
                                    }
                                }
                            },
                            new SectionModelView
                            {
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
        /// </summary>
        [TestMethod]
        public void TestValidateSections()
        {
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            this.sut.ValidateSections(this.sections, validationErrors);
            Assert.AreEqual(validationErrors.Count, 3);
        }
    }
}
