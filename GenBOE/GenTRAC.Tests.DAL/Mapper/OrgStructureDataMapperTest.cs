// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mappers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// This is a test class for OrgStructureDataMapperTest and is intended
    /// to contain all OrgStructureDataMapperTest Unit Tests
    /// </summary>
    [TestClass]
    public class OrgStructureDataMapperTest
    {
        #region Setup

        /// <summary>
        /// ILineOfBusinessDataLoader mock
        /// </summary>
        private Mock<IPickListLoader> lineOfBusinessDataLoader = null;

        /// <summary>
        /// IProgramAreaDataLoader mock
        /// </summary>
        private Mock<IPickListLoader> programAreaDataLoader = null;

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>Org Structure Data Mapper</returns>
        private OrgStructureDataMapper CreateSystem()
        {
            this.lineOfBusinessDataLoader = new Mock<IPickListLoader>();
            this.programAreaDataLoader = new Mock<IPickListLoader>();
            OrgStructureDataMapper sut = new OrgStructureDataMapper(this.lineOfBusinessDataLoader.Object, this.programAreaDataLoader.Object);

            return sut;
        }

        #endregion Setup

        /// <summary>
        /// Test for Get All Lines of Business
        /// </summary>
        [TestMethod]
        public void M_GetAllLineOfBusinesssTest()
        {
            var sut = this.CreateSystem();

            // test data
            var pl1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil",
                IsActive = true
            };
            var pl2 = new PickListDto()
            {
                Id = 10,
                Text = "Defense",
                IsActive = false
            };

            List<PickListDto> prodLines = new List<PickListDto>() { pl1, pl2 };

            this.lineOfBusinessDataLoader.Setup(x => x.GetPickListValues()).Returns(prodLines);

            var result = sut.GetAllLinesOfBusiness();

            Assert.AreEqual(prodLines.Count, result.Count);
            DtoAssertHelpers.AssertDtos(pl1, result.ElementAt(0));
            DtoAssertHelpers.AssertDtos(pl2, result.ElementAt(1));
        }

        /// <summary>
        /// Test for Get Line of Business By Id
        /// </summary>
        [TestMethod]
        public void M_GetLineOfBusinessByIdTest()
        {
            var sut = this.CreateSystem();

            // test data
            var pl1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil",
                IsActive = true
            };

            this.lineOfBusinessDataLoader.Setup(x => x.GetById(pl1.Id)).Returns(pl1);

            var result = sut.GetLineOfBusinessById(pl1.Id);

            DtoAssertHelpers.AssertDtos(pl1, result);
        }

        /// <summary>
        /// Test for Get Line of Business By Id (multiple)
        /// </summary>
        [TestMethod]
        public void M_GetLineOfBusinessByIdMultipleTest()
        {
            var sut = this.CreateSystem();

            // test data
            var pl1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil",
                IsActive = true
            };
            var pl2 = new PickListDto()
            {
                Id = 10,
                Text = "Defense",
                IsActive = true
            };
            ICollection<PickListDto> linesOfBusiness = new Collection<PickListDto>() { pl1, pl2 };
            ICollection<int> lineOfBusinessIds = new Collection<int>() { pl1.Id, pl1.Id };

            this.lineOfBusinessDataLoader.Setup(x => x.GetByIds(lineOfBusinessIds)).Returns(linesOfBusiness);

            var result = sut.GetLineOfBusinessById(lineOfBusinessIds);

            Assert.AreEqual(2, result.Count);
            DtoAssertHelpers.AssertDtos(pl1, result.Where(x => x.Id == pl1.Id).First());
            DtoAssertHelpers.AssertDtos(pl2, result.Where(x => x.Id == pl2.Id).First());
        }

        /// <summary>
        /// Test for Get All Program Area
        /// </summary>
        [TestMethod]
        public void M_GetAllProgramAreaTest()
        {
            var sut = this.CreateSystem();

            // test data
            var programArea1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil PA",
                IsActive = true,
                ParentIds = new int[] { 2 }
            };
            var programArea2 = new PickListDto()
            {
                Id = 1,
                Text = "Defense PA",
                IsActive = false,
                ParentIds = new int[] { 3 }
            };

            List<PickListDto> programAreas = new List<PickListDto>() { programArea1, programArea2 };

            this.programAreaDataLoader.Setup(x => x.GetPickListValues()).Returns(programAreas);

            var result = sut.GetAllProgramAreas();

            Assert.AreEqual(programAreas.Count, result.Count);
            DtoAssertHelpers.AssertDtos(programArea1, result.ElementAt(0));
            DtoAssertHelpers.AssertDtos(programArea2, result.ElementAt(1));
        }

        /// <summary>
        /// Test for Get Program Area By Id
        /// </summary>
        [TestMethod]
        public void M_GetProgramAreaByIdTest()
        {
            var sut = this.CreateSystem();

            // test data
            var programArea1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil PA",
                IsActive = true,
                ParentIds = new int[] { 2 }
            };

            this.programAreaDataLoader.Setup(x => x.GetById(programArea1.Id)).Returns(programArea1);

            var result = sut.GetProgramAreaById(programArea1.Id);

            DtoAssertHelpers.AssertDtos(programArea1, result);
        }

        /// <summary>
        /// Test for Get program Area By Id (multiple)
        /// </summary>
        [TestMethod]
        public void M_GetProgramAreaByIdMultipleTest()
        {
            var sut = this.CreateSystem();

            // test data
            var programArea1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil PA",
                IsActive = true,
                ParentIds = new int[] { 2 }
            };
            var programArea2 = new PickListDto()
            {
                Id = 1,
                Text = "Defense PA",
                IsActive = true,
                ParentIds = new int[] { 3 }
            };
            ICollection<PickListDto> programAreas = new Collection<PickListDto>() { programArea1, programArea2 };
            ICollection<int> programAreaIds = new Collection<int>() { programArea1.Id, programArea2.Id };

            this.programAreaDataLoader.Setup(x => x.GetByIds(programAreaIds)).Returns(programAreas);

            var result = sut.GetProgramAreaById(programAreaIds);

            Assert.AreEqual(2, result.Count);
            DtoAssertHelpers.AssertDtos(programArea1, result.Where(x => x.Id == programArea1.Id).First());
            DtoAssertHelpers.AssertDtos(programArea2, result.Where(x => x.Id == programArea2.Id).First());
        }

        /// <summary>
        /// Test for Get Program Areas For Line of Business
        /// </summary>
        [TestMethod]
        public void M_GetProgramAreaHtmlOptionsForLineOfBusinessTest()
        {
            var sut = this.CreateSystem();

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2,
                IsActive = true
            };

            PickListDto programArea1 = new PickListDto()
            {
                Text = "myFirstProgramArea",
                Id = 1,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            PickListDto programArea2 = new PickListDto()
            {
                Text = "mySecondProgramArea",
                Id = 2,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            PickListDto programArea3 = new PickListDto()
            {
                Text = "inactiveProgramArea",
                Id = 3,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = false
            };

            this.lineOfBusinessDataLoader.Setup(x => x.GetById(lineOfBusiness.Id)).Returns(lineOfBusiness);
            this.programAreaDataLoader.Setup(x => x.GetPickListValues()).Returns(new List<PickListDto>() { programArea1, programArea2, programArea3 });

            string programAreaHtml = sut.GetProgramAreaHtmlOptionsForLineOfBusiness(lineOfBusiness.Id);

            Assert.IsTrue(programAreaHtml.Contains("<option value=\"\">Select Program Area</option>"));
            Assert.IsTrue(programAreaHtml.Contains(string.Format("<option value=\"{0}\">{1}</option>", programArea1.Id, programArea1.Text)));
            Assert.IsTrue(programAreaHtml.Contains(string.Format("<option value=\"{0}\">{1}</option>", programArea2.Id, programArea2.Text)));
            Assert.IsFalse(programAreaHtml.Contains(programArea3.Text));

            // test new proposal with blank line of business
            programAreaHtml = sut.GetProgramAreaHtmlOptionsForLineOfBusiness(null);
            Assert.IsTrue(programAreaHtml.Contains("<option value=\"\">Select Program Area</option>"));
            Assert.IsTrue(programAreaHtml.Contains(programArea1.Text));
            Assert.IsTrue(programAreaHtml.Contains(programArea2.Text));
            Assert.IsFalse(programAreaHtml.Contains(programArea3.Text));
        }

        /// <summary>
        /// Test for Get Program Area Dynamic Help Text
        /// </summary>
        [TestMethod]
        public void M_GetProgramAreaDynamicHelpTextTest()
        {
            var sut = this.CreateSystem();

            // test data
            var pl1 = new PickListDto()
            {
                Id = 0,
                Text = "Civil",
                IsActive = true
            };
            var pl2 = new PickListDto()
            {
                Id = 10,
                Text = "Defense",
                IsActive = true
            };
            var programArea1 = new PickListDto()
            {
                Id = 0,
                Text = "C - DEF",
                IsActive = true,
                ParentIds = new int[] { 0 }
            };
            var programArea2 = new PickListDto()
            {
                Id = 1,
                Text = "C - ABC",
                IsActive = true,
                ParentIds = new int[] { 0 }
            };
            var programArea3 = new PickListDto()
            {
                Id = 2,
                Text = "D - ABC",
                IsActive = true,
                ParentIds = new int[] { 10 }
            };
            var programArea4 = new PickListDto()
            {
                Id = 3,
                Text = "D - DEF",
                IsActive = false,
                ParentIds = new int[] { 10 }
            };

            List<PickListDto> prodLines = new List<PickListDto>() { pl1, pl2 };
            List<PickListDto> programAreas = new List<PickListDto>() { programArea1, programArea2, programArea3, programArea4 };

            this.lineOfBusinessDataLoader.Setup(x => x.GetPickListValues()).Returns(prodLines);
            this.programAreaDataLoader.Setup(x => x.GetPickListValues()).Returns(programAreas);

            string result = sut.GetProgramAreaDynamicHelpText();
            string activeProgramAreas = "<b><u>Active Program Areas:</u></b>";
            string inactiveProgramAreas = "<b><u>Inactive Program Areas:</u></b>";
            Assert.IsTrue(result.Contains(activeProgramAreas));
            Assert.IsTrue(result.Contains(inactiveProgramAreas));
            string programArea1String = string.Format("{0} ({1})", programArea1.Text, pl1.Text);
            string programArea2String = string.Format("{0} ({1})", programArea2.Text, pl1.Text);
            string programArea3String = string.Format("{0} ({1})", programArea3.Text, pl2.Text);
            string programArea4String = string.Format("{0} ({1})", programArea4.Text, pl2.Text);
            Assert.IsTrue(result.Contains(programArea1String));
            Assert.IsTrue(result.Contains(programArea2String));
            Assert.IsTrue(result.Contains(programArea3String));
            Assert.IsTrue(result.Contains(programArea4String));

            // verify program areas are under correct headings
            // actives before inactive header
            Assert.IsTrue(result.IndexOf(programArea1String) < result.IndexOf(inactiveProgramAreas));
            Assert.IsTrue(result.IndexOf(programArea2String) < result.IndexOf(inactiveProgramAreas));
            Assert.IsTrue(result.IndexOf(programArea3String) < result.IndexOf(inactiveProgramAreas));
            // inactives after inactive header
            Assert.IsTrue(result.IndexOf(programArea4String) > result.IndexOf(inactiveProgramAreas));

            // verify sorting
            Assert.IsTrue(result.IndexOf(programArea2String) < result.IndexOf(programArea1String));
        }
    }
}
