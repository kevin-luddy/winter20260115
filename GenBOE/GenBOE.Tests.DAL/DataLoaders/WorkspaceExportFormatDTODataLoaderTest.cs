// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;
    using System.Transactions;

    [TestClass]
    public class WorkspaceExportFormatDTODataLoaderTest
    {
        [TestInitialize]
        /// Since this function is defined as ClassInitialize, it will be the first function
        /// called in this entire class. Therefore, the workspace used in all of these test methods
        /// will be the same
        public void GetCurrentWorkspace()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
        }

        /// <summary>
        /// Test GetWorkspaceExportFormatsForWorkspace
        /// </summary>
        [TestMethod]
        public void L_GetAllWorkspaceExportFormatIdsForWorkspaceId()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            Collection<WorkspaceExportFormatDTO> exportFormatIdsForWS = sut.GetWorkspaceExportFormatsForWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(1, exportFormatIdsForWS.Where(x => x.ExportFormat.TemplateId == 1).First().ExportFormat.TemplateId); // check landscape
            Assert.AreEqual(2, exportFormatIdsForWS.Where(x => x.ExportFormat.TemplateId == 2).First().ExportFormat.TemplateId); // check portrait
            Assert.AreEqual(GlobalTestCaseSetup.GlobalWorkspaceTemplateID, // check our custom for this WS
                            exportFormatIdsForWS.Where(x => x.ExportFormat.TemplateId == GlobalTestCaseSetup.GlobalWorkspaceTemplateID).First().ExportFormat.TemplateId);
        }

        /// <summary>
        /// Test that a template set to Available to all Workspaces is returned by GetWorkspaceExportFormatsForWorkspace
        /// </summary>
        [TestMethod]
        public void L_GetAllWorkspaceExportFormatIdsForWorkspaceId_AvailableToAll()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // Update Global Template to be available to all
            WorkspaceExportFormatDTO template = sut.GetById(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);
            template.IsAvailableToAllWorkspaces = true;
            template.Updateable = UpdateType.Upsert;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(template);
                sut.DeleteTemplateForAllWorkspaces(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);
                scope.Complete();
            }

            ICollection<WorkspaceExportFormatDTO> results =
                sut.GetWorkspaceExportFormatsForWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // Delete global template
            template = sut.GetById(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);
            template.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(template);
                scope.Complete();
            }

            Assert.IsTrue(results.Select(x => x.Id).Contains(GlobalTestCaseSetup.GlobalWorkspaceTemplateID));
        }

        [TestMethod]
        public void L_GetAllWorkspaceExportFormatIds()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            Collection<int> exportFormatIds = sut.GetAllWorkspaceExportFormatIds().Select(x=>x.Id).ToCollection();

            Assert.IsTrue(exportFormatIds.Count() >= 3); // there should be at least 3
            Assert.AreEqual(1, exportFormatIds.Where(x => x == 1).First()); // check landscape
            Assert.AreEqual(2, exportFormatIds.Where(x => x == 2).First()); // check portrait
            Assert.AreEqual(GlobalTestCaseSetup.GlobalWorkspaceTemplateID, // check our custom for this WS
                            exportFormatIds.Where(x => x == GlobalTestCaseSetup.GlobalWorkspaceTemplateID).First());
        }

        [TestMethod]
        public void L_GetWorkspaceExportFormat()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            OutputFormatTemplate template;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                template = gbe.OutputFormatTemplates.OrderBy(x => Guid.NewGuid()).FirstOrDefault();   
            }

            if (template != null)
            {
                WorkspaceExportFormatDTO exportFormatForWS = sut.GetById(template.TemplateID);

                // 1 -> UpdateableDTO.Id
                // 2 -> UpdateableDTO.Updateable
                Assert.AreEqual(template.TemplateID, exportFormatForWS.ExportFormat.TemplateId);
                Assert.AreEqual(template.TemplateDescription, exportFormatForWS.ExportFormatDescription);
                Assert.AreEqual(template.Template, exportFormatForWS.ExportFormatName);
                // 5
                CollectionAssert.AreEqual(template.TemplateFile, exportFormatForWS.FileData);
                Assert.AreEqual(template.IsActive, exportFormatForWS.IsActive);
                Assert.AreEqual(template.UpdateDT, exportFormatForWS.UpdateDate);
                Assert.AreEqual(template.ParentTemplateID, exportFormatForWS.ExportFormat.ParentTemplateId);
                // 9-> UpdateableDTO.Updatedatelong
                // 10
                Assert.AreEqual(template.IsAvailableToAllWorkspaces, exportFormatForWS.IsAvailableToAllWorkspaces);
            }

            // File Path properties are not in the database, so those are ignored.
            Type dtoType = typeof(WorkspaceExportFormatDTO);
            int numProperties = dtoType.GetProperties().Count();
            Assert.AreEqual(11, numProperties, "Untested properties exist in the Workspace Export Format DTO");
        }

        [TestMethod]
        public void L_InsertWorkspaceExportFormatPicklist()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // NOT FOR TEST: create a new export format choice
            int? newWSExportFormatNullable;
            using (TransactionScope scope = new TransactionScope())
            {
                newWSExportFormatNullable = sut.Save(new WorkspaceExportFormatDTO
                {
                    ExportFormatName = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID,
                    ExportFormatDescription = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID + "1",
                    FileData = GlobalTestCaseSetup.GlobalWorkspaceExportFile,
                    Updateable = UpdateType.Upsert
                });
                scope.Complete();
            }

            int newWSExportFormat = (newWSExportFormatNullable.HasValue ? (int)newWSExportFormatNullable : -1);

            Assert.IsTrue(newWSExportFormat > 0);

            // Delete the template
            using (TransactionScope scope = new TransactionScope())
            {
                sut.DeleteWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormat);
                scope.Complete();
            }

            // TEST: associated the new wsExportFormat
            using (TransactionScope scope = new TransactionScope())
            {
                sut.InsertWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormat);
                scope.Complete();
            }

            Collection<WorkspaceExportFormatDTO> templatesForWS = sut.GetWorkspaceExportFormatsForWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // baseline, check the GlobalTestCase insertion
            Assert.IsTrue(templatesForWS.Select(x=>x.ExportFormat.TemplateId == GlobalTestCaseSetup.GlobalWorkspaceTemplateID).Any());

            // this is the one THIS test inserted
            Assert.IsTrue(templatesForWS.Select(x => x.ExportFormat.TemplateId == newWSExportFormat).Any());
        }

        [TestMethod]
        public void L_DeleteWorkspaceExportFormatPicklist()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // NOT FOR TEST: create a new export format choice
            int? newWSExportFormatNullable;
            using (TransactionScope scope = new TransactionScope())
            {
                newWSExportFormatNullable = sut.Save(new WorkspaceExportFormatDTO
                {
                    ExportFormatName = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID,
                    ExportFormatDescription = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID + "1",
                    FileData = GlobalTestCaseSetup.GlobalWorkspaceExportFile,
                    Updateable = UpdateType.Upsert
                });
                scope.Complete();
            }

            int newWSExportFormatId = (newWSExportFormatNullable.HasValue ? (int) newWSExportFormatNullable : -1);
            Assert.IsTrue(newWSExportFormatId > 0);

            // TEST: associated the new wsExportFormat
            sut.InsertWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormatId);

            Collection<WorkspaceExportFormatDTO> templatesForWS = sut.GetWorkspaceExportFormatsForWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // baseline, check the GlobalTestCase insertion
            Assert.IsTrue(templatesForWS.Select(x => x.ExportFormat.TemplateId == GlobalTestCaseSetup.GlobalWorkspaceTemplateID).Any());

            // this is the one THIS test inserted
            Assert.IsTrue(templatesForWS.Select(x => x.ExportFormat.TemplateId == newWSExportFormatId).Any());

            // now delete the template
            sut.DeleteWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormatId);

            templatesForWS = sut.GetWorkspaceExportFormatsForWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // baseline, check the GlobalTestCase insertion
            Assert.IsTrue(templatesForWS.Where(x => x.ExportFormat.TemplateId == GlobalTestCaseSetup.GlobalWorkspaceTemplateID).Any());

            // check the deleted id is no longer present
            Assert.IsFalse(templatesForWS.Where(x => x.ExportFormat.TemplateId == newWSExportFormatId).Any());
        }

        [TestMethod]
        public void L_GetAvailableWorkspaceIdsForExportFormatId()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // NOT FOR TEST: create a new export format choice
            int? newWSExportFormatNullable;

            using (TransactionScope scope = new TransactionScope())
            {
                newWSExportFormatNullable = sut.Save(new WorkspaceExportFormatDTO
                {
                    ExportFormatName = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID,
                    ExportFormatDescription = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID + "1",
                    FileData = GlobalTestCaseSetup.GlobalWorkspaceExportFile,
                    Updateable = UpdateType.Upsert
                });
                scope.Complete();
            }

            int newWSExportFormatId = (newWSExportFormatNullable.HasValue ? (int) newWSExportFormatNullable : -1);
            Assert.IsTrue(newWSExportFormatId > 0);

            // TEST: associated the new wsExportFormat
            sut.InsertWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormatId);

            Collection<int> workspacesForTemplate = sut.GetAvailableWorkspaceIdsForExportFormatId(newWSExportFormatId);

            // Verify the new format id is not available since it was previously inserted
            Assert.IsTrue(!workspacesForTemplate.Contains(GlobalTestCaseSetup.GlobalWorkspaceID));

            // now delete the template
            sut.DeleteWorkspaceExportFormatPicklist(new Collection<int> { GlobalTestCaseSetup.GlobalWorkspaceID }, newWSExportFormatId);

            workspacesForTemplate = sut.GetAvailableWorkspaceIdsForExportFormatId(newWSExportFormatId);

            // Verify the new format id is available for the workspace
            Assert.IsTrue(workspacesForTemplate.Contains(GlobalTestCaseSetup.GlobalWorkspaceID));
        }

        [TestMethod]
        public void L_GetWorkspaceIdsUsingExportTemplateId()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            Collection<int> workspaceIds = sut.GetWorkspaceIdsByExportTemplateId(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);

            Assert.IsTrue(workspaceIds.Contains(GlobalTestCaseSetup.GlobalWorkspaceID));
        }

        /// <summary>
        /// Test DeleteTemplateForAllWorkspaces
        /// </summary>
        [TestMethod]
        public void L_DeleteTemplateForAllWorkspaces()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // Add template to workspace if missing
            if (!sut.GetAssignedWorkspaceIdsForExportTemplateId(GlobalTestCaseSetup.GlobalWorkspaceTemplateID).Any())
            {
                sut.InsertWorkspaceExportFormatPicklist(new Collection<int>() { GlobalTestCaseSetup.GlobalWorkspaceID },
                    GlobalTestCaseSetup.GlobalWorkspaceTemplateID );
            }

            sut.DeleteTemplateForAllWorkspaces(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);

            ICollection<int> workspacesUsingTemplate =
                sut.GetAssignedWorkspaceIdsForExportTemplateId(GlobalTestCaseSetup.GlobalWorkspaceTemplateID);
            Assert.IsFalse(workspacesUsingTemplate.Any());
        }

        /// <summary>
        /// Test archiving and then restoring a template
        /// </summary>
        [TestMethod]
        public void ArchiveAndRestore()
        {
            WorkspaceExportFormatDTODataLoader sut = new WorkspaceExportFormatDTODataLoader();

            // NOT FOR TEST: create a new export format choice
            int? templateId;
            using (TransactionScope scope = new TransactionScope())
            {
                templateId = sut.Save(new WorkspaceExportFormatDTO
                {
                    ExportFormatName = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID,
                    ExportFormatDescription = "Mock_" + GlobalTestCaseSetup.GlobalWorkspaceID + "1",
                    FileData = GlobalTestCaseSetup.GlobalWorkspaceExportFile,
                    IsAvailableToAllWorkspaces = true,
                    Updateable = UpdateType.Upsert
                });
                scope.Complete();
            }

            Assert.IsNotNull(templateId);
            WorkspaceExportFormatDTO template = sut.GetById((int)templateId);

            // Select workspace for template
            sut.InsertWorkspaceExportFormatPicklist(new Collection<int>() { GlobalTestCaseSetup.GlobalWorkspaceID }, template.Id);

            // Archive the template
            template.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                templateId = sut.Save(template);
                scope.Complete();
            }
            
            Assert.IsNotNull(templateId);
            template = sut.GetById((int)templateId);
            ICollection<int> workspacesForTemplate = sut.GetWorkspaceIdsByExportTemplateId((int)templateId);

            // Confirm template successfully archived
            Assert.IsFalse(template.IsActive);
            Assert.IsFalse(template.IsAvailableToAllWorkspaces);
            Assert.IsFalse(workspacesForTemplate.Any());

            // Restore the template
            sut.RestoreTemplate(template);
            template = sut.GetById((int) templateId);
            workspacesForTemplate = sut.GetWorkspaceIdsByExportTemplateId((int)templateId);

            //Confirm successfully restored
            Assert.IsTrue(template.IsActive);
            Assert.IsFalse(template.IsAvailableToAllWorkspaces);
            Assert.IsFalse(workspacesForTemplate.Any());
        }
    }
}
