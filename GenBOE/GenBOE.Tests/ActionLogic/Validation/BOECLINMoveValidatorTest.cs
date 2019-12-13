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
    using System.Linq;
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
    public class BOECLINMoveValidatorTest
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

        #region validationTest (object value, Collectoin<Dictionary<string, string>> inData)
        /// <summary>
        /// This test case will test validation. It will verify that the BOE
        /// CLIN move will correctly notify if it will create a circular
        /// reference. The collection of strings will return once an error 
        /// has been passed through
        /// </summary>
        [TestMethod]
        public void validationTestString()
        {
            //Value Declarations
            string value = "123";
            string valueZero = "0";

            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string> { {"BoeID", "1" }, {"WorkspaceID", "2" } }};
            Collection<Dictionary<string, string>> inDataZero = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string> { {"BoeID", "0" }, {"WorkspaceID", "2" } }};

            FullBoe boeObject = new FullBoe() { Id = 1, Title = "Test 1" };
            FullClin clin = new FullClin(new ClinDTO() { Id = 123 });
            FullWorkspace ws = new FullWorkspace() { Id = 1 };

            //Setup
            factory.Setup(x => x.CreateFullBoe(1)).Returns(boeObject);
            factory.Setup(x => x.CreateFullClin(123)).Returns(clin);
            retriever.Setup(x => x.GetFullWorkspaceById(0)).Returns(ws);
            _VariableCircularReferenceChecker.Setup(x => x.BOECLINMoveCreatesCircularReference(ws, It.IsAny<VariableCircularReferenceCheckerCache>(), boeObject, clin, null)).Returns(true);

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueZero = sut.validation(valueZero, inData);
            Collection<string> returnValueinDataZero = sut.validation(value, inDataZero);

            //Assert
            Assert.IsTrue(returnValue.Count > 0);
            Assert.IsTrue(returnValueZero.Count == 0);
            Assert.IsTrue(returnValueinDataZero.Count == 0);
            Assert.AreEqual("The selected CLIN # would create a circular reference.", returnValue[0]);
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
            Collection<Dictionary<string, string>> inDataNull = null;

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueNull = sut.validation(value, inDataNull);

            //Assert
            Assert.AreEqual(1, returnValue.Count());
            Assert.AreEqual(1, returnValueNull.Count());
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
            object value = 123;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string,string>>();

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }
        
        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a null string for Value will throw an ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void validationTestStringNull()
        {
            //Value Declarations
            string value = null;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }
        #endregion

        #region validationTest (object value, Collection<Dictionary<string, object>> inData)
        /// <summary>
        /// This test case will test validation. It will verify that the BOE
        /// CLIN move will correctly notify if it will create a circular
        /// reference. The collection of strings will return once an error 
        /// has been passed through
        /// </summary>
        [TestMethod]
        public void validationTest()
        {
            //Value Declarations
            FullClin valueFalse = new FullClin(new ClinDTO() { Id = 1 });
            FullClin valueTrue = new FullClin(new ClinDTO() { Id = 2 });
            FullBoe boe = new FullBoe() { Id = 1 };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string,object>(){ {"Boe", boe}, 
                                                 {"Workspace", workspace} }};

            //Setup
            _VariableCircularReferenceChecker.Setup(x => x.BOECLINMoveCreatesCircularReference(workspace, It.IsAny<VariableCircularReferenceCheckerCache>(), boe, valueFalse, null)).Returns(false);
            _VariableCircularReferenceChecker.Setup(x => x.BOECLINMoveCreatesCircularReference(workspace, It.IsAny<VariableCircularReferenceCheckerCache>(), boe, valueTrue, null)).Returns(true);

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue1 = sut.validation(valueFalse, inData);
            Collection<string> returnValue2 = sut.validation(valueTrue, inData);

            //Assert
            Assert.AreEqual(returnValue1.Count(), 0);
            Assert.AreEqual(returnValue2.Count(), 1);
            Assert.AreEqual("The selected CLIN # would create a circular reference.", returnValue2[0]);
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
            FullClin value = new FullClin(new ClinDTO() { Id = 1 });
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();
            Collection<Dictionary<string, object>> inDataNull = null;

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueNull = sut.validation(value, inDataNull);

            //Assert
            Assert.AreEqual(1, returnValue.Count());
            Assert.AreEqual(1, returnValueNull.Count());
            Assert.AreEqual("Invalid data", returnValue[0]);
            Assert.AreEqual("Invalid data", returnValueNull[0]);
        }

        /// <summary>
        /// This test case will test validation. it will verify that when
        /// inData has a null FullBoe object will throw the ArgumentException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void validationTestboeNull()
        {
            //Value Declarations
            FullClin value = new FullClin(new ClinDTO() { Id = 1 });
            FullBoe boe = null;
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string,object>(){ {"Boe", boe}, 
                                                 {"Workspace", workspace} }};

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. it will verify that when inData 
        /// has a null Fullworkdpace object will throw the ArgumentException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]      
        public void volidationTestworkspaceNull()
        {
            //Value Declarations
            FullClin value = new FullClin(new ClinDTO() { Id = 1 });
            FullBoe boe = new FullBoe() { Id = 1 };
            FullWorkspace workspace = null;

            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>(){
                new Dictionary<string,object>(){ {"Boe", boe}, 
                                                 {"Workspace", workspace} }};

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a null object for Value will throw an ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void validationTestvalueNull()
        {
            //Value Declarations
            object value = null;
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This test case will test validation. It will verify that passing
        /// in a ClinDTO for the value will throw an InvalidCastException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void validationTestvalueTypeOf()
        {
            //Value Declarations
            ClinDTO value = new ClinDTO();
            Collection<Dictionary<string, object>> inData = new Collection<Dictionary<string, object>>();

            BOECLINMoveValidator sut = new BOECLINMoveValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }
        #endregion

    }
}
