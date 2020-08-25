// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
            var workspaceUniqueNameValidator = new Mock<Validator>();
            var workspaceUniqueShortnameValidator = new Mock<Validator>();
            var workspaceCostVolumeLeadNotGroupValidator = new Mock<Validator>();
            var boeTaskIdUniqueValidator = new Mock<Validator>();
            var boeDateRangeValidator = new Mock<Validator>();
            var wbsUniqueNumberValidator = new Mock<Validator>();
            var resourceUniqueIDValidator = new Mock<Validator>();
            var performingOrgUniqueIDValidator = new Mock<Validator>();
            var wbsRenumberValidator = new Mock<Validator>();
            var boeWBSMoveValidator = new Mock<Validator>();
            var boeCLINMoveValidator = new Mock<Validator>();
            var boeMaterialExistsforWbsValidator = new Mock<Validator>();
            var boeMaterialElementExistsValidator = new Mock<Validator>();
            var resourceUniqueDescValidator = new Mock<Validator>();
            var workspaceCostVolumeLeadNotSubcontractorValidator = new Mock<Validator>();

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
