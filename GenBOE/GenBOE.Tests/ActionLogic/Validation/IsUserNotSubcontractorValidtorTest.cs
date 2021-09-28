// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class IsUserNotSubcontractorValidtorTest
    {
        [TestMethod]
        public void IsUserNotSubcontractorValidtor_IsValid()
        {
            Mock<ISecurityInformation> securityInformation = new Mock<ISecurityInformation>();
            Mock<IUserDTODataLoader> userDataLoader = new Mock<IUserDTODataLoader>();

            UserDTO nonSub = new UserDTO() { NTID = "a-wilsot", IsSubcontractor = false };
            UserDTO sub = new UserDTO() { NTID = "subcontractor", IsSubcontractor = true };

            securityInformation.Setup(x => x.IsSubcontractorUser(nonSub.NTID, nonSub.IsSubcontractor)).Returns(false);
            securityInformation.Setup(x => x.IsSubcontractorUser(sub.NTID, sub.IsSubcontractor)).Returns(true);

            userDataLoader.Setup(x => x.GetOrCreateUserByNtid(nonSub.NTID)).Returns(nonSub);
            userDataLoader.Setup(x => x.GetOrCreateUserByNtid(sub.NTID)).Returns(sub);

            IsUserNotSubcontractorValidator sut = new IsUserNotSubcontractorValidator(securityInformation.Object, userDataLoader.Object);

            bool returnValue = sut.isValid("a-wilsot", null);
            Assert.IsTrue(returnValue);

            returnValue = sut.isValid("subcontractor", null);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void IsUserNotSubcontractorValidator_InvalidParameterTest()
        {
            var securityInformation = new Mock<ISecurityInformation>();
            Mock<IUserDTODataLoader> userDataLoader = new Mock<IUserDTODataLoader>();

            IsUserNotSubcontractorValidator sut = new IsUserNotSubcontractorValidator(securityInformation.Object, userDataLoader.Object);

            decimal[] test = { 3.14m, 6.77m };
            bool returnValue = sut.isValid(test, null);
            Assert.IsFalse(returnValue);

            sut.isValid(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUserNotSubcontractorValidtor_NullParameterTest()
        {
            var securityInformation = new Mock<ISecurityInformation>();
            Mock<IUserDTODataLoader> userDataLoader = new Mock<IUserDTODataLoader>();
            IsUserNotSubcontractorValidator sut = new IsUserNotSubcontractorValidator(securityInformation.Object, userDataLoader.Object);
            sut.validation(null, new Collection<Dictionary<string, string>>());
        }
        
    }
}
