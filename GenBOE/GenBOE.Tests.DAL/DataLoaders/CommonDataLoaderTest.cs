using System.Collections.ObjectModel;
using System.Linq;
using GenBOE.DataBridge.Common;
using GenBOE.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;
using IES.Common;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class CommonDataLoaderTest
    {
         [TestMethod]
         public void GetSpreadCurve()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<SpreadCurveModelView> results = cmd.GetSpreadCurve();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in SpreadCurve table");

         }

        [TestMethod]
        public void GetProjectMapSpreadCurve()
        {
            //Arrange
            CommonDataLoader cmd = new CommonDataLoader();

            //Act
            Collection<SpreadCurveModelView> results = cmd.GetProjectMapSpreadCurve();

            //Assert
            Assert.IsTrue(results.Any(), "No results found in SpreadCurve table");
        }

         [TestMethod]
         public void GetProposalStatusTypes()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<ProposalStatusTypeModelView> results = cmd.GetProposalStatusTypes();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in ProposalStatusTypes table");
         }

         [TestMethod]
         public void GetFieldTypes()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<FieldTypeModelView> results = cmd.GetFieldTypes();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in FieldLU table");
         }

         [TestMethod]
         public void getResourceName()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             Resource resource;
             using (GenBoeEntities gbe = new GenBoeEntities())
             {
                 resource = (from x in gbe.Resources
                             select x).First();
             }

             //Act
             string result = cmd.getResourceName(resource.ResourceID);

             //Assert
             Assert.AreEqual(resource.ResourceName, result, "No results found in FieldLU table");
         }

         [TestMethod]
         public void GetWorkspaceStates()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<WorkspaceStateModelView> results = cmd.GetWorkspaceStates();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in WorkspaceStates table");

         }

         [TestMethod]
         public void GetBOEStates()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<BOEStateModelView> results = cmd.GetBOEStates();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in BOEStates table");

         }

         [TestMethod]
         public void GetRoles()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<RoleModelView> results = cmd.GetRoles();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in Role table");

         }

         [TestMethod]
         public void GetSortBy()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<SortByModelView> results = cmd.GetSortBy();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in Role table");
         }

         [TestMethod]
         public void GetMOQTypes()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act
             Collection<MOQTypeModelView> results = cmd.GetMOQTypes();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in MOQType table");

         }

        [TestMethod]
        public void GetEmails()
        {
            //Arrange
            CommonDataLoader cmd = new CommonDataLoader();

            //Act
            Collection<EmailModelDomain> results = cmd.GetEmails();

            //Assert
            Assert.IsTrue(results.Count > 0, "No results found in EmailLU table");

            // test save
            EmailModelDomain first = results.First();
            bool initialDefault = first.DefaultOn;
            bool initialForced = first.Forced;
            string recipient = first.Recipient;
            EmailTypes emailType = first.EmailType;

            first.DefaultOn = !initialDefault;
            first.Forced = true;

            cmd.SaveSystemEmail(first);
            results = cmd.GetEmails();

            first = results.First(r => r.EmailType == emailType);
            Assert.AreEqual(!initialDefault, first.DefaultOn);
            Assert.AreEqual(true, first.Forced);
            Assert.IsNotNull(recipient);

            first.DefaultOn = initialDefault;
            first.Forced = initialForced;

            cmd.SaveSystemEmail(first);
        }
        
         [TestMethod]
         public void GetReports()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act
             Collection<ReportDTO> results = cmd.GetReports();

             //Assert
             Assert.IsTrue(results.Count > 0, "No results found in Reports table");
         }

         [TestMethod]
         public void GetSegmentTypes()
         {

             CommonDataLoader cmd = new CommonDataLoader();

             Collection<SegmentTypeModelView> results = cmd.GetSegmentTypes();

             Assert.IsTrue(results.Count > 0, "No results found in Segment Type table");
         }

         [TestMethod]
         public void GetOdcSpreadCurve()
         {
             //Arrange
             CommonDataLoader cmd = new CommonDataLoader();

             //Act

             Collection<OtherDirectCostSpreadCurveModelView> results = cmd.GetODCSpreadCurve();

             //Assert
             Assert.IsTrue(results.Count == 3, "More than 3 results found");

         }

         [TestMethod]
         public void GetSumVariableResourceTypes()
         {

             CommonDataLoader cmd = new CommonDataLoader();

             Collection<SumVariableResourceTypeModelView> results = cmd.GetSumVariableResourceTypes();

             Assert.IsTrue(results.Count > 0, "No results found in SumVariableResourceTypeLU table");
         }
        

    }
}
