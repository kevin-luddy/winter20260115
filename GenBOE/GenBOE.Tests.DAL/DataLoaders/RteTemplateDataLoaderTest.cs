// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test class for RTE Custom Templates Data Loader
    /// </summary>
    [TestClass]
    public class RteTemplateDataLoaderTest : MOQLoaderObject
    {
        private Mock<IRetriever> retriever;
        private IFullObjectFactory fullObjectFactory;

        /// <summary>
        /// Initialize the database.
        /// </summary>
        [TestInitialize]
        public void InitializeTestMethod()
        {
            fullObjectFactory = new FullObjectFactory(null, null, null, null, null, null, null, null, null, null, null, null);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), fullObjectFactory);

            retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

            // delete any templates still in the temp workspace
            var sut = new RteTemplateDataLoader();

            ICollection<RteCustomTemplateModelView> templates = sut.GetTemplates(this.Workspace.Id);
            if (templates.Any())
            {
                templates.ForEach(t => t.Updateable = UpdateType.Deleted);
            }

            sut.Save(templates);
        }

        /// <summary>
        /// This comprehensive test does insertion, deletion, modifying, and searching of RTE Custom Templates
        /// </summary>
        [TestMethod]
        public void RteTemplateSearchTest()
        {
            var sut = new RteTemplateDataLoader();
            ICollection<RteCustomTemplateModelView> templates = sut.GetTemplates(this.Workspace.Id);

            Assert.AreEqual(0, templates.Count);

            templates.Add(new RteCustomTemplateModelView
            {
                Id = -1,
                Assigned = new int[] { 1, 2 },
                AuthorId = this.Workspace.CostVolumeLeadPricerUserID,
                Description = "googily",
                Updateable = UpdateType.Upsert,
                WorkspaceId = this.Workspace.Id,
                Questions = new List<RteCustomTemplateQuestionModelView> {
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -3,
                        Required = true,
                        SortOrder = 0,
                        Text = "Q1",
                        Updateable = UpdateType.Upsert
                    },
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -4,
                        Required = true,
                        SortOrder = 2,
                        Text = "Q2",
                        Updateable = UpdateType.Upsert
                    }
                }
            });

            templates.Add(new RteCustomTemplateModelView
            {
                Id = -2,
                AuthorId = this.Workspace.CostVolumeLeadPricerUserID,
                Description = "moogily",
                Updateable = UpdateType.Upsert,
                WorkspaceId = this.Workspace.Id,
                Assigned = new int[] { 3, 4 },
                Questions = new List<RteCustomTemplateQuestionModelView> {
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -5,
                        Required = true,
                        SortOrder = 0,
                        Text = "Q3",
                        Updateable = UpdateType.Upsert
                    }
                }
            });

            sut.Save(templates);
            foreach (RteCustomTemplateModelView template in templates)
            {
                sut.SaveQuestions(template.Questions, template.Id);
            }

            // get the question/answers for the boe -- should return 4 (2x the first two questions)
            ICollection<RTECustomTemplateQuestionAnswerModelView> questions = sut.GetByBoeId(this.Boe1.WorkspaceID, this.Boe1.Id);
            Assert.AreEqual(4, questions.Count);

            // get the question/answers for the boe -- should return 2 (2x the last question)
            questions = sut.GetByBoeIdAndTaskId(this.Workspace.Id, this.TaskElement.BoeID, this.TaskElement.Id);
            Assert.AreEqual(2, questions.Count);

            RTECustomTemplateQuestionAnswerModelView answer = questions.First();
            answer.AnswerText = "new answer";

            sut.SaveAnswers(questions);

            questions = sut.GetByBoeIdAndTaskId(this.Workspace.Id, this.TaskElement.BoeID, this.TaskElement.Id);
            Assert.AreEqual(2, questions.Count);

            Assert.IsTrue(questions.Any(q => q.AnswerText == "new answer"));

            ICollection<FullBoe> boes = new List<FullBoe>();
            FullBoe fullBoe = new FullBoe(this.Boe1);
            boes.Add(fullBoe);
            fullBoe.SetTaskElements(new BoeTaskElementDTO[] { this.TaskElement });
            FullBoe fullBoe2 = new FullBoe(this.Boe2);
            boes.Add(fullBoe2);
            fullBoe2.SetTaskElements(new BoeTaskElementDTO[] { });

            questions = sut.GetByWorkspaceId(this.Workspace.Id, boes);
            // There should be all of the boe and task questions (2x 4 questions for boe + 1x 2 questions for task == 10)
            Assert.AreEqual(10, questions.Count);
            Assert.AreEqual(1, questions.Count(q => q.AnswerText == "new answer"));

            ICollection<RteCustomTemplateModelView> updatedTemplates = sut.GetTemplates(this.Workspace.Id);
            Assert.AreEqual(2, updatedTemplates.Count);

            ICollection<RteCustomTemplateModelView> found = sut.Search("googily");
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(2, found.First().Questions.Count);

            found = sut.Search("oogily");
            Assert.AreEqual(2, found.Count);

            found = sut.Search(this.Workspace.WorkspaceName);
            Assert.AreEqual(2, found.Count);

            updatedTemplates.ForEach(t => t.Updateable = UpdateType.Deleted);
            sut.Save(updatedTemplates);

            found = sut.Search("oogily");
            Assert.AreEqual(0, found.Count);
        }

        /// <summary>
        /// I took code from RteTemplateSearchTest, made the workspace not searchable, and adjusted verification to result in 0 counts
        /// </summary>
        [TestMethod]
        public void RteTemplateSearchTest_AllowedSearch()
        {
            var sut = new RteTemplateDataLoader();
            ICollection<RteCustomTemplateModelView> templates = sut.GetTemplates(this.Workspace.Id);

            Assert.AreEqual(0, templates.Count);

            templates.Add(new RteCustomTemplateModelView
            {
                Id = -1,
                Assigned = new int[] { 1, 2 },
                AuthorId = this.Workspace.CostVolumeLeadPricerUserID,
                Description = "googily",
                Updateable = UpdateType.Upsert,
                WorkspaceId = this.Workspace.Id,
                Questions = new List<RteCustomTemplateQuestionModelView> {
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -3,
                        Required = true,
                        SortOrder = 0,
                        Text = "Q1",
                        Updateable = UpdateType.Upsert
                    },
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -4,
                        Required = true,
                        SortOrder = 2,
                        Text = "Q2",
                        Updateable = UpdateType.Upsert
                    }
                }
            });
            templates.Add(new RteCustomTemplateModelView
            {
                Id = -2,
                AuthorId = this.Workspace.CostVolumeLeadPricerUserID,
                Description = "moogily",
                Updateable = UpdateType.Upsert,
                WorkspaceId = this.Workspace.Id,
                Assigned = new int[] { 3, 4 },
                Questions = new List<RteCustomTemplateQuestionModelView> {
                    new RteCustomTemplateQuestionModelView
                    {
                        Id = -5,
                        Required = true,
                        SortOrder = 0,
                        Text = "Q3",
                        Updateable = UpdateType.Upsert
                    }
                }
            });

            sut.Save(templates);
            foreach (RteCustomTemplateModelView template in templates)
            {
                sut.SaveQuestions(template.Questions, template.Id);
            }

            WorkspaceDTODataLoader wsLoader = new WorkspaceDTODataLoader();
            this.Workspace.AllowSearch = false;
            wsLoader.SaveAllowSearch(this.Workspace);

            ICollection<RTECustomTemplateQuestionAnswerModelView> questions = sut.GetByBoeId(this.Boe1.WorkspaceID, this.Boe1.Id);
            questions = sut.GetByBoeIdAndTaskId(this.Workspace.Id, this.TaskElement.BoeID, this.TaskElement.Id);
            RTECustomTemplateQuestionAnswerModelView answer = questions.First();
            answer.AnswerText = "new answer";
            sut.SaveAnswers(questions);
            questions = sut.GetByBoeIdAndTaskId(this.Workspace.Id, this.TaskElement.BoeID, this.TaskElement.Id);
            ICollection<FullBoe> boes = new List<FullBoe>();
            FullBoe fullBoe = new FullBoe(this.Boe1);
            boes.Add(fullBoe);
            fullBoe.SetTaskElements(new BoeTaskElementDTO[] { this.TaskElement });
            FullBoe fullBoe2 = new FullBoe(this.Boe2);
            boes.Add(fullBoe2);
            fullBoe2.SetTaskElements(new BoeTaskElementDTO[] { });
            questions = sut.GetByWorkspaceId(this.Workspace.Id, boes);
            ICollection<RteCustomTemplateModelView> updatedTemplates = sut.GetTemplates(this.Workspace.Id);

            ICollection<RteCustomTemplateModelView> found = sut.Search("googily");
            Assert.AreEqual(0, found.Count);

            found = sut.Search("oogily");
            Assert.AreEqual(0, found.Count);

            found = sut.Search(this.Workspace.WorkspaceName);
            Assert.AreEqual(0, found.Count);

            updatedTemplates.ForEach(t => t.Updateable = UpdateType.Deleted);
            sut.Save(updatedTemplates);

            found = sut.Search("oogily");
            Assert.AreEqual(0, found.Count);

            this.Workspace.AllowSearch = true;
            wsLoader.SaveAllowSearch(this.Workspace);
        }
    }
}
