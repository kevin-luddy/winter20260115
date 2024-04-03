// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using GenBOE.ActionLogic.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ValidationFactoryTest
    {
        [TestMethod]
        public void GetValidatorTest()
        {
			Mock<Validator> workspaceUniqueNameValidator = new Mock<Validator>();
			Mock<Validator> workspaceUniqueShortnameValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotGroupValidator = new Mock<Validator>();
			Mock<Validator> boeTaskIdUniqueValidator = new Mock<Validator>();
			Mock<Validator> boeDateRangeValidator = new Mock<Validator>();
			Mock<Validator> wbsUniqueNumberValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> performingOrgUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> wbsRenumberValidator = new Mock<Validator>();
			Mock<Validator> boeWBSMoveValidator = new Mock<Validator>();
			Mock<Validator> boeCLINMoveValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialExistsforWbsValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueDescValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotSubcontractorValidator = new Mock<Validator>();

            ValidationFactory sut = new ValidationFactory(
                workspaceUniqueNameValidator.Object,
                workspaceUniqueShortnameValidator.Object,
                workspaceCostVolumeLeadNotGroupValidator.Object,
                boeTaskIdUniqueValidator.Object,
                boeDateRangeValidator.Object,
                wbsUniqueNumberValidator.Object,
                resourceUniqueIDValidator.Object,
                performingOrgUniqueIDValidator.Object,
                wbsRenumberValidator.Object,
                boeWBSMoveValidator.Object,
                boeCLINMoveValidator.Object,
                boeMaterialExistsforWbsValidator.Object,
                boeMaterialElementExistsValidator.Object,
                resourceUniqueDescValidator.Object,
                workspaceCostVolumeLeadNotSubcontractorValidator.Object);

            // Validate the singleton
            Assert.AreSame(sut, ValidationFactory.Instance);

            // Validate the getValidator method for each type.
            ValidationType type = ValidationType.None;
            Validator validator = sut.getValidator(type);
            Assert.IsNull(validator);

            type = ValidationType.WorkspaceUniqueName;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.WorkspaceUniqueShortname;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.IsUserNotGroup;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.BoeTaskIDUnique;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.DateRangeValidator;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.WBSUniqueNumber;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.ResourceUniqueID;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.PerformingOrgUniqueID;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.BoeMaterialElementExists;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.BoeLaborCostElementExists;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.ResourceUniqueDesc;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));

            type = ValidationType.IsUserNotSubcontractor;
            validator = sut.getValidator(type);
            Assert.IsInstanceOfType(validator, typeof(Validator));
        }
    }
}
