// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mapper
{
    using GenTRAC.DataBridge.Common;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test checklist content mapper
    /// </summary>
    [TestClass]
    public class ChecklistContentMapperTest
    {
        /// <summary>
        /// The mock loader
        /// </summary>
        private Mock<IChecklistContentLoader> checklistContentLoader = null;

        /// <summary>
        /// The mock cache loader
        /// </summary>
        private Mock<ICacheDataLoader> cacheLoader = null;

        /// <summary>
        /// Test get PPR checklist content by proposal ID
        /// </summary>
        [TestMethod]
        public void M_GetPPRChecklistContentByProposalIDTest()
        {
            var sut = this.CreatePPRSystem();

            this.GetChecklistContentByProposalID(sut);
        }

        /// <summary>
        /// Test get PAR checklist content by proposal ID
        /// </summary>
        [TestMethod]
        public void M_GetPARChecklistContentByProposalIDTest()
        {
            var sut = this.CreatePARSystem();

            this.GetChecklistContentByProposalID(sut);
        }

        /// <summary>
        /// Test get checklist content by proposal ID
        /// </summary>
        /// <param name="mapper">Checklist content mapper</param>
        private void GetChecklistContentByProposalID(IChecklistContentMapper mapper)
        {
            int proposalId = 15;
            ChecklistContentDto testChecklistContent = new ChecklistContentDto()
            {
                Version = 3
            };

            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetIdByIdDelegate>(), new object[] { proposalId }, It.IsAny<string>())).Returns(1);
            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetDtoByIdDelegate>(), new object[] { proposalId }, It.IsAny<string>())).Returns(testChecklistContent);

            ChecklistContentDto actualResult = mapper.GetChecklistByProposalId(proposalId);

            DtoAssertHelpers.AssertDtos(testChecklistContent, actualResult);
        }

        /// <summary>
        /// Creates the PPR Checklist system under test
        /// </summary>
        /// <returns>The system</returns>
        private IChecklistContentMapper CreatePPRSystem()
        {
            this.checklistContentLoader = new Mock<IChecklistContentLoader>();
            this.cacheLoader = new Mock<ICacheDataLoader>();

            PPRChecklistContentMapper system = new PPRChecklistContentMapper(
                this.checklistContentLoader.Object,
                this.cacheLoader.Object);

            return system;
        }

        /// <summary>
        /// Creates the PAR Checklist system under test
        /// </summary>
        /// <returns>The system</returns>
        private IChecklistContentMapper CreatePARSystem()
        {
            this.checklistContentLoader = new Mock<IChecklistContentLoader>();
            this.cacheLoader = new Mock<ICacheDataLoader>();

            PARChecklistContentMapper system = new PARChecklistContentMapper(
                this.checklistContentLoader.Object,
                this.cacheLoader.Object);

            return system;
        }
    }
}
