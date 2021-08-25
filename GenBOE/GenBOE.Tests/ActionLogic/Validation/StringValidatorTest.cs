// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringValidatorTest
    {
        [TestMethod]
        public void StringValidator_IsValid()
        {
            UniqueStringValidatorHelper sut = new UniqueStringValidatorHelper();

            Collection<string> stringCollection = new Collection<string>();

            stringCollection.Add("item 1");
            stringCollection.Add("item 2");
            stringCollection.Add("item 3");
            stringCollection.Add("item 4");
            stringCollection.Add("item 5");


            Boolean stringValue = sut.Validator(stringCollection);

            Assert.IsTrue(stringValue);
        }

        [TestMethod]
        public void StringValidator_InValid()
        {
            UniqueStringValidatorHelper sut = new UniqueStringValidatorHelper();

            Collection<string> stringCollection = new Collection<string>();

            stringCollection.Add("item 1");
            stringCollection.Add("item 2");
            stringCollection.Add("item 3");
            stringCollection.Add("item 4");
            stringCollection.Add("item 3");


            Boolean stringValue = sut.Validator(stringCollection);

            Assert.IsFalse(stringValue);
        }
    }
}
