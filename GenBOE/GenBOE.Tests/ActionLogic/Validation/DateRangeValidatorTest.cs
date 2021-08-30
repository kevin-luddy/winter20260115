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
    public class DateRangeValidatorTest
    {
        /// <summary>
        /// Checks that the validation returns no response
        /// when inData does not have either a Start Date 
        /// or an End Date.
        /// </summary>
        [TestMethod]
        public void validationTest()
        {
            //Variable Declarations
            Collection<Dictionary<String, String>> inDataStart = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>() { {"StartDate", "12/12/2014"}, {"EndDate","123"}} };
            Collection<Dictionary<String, String>> inDataEnd = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>() { {"StartDate", "123"}, {"EndDate","12/12/2015"}} };

            Collection<Dictionary<String, String>> inDataSame = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>(){ {"StartDate", "12/12/2014"}, {"EndDate", "12/12/2014"} }};
            Collection<Dictionary<String, String>> inDataCorrect = new Collection<Dictionary<string, string>>(){
                new Dictionary<String, String>(){ {"StartDate", "12/12/2014"}, {"EndDate", "12/12/2015"} }};

            DateRangeValidator sut = new DateRangeValidator();

            //Act
            Collection<String> responseStart = sut.validation(null, inDataStart);
            Collection<String> responseEnd = sut.validation(null, inDataEnd);
            Collection<String> responseSame = sut.validation(null, inDataSame);
            Collection<String> responseCorrect = sut.validation(null, inDataCorrect);

            //Assert
            Assert.AreEqual(0, responseStart.Count);
            Assert.AreEqual(0, responseEnd.Count);
            Assert.AreEqual(0, responseSame.Count);
            Assert.AreEqual(0, responseCorrect.Count);
        }

        /// <summary>
        /// Checks that the validation returns the correct
        /// response for when the starte date is after the
        /// end date from inData
        /// </summary>
        [TestMethod]
        public void validationTestResponse()
        {
            //Variable Declarations
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>(){ {"StartDate", "12/12/2015"}, {"EndDate", "12/12/2014"} }};
            
            DateRangeValidator sut = new DateRangeValidator();

            //Act
            Collection<String> response = sut.validation(null, inData);

            //Assert
            Assert.AreEqual(1, response.Count);
            Assert.AreEqual("End Date must be after the Start Date.", response[0]);
        }

        /// <summary>
        /// Checks that the validation returns no response
        /// when indata does not have either a start or
        /// an end date. 
        /// </summary>
        [TestMethod]
        public void validationTestinDataNull()
        {
            //Variable Declarations
            Collection<Dictionary<String, String>> inDataStart = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>() { {"StartDate", "12/12/2014"} }};
            Collection<Dictionary<String, String>> inDataEnd = new Collection<Dictionary<String, String>>(){
                new Dictionary<String, String>() { {"EndDate", "12/12/2015"} }};

            DateRangeValidator sut = new DateRangeValidator();

            //Act
            Collection<String> responseStart = sut.validation(null, inDataStart);
            Collection<String> responseEnd = sut.validation(null, inDataEnd);

            //Assert
            Assert.AreEqual(0, responseStart.Count);
            Assert.AreEqual(0, responseEnd.Count);
        }

        /// <summary>
        /// Checks to make sure that the ArgumentNullException 
        /// is thrown when inData is Null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "inData was not NULL")]
        public void validationTestNullindata()
        {
            //Variable Declarations
            Collection<Dictionary<String, String>> inData = null;

            DateRangeValidator sut = new DateRangeValidator();

            //Act
            sut.validation(null, inData);
        }
    }
}
