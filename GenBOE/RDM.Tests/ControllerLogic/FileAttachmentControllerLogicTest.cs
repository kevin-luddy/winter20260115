// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace RDM.Tests.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;


    [TestClass]
    public class FileAttachmentControllerLogicTest
    {
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

        public FileAttachmentControllerLogic CreateSut()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);
            return new FileAttachmentControllerLogic(this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object);
        }

        public ICollection<OptionModelView> CreateSections()
        {
            ICollection < OptionModelView > sections = new Collection<OptionModelView>
            {
                new OptionModelView {
                    Id = 1,
                    Label = "Section1"
                },
                new OptionModelView {
                    Id = 2,
                    Label = "Section2"
                }
            };

            return sections;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateFileAttachments_EX1()
        {
            FileAttachmentControllerLogic sut = this.CreateSut();
            sut.ValidateFileAttachments(null, new Collection<OptionModelView>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateFileAttachments_EX2()
        {
            FileAttachmentControllerLogic sut = this.CreateSut();
            sut.ValidateFileAttachments(new Collection<FileAttachmentRowModelView>(), null);
        }

        [TestMethod]
        public void ValidateFileAttachments_NoErrors()
        {
            FileAttachmentControllerLogic sut = this.CreateSut();
            Collection<FileAttachmentRowModelView> fileAttachments = new Collection<FileAttachmentRowModelView>();
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test1"
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test2",
                SectionId = 1
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test3",
                SectionId = 2
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                IsDeleted = true
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test5",
                IsDeleted = true
            });

            
            ICollection<ValidationMessage> messages = sut.ValidateFileAttachments(fileAttachments, this.CreateSections());

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void ValidateFileAttachments_MaxLength()
        {
            FileAttachmentControllerLogic sut = this.CreateSut();
            Collection<FileAttachmentRowModelView> fileAttachments = new Collection<FileAttachmentRowModelView>();
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test1"
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com/Test5sfdjklllllllllllllllllllllllllllllllllTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjlllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj",
                Name = "Test2",
                SectionId = 1
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjTest5sfdjkllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllsdflsdlsfdljkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkksdfdsfdslfdslkfdslkjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj",
                SectionId = 2
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                IsDeleted = true
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test",
                IsDeleted = true
            });


            ICollection<ValidationMessage> messages = sut.ValidateFileAttachments(fileAttachments, this.CreateSections());

            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages.ElementAt(0).ValidationIssue.Equals("One or more rows has a Url field over 255 characters."));
            Assert.IsTrue(messages.ElementAt(1).ValidationIssue.Equals("One or more rows has a Name field over 100 characters."));
        }

        [TestMethod]
        public void ValidateFileAttachments_RequiredFields()
        {
            FileAttachmentControllerLogic sut = this.CreateSut();
            Collection<FileAttachmentRowModelView> fileAttachments = new Collection<FileAttachmentRowModelView>();
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "www.google.com",
                Name = "Test1"
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com"
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "https://www.google.com"
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Name = "Test4",
                SectionId = 3
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                IsDeleted = true
            });
            fileAttachments.Add(new FileAttachmentRowModelView
            {
                Link = "http://www.google.com",
                Name = "Test5",
                IsDeleted = true
            });
            ICollection<ValidationMessage> messages = sut.ValidateFileAttachments(fileAttachments, this.CreateSections());

            Assert.AreEqual(4, messages.Count);
            Assert.IsTrue(messages.ElementAt(0).ValidationIssue.Equals("One or more rows are missing the required Name field."));
            Assert.IsTrue(messages.ElementAt(1).ValidationIssue.Equals("One or more rows are missing the required Url field."));
            Assert.IsTrue(messages.ElementAt(2).ValidationIssue.Equals("Row has an invalid Url: www.google.com"));
            Assert.IsTrue(messages.ElementAt(3).ValidationIssue.Equals("Row with Name Test4 has a bad or old section selected."));
        }
    }
}
