namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;

    /// <summary>
    /// RTE Template Controller Logic
    /// </summary>
    public class RTETemplatesControllerLogic : GenBOEControllerLogic, IRTETemplatesControllerLogic
    {
        #region Private Properties

        /// <summary>
        /// The version loader
        /// </summary>
        private IWorkspaceVersionMetaDataDTODataLoader versionLoader;

        /// <summary>
        /// rteTemplate Data Loader
        /// </summary>
        private IRteTemplateDataLoader rteTemplateDataLoader;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rteTemplateDataLoader">Template Loader</param>
        /// <param name="versionLoader">Version Loader</param>
        public RTETemplatesControllerLogic(IRteTemplateDataLoader rteTemplateDataLoader, IWorkspaceVersionMetaDataDTODataLoader versionLoader)
        {
            this.rteTemplateDataLoader = rteTemplateDataLoader;
            this.versionLoader = versionLoader;
        }

        /// <summary>
        /// Retrieves Templates for the specific workspace
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>RTE Templates</returns>
        public ICollection<RteCustomTemplateModelView> GetTemplates(int wsId)
        {
            ICollection<RteCustomTemplateModelView> templates = this.rteTemplateDataLoader.GetTemplates(wsId);

            foreach (RteCustomTemplateModelView template in templates)
            {
                IList<RteCustomTemplateQuestionModelView> questions = template.Questions.OrderBy(q => q.SortOrder).ToList();

                // Set sort order property
                for (int i = 0; i < questions.Count; i++) { questions[i].SortOrder = i; }

                template.Questions = questions;
            }

            return templates;
        }

        /// <summary>
        /// Gets all of the sources from lookup table.
        /// </summary>
        /// <returns>All of the sources from lookup table.</returns>
        public ICollection<RteCustomTemplateSourceModelView> GetSources()
        {
            return this.rteTemplateDataLoader.GetSources();
        }

        /// <summary>
        /// Validates RTE templates
        /// </summary>
        /// <param name="templates">Templates to validate</param>
        /// <exception cref="GenValidationException">Throws GenValidationException with validation errors, if any</exception>
        public void ValidateTemplates(ICollection<RteCustomTemplateModelView> templates)
        {
            if (templates == null || templates.None()) { throw new ArgumentNullException(nameof(templates)); }

            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(t.Description)))
            {
                validationErrors.Add(new ValidationMessage("Template Name is required."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && !t.Questions.Any(q => q.Updateable != UpdateType.Deleted)))
            {
                validationErrors.Add(new ValidationMessage("At least one prompt is required for a Template."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && t.Questions.Any(q => q.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(q.Text))))
            {
                validationErrors.Add(new ValidationMessage("Prompt text is required."));
            }

            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }
        }

        /// <summary>
        /// Save Templates
        /// </summary>
        /// <param name="templates">Templates to save</param>
        /// <param name="ws">WS to which the template belongs</param>
        public void SaveTemplates(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws)
        {
            if (templates == null || templates.None()) { throw new ArgumentNullException(nameof(templates)); }
            if (ws == null ) { throw new ArgumentNullException(nameof(ws)); }

            ICollection<RteCustomTemplateModelView> templatesFromDb = this.rteTemplateDataLoader.GetTemplates(ws.Id);

            // Set current user as author
            foreach (RteCustomTemplateModelView template in templates)
            {
                template.AuthorId = ws.CurrentActiveUser.UserID;
                template.WorkspaceId = ws.Id;

                if (template.Updateable != UpdateType.Deleted)
                {
                    template.Updateable = UpdateType.Upsert;
                }
            }

            this.ProcessTemplateAssignments(templates, ws, templatesFromDb);
            this.ProcessTemplatesWithDeletedPrompts(templates, ws);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.rteTemplateDataLoader.Save(templates);
                scope.Complete();
            }
        }

        /// <summary>
        /// Search templates
        /// </summary>
        /// <param name="search">Search Text</param>
        /// <returns>Matching results</returns>
        public ICollection<RteCustomTemplateModelView> SearchTemplates(string search)
        {
            return this.rteTemplateDataLoader.Search(search);
        }

        /// <summary>
        /// Copies selected template into specified workspace
        /// </summary>
        /// <param name="templateId">Template Id</param>
        /// <param name="ws">Target WS</param>
        public void CopyTemplate(int templateId, FullWorkspace ws)
        {
            if (templateId <= 0) { throw new ArgumentOutOfRangeException("templateId", templateId, "Template Id must be a positive number."); }
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }

            RteCustomTemplateModelView template = this.rteTemplateDataLoader.GetById(templateId);
            if (template == null) { throw new GenValidationException("Template not found"); }

            // Reset template
            int newId = -1;
            template.Id = newId--;
            template.Assigned = new List<int>();
            template.AuthorId = ws.CurrentActiveUser.UserID;
            template.Updateable = UpdateType.Upsert;
            template.WorkspaceId = ws.Id;

            if (template.Questions.Any())
            {
                template.Questions.ToList().ForEach(question =>
                {
                    question.Id = newId--;
                    question.TemplateId = template.Id;
                    question.Updateable = UpdateType.Upsert;
                });
            }

            // save template with current workspace
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.rteTemplateDataLoader.Save(template);

                scope.Complete();
            }
        }

        /// <summary>
        /// This method looks for templates that are being assigned, or being unassigned
        /// </summary>
        /// <param name="templatesBeingSaved">Templates being saved</param>
        /// <param name="ws">Workspace</param>
        /// <param name="templatesFromDb">Templates as they were prior to modification</param>
        private void ProcessTemplateAssignments(ICollection<RteCustomTemplateModelView> templatesBeingSaved, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesFromDb)
        {
            if (ws.WorkspaceState != WorkspaceState.Initialization)
            {
                // Assumption based on the UI -> a template must exist before it can be assigned, or unassigned, therefore we only need to compare prior assignment to new assignment

                List<RteCustomTemplateModelView> templatesBeingAssigned = new List<RteCustomTemplateModelView>();
                List<RteCustomTemplateModelView> templatesBeingUnassigned = new List<RteCustomTemplateModelView>();

                templatesFromDb.ToList().ForEach(templateFromDb => {
                    RteCustomTemplateModelView templateBeingSaved = templatesBeingSaved.First(saveTemplate => saveTemplate.Id == templateFromDb.Id);

                    // assignment
                    //     [we do not care about in-use, as a new template could be assigned, or an existing one be assigned to an additional field]
                    //     - template being saved has an assignment that the existing template record (in the DB) doesn't have
                    if (templateBeingSaved.Assigned.Any(x => !templateFromDb.Assigned.Contains(x)))
                    {
                        templatesBeingAssigned.Add(templateBeingSaved);
                    }

                    // unassignment
                    //     - template is in-use
                    //      AND
                    //          - template record from the DB has an assignment that an updated template no longer has
                    //          OR
                    //          - (template is being deleted AND had at least 1 assignment before)
                    if (templateFromDb.InUse 
                            && (
                                templateFromDb.Assigned.Any(x => !templateBeingSaved.Assigned.Contains(x))
                                || (templateBeingSaved.Updateable == UpdateType.Deleted && templateFromDb.Assigned.Any())
                        ))
                    {
                        templatesBeingUnassigned.Add(templateBeingSaved);
                    }
                });

                // ToDo: RJ -> do what you need to do w/ templates that are being assigned or unassigned

                if (templatesBeingAssigned.Any() || templatesBeingUnassigned.Any())
                {
                    this.BackupWorkspace(ws, CommonConstants.AUTO_SYSTEM_BACKUP_TEMPLATE_ASSIGN_CHANGE);
                }
            }
        }

        /// <summary>
        /// This method looks for in-use templates that have a prompt / question being deleted
        /// </summary>
        /// <param name="templates">Templates being saved</param>
        /// <param name="ws">Workspace</param>
        private void ProcessTemplatesWithDeletedPrompts(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws)
        {
            if (ws.WorkspaceState != WorkspaceState.Initialization)
            {
                // In Use templates, with at least one prompt / question being deleted
                ICollection<RteCustomTemplateModelView> templatesToProcess = templates.Where(x => x.InUse && x.Questions.Any(z => z.Updateable == UpdateType.Deleted)).ToList();

                // ToDo: RJ -> do what you need to do w/ templates that are in use, and their prompt is being deleted

                if (templatesToProcess.Any())
                {
                    this.BackupWorkspace(ws, CommonConstants.AUTO_SYSTEM_BACKUP_TEMPLATE_QUESTION_DELETE);
                }
            }
        }

        /// <summary>
        /// Creates a WS backup
        /// </summary>
        /// <param name="ws">WS which to backup</param>
        /// <param name="versionDescription">Description text</param>
        private void BackupWorkspace(WorkspaceDTO ws, string versionDescription)
        {
            WorkspaceVersionMetaDataDTO backup = new WorkspaceVersionMetaDataDTO()
            {
                VersionID = -1,
                CreatedByID = CommonConstants.SYSTEM_USER_ID,
                Updateable = UpdateType.Upsert,
                VersionName = $"{versionDescription} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}",
                VersionState = ws.WorkspaceState,
                WorkspaceID = ws.Id
            };

            versionLoader.Upsert(backup, backup.WorkspaceID);
        }
    }
}
