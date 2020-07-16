// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEDateValidatorTest : MOQObject
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IPermissionsDTODataLoader> permissionsDTODataLoader = new Mock<IPermissionsDTODataLoader>();

        [TestInitialize]
        override public void Setup()
        {
            base.Setup();

            DateTime workspaceStart = Convert.ToDateTime("01/01/2011");
            DateTime workspaceEnd = Convert.ToDateTime("01/01/2014");
            DateTime clinStart = Convert.ToDateTime("02/01/2012");
            DateTime clinEnd = Convert.ToDateTime("01/01/2013");
            DateTime boeStart = Convert.ToDateTime("04/01/2012");
            DateTime boeEnd = Convert.ToDateTime("12/01/2012");

            this.Workspace.ContractStartDate = workspaceStart;
            this.Workspace.ContractEndDate = workspaceEnd;
            this.Clin1.StartDate = clinStart;
            this.Clin1.EndDate = clinEnd;
            this.Boe1.StartDate = boeStart;
            this.Boe1.EndDate = boeEnd;
        }

        [TestMethod]
        public void BOEDataValidator_IsValid()
        {
            //workspaceDTOMapper.Setup(x => x.GetWorkspaceById(this.Workspace.Id)).Returns(this.Workspace);
  
            BOEDateValidator sut = new BOEDateValidator(factory.Object);

            // set up dictionary
            Collection<Dictionary<string, string>> boeDictionary = new Collection<Dictionary<string, string>>();
            boeDictionary.Add(new Dictionary<string, string>());
            boeDictionary.First<Dictionary<string, string>>().Add("pkid", this.Boe1.Id.ToString());
            boeDictionary.First<Dictionary<string, string>>().Add("StartDate", this.Boe1.StartDate.ToString("MM/yyyy"));
            boeDictionary.First<Dictionary<string, string>>().Add("EndDate", this.Boe1.EndDate.ToString("MM/yyyy"));
            boeDictionary.First<Dictionary<string, string>>().Add("context", "true");
            boeDictionary.First<Dictionary<string, string>>().Add("contextElement", "StartDate");
            boeDictionary.First<Dictionary<string, string>>().Add("ClinID", this.Boe1.CLINID.ToString());
            boeDictionary.First<Dictionary<string, string>>().Add("WorkspaceID", this.Boe1.WorkspaceID.ToString());

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDTODataLoader.Object);
            
            factory.Setup(x => x.CreateFullClin(this.Clin1.Id)).Returns(new FullClin(this.Clin1));
            factory.Setup(x => x.CreateFullClin(this.Clin1)).Returns(new FullClin(this.Clin1));
            factory.Setup(x => x.CreateFullWorkspace(this.Boe1.WorkspaceID)).Returns(new FullWorkspace(this.Workspace));
            retriever.Setup(x => x.GetClinById(this.Clin1.Id)).Returns(this.Clin1);

            object value = string.Empty; // don't need a value
            bool returnValue = sut.isValid(value, boeDictionary);
            Assert.IsTrue(returnValue);

            //now send in a bad date
            boeDictionary.Clear();
            boeDictionary.Add(new Dictionary<string, string>());
            boeDictionary.First<Dictionary<string, string>>().Add("pkid", this.Boe1.Id.ToString());
            boeDictionary.First<Dictionary<string, string>>().Add("StartDate", this.Workspace.ContractStartDate.ToString("MM/yyyy"));  // use the workspace start
            boeDictionary.First<Dictionary<string, string>>().Add("EndDate", this.Boe1.EndDate.ToString("MM/yyyy"));
            boeDictionary.First<Dictionary<string, string>>().Add("context", "true");
            boeDictionary.First<Dictionary<string, string>>().Add("contextElement", "StartDate");
            boeDictionary.First<Dictionary<string, string>>().Add("ClinID", this.Boe1.CLINID.ToString());
            boeDictionary.First<Dictionary<string, string>>().Add("WorkspaceID", this.Boe1.WorkspaceID.ToString());

            returnValue = sut.isValid(value, boeDictionary);
            Assert.IsFalse(returnValue);
        }
    }
}
