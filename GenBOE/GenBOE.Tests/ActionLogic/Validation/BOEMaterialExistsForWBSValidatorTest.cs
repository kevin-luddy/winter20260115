// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEMaterialExistsForWBSValidatorTest
    {
        [TestMethod]
        public void BOEMaterialExistsForWBS_IsValid()
        {
            var wbsLoader = new Mock<WbsDTODataLoader>();
            BOEMaterialExistsForWBSValidator sut = new BOEMaterialExistsForWBSValidator(wbsLoader.Object);

            wbsLoader.Setup(x => x.IsWbsTiedToMaterialBoeAndClin(1, 1, 1)).Returns(true);
            wbsLoader.Setup(x => x.IsWbsTiedToMaterialBoeAndClin(2, 2, 2)).Returns(false);
            wbsLoader.Setup(x => x.IsWbsTiedToMaterialBoe(1, 1)).Returns(true);
            wbsLoader.Setup(x => x.IsWbsTiedToMaterialBoe(2, 2)).Returns(true);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "1"},
                {"WbsID", "1"},
                {"ClinID", "1"},
                {"Material", "on"}
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "2"},
                {"WbsID", "2"},
                {"ClinID", "2"},
                {"Material", "on"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsFalse(returnValue);

           
        }
    }
}
