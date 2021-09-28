// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView;
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
        private readonly IWorkspaceVersionMetaDataDTODataLoader versionLoader;

        /// <summary>
        /// rteTemplate Data Loader
        /// </summary>
        private readonly IRteTemplateDataLoader rteTemplateDataLoader;

        /// <summary>
        /// BOE DTO Data Loader
        /// </summary>
        private readonly IBoeDTODataLoader boeDtoDataLoader;

        /// <summary>
        /// BOE Mediator
        /// </summary>
        private readonly IBoeMediator boeMediator;

        /// <summary>
        /// Task Element DTO Data Loader
        /// </summary>
        private readonly IBoeTaskElementDTODataLoader taskElementDtoDataLoader;

        /// <summary>
        /// Task Element Mediator
        /// </summary>
        private readonly IBoeTaskElementMediator taskElementMediator;

        /// <summary>
        /// Boe Emailer
        /// </summary>
        private readonly IBoeEmailer emailer;

        /// <summary>
        /// The BOE state machine.
        /// </summary>
        private readonly IBOEStateMachine boeStateMachine;

        /// <summary>
        /// MOQ Type Loader
        /// </summary>
        private readonly IMoqTypeDataLoader moqTypeLoader;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rteTemplateDataLoader">Template Loader</param>
        /// <param name="versionLoader">Version Loader</param>
        /// <param name="boeDtoDataLoader">BOE DTO Data Loader</param>
        /// <param name="boeMediator">BOE Mediator</param>
        /// <param name="taskElementDtoDataLoader">Task Element DTO Data Loader</param>
        /// <param name="taskElementMediator">Task Element Mediator</param>
        /// <param name="emailer">BOE Emailer</param>
        public RTETemplatesControllerLogic(IRteTemplateDataLoader rteTemplateDataLoader, IWorkspaceVersionMetaDataDTODataLoader versionLoader, 
            IBoeDTODataLoader boeDtoDataLoader, IBoeMediator boeMediator, IBoeTaskElementDTODataLoader taskElementDtoDataLoader, IBoeTaskElementMediator taskElementMediator,
            IBoeEmailer emailer, IBOEStateMachine boeStateMachine, IMoqTypeDataLoader moqTypeLoader)
        {
            this.rteTemplateDataLoader = rteTemplateDataLoader;
            this.versionLoader = versionLoader;
            this.boeDtoDataLoader = boeDtoDataLoader;
            this.boeMediator = boeMediator;
            this.taskElementDtoDataLoader = taskElementDtoDataLoader;
            this.taskElementMediator = taskElementMediator;
            this.emailer = emailer;
            this.boeStateMachine = boeStateMachine;
            this.moqTypeLoader = moqTypeLoader;
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
        /// <param name="usingTemplateBOE">Is the WS using Template BOEs</param>
        /// <returns>All of the sources from lookup table.</returns>
        public ICollection<RteCustomTemplateSourceModelView> GetSources(bool usingTemplateBOE)
        {
            return this.rteTemplateDataLoader.GetSources(usingTemplateBOE);
        }

        /// <summary>
        /// Validates RTE templates
        /// </summary>
        /// <param name="templates">Templates to validate</param>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <param name="moveToPrompt">ID of the Prompt to move the deleted prompt data to</param>
        /// <exception cref="GenValidationException">Throws GenValidationException with validation errors, if any</exception>
        public void ValidateTemplates(ICollection<RteCustomTemplateModelView> templates, int wsId, bool moveDeletedPromptData, int? moveToPrompt)
        {
            if (templates == null || templates.None()) { throw new ArgumentNullException(nameof(templates)); }

            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(t.Description)))
            {
                validationErrors.Add(new ValidationMessage("Template Name is required."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && !t.Questions.Any(q => q.Updateable != UpdateType.Deleted)))
            {
                validationErrors.Add(new ValidationMessage("At least one prompt is required for a Template. If you intend to delete the template, close this dialog and do so on the Manage Custom RTE Templates page."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && t.Questions.Any(q => q.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(q.Text))))
            {
                validationErrors.Add(new ValidationMessage("Prompt text is required."));
            }

            ICollection<RteCustomTemplateModelView> templatesFromDb = this.rteTemplateDataLoader.GetTemplates(wsId);

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && templatesFromDb.Any(tdb => tdb.Description == t.Description && tdb.Id != t.Id) == true))
            {
                validationErrors.Add(new ValidationMessage("Template name must be unique."));
            }

            if (moveDeletedPromptData && moveToPrompt == null)
            {
                validationErrors.Add(new ValidationMessage("A prompt must be selected if moving the deleted data to another prompt."));
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
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <param name="moveToPrompt">ID of the Prompt to move the deleted prompt data to</param>
        public void SaveTemplates(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws, bool moveDeletedPromptData, int? moveToPrompt)
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
            
            List<RteCustomTemplateModelView> templatesBeingAssigned = this.GetTemplatesBeingAssigned(templates, ws, templatesFromDb);
            List<RteCustomTemplateModelView> templatesBeingUnassigned = this.GetTemplatesBeingUnassigned(templates, ws, templatesFromDb);
            List<RteCustomTemplateModelView> templatesWithDeletedPrompts = this.GetTemplatesWithDeletedPrompts(templates, ws);

            if(templatesBeingAssigned.Any() || templatesBeingUnassigned.Any() || templatesWithDeletedPrompts.Any())
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this.BackupWorkspace(ws, templatesWithDeletedPrompts.Any() ? CommonConstants.AUTO_SYSTEM_BACKUP_TEMPLATE_PROMPT_DELETE : CommonConstants.AUTO_SYSTEM_BACKUP_TEMPLATE_ASSIGN_CHANGE);
                    scope.Complete();
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                ICollection<BoeDTO> boes = new Collection<BoeDTO>();
                ICollection<BoeTaskElementDTO> tasks = new Collection<BoeTaskElementDTO>();

                // Get the boes and tasks if unassigning templates or deleting prompts so that their data can be updated
                if(templatesBeingUnassigned.Any() || templatesWithDeletedPrompts.Any())
                {
                    boes = boeDtoDataLoader.GetByWorkspaceId(ws.Id, true);
                    tasks = taskElementDtoDataLoader.GetByWorkspaceId(ws.Id, true, ws.DecimalPrecision, ws.CostDecimalPrecision);
                }

                // Process unassigned sources before saving templates so we can get template answer data before it's cleared
                if (templatesBeingUnassigned.Any())
                {
                    this.ProcessUnassignedSources(templatesBeingUnassigned, templatesFromDb, ws, boes, tasks);
                }

                ICollection<RTECustomTemplateQuestionAnswerModelView> deletedQandAs = new Collection<RTECustomTemplateQuestionAnswerModelView>();
                if (templatesWithDeletedPrompts.Any() && moveDeletedPromptData)
                {
                    deletedQandAs = this.GetDeletedQandAs(templatesWithDeletedPrompts, ws, boes, tasks);
                }

                // Get BOE and Task data before saving templates if assigning templates so the necessary source data can be used to populate the first template answer
                // Additionally, this will refresh the data if it was updated while unassigning a template
                if (templatesBeingAssigned.Any())
                {
                    boes = boeDtoDataLoader.GetByWorkspaceId(ws.Id, true);
                    tasks = taskElementDtoDataLoader.GetByWorkspaceId(ws.Id, true, ws.DecimalPrecision, ws.CostDecimalPrecision);
                }

                // Save Templates
                this.rteTemplateDataLoader.Save(templates);

                // Save Questions (Prompts)
                Dictionary<int, int> templateQuestionIdMapping = new Dictionary<int, int>();
                foreach(RteCustomTemplateModelView template in templates.Where(x => x.Updateable != UpdateType.Deleted))
                {
                    Dictionary<int, int> currentQuestionIdMapping = this.rteTemplateDataLoader.SaveQuestions(template.Questions, template.Id);
                    foreach(KeyValuePair<int, int> mapping in currentQuestionIdMapping)
                    {
                        templateQuestionIdMapping.Add(mapping.Key, mapping.Value);
                    }
                }

                if (templatesWithDeletedPrompts.Any() && deletedQandAs.Any() && moveDeletedPromptData)
                {
                    this.ProcessTemplatesWithDeletedPrompts(templatesWithDeletedPrompts, deletedQandAs, ws, boes, tasks, templateQuestionIdMapping[moveToPrompt.Value]);
                }

                // Process assigned sources after saving templates because the Answers must exist before they can be saved to
                if (templatesBeingAssigned.Any())
                {
                    this.ProcessAssignedSources(templatesBeingAssigned, templatesFromDb, ws, boes, tasks);
                }

                // change state if any in-use templates was changed (name, assignment), or any of its prompts changed (add, rename, delete, etc)
                if (templates.Any(x => x.InUse && (x.Updateable != UpdateType.None || x.Questions.Any(z => z.Updateable != UpdateType.None))))
                {
                    ws.RefreshBoes();

                    foreach (FullBoe boe in ws.Boes)
                    {
                        BOEState originalState = boe.State;
                        boe.Updateable = UpdateType.Upsert;
                        boe.State = BOEState.Draft;
                        boe.UpdatedByUserId = ws.CurrentActiveUser.UserID;

                        this.boeDtoDataLoader.Save(boe);
                        this.boeStateMachine.PerformStateTransitionAction(boe, ws, originalState, BOEState.Draft);
                    }
                }

                scope.Complete();
            }

            // Send any emails now that Save is successful
            if (templatesBeingUnassigned.Any())
            {
                this.emailer.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned);
            }

            if (templatesBeingAssigned.Any())
            {
                this.emailer.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned);
            }

            if (templatesWithDeletedPrompts.Any())
            {
                this.emailer.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted);
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
        /// <param name="newTemplateName">New Template Name</param>
        public void CopyTemplate(int templateId, FullWorkspace ws, string newTemplateName)
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
            template.Description = newTemplateName;

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

                // Save Questions (Prompts)
                this.rteTemplateDataLoader.SaveQuestions(template.Questions, template.Id);

                scope.Complete();
            }
        }
        
        /// <summary>
        /// Gets the templates that are being assigned
        /// </summary>
        /// <param name="templatesBeingSaved">Templates being saved</param>
        /// <param name="ws">Workspace</param>
        /// <param name="templatesFromDb">Templates as they were prior to modification</param>
        /// <returns>List of templates being assigned so they can be used to update sources after templates are saved</returns>
        private List<RteCustomTemplateModelView> GetTemplatesBeingAssigned(ICollection<RteCustomTemplateModelView> templatesBeingSaved, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesFromDb)
        {
            List<RteCustomTemplateModelView> templatesBeingAssigned = new List<RteCustomTemplateModelView>();

            if (ws.WorkspaceState != WorkspaceState.Initialization)
            {
                // Assumption based on the UI -> a template must exist before it can be assigned, or unassigned, therefore we only need to compare prior assignment to new assignment
                templatesFromDb.ToList().ForEach(templateFromDb => {
                    RteCustomTemplateModelView templateBeingSaved = templatesBeingSaved.FirstOrDefault(saveTemplate => saveTemplate.Id == templateFromDb.Id);

                    if (templateBeingSaved != null)
                    {
                        // assignment
                        //     [we do not care about in-use, as a new template could be assigned, or an existing one be assigned to an additional field]
                        //     - template being saved has an assignment that the existing template record (in the DB) doesn't have
                        if (templateBeingSaved.Assigned.Any(x => !templateFromDb.Assigned.Contains(x)))
                        {
                            templatesBeingAssigned.Add(templateBeingSaved);
                        }
                    }
                });
            }

            return templatesBeingAssigned;
        }

        /// <summary>
        /// Gets the templates that are being unassigned
        /// </summary>
        /// <param name="templatesBeingSaved">Templates being saved</param>
        /// <param name="ws">Workspace</param>
        /// <param name="templatesFromDb">Templates as they were prior to modification</param>
        /// <returns>List of templates being unassinged</returns>
        private List<RteCustomTemplateModelView> GetTemplatesBeingUnassigned(ICollection<RteCustomTemplateModelView> templatesBeingSaved, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesFromDb)
        {
            List<RteCustomTemplateModelView> templatesBeingUnassigned = new List<RteCustomTemplateModelView>();

            if (ws.WorkspaceState != WorkspaceState.Initialization)
            {
                // Assumption based on the UI -> a template must exist before it can be assigned, or unassigned, therefore we only need to compare prior assignment to new assignment
                templatesFromDb.ToList().ForEach(templateFromDb => {
                    RteCustomTemplateModelView templateBeingSaved = templatesBeingSaved.FirstOrDefault(saveTemplate => saveTemplate.Id == templateFromDb.Id);

                    if (templateBeingSaved != null)
                    {
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
                    }
                });
            }

            return templatesBeingUnassigned;
        }

        /// <summary>
        /// Move data in RTE Templates that are being unassigned into the appropriate source fields
        /// </summary>
        /// <param name="templatesBeingUnassigned">Templates being unassigned</param>
        /// <param name="templatesFromDb">Templates as they were prior to modification</param>
        /// <param name="ws">The workspace</param>
        /// <param name="boes">The BOEs</param>
        /// <param name="tasks">The Tasks</param>
        private void ProcessUnassignedSources(List<RteCustomTemplateModelView> templatesBeingUnassigned, ICollection<RteCustomTemplateModelView> templatesFromDb, FullWorkspace ws,
            ICollection<BoeDTO> boes, ICollection<BoeTaskElementDTO> tasks)
        {
            bool saveBoes = false;
            bool saveTasks = false;
            ICollection<MoqTypeSelection> moqTypesToSave = new List<MoqTypeSelection>();

            foreach (RteCustomTemplateModelView template in templatesBeingUnassigned)
            {
                RteCustomTemplateModelView originalTemplate = templatesFromDb.First(x => x.Id == template.Id);
                ICollection<int> removedAssignments;

                if (template.Updateable == UpdateType.Deleted)
                {
                    // For deleted templates, need to handle all sources
                    removedAssignments = originalTemplate.Assigned.ToCollection();
                }
                else
                {
                    // For templates only being unassigned, just need sources being unassigned
                    removedAssignments = originalTemplate.Assigned.Except(template.Assigned).ToCollection();
                }

                foreach (int removedAssignment in removedAssignments)
                {
                    switch (removedAssignment)
                    {
                        case (int)RteTemplateSource.BoeDescription:
                            foreach (BoeDTO boe in boes)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> descQuestionsAndAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id).Where(x => x.SourceId == (int)RteTemplateSource.BoeDescription).ToCollection();

                                boe.Description = this.ConvertQandAsToText(descQuestionsAndAnswers, (int)RteTemplateSource.BoeDescription);
                                boe.Updateable = UpdateType.Upsert;
                            }

                            saveBoes = true;
                            break;
                        case (int)RteTemplateSource.BoeSources:
                            foreach (BoeDTO boe in boes)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> sourcesQuestionsAndAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id).Where(x => x.SourceId == (int)RteTemplateSource.BoeSources).ToCollection();

                                boe.DataSource = this.ConvertQandAsToText(sourcesQuestionsAndAnswers, (int)RteTemplateSource.BoeSources);
                                boe.Updateable = UpdateType.Upsert;
                            }

                            saveBoes = true;
                            break;
                        case (int)RteTemplateSource.TaskDescription:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> taskDescQuestionsAndAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id).Where(x => x.SourceId == (int)RteTemplateSource.TaskDescription).ToCollection();

                                task.Description = this.ConvertQandAsToText(taskDescQuestionsAndAnswers, (int)RteTemplateSource.TaskDescription);
                                task.Updateable = UpdateType.Upsert;
                            }

                            saveTasks = true;
                            break;
                        case (int)RteTemplateSource.TaskMOQ:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> taskDescQuestionsAndAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id).Where(x => x.SourceId == (int)RteTemplateSource.TaskMOQ).ToCollection();

                                if (ws.UsingTemplateBOE)
                                {
                                    MoqTypeSelection tasksFirstMoqType = ws.MoqTypeSelections.OrderBy(x => x.Order).FirstOrDefault(x => x.TaskId == task.Id) ??  throw new GenValidationException("Operation cannot be completed as requested. " +
                                        "This is likely due to incomplete or invalid data. Please verify that all BOEs and Tasks are complete and valid. If the issue persists, please contact the administrator.");

                                    tasksFirstMoqType.Updateable = UpdateType.Upsert;

                                    if (tasksFirstMoqType.SelectedMOQType == MOQType.SME)
                                    {
                                        tasksFirstMoqType.SmeReason += this.ConvertQandAsToText(taskDescQuestionsAndAnswers, (int)RteTemplateSource.TaskMOQ);
                                    }
                                    else
                                    {
                                        tasksFirstMoqType.Rationale += this.ConvertQandAsToText(taskDescQuestionsAndAnswers, (int)RteTemplateSource.TaskMOQ);
                                    }

                                    moqTypesToSave.Add(tasksFirstMoqType);
                                }
                                else
                                {
                                    task.MOQText = this.ConvertQandAsToText(taskDescQuestionsAndAnswers, (int)RteTemplateSource.TaskMOQ);
                                    task.Updateable = UpdateType.Upsert;
                                }
                            }

                            saveTasks = saveTasks || !ws.UsingTemplateBOE;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (saveBoes)
            {
                foreach (BoeDTO boe in boes)
                {
                    this.boeMediator.SaveEditBoeHeader(boe);
                }
            }

            if (saveTasks)
            {
                this.taskElementMediator.MediatedBulkSaveTaskElements(tasks, ws);
            }

            if (moqTypesToSave.Any())
            {
                this.moqTypeLoader.Save(moqTypesToSave);
            }
        }

        /// <summary>
        /// Move data in the source fields that are being assigned into the appropriate RTE Templates
        /// </summary>
        /// <param name="templatesBeingAssigned">Templates being assigned</param>
        /// <param name="templatesFromDb">Templates as they were prior to modification</param>
        /// <param name="ws">The workspace</param>
        /// <param name="boes">BOEs from before templates are saved</param>
        /// <param name="tasks">Tasks from before templates are saved</param>
        /// <returns>Answers that need to be saved</returns>
        private void ProcessAssignedSources(List<RteCustomTemplateModelView> templatesBeingAssigned, ICollection<RteCustomTemplateModelView> templatesFromDb, 
        FullWorkspace ws, ICollection<BoeDTO> boes, ICollection<BoeTaskElementDTO> tasks)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> answersToSave = new Collection<RTECustomTemplateQuestionAnswerModelView>();

            foreach (RteCustomTemplateModelView template in templatesBeingAssigned)
            {
                RteCustomTemplateModelView originalTemplate = templatesFromDb.First(x => x.Id == template.Id);
                ICollection<int> addedAssignments = template.Assigned.Except(originalTemplate.Assigned).ToCollection();

                foreach (int addedAssignment in addedAssignments)
                {
                    switch (addedAssignment)
                    {
                        case (int)RteTemplateSource.BoeDescription:
                            foreach (BoeDTO boe in boes)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id);
                                RTECustomTemplateQuestionAnswerModelView firstPrompt = questionsAndAnswers.Where(x => x.SourceId == (int)RteTemplateSource.BoeDescription).OrderBy(x => x.SortOrder).First();
                                firstPrompt.AnswerText = boe.Description;
                                firstPrompt.Updateable = UpdateType.Upsert;
                                answersToSave.Add(firstPrompt);
                            }
                            break;
                        case (int)RteTemplateSource.BoeSources:
                            foreach (BoeDTO boe in boes)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id);
                                RTECustomTemplateQuestionAnswerModelView firstPrompt = questionsAndAnswers.Where(x => x.SourceId == (int)RteTemplateSource.BoeSources).OrderBy(x => x.SortOrder).First();
                                firstPrompt.AnswerText = boe.DataSource;
                                firstPrompt.Updateable = UpdateType.Upsert;
                                answersToSave.Add(firstPrompt);
                            }
                            break;
                        case (int)RteTemplateSource.TaskDescription:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id);
                                RTECustomTemplateQuestionAnswerModelView firstPrompt = questionsAndAnswers.Where(x => x.SourceId == (int)RteTemplateSource.TaskDescription).OrderBy(x => x.SortOrder).First();
                                firstPrompt.AnswerText = task.Description;
                                firstPrompt.Updateable = UpdateType.Upsert;
                                answersToSave.Add(firstPrompt);
                            }
                            break;
                        case (int)RteTemplateSource.TaskMOQ:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id);
                                RTECustomTemplateQuestionAnswerModelView firstPrompt = questionsAndAnswers.Where(x => x.SourceId == (int)RteTemplateSource.TaskMOQ).OrderBy(x => x.SortOrder).First();
                                firstPrompt.AnswerText = task.MOQText;
                                firstPrompt.Updateable = UpdateType.Upsert;
                                answersToSave.Add(firstPrompt);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            this.rteTemplateDataLoader.SaveAnswers(answersToSave);
        }
        
        /// <summary>
        /// Converts the Prompts and Answers for the RTE Templates into text to put into the source field
        /// </summary>
        /// <param name="questionsAndAnswers">RTE Template Prompts/Answers</param>
        /// <param name="source">Source for the template</param>
        /// <returns>Propmts and Answers as a single string</returns>
        private string ConvertQandAsToText(ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers, int source)
        {
            string newText = string.Empty;

            foreach (RTECustomTemplateQuestionAnswerModelView qAndA in questionsAndAnswers.Where(x => x.SourceId == source && !string.IsNullOrEmpty(x.AnswerText)))
            {
                newText += "<p><strong>" + qAndA.QuestionText + "</strong></p>"
                    + "<br/>" + qAndA.AnswerText + "<br/>";
            }

            return newText;
        }

        /// <summary>
        /// Get Templates that have deleted prompts that will need to be processed
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="ws"></param>
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <returns></returns>
        private List<RteCustomTemplateModelView> GetTemplatesWithDeletedPrompts(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws)
        {
            List<RteCustomTemplateModelView> templatesToProcess = new List<RteCustomTemplateModelView>();

            if (ws.WorkspaceState != WorkspaceState.Initialization)
            {
                // In Use templates, with at least one prompt / question being deleted
                templatesToProcess = templates.Where(x => x.InUse && x.Questions.Any(z => z.Updateable == UpdateType.Deleted)).ToList();
            }

            return templatesToProcess;
        }

        /// <summary>
        /// Get the QuestionAnswer Model Views for the deleted Prompts
        /// </summary>
        /// <param name="templates">Templates with deleted prompts</param>
        /// <param name="ws">The Workspace</param>
        /// <param name="boes">The BOEs</param>
        /// <param name="tasks">The Tasks</param>
        /// <returns>QuestionAnswer Model Views for the deleted Prompts</returns>
        private ICollection<RTECustomTemplateQuestionAnswerModelView> GetDeletedQandAs(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws, 
            ICollection<BoeDTO> boes, ICollection<BoeTaskElementDTO> tasks)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> qAndAsBeingDeleted = new Collection<RTECustomTemplateQuestionAnswerModelView>();

            foreach (RteCustomTemplateModelView template in templates)
            {
                ICollection<int> deletedPrompts = template.Questions.Where(x => x.Updateable == UpdateType.Deleted).Select(x => x.Id).ToCollection();

                foreach (int assignment in template.Assigned)
                {
                    switch (assignment)
                    {
                        case (int)RteTemplateSource.BoeDescription:
                        case (int)RteTemplateSource.BoeSources:
                            foreach (BoeDTO boe in boes)
                            {
                                qAndAsBeingDeleted.AddRange(this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id).Where(x => x.SourceId == assignment && deletedPrompts.Contains(x.QuestionId)).ToCollection());
                            }
                            break;
                        case (int)RteTemplateSource.TaskDescription:
                        case (int)RteTemplateSource.TaskMOQ:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                qAndAsBeingDeleted.AddRange(this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id).Where(x => x.SourceId == assignment && deletedPrompts.Contains(x.QuestionId)).ToCollection());                                
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return qAndAsBeingDeleted;
        }

        /// <summary>
        /// Process the Templates with Deleted Prompts, moving the data from the deleted prompts to the selected Prompt
        /// </summary>
        /// <param name="templates">Templates with deleted Prompts</param>
        /// <param name="qAndAsBeingDeleted">QuestionAnswer Modelviews for the Prompts being deleted</param>
        /// <param name="ws">The Workspace</param>
        /// <param name="boes">The BOEs</param>
        /// <param name="tasks">The Tasks</param>
        /// <param name="moveToPromptId">ID of the prompt to move the deleted Prompt data to</param>
        private void ProcessTemplatesWithDeletedPrompts(ICollection<RteCustomTemplateModelView> templates, ICollection<RTECustomTemplateQuestionAnswerModelView> qAndAsBeingDeleted,
            FullWorkspace ws, ICollection<BoeDTO> boes, ICollection<BoeTaskElementDTO> tasks, int moveToPromptId)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> answersToSave = new Collection<RTECustomTemplateQuestionAnswerModelView>();

            foreach (RteCustomTemplateModelView template in templates)
            {
                foreach (int assignment in template.Assigned)
                {
                    switch (assignment)
                    {
                        case (int)RteTemplateSource.BoeDescription:
                        case (int)RteTemplateSource.BoeSources:
                            foreach (BoeDTO boe in boes)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id).Where(x => x.SourceId == assignment).ToCollection();
                                RTECustomTemplateQuestionAnswerModelView moveToQandA = questionsAndAnswers.First(x => x.QuestionId == moveToPromptId);

                                foreach (RTECustomTemplateQuestionAnswerModelView qAndA in qAndAsBeingDeleted.Where(x => x.BoeId == boe.Id && x.SourceId == assignment))
                                {
                                    moveToQandA.AnswerText += "<br />" + qAndA.AnswerText;
                                }

                                answersToSave.Add(moveToQandA);
                            }
                            break;
                        case (int)RteTemplateSource.TaskDescription:
                        case (int)RteTemplateSource.TaskMOQ:
                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                ICollection<RTECustomTemplateQuestionAnswerModelView> questionsAndAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, task.BoeID, task.Id).Where(x => x.SourceId == assignment).ToCollection();
                                RTECustomTemplateQuestionAnswerModelView moveToQandA = questionsAndAnswers.First(x => x.QuestionId == moveToPromptId);

                                foreach (RTECustomTemplateQuestionAnswerModelView qAndA in qAndAsBeingDeleted.Where(x => x.BoeId == task.BoeID && x.TaskId == task.Id && x.SourceId == assignment))
                                {
                                    moveToQandA.AnswerText += "<br />" + qAndA.AnswerText;
                                }

                                answersToSave.Add(moveToQandA);
                            }
                            break;
                        default:
                            break;
                    }
                }

                this.rteTemplateDataLoader.SaveAnswers(answersToSave);
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
                VersionName = $"{versionDescription} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}",
                VersionState = ws.WorkspaceState,
                WorkspaceID = ws.Id
            };

            this.versionLoader.Upsert(backup, backup.WorkspaceID);
        }
    }
}
