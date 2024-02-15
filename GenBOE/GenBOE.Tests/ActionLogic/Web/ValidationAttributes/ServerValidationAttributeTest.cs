// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ValidationAttributes
{
    using System;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using IES.Common.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ServerValidationAttributeTest
    {
        private ServerValidationAttribute CreateSystem()
        {
            ValidationFactory validationFactory = GetValidationFactory();
            Mock<ValidationFactoryWrapper> _ValidationFactory = new Mock<ValidationFactoryWrapper>();
            _ValidationFactory.Setup(x => x.Instance).Returns(validationFactory);
            return new ServerValidationAttribute();
        }

        [TestMethod]
        public void SV_IsValidTest()
        {
			ServerValidationAttribute sut = CreateSystem();

            Assert.AreEqual(sut.IsValid(null), true);

        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void SV_IsValidExceptionTest1()
        {
			ServerValidationAttribute sut = CreateSystem();
            sut.ValidationToPerform = ValidationType.None;
            sut.IsValid("xyz");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void SV_IsValidExceptionTest2()
        {
			ServerValidationAttribute sut = CreateSystem();
            sut.ValidationToPerform = ValidationType.DateRangeValidator;
            sut.IsValid("xyz");
        }


        #endregion Exception Tests

        private ValidationFactory GetValidationFactory()
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

            ValidationFactory vf = new ValidationFactory(
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
            
            return vf;
        }
    }

    /// <summary>
    /// Create a wrapper class (Adapter Pattern) for the ValidationFactory class and fake its static members.
    /// Note: This approach is used since MOQ can't fake static members.
    /// </summary>
    public class ValidationFactoryWrapper
    {
        public virtual ValidationFactory Instance
        {
            get
            {
                return ValidationFactory.Instance;
            }
        }
    }

}
