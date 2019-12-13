// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StartEndDateTypeValidatorTest
    {
        private DateTime? _StartDate;
        private DateTime? _EndDate;
        private string _TypeForErrorMessage;
        private string _ElementName;

        /// <summary>
        /// This checks to make sure that the date validation is between 
        /// the start and end date as well as to make sure that the
        /// start date is before the end date.
        /// </summary>
        [TestMethod]
        public void StartEndDateTypeValidatorvalidationTest()
        {
            //Variable Declarations
            List<IStartEndDates> value = new List<IStartEndDates>(){
                new verificationDateTime(){ StartDate = new DateTime(2014, 11, 20), EndDate = null },
                new verificationDateTime(){ StartDate = null, EndDate = new DateTime(2015, 12, 21) },
                new verificationDateTime(){ StartDate = new DateTime(2015, 11, 21), EndDate = new DateTime(2014, 11, 19) },
                new verificationDateTime(){ StartDate = new DateTime(2014, 11, 19), EndDate = new DateTime(2015, 11, 21) },
                new verificationDateTime(){ StartDate = new DateTime(2014, 11, 20), EndDate = new DateTime(2015, 11, 20) },
                new verificationDateTime(){ StartDate = new DateTime(2014, 11, 21), EndDate = new DateTime(2015, 11, 19) }};
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>();

            _StartDate = new DateTime(2014, 11, 20);
            _EndDate = new DateTime(2015, 11, 20);
            _TypeForErrorMessage = "Type Error message";
            _ElementName = "Elemented Name";

            StartEndDateTypeValidator sut1 = new StartEndDateTypeValidator(_StartDate, _EndDate, _TypeForErrorMessage, _ElementName, false);
            StartEndDateTypeValidator sut2 = new StartEndDateTypeValidator(_StartDate, _EndDate, _TypeForErrorMessage, _ElementName, true);
            _ElementName = String.IsNullOrEmpty(_ElementName) ? "" : _ElementName + " ";

            //Act
            Collection<String> returnValue1 = sut1.validation(value, inData);
            Collection<String> returnValue2 = sut2.validation(value, inData);
            
            //Assert
            Assert.AreEqual(3, returnValue1.Count);
            Assert.AreEqual(_ElementName + "Start date must be before End date", returnValue1[0]);
            Assert.AreEqual(_ElementName + "Start date must be after the " + _TypeForErrorMessage + " Start date", returnValue1[1]);
            Assert.AreEqual(_ElementName + "End date must be before the " + _TypeForErrorMessage + " End date", returnValue1[2]);

            Assert.AreEqual(3, returnValue2.Count);
            Assert.AreEqual(_ElementName + "Start date must be before End date", returnValue2[0]);
            Assert.AreEqual(_ElementName + "Start date must be after the " + _TypeForErrorMessage + " Start date", returnValue2[1]);
            Assert.AreEqual(_ElementName + "End date must be before the " + _TypeForErrorMessage + " End date", returnValue2[2]);
        }

        /// <summary>
        /// This checks to make sure that the ArgumentNullException
        /// is thrown when value is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException),"Value was not NULL")]
        public void StartEndDateTypeValidatorvalidationTestValueNull()
        {
            //Variable Declarations
            object value = null;
            Collection<Dictionary<String, String>> inData = new Collection<Dictionary<String, String>>();

            StartEndDateTypeValidator sut = new StartEndDateTypeValidator(_StartDate, _EndDate, _TypeForErrorMessage);

            //Act
            sut.validation(value, inData);
        }
    }
}