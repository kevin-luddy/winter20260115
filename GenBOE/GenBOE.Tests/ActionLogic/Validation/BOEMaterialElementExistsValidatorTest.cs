// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEMaterialElementExistsValidatorTest
    {
        [TestMethod]
        public void BoeMaterialElementExists()
        {
            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            BOEMaterialElementExistsValidator sut = new BOEMaterialElementExistsValidator(factory.Object);

            BoeDTO boe = new BoeDTO();
            factory.Setup(x => x.CreateFullBoe(1)).Returns(new FullBoe(boe) { Id = 1 });
            factory.Setup(x => x.CreateFullBoe(2)).Returns(new FullBoe(boe) { Id = 2 });
            retriever.Setup(x => x.CheckIfBoeContainsMaterialElements(1)).Returns(true);
            retriever.Setup(x => x.CheckIfBoeContainsMaterialElements(2)).Returns(true);


            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "1"},
                {"Material", "false" }
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "2"},
                {"Material", "on"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsTrue(returnValue);


        }
    }
}
