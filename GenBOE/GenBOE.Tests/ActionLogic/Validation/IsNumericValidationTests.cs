// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using GenBOE.ActionLogic.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IsNumericValidationTests
    {
        private IsNumericValidation CreateSystem()
        {
            return new IsNumericValidation();
        }

        [TestMethod]
        public void ValueIsNotNumeric()
        {
            var nv = CreateSystem();
            string sValue;

            sValue = "123ValueIsNotANumericNumber";
            Assert.AreEqual(false, nv.isNumeric(sValue));

            sValue = "ValueIsNotANumericNumber";
            Assert.AreEqual(false, nv.isNumeric(sValue));

            nv = null;
        }

        [TestMethod]
        public void ValueIsNumeric()
        {
            var nv = CreateSystem();
            string sValue;
            
            sValue = "12345678901";
            Assert.AreEqual(true, nv.isNumeric(sValue));

            sValue = "123";
            Assert.AreEqual(true, nv.isNumeric(sValue));

            nv = null;
        }

        [TestMethod]
        public void ValueIsNotInteger()
        {
            var nv = CreateSystem();
            string sValue;

            sValue = "12345678901";
            Assert.AreEqual(false, nv.isInteger(sValue));

            sValue = "ValueIsNotInteger";
            Assert.AreEqual(false, nv.isInteger(sValue));

            sValue = "12.34";
            Assert.AreEqual(false, nv.isInteger(sValue));

            sValue = "$1234";
            Assert.AreEqual(false, nv.isInteger(sValue));

            nv = null;
        }

        [TestMethod]
        public void ValueIsInteger()
        {
            var nv = CreateSystem();
            string sValue;

            sValue = "123";
            Assert.AreEqual(true, nv.isInteger(sValue));

            sValue = "0000001";
            Assert.AreEqual(true, nv.isInteger(sValue));

            sValue = "0000000000000001";
            Assert.AreEqual(true, nv.isInteger(sValue));

            nv = null;
        }
    
    }
}
