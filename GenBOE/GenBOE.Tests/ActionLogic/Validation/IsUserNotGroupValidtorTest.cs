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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IsUserNotGroupValidtorTest
    {
        [TestMethod]
        public void IsUserNotGroupValidtor_IsValid()
        {
            IsUserNotGroupValidator sut = new IsUserNotGroupValidator();

            bool returnValue = sut.isValid("EBS.EstimationInitiative.DevTeam", null);
            Assert.IsFalse(returnValue);

            returnValue = sut.isValid("testuser", null);
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void IsUserNotGroupValidator_InvalidParameterTest()
        {
            IsUserNotGroupValidator sut = new IsUserNotGroupValidator();

            decimal[] test = { 3.14m, 6.77m };
            bool returnValue = sut.isValid(test, null);
            Assert.IsFalse(returnValue);

            sut.isValid(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUserNotGroupValidtor_NullParameterTest()
        {
            IsUserNotGroupValidator sut = new IsUserNotGroupValidator();
            sut.validation(null, new Collection<Dictionary<string, string>>());
        }
    }
}
