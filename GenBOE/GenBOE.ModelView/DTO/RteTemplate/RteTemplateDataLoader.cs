// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Models;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    /// <summary>
    /// This loader is exclusively used for retrievals only
    /// </summary>
    public class RteTemplateDataLoader : DataLoader<RteCustomTemplateModelView>, IRteTemplateDataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RteTemplateDataLoader()
        {
            this.Log = new Logger(typeof(RteTemplateDataLoader));
        }

        /// <summary>
        /// Gets all of the sources from lookup table.
        /// </summary>
        /// <returns>All of the sources from lookup table.</returns>
        [DbQuery]
        public ICollection<RteCustomTemplateSourceModelView> GetSources()
        {
            ICollection<RteCustomTemplateSourceModelView> toReturn = new Collection<RteCustomTemplateSourceModelView>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from t in gbe.RteTemplateSources
                                select new RteCustomTemplateSourceModelView
                                {
                                    SourceId = t.RteTemplateSourceId,
                                    Description = t.Description,
                                    TaskOnly = t.TaskOnly
                                }).ToCollection<RteCustomTemplateSourceModelView>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Searches Templates.
        /// </summary>
        /// <param name="searchTerm">Search term.</param>
        /// <returns>List of matching RTE Templates.</returns>
        public ICollection<RteCustomTemplateModelView> Search(string searchTerm)
        {
            ICollection<RteCustomTemplateModelView> toReturn = new Collection<RteCustomTemplateModelView>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                ICollection<RteCustomTemplateQuestionModelView> questions;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from t in gbe.RteTemplates.Where(rt => rt.Workspace.AllowSearch && (rt.Description.Contains(searchTerm) || rt.ETIuser.DisplayName.Contains(searchTerm) || rt.Workspace.WorkspaceName.Contains(searchTerm)))
                                select new RteCustomTemplateModelView
                                {
                                    Id = t.TemplateID,
                                    Author = t.ETIuser.DisplayName,
                                    AuthorId = t.AuthorID,
                                    CreatedOn = t.CreatedOn,
                                    Description = t.Description,
                                    InUse = t.RteTemplateQuestions.Any(q => q.RteTemplateAnswers.Any()),
                                    UpdateDate = t.UpdateDT,
                                    WorkspaceId = t.WorkspaceID,
                                    WorkspaceName = t.Workspace.WorkspaceName,
                                    AssignedList = t.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                }).ToCollection<RteCustomTemplateModelView>();

                    ICollection<int> ids = toReturn.Select(t => t.Id).Distinct().ToList();

                    questions = (from q in gbe.RteTemplateQuestions.Where(rtq => ids.Contains(rtq.TemplateID))
                                 select new RteCustomTemplateQuestionModelView
                                 {
                                     Id = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     Text = q.Text,
                                     UpdateDate = q.UpdateDT
                                 }).ToCollection<RteCustomTemplateQuestionModelView>();
                }

                MatchQuestionsToTemplates(toReturn, questions);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all templates for a workspace
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <returns>Collection of RTE Templates</returns>
        [DbQuery(2)]
        public ICollection<RteCustomTemplateModelView> GetTemplates(int workspaceId)
        {
            ICollection<RteCustomTemplateModelView> toReturn = new Collection<RteCustomTemplateModelView>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                ICollection<RteCustomTemplateQuestionModelView> questions;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from t in gbe.RteTemplates.Where(rt => rt.WorkspaceID == workspaceId)
                                select new RteCustomTemplateModelView
                                {
                                    Id = t.TemplateID,
                                    Author = t.ETIuser.DisplayName,
                                    AuthorId = t.AuthorID,
                                    CreatedOn = t.CreatedOn,
                                    Description = t.Description,
                                    InUse = t.RteTemplateQuestions.Any(q => q.RteTemplateAnswers.Any()),
                                    UpdateDate = t.UpdateDT,
                                    WorkspaceId = t.WorkspaceID,
                                    WorkspaceName = t.Workspace.WorkspaceName,
                                    AssignedList = t.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                }).ToCollection<RteCustomTemplateModelView>();

                    questions = (from q in gbe.RteTemplateQuestions.Where(rtq => rtq.RteTemplate.WorkspaceID == workspaceId)
                                 select new RteCustomTemplateQuestionModelView
                                 {
                                     Id = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     Text = q.Text,
                                     UpdateDate = q.UpdateDT
                                 }).ToCollection<RteCustomTemplateQuestionModelView>();
                }

                MatchQuestionsToTemplates(toReturn, questions);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a collection of RteCustomTemplateModelViews matching the ids.
        /// </summary>
        /// <param name="ids">Rte Templates to find</param>
        /// <returns>Collection of matching Templates.</returns>
        [DbQuery(2)]
        public override ICollection<RteCustomTemplateModelView> GetByIds(ICollection<int> ids)
        {
            ICollection<RteCustomTemplateModelView> toReturn = new Collection<RteCustomTemplateModelView>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                ICollection<RteCustomTemplateQuestionModelView> questions;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from t in gbe.RteTemplates.Where(rt => ids.Contains(rt.TemplateID))
                                select new RteCustomTemplateModelView
                                {
                                    Id = t.TemplateID,
                                    Author = t.ETIuser.DisplayName,
                                    AuthorId = t.AuthorID,
                                    CreatedOn = t.CreatedOn,
                                    Description = t.Description,
                                    InUse = t.RteTemplateQuestions.Any(q => q.RteTemplateAnswers.Any()),
                                    UpdateDate = t.UpdateDT,
                                    WorkspaceId = t.WorkspaceID,
                                    WorkspaceName = t.Workspace.WorkspaceName,
                                    AssignedList = t.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                }).ToCollection<RteCustomTemplateModelView>();

                    questions = (from q in gbe.RteTemplateQuestions.Where(rtq => ids.Contains(rtq.TemplateID))
                                 select new RteCustomTemplateQuestionModelView
                                 {
                                     Id = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     Text = q.Text,
                                     UpdateDate = q.UpdateDT
                                 }).ToCollection<RteCustomTemplateQuestionModelView>();
                }
                
                MatchQuestionsToTemplates(toReturn, questions);
            }

            return toReturn;
        }

        /// <summary>
        /// Get the Questions and Answers by Boe Id.
        /// </summary>
        /// <param name="boeId">The id of the BOE to retrieve information for.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> GetByBoeId(int workspaceId, int boeId)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> questions;
            ICollection<RteTemplateAnswer> answers;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    questions = (from q in gbe.RteTemplateQuestions.Where(rtq => rtq.RteTemplate.WorkspaceID == workspaceId && (rtq.RteTemplate.RteTemplateSources.Any(s => !s.TaskOnly)))
                                 select new RTECustomTemplateQuestionAnswerModelView
                                 {
                                     QuestionId = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     QuestionText = q.Text,
                                     SourceIdEnum = q.RteTemplate.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                 }).ToCollection<RTECustomTemplateQuestionAnswerModelView>();

                    answers = gbe.RteTemplateAnswers.Where(rta => rta.BOEID == boeId).ToList();
                }
            }

            DoPostProcessing(questions);

            // join answers onto questions
            questions = MatchAnswersToQuestions(boeId, null, questions, answers);

            return questions;
        }

        /// <summary>
        /// Get the Questions and Answers by Boe Id.
        /// </summary>
        /// <param name="boeId">The id of the BOE to retrieve information for.</param>
        /// <param name="taskId">The id of the task to retrieve information for.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> GetByBoeIdAndTaskId(int workspaceId, int boeId, int? taskId)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> questions;
            ICollection<RteTemplateAnswer> answers = new List<RteTemplateAnswer>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    questions = (from q in gbe.RteTemplateQuestions.Where(rtq => rtq.RteTemplate.WorkspaceID == workspaceId && (rtq.RteTemplate.RteTemplateSources.Any(s => s.TaskOnly)))
                                 select new RTECustomTemplateQuestionAnswerModelView
                                 {
                                     QuestionId = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     QuestionText = q.Text,
                                     SourceIdEnum = q.RteTemplate.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                 }).ToCollection<RTECustomTemplateQuestionAnswerModelView>();

                    if (taskId.HasValue)
                    {
                        answers = gbe.RteTemplateAnswers.Where(rta => rta.BOEID == boeId && rta.TaskID == taskId.Value).ToList();
                    }
                }
            }

            DoPostProcessing(questions);

            // join answers onto questions
            questions = MatchAnswersToQuestions(boeId, taskId, questions, answers);

            return questions;
        }

        /// <summary>
        /// Gets the Questions and Answers by Workspace Id.
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <param name="boes">The full boes with tasks to retrieve questions and answers against.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> GetByWorkspaceId(int workspaceId, ICollection<FullBoe> boes)
        {
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            if (boes.None())
            {
                return new List<RTECustomTemplateQuestionAnswerModelView>();
            }

            ICollection<RTECustomTemplateQuestionAnswerModelView> boeQuestions;
            ICollection<RTECustomTemplateQuestionAnswerModelView> taskQuestions;
            ICollection<RteTemplateAnswer> answers;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    boeQuestions = (from q in gbe.RteTemplateQuestions.Where(rtq => rtq.RteTemplate.WorkspaceID == workspaceId && rtq.RteTemplate.RteTemplateSources.Any(s => !s.TaskOnly))
                                 select new RTECustomTemplateQuestionAnswerModelView
                                 {
                                     QuestionId = q.QuestionID,
                                     Required = q.Required,
                                     SortOrder = q.SortOrder,
                                     TemplateId = q.TemplateID,
                                     QuestionText = q.Text,
                                     SourceIdEnum = q.RteTemplate.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                 }).ToCollection<RTECustomTemplateQuestionAnswerModelView>();

                    taskQuestions = (from q in gbe.RteTemplateQuestions.Where(rtq => rtq.RteTemplate.WorkspaceID == workspaceId && rtq.RteTemplate.RteTemplateSources.Any(s => s.TaskOnly))
                                    select new RTECustomTemplateQuestionAnswerModelView
                                    {
                                        QuestionId = q.QuestionID,
                                        Required = q.Required,
                                        SortOrder = q.SortOrder,
                                        TemplateId = q.TemplateID,
                                        QuestionText = q.Text,
                                        SourceIdEnum = q.RteTemplate.RteTemplateSources.Select(s => s.RteTemplateSourceId)
                                    }).ToCollection<RTECustomTemplateQuestionAnswerModelView>();

                    answers = gbe.RteTemplateAnswers.Where(rta => rta.BOE.WorkspaceID == workspaceId).ToList();
                }
            }

            // enumerate proxy once
            DoPostProcessing(boeQuestions);
            DoPostProcessing(taskQuestions);

            List<RTECustomTemplateQuestionAnswerModelView> questions = new List<RTECustomTemplateQuestionAnswerModelView>();

            // join answers onto questions for each Boe
            foreach (FullBoe boe in boes)
            {
                questions.AddRange(MatchAnswersToQuestions(boe.Id, null, boeQuestions, answers));

                foreach(BoeTaskElementDTO task in boe.TaskElements)
                {
                    questions.AddRange(MatchAnswersToQuestions(boe.Id, task.Id, taskQuestions, answers));
                }
            }

            return questions;
        }

        private void DoPostProcessing(ICollection<RTECustomTemplateQuestionAnswerModelView> questions)
        {
            foreach(RTECustomTemplateQuestionAnswerModelView question in questions)
            {
                question.SourceIdList = question.SourceIdEnum.ToList();
                question.SourceIdEnum = null;
            }
        }

        /// <summary>
        /// Save the answers for RTE questions.
        /// </summary>
        /// <param name="answers">The answers to save.</param>
        public void SaveAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> answers)
        {
            if (answers != null && answers.Any())
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.Log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        foreach(RTECustomTemplateQuestionAnswerModelView answer in answers)
                        {
                            gbe.upsertRteTemplateAnswer(answer.Id, answer.UpdateDate, answer.QuestionId, answer.BoeId, answer.TaskId, answer.AnswerText ?? string.Empty, answer.SourceId);
                        }
                    }
                }
            }
        }

        #region Protected

        /// <summary>
        /// Upsert a Template
        /// Note: Does not save questions - SaveQuestions() will need to be called separately
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>id of upserted entry</returns>
        protected override int? Upsert(RteCustomTemplateModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                // save
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    string joinedIds = null;
                    if (dtoToUpsert.Assigned.Any())
                    {
                        joinedIds = string.Join(",", dtoToUpsert.Assigned.Select(a => a.ToString()));
                    }

                    int passedBackId = Convert.ToInt32(gbe.upsertRteTemplate(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.WorkspaceId, dtoToUpsert.Description, dtoToUpsert.AuthorId, joinedIds).ToList().FirstOrDefault());
                    dtoToUpsert.Id = passedBackId;
                    toReturn = passedBackId;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Matches answers to questions.
        /// </summary>
        /// <param name="boeId">The boe id to match against</param>
        /// <param name="questions"></param>
        /// <param name="answers"></param>
        private ICollection<RTECustomTemplateQuestionAnswerModelView> MatchAnswersToQuestions(int boeId, int? taskId, ICollection<RTECustomTemplateQuestionAnswerModelView> questions, ICollection<RteTemplateAnswer> answers)
        {
            List<RTECustomTemplateQuestionAnswerModelView> results = new List<RTECustomTemplateQuestionAnswerModelView>();
            if (questions.Any())
            {
                foreach (RTECustomTemplateQuestionAnswerModelView question in questions)
                {
                    foreach (int sourceId in question.SourceIdList)
                    {
                        RteTemplateAnswer answer = answers.FirstOrDefault(a => a.QuestionID == question.QuestionId && a.RteTemplateSourceId == sourceId 
                            && a.BOEID == boeId && a.TaskID == taskId);

                        RTECustomTemplateQuestionAnswerModelView result = new RTECustomTemplateQuestionAnswerModelView(question, boeId, taskId, sourceId);
                        
                        if (answer != null)
                        {
                            result.AnswerText = answer.Text;
                            result.Id = answer.AnswerID;
                            result.UpdateDate = answer.UpdateDT;
                        }

                        results.Add(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Saves the questions for an updated rte template
        /// </summary>
        /// <param name="questions">The questions to save.</param>
        /// <param name="templateId">The template ID for the questions.</param>
        /// <returns>Mapping of old and new IDs for the questions</returns>
        public Dictionary<int, int> SaveQuestions(ICollection<RteCustomTemplateQuestionModelView> questions, int templateId)
        {
            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                // save
                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    if (questions != null && questions.Any())
                    {
                        foreach (RteCustomTemplateQuestionModelView question in questions)
                        {
                            if (question.Updateable == UpdateType.Deleted)
                            {
                                gbe.deleteRteTemplateQuestion(question.Id);
                            }
                            else // must be an update
                            {
                                int? newId = Convert.ToInt32(gbe.upsertRteTemplateQuestion(question.Id, question.UpdateDate, templateId, question.Text, question.SortOrder, question.Required).ToList().FirstOrDefault());
                                toReturn.Add(question.Id, newId.Value);
                            }
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletion of a template.
        /// </summary>
        /// <param name="dtoToDelete">the element to delete</param>
        /// <returns>id of the deleted item</returns>
        protected override int? Delete(RteCustomTemplateModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                return gbe.deleteRteTemplate(dtoToDelete.Id);
            }
        }

        #endregion

        /// <summary>
        /// Matches questions to templates.
        /// </summary>
        /// <param name="templates">Templates.</param>
        /// <param name="questions">Questions.</param>
        private static void MatchQuestionsToTemplates(ICollection<RteCustomTemplateModelView> templates, ICollection<RteCustomTemplateQuestionModelView> questions)
        {
            // match questions to templates
            if (templates.Any() && questions.Any())
            {
                templates.AsParallel().ForAll(
                    rt =>
                    {
                        rt.Assigned = rt.AssignedList.ToList();
                        rt.AssignedList = null;
                        rt.Questions = questions.Where(q => q.TemplateId == rt.Id).ToList();
                    });
            }
        }
    }
}
