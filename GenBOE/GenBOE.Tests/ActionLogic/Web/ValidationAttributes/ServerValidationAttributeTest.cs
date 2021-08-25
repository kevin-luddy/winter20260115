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
            var sut = CreateSystem();

            Assert.AreEqual(sut.IsValid(null), true);

        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void SV_IsValidExceptionTest1()
        {
            var sut = CreateSystem();
            sut.ValidationToPerform = ValidationType.None;
            sut.IsValid("xyz");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void SV_IsValidExceptionTest2()
        {
            var sut = CreateSystem();
            sut.ValidationToPerform = ValidationType.DateRangeValidator;
            sut.IsValid("xyz");
        }


        #endregion Exception Tests

        private ValidationFactory GetValidationFactory()
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
