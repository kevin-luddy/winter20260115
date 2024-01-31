// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ActionLogic;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the user dto data mapper
    /// </summary>
    [TestClass]
    public class PickListMapperTest
    {
        /// <summary>
        /// LU Table Loader
        /// </summary>
        private Mock<ProposalTypeLULoader> proposalLuLoader;

        /// <summary>
        /// LU Table Loader
        /// </summary>
        private Mock<ProposalClassLULoader> proposalClassLoader;

        /// <summary>
        /// LU Table Loader
        /// </summary>
        private Mock<TypeOfRequestLULoader> typeOfRequestLuLoader;

        /// <summary>
        /// The lob loader.
        /// </summary>
        private Mock<LineOfBusinessDataLoader> lobLoader;

        /// <summary>
        /// The pa loader
        /// </summary>
        private Mock<ProgramAreaDataLoader> programAreaLoader;

        /// <summary>
        /// The contract type loader
        /// </summary>
        private Mock<ContractTypeLULoader> contractTypeLoader;

        /// <summary>
        /// The contract type group loader
        /// </summary>
        private Mock<ContractTypeGroupLULoader> contractTypeGroupLoader;

        /// <summary>
        /// Creates the mapper we are testing
        /// </summary>
        /// <returns>System Under Test</returns>
        public IPickListMapper CreateSut()
        {
            this.proposalLuLoader = new Mock<ProposalTypeLULoader>();
            this.proposalClassLoader = new Mock<ProposalClassLULoader>();
            this.typeOfRequestLuLoader = new Mock<TypeOfRequestLULoader>();
            this.lobLoader = new Mock<LineOfBusinessDataLoader>();
            this.programAreaLoader = new Mock<ProgramAreaDataLoader>();
            this.contractTypeGroupLoader = new Mock<ContractTypeGroupLULoader>();
            this.contractTypeLoader = new Mock<ContractTypeLULoader>();

            IPickListMapper sut = new PtmPickListMapper(this.proposalLuLoader.Object, this.proposalClassLoader.Object, this.typeOfRequestLuLoader.Object, 
                this.lobLoader.Object, this.programAreaLoader.Object, this.contractTypeLoader.Object,
                this.contractTypeGroupLoader.Object);

            return sut;
        }

        /// <summary>
        /// Test Exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_Test_Ex1()
        {
            IPickListMapper sut = this.CreateSut();

            sut.SavePickList(PickListEnum.ProposalClass, null);
        }

        /// <summary>
        /// Test Exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_Test_Ex2()
        {
            IPickListMapper sut = this.CreateSut();

            sut.SavePickList(PickListEnum.ProposalClass, new List<PickListDto>());
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Save_LevelofCommitment()
        {
			IPickListMapper sut = this.CreateSut();

            this.proposalClassLoader.Setup(x => x.Save(It.IsAny<PickListDto>())).Verifiable();

            sut.SavePickList(PickListEnum.ProposalClass, new List<PickListDto>() { new PickListDto() });

            this.proposalClassLoader.Verify(x => x.Save(It.IsAny<PickListDto>()), Times.Once());
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Save_ProposalType()
        {
			IPickListMapper sut = this.CreateSut();

            this.proposalLuLoader.Setup(x => x.Save(It.IsAny<PickListDto>())).Verifiable();

            sut.SavePickList(PickListEnum.ProposalType, new List<PickListDto>() { new PickListDto() });

            this.proposalLuLoader.Verify(x => x.Save(It.IsAny<PickListDto>()), Times.Once());
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Save_TypeOfRequest()
        {
			IPickListMapper sut = this.CreateSut();

            this.typeOfRequestLuLoader.Setup(x => x.Save(It.IsAny<PickListDto>())).Verifiable();

            sut.SavePickList(PickListEnum.TypeOfRequest, new List<PickListDto>() { new PickListDto() });

            this.typeOfRequestLuLoader.Verify(x => x.Save(It.IsAny<PickListDto>()), Times.Once());
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Get_TypeOfRequest()
        {
            IPickListMapper sut = this.CreateSut();
            List<PickListDto> dataFromLoader = new List<PickListDto>();

            this.typeOfRequestLuLoader.Setup(x => x.GetPickListValues()).Returns(dataFromLoader);

			PickListGridMV actualData = sut.GetPickListValues(PickListEnum.TypeOfRequest);

            Assert.AreSame(dataFromLoader, actualData.PickLists);
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Get_LevelofCommitment()
        {
            IPickListMapper sut = this.CreateSut();
            List<PickListDto> dataFromLoader = new List<PickListDto>();

            this.proposalClassLoader.Setup(x => x.GetPickListValues()).Returns(dataFromLoader);

			PickListGridMV actualData = sut.GetPickListValues(PickListEnum.ProposalClass);

            Assert.AreSame(dataFromLoader, actualData.PickLists);
        }

        /// <summary>
        /// Test a specific save
        /// </summary>
        [TestMethod]
        public void M_Test_Get_ProposalType()
        {
            IPickListMapper sut = this.CreateSut();
            List<PickListDto> dataFromLoader = new List<PickListDto>();

            this.proposalLuLoader.Setup(x => x.GetPickListValues()).Returns(dataFromLoader);

			PickListGridMV actualData = sut.GetPickListValues(PickListEnum.ProposalType);

            Assert.AreSame(dataFromLoader, actualData.PickLists);
        }

        /// <summary>
        /// Test for enum GetContractTypes
        /// </summary>
        [TestMethod]
        public void GetContractTypesTest()
        {
            IPickListMapper sut = new PtmPickListMapper(null, null, null, null, null, new ContractTypeLULoader(), new ContractTypeGroupLULoader());

            // no contract types specified
            ICollection<PickListDto> contractTypes = sut.GetChildren(PickListEnum.ContractType, 0);
            Assert.IsFalse(contractTypes.Any());

            // should be at least one contract type
            contractTypes = sut.GetChildren(PickListEnum.ContractType, 1);
            Assert.IsTrue(contractTypes.Any());
        }

        /// <summary>
        /// Test for GetContractTypesForContractTypeGroup
        /// </summary>
        [TestMethod]
        public void C_GetContractTypesForContractTypeGroupTest()
        {
            IPickListMapper sut = new PtmPickListMapper(null, null, null, null, null, new ContractTypeLULoader(), new ContractTypeGroupLULoader());
            ProposalControllerLogic logic = this.CreateSystem(sut);
            int contractTypeGroup = 1; // ContractTypeGroup.CP;
            string contractTypeHtml = logic.GetContractTypesForContractTypeGroup(contractTypeGroup.ToString(), true);
            this.AssertContractTypeHtml(contractTypeGroup, contractTypeHtml, sut);

            contractTypeGroup = 2; // ContractTypeGroup.FP;
            contractTypeHtml = logic.GetContractTypesForContractTypeGroup(contractTypeGroup.ToString(), true);
            this.AssertContractTypeHtml(contractTypeGroup, contractTypeHtml, sut);

            contractTypeGroup = 5; // ContractTypeGroup.IWTA;
            contractTypeHtml = logic.GetContractTypesForContractTypeGroup(contractTypeGroup.ToString(), true);
            this.AssertContractTypeHtml(contractTypeGroup, contractTypeHtml, sut);

            contractTypeGroup = 3; // ContractTypeGroup.Hybrid;
            contractTypeHtml = logic.GetContractTypesForContractTypeGroup(contractTypeGroup.ToString(), true);
            this.AssertContractTypeHtml(contractTypeGroup, contractTypeHtml, sut);

            contractTypeGroup = 3; // ContractTypeGroup.Hybrid;
            string nonIDIQcontractTypeHtml = logic.GetContractTypesForContractTypeGroup(contractTypeGroup.ToString(), false);
            Assert.AreNotEqual(contractTypeHtml, nonIDIQcontractTypeHtml);
        }

        /// <summary>
        /// Helper method to assert content returned by GetContractTypesForContractTypeGroup method
        /// </summary>
        /// <param name="contractTypeGroup">Contract Type Group</param>
        /// <param name="contractTypeHtml">Resulting html output</param>
        /// <param name="pickListMapper">The pick list mapper.</param>
        private void AssertContractTypeHtml(int contractTypeGroup, string contractTypeHtml, IPickListMapper pickListMapper)
        {
            foreach (PickListDto contractType in pickListMapper.GetChildren(PickListEnum.ContractType, contractTypeGroup).Where(x => x.IsActive))
            {
                if (contractType.IsActive)
                {
                    Assert.IsTrue(contractTypeHtml.Contains(string.Format("<option value=\"{0}\">{1}</option>", contractType.Id, contractType.Text)));
                }
                else
                {
                    Assert.IsFalse(contractTypeHtml.Contains(contractType.Text));
                }
            }
        }

        /// <summary>
        /// Proposal Controller logic under test
        /// </summary>
        /// <param name="pickListMapper">The pick list mapper.</param>
        /// <returns>
        /// home controller logic
        /// </returns>
        private ProposalControllerLogic CreateSystem(IPickListMapper pickListMapper)
        {
            return new ProposalControllerLogic(null, null, null, null, null, null, null, null, null,
                null, pickListMapper, null, null, null, null, null, null, null, null);
        }
    }
}
