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
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WBSRenumberValidatorTest
    {
        Mock<VariableCircularReferenceChecker> _VariableCircularReferenceChecker = null;
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IWorkspaceVariableDTODataLoader> _WorkspaceVariableDTODataLoader = new Mock<IWorkspaceVariableDTODataLoader>();
        Mock<Validator> wbsUniqueNumberValidator = new Mock<Validator>();

        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
            _VariableCircularReferenceChecker = new Mock<VariableCircularReferenceChecker>(_WorkspaceVariableDTODataLoader.Object);
        }

        /// <summary>
        /// Checks that WBSRenumbervalidation correctly
        /// test for a circular reference
        /// </summary>
        [TestMethod]
        public void WBSRenumbervalidationTest_CircularReference()
        {
            //Variable Declarations
            string value = "123";
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WbsID", "1"}, {"WorkspaceID", "2"} }};
            Collection<Dictionary<string, string>> inDatainUse = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WbsID", "11"}, {"WorkspaceID", "22"} }};

            ICollection<FullWbs> allWbs = new Collection<FullWbs>();
            ICollection<FullWbs> childWbs = new Collection<FullWbs>(){ 
                 new FullWbs() { Id = 2, WorkspaceID = 1, WbsNumber = "123", inUse = true }};

            FullWbs wbs = new FullWbs() { Id = 1, WorkspaceID = 2, inUse = true, WbsNumber = "123" };
            FullWbs wbsinUse = new FullWbs() { Id = 11, inUse = false, WbsNumber = "123" };
            FullWorkspace ws = new FullWorkspace() { Id = 2 };

            Collection<string> wbsValidatorMessages = new Collection<string>() { "Test" };
            Collection<string> wbsValidatorMessagesNull = new Collection<string>();
            ValidationFactory validationFactory = GetValidationFactory();
            Mock<ValidationFactoryWrapper> _ValidationFactory = new Mock<ValidationFactoryWrapper>();

            wbsUniqueNumberValidator.Setup(x => x.validation(value, inData)).Returns(wbsValidatorMessages);
            wbsUniqueNumberValidator.Setup(x => x.validation(value, inDatainUse)).Returns(wbsValidatorMessagesNull);
            _ValidationFactory.Setup(x => x.Instance).Returns(validationFactory);
            
            factory.Setup(x => x.CreateFullWbs(wbs.Id)).Returns(wbs);
            factory.Setup(x => x.CreateFullWbs(wbsinUse.Id)).Returns(wbsinUse);

            retriever.Setup(x => x.GetFullWorkspaceById(wbs.WorkspaceID)).Returns(ws);
            retriever.Setup(x => x.GetFullWorkspaceById(-1)).Returns(ws);

            _VariableCircularReferenceChecker.Setup(x => x.WBSRenumberCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), wbs, value, null, ws)).Returns(true);
            _VariableCircularReferenceChecker.Setup(x => x.WBSRenumberCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), wbsinUse, value, null, ws)).Returns(false);
            
            retriever.Setup(x => x.GetParentWbs(wbs.WorkspaceID, wbs.WbsNumber)).Returns(allWbs);
            retriever.Setup(x => x.GetChildWbs(wbs.WorkspaceID, wbs.WbsNumber)).Returns(childWbs);

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);
            Collection<string> returnValueinUse = sut.validation(value, inDatainUse);

            //Assert
            Assert.AreEqual(2, returnValue.Count);
            Assert.AreEqual("Test", returnValue[0]);
            Assert.AreEqual("The new WBS # would create a circular reference.", returnValue[1]);
            Assert.AreEqual(0, returnValueinUse.Count);
        }

        /// <summary>
        /// Checks that WBSRenumbervalidation correctly
        /// tests for a Parent WBS having a BOE
        /// </summary>
        [TestMethod]
        public void WBSRenumbervalidationTest_ParentHasWbs()
        {
            //Variable Declarations
            string value = "1.1";
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WbsID", "1"}, {"WorkspaceID", "2"} }};

            ICollection<FullWbs> parentWbs = new Collection<FullWbs>(){
                 new FullWbs() { Id = 2, WorkspaceID = 1, WbsNumber = "1", inUse = true }};
            ICollection<FullWbs> childWbs = new Collection<FullWbs>();

            FullWbs wbs = new FullWbs() { Id = 1, WorkspaceID = 2, inUse = true, WbsNumber = "3" };
            FullWbs wbsinUse = new FullWbs() { Id = 11, inUse = false, WbsNumber = "123" };
            FullWorkspace ws = new FullWorkspace() { Id = 2 };

            Collection<string> wbsValidatorMessages = new Collection<string>() { "Test" };
            Collection<string> wbsValidatorMessagesNull = new Collection<string>();
            ValidationFactory validationFactory = GetValidationFactory();
            Mock<ValidationFactoryWrapper> _ValidationFactory = new Mock<ValidationFactoryWrapper>();
            _ValidationFactory.Setup(x => x.Instance).Returns(validationFactory);

            wbsUniqueNumberValidator.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>());

            factory.Setup(x => x.CreateFullWbs(wbs.Id)).Returns(wbs);
            factory.Setup(x => x.CreateFullWbs(wbsinUse.Id)).Returns(wbsinUse);

            retriever.Setup(x => x.GetFullWorkspaceById(wbs.WorkspaceID)).Returns(ws);
            retriever.Setup(x => x.GetFullWorkspaceById(-1)).Returns(ws);

            _VariableCircularReferenceChecker.Setup(x => x.WBSRenumberCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<FullWbs>(), value, null, ws)).Returns(false);

            retriever.Setup(x => x.GetParentWbs(wbs.WorkspaceID, wbs.WbsNumber)).Returns(new Collection<FullWbs>());
            retriever.Setup(x => x.GetParentWbs(wbs.WorkspaceID, value)).Returns(parentWbs);
            retriever.Setup(x => x.GetChildWbs(wbs.WorkspaceID, It.IsAny<string>())).Returns(childWbs);

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);

            //Assert
            Assert.AreEqual(1, returnValue.Count);
            Assert.IsTrue(returnValue[0].Contains("which would become a parent of"));
        }


        /// <summary>
        /// Checks that WBSRenumbervalidation correctly
        /// tests for a Child WBS having a BOE
        /// </summary>
        [TestMethod]
        public void WBSRenumbervalidationTest_ChildHasWbs()
        {
            //Variable Declarations
            string value = "1";
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WbsID", "1"}, {"WorkspaceID", "2"} }};

            ICollection<FullWbs> parentWbs = new Collection<FullWbs>();
            ICollection<FullWbs> childWbs = new Collection<FullWbs>(){
                 new FullWbs() { Id = 2, WorkspaceID = 1, WbsNumber = "1.1", inUse = true }};

            FullWbs wbs = new FullWbs() { Id = 1, WorkspaceID = 2, inUse = true, WbsNumber = "3" };
            FullWbs wbsinUse = new FullWbs() { Id = 11, inUse = false, WbsNumber = "123" };
            FullWorkspace ws = new FullWorkspace() { Id = 2 };

            Collection<string> wbsValidatorMessages = new Collection<string>() { "Test" };
            Collection<string> wbsValidatorMessagesNull = new Collection<string>();
            ValidationFactory validationFactory = GetValidationFactory();
            Mock<ValidationFactoryWrapper> _ValidationFactory = new Mock<ValidationFactoryWrapper>();
            _ValidationFactory.Setup(x => x.Instance).Returns(validationFactory);

            wbsUniqueNumberValidator.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>());

            factory.Setup(x => x.CreateFullWbs(wbs.Id)).Returns(wbs);
            factory.Setup(x => x.CreateFullWbs(wbsinUse.Id)).Returns(wbsinUse);

            retriever.Setup(x => x.GetFullWorkspaceById(wbs.WorkspaceID)).Returns(ws);
            retriever.Setup(x => x.GetFullWorkspaceById(-1)).Returns(ws);

            _VariableCircularReferenceChecker.Setup(x => x.WBSRenumberCreatesCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<FullWbs>(), value, null, ws)).Returns(false);

            retriever.Setup(x => x.GetParentWbs(wbs.WorkspaceID, It.IsAny<string>())).Returns(parentWbs);
            retriever.Setup(x => x.GetChildWbs(wbs.WorkspaceID, value)).Returns(childWbs);
            retriever.Setup(x => x.GetChildWbs(wbs.WorkspaceID, wbs.WbsNumber)).Returns(new Collection<FullWbs>());

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> returnValue = sut.validation(value, inData);

            //Assert
            Assert.AreEqual(1, returnValue.Count);
            Assert.IsTrue(returnValue[0].Contains("BOEs exist for one or more level 2 WBSs"));
        }

        /// <summary>
        /// Checks the different ways to input no data and
        /// that the validation returns either an Invalid Data
        /// response or no response at all.
        /// </summary>
        [TestMethod]
        public void WBSRenumbervalidationTestNoinData()
        {
            //Variable Declarations
            string value = "123";
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();
            Collection<Dictionary<string, string>> inDataNull = null;
            Collection<Dictionary<string, string>> inDataWbsId = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WbsID", "123"} }};
            Collection<Dictionary<string, string>> inDataWorkspace = new Collection<Dictionary<string, string>>(){
                new Dictionary<string, string>() { {"WorkspaceID", "1"} }};

            Collection<string> wbsValidatorMessages = new Collection<string>() { "Test" };
            Collection<string> wbsValidatorMessagesNull = new Collection<string>() { "Test Null" };
            Collection<string> wbsValidatorMessagesWbsId = new Collection<string>() { "Test WbsId" };
            Collection<string> wbsValidatorMessagesWorkspaceId = new Collection<string>() { "Test WorkspaceId" };

            ValidationFactory validationFactory = GetValidationFactory();
            Mock<ValidationFactoryWrapper> _ValidationFactory = new Mock<ValidationFactoryWrapper>();

            _ValidationFactory.Setup(x => x.Instance).Returns(validationFactory);
            wbsUniqueNumberValidator.Setup(x => x.validation(value, inData)).Returns(wbsValidatorMessages);
            wbsUniqueNumberValidator.Setup(x => x.validation(value, inDataNull)).Returns(wbsValidatorMessagesNull);
            wbsUniqueNumberValidator.Setup(x => x.validation(value, inDataWbsId)).Returns(wbsValidatorMessagesWbsId);
            wbsUniqueNumberValidator.Setup(x => x.validation(value, inDataWorkspace)).Returns(wbsValidatorMessagesWorkspaceId);

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            Collection<string> valueReturn = sut.validation(value, inData);
            Collection<string> valueReturnNull = sut.validation(value, inDataNull);
            Collection<string> valueReturnWbsId = sut.validation(value, inDataWbsId);
            Collection<string> valueReturnWorkspace = sut.validation(value, inDataWorkspace);

            //Assert
            Assert.AreEqual(2, valueReturn.Count);
            Assert.AreEqual("Test", valueReturn[0]);
            Assert.AreEqual("Invalid data", valueReturn[1]);
            
            Assert.AreEqual(2, valueReturnNull.Count);
            Assert.AreEqual("Test Null", valueReturnNull[0]);
            Assert.AreEqual("Invalid data", valueReturnNull[1]);

            Assert.AreEqual(1, valueReturnWbsId.Count);
            Assert.AreEqual("Test WbsId", valueReturnWbsId[0]);

            Assert.AreEqual(1, valueReturnWorkspace.Count);
            Assert.AreEqual("Test WorkspaceId", valueReturnWorkspace[0]);
        }

        /// <summary>
        /// This checks to make sure that the InvalidCastException
        /// is thrown when value is not of type string.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Value is of type String")]
        public void WBSRenumbervalidationTesttypeOf()
        {
            //Variable Declarations
            object value = 123;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// This checks to make sure that the ArgumentNullException
        /// is thrown when value is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Value was not NULL")]
        public void WBSRenumbervalidationTestValueNull()
        {
            //Variable Declarations
            object value = null;
            Collection<Dictionary<string, string>> inData = new Collection<Dictionary<string, string>>();

            WBSRenumberValidator sut = new WBSRenumberValidator(_VariableCircularReferenceChecker.Object, factory.Object);

            //Act
            sut.validation(value, inData);
        }

        /// <summary>
        /// Needed to create the Mock Validation Factory to be used
        /// for this test
        /// </summary>
        private ValidationFactory GetValidationFactory()
        {
			Mock<Validator> workspaceUniqueNameValidator = new Mock<Validator>();
			Mock<Validator> workspaceUniqueShortnameValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotGroupValidator = new Mock<Validator>();
			Mock<Validator> boeTaskIdUniqueValidator = new Mock<Validator>();
			Mock<Validator> boeDateRangeValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> performingOrgUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> wbsRenumberValidator = new Mock<Validator>();
			Mock<Validator> boeWBSMoveValidator = new Mock<Validator>();
			Mock<Validator> boeCLINMoveValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialExistsforWbsValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueDescValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotSubcontractorValidator = new Mock<Validator>();

            ValidationFactory vf = new ValidationFactory(
                workspaceUniqueNameValidator.Object,
                workspaceUniqueShortnameValidator.Object,
                workspaceCostVolumeLeadNotGroupValidator.Object,
                boeTaskIdUniqueValidator.Object,
                boeDateRangeValidator.Object,
                wbsUniqueNumberValidator.Object,
                resourceUniqueIDValidator.Object,
                performingOrgUniqueIDValidator.Object,
                wbsRenumberValidator.Object,
                boeWBSMoveValidator.Object,
                boeCLINMoveValidator.Object,
                boeMaterialExistsforWbsValidator.Object,
                boeMaterialElementExistsValidator.Object,
                resourceUniqueDescValidator.Object,
                workspaceCostVolumeLeadNotSubcontractorValidator.Object);

            return vf;
        }
    }

    /// <summary>
    /// Create a wrapper class (Adapter Pattern) for the ValidationFactory 
    /// class and fake its static members.
    /// Note: This approach is used since MOQ can't fake static members.
    /// </summary>
    public class ValidationFactoryWrapper
    {
        public virtual ValidationFactory Instance
        {
            get
            {
                return ValidationFactory.Instance;
            }
        }
    }
}
