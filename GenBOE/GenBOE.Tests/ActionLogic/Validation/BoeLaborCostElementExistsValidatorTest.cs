// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    public class BoeLaborCostElementExistsValidatorTest
    {
        [TestMethod]
        public void BoeLaborCostElementExists()
        {
            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            BoeLaborCostElementExistsValidator sut = new BoeLaborCostElementExistsValidator(factory.Object);

            BoeDTO boe = new BoeDTO();
            factory.Setup(x => x.CreateFullBoe(1)).Returns(new FullBoe(boe) { Id = 1 } );
            factory.Setup(x => x.CreateFullBoe(2)).Returns(new FullBoe(boe) { Id = 2 } );
            ////retriever.Setup(x => x
            retriever.Setup(x => x.CheckIfBoeContainsLaborCostElements(1)).Returns(true);
            retriever.Setup(x => x.CheckIfBoeContainsLaborCostElements(2)).Returns(false);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "1"},
                {"Material", "on"}
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"BoeID", "2"},
                {"Material", "false"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsTrue(returnValue);


        }
    }
}
