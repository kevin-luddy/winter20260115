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
    using GenBOE.Dtos;
    using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Common;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEWBSMoveValidatorTest
    {
        Mock<VariableCircularReferenceChecker> _VariableCircularReferenceChecker;
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<IWorkspaceVariableDTODataLoader> _WorkspaceVariableDTODataLoader = new Mock<IWorkspaceVariableDTODataLoader>();
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();

        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
            _VariableCircularReferenceChecker = new Mock<VariableCircularReferenceChecker>(_WorkspaceVariableDTODataLoader.Object);
        }

        #region validation (object value, Collection<Dictionary<string, string>> inData)
        /// <summary>
        /// This test case will test validation. It will verify that the BOE
        /// WBS move will correctly notify if it will create a circular
        /// reference. The collection of strings will return once an error 
        /// has been passed through
        /// </summary>
        [TestMethod]
        public void validationTestString()
        {
            //Value Declarations
            string value = "123";
            string valueZero = "0";

            FullBoe boe = new FullBoe() { Id = 1 };
            FullWbs wbs = new FullWbs() { Id = 123 };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>() {
                new Dictionary<string, string>(){ {"BoeID", "1"}, {"WorksapceID", "2"} }};
            Collection<Dictionary<string, string>> inDataZero = new Collection<Dictionary<string, string>>() {
                new Dictionary<string, string>(){ {"BoeID", "0"}, {"WorkspaceID", "2"} }};

            //Setup
            factory.Setup(x => x.CreateFullBoe(1)).Returns(boe);
            factory.Setup(x => x.CreateFullWbs(123)).Returns(wbs);

            retriever.Setup(x => x.GetFullWorkspaceById(0)).Returns(workspace);
            _VariableCircularReferenceChecker.Setup(x => x.BOEWBSMoveCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), boe, wbs, null, workspace)).Returns(true);

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueZero = sut.validation(valueZero, inData);
            Collection<string> returninDataZero = sut.validation(value, inDataZero);

            //Assert
            Assert.AreEqual(1, returnValue.Count);
            Assert.AreEqual(0, returnValueZero.Count);
            Assert.AreEqual(0, returninDataZero.Count);
            Assert.AreEqual("The selected WBS # would create a circular reference.", returnValue[0]);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in null or no inData (Collection<Dictionary<String, String>>)  
        /// will return the string "Invalid data" in a collection.
        /// </summary>
        [TestMethod]
        public void validationTestStringNoinData()
        {
            //Value Declarations
            string value = "123";
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();
            Collection<Dictionary<string, string>> indataNull = null;

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueNull = sut.validation(value, indataNull);

            //Assert
            Assert.AreEqual(1, returnValue.Count);
            Assert.AreEqual(1, returnValueNull.Count);
            Assert.AreEqual("Invalid data", returnValue[0]);
            Assert.AreEqual("Invalid data", returnValueNull[0]);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a integer for the value will throw an InvalidCastException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void validationTestStringTypeOf()
        {
            //Value Declarations
            object value = 1;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a null string for Value will throw an ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void validationTestStringValueNull()
        {
            //Value Declarations
            string value = null;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }
        #endregion

        #region validation (object value, Collection<Dictionary<string, object>> inData)
        /// <summary>
        /// This test case will test validation. It will verify that the BOE
        /// WBS move will correctly notify if it will create a circular
        /// reference. The collection of strings will return once an error 
        /// has been passed through
        /// </summary>
        [TestMethod]
        public void validationTestObject()
        {
            //Value Declarations
            FullWbs valueFalse = new FullWbs(new WbsDTO() { Id = 1 });
            FullWbs valueTrue = new FullWbs(new WbsDTO() { Id = 1 });
            FullBoe boe = new FullBoe() { Id = 1 };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string, object>() { {"Boe", boe }, {"Workspace", workspace } }};

            //Setup
            _VariableCircularReferenceChecker.Setup(x => x.BOEWBSMoveCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), boe, valueFalse, null, workspace)).Returns(false);
            _VariableCircularReferenceChecker.Setup(x => x.BOEWBSMoveCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), boe, valueTrue, null, workspace)).Returns(true);

            factory.Setup(x => x.CreateFullWbs(valueFalse)).Returns(valueFalse);
            factory.Setup(x => x.CreateFullWbs(valueTrue)).Returns(valueTrue);

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnFalse = sut.validation(valueFalse, inData);
            Collection<string> returnTrue = sut.validation(valueTrue, inData);

            //Assert
            Assert.AreEqual(1, returnTrue.Count);
            Assert.AreEqual(0, returnFalse.Count);
            Assert.AreEqual("The selected WBS # would create a circular reference.", returnTrue[0]);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in null or no inData (Collection<Dictionary<String, Object>>)  
        /// will return the string "Invalid data" in a collection.
        /// </summary>
        [TestMethod]
        public void validationTestNoinData()
        {
            //Value Declarations
            FullWbs value = new FullWbs(new WbsDTO() { Id = 1 });
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();
            Collection<Dictionary<string, object>> inDataNull = null;

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueNull = sut.validation(value, inDataNull);

            //Assert
            Assert.AreEqual(1, returnValue.Count);
            Assert.AreEqual(1, returnValueNull.Count);
            Assert.AreEqual("Invalid data", returnValue[0]);
            Assert.AreEqual("Invalid data", returnValueNull[0]);
        }

        /// <summary>
        /// This test case will test validation. it will verify that when
        /// inData has a null FullBoe object will throw the ArgumentException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void validationTestBoeNull()
        {
            //Value Declarations
            FullWbs value = new FullWbs(new WbsDTO() { Id = 1 });
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string, object>() { {"Boe", (FullBoe)null}, {"Workspace", (FullWorkspace)null } }};

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. it will verify that when inData 
        /// has a null Fullworkdpace object will throw the ArgumentException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void validationTestWorkspaceNull()
        {
            //Value Declarations
            FullWbs value = new FullWbs(new WbsDTO() { Id = 1 });
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string, object>() { {"Boe", new FullBoe()}, {"Workspace", (FullWorkspace)null } }};

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a WbsDTO for the value will throw an InvalidCastException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void validationTestTypeOf()
        {
            //Value Declarations
            WbsDTO value = new WbsDTO() { Id = 1 };
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a null FullWbs for Value will throw an ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void validationTestValueNull()
        {
            //Value Declarations
            FullWbs value = null;
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();

            BOEWBSMoveValidator sut = new BOEWBSMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }
        #endregion
    }
}
