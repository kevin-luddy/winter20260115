// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GenboeControllerLogicTest : MOQObject
    {
        /// <summary>
        /// Creates the system to test.
        /// </summary>
        /// <returns>The system under test.</returns>
        private GenBOEControllerLogic CreateSystem()
        {
            return new GenBOEControllerLogic();
        }

        /// <summary>
        /// Create a static set of Answers to use.
        /// </summary>
        /// <returns>A set of answers.</returns>
        private static ICollection<RTECustomTemplateQuestionAnswerModelView> CreateAnswers()
        {
            return new List<RTECustomTemplateQuestionAnswerModelView>
            {
                new RTECustomTemplateQuestionAnswerModelView
                {
                    Id = 1,
                    AnswerText = "Answer",
                    BoeId = 2,
                    QuestionId = 3,
                    QuestionText = "Quest # 1",
                    Required = true,
                    SortOrder = 1,
                    SourceId = 1,
                    TaskId = null,
                    TemplateId = 5,
                },
                new RTECustomTemplateQuestionAnswerModelView
                {
                    Id = 6,
                    AnswerText = "Answer 2",
                    BoeId = 2,
                    QuestionId = 4,
                    QuestionText = "Quest # 2",
                    Required = true,
                    SortOrder = 2,
                    SourceId = 1,
                    TaskId = null,
                    TemplateId = 5,
                }
            };
        }

        /// <summary>
        /// Creates a static set of Sources.
        /// </summary>
        /// <returns>A set of RTE Sources.</returns>
        private static ICollection<RteCustomTemplateSourceModelView> CreateSources()
        {
            return new List<RteCustomTemplateSourceModelView>
            {
                new RteCustomTemplateSourceModelView
                {
                    SourceId = 1,
                    Description = "Test Source",
                    TaskOnly = false
                }
            };
        }

        /// <summary>
        /// Test RTE Validation with bad RTE total length.
        /// </summary>
        [TestMethod]
        public void TestRteValidation_Rte_BAD_SIZE()
        {
            ICollection<RteCustomTemplateSourceModelView> sources = CreateSources();
            ICollection<RTECustomTemplateQuestionAnswerModelView> answers = CreateAnswers();
			GenBOEControllerLogic sut = CreateSystem();

            ICollection<ValidationMessage> messages = sut.ValidateRteAnswers(answers, sources, 2);

            Assert.IsTrue(messages.Any(m => m.ValidationIssue.Contains("The maximum length of a Custom RTE Template text")));
        }

        /// <summary>
        /// Test RTE Validation with missing required field.
        /// </summary>
        [TestMethod]
        public void TestRteValidation_Rte_MISSING_REQUIRED()
        {
            ICollection<RteCustomTemplateSourceModelView> sources = CreateSources();
            ICollection<RTECustomTemplateQuestionAnswerModelView> answers = CreateAnswers();
			GenBOEControllerLogic sut = CreateSystem();
            answers.First().AnswerText = null;

            ICollection<ValidationMessage> messages = sut.ValidateRteAnswers(answers, sources, 200);

            Assert.IsTrue(messages.Any(m => m.ValidationIssue.Contains("missing from required")));
        }

        /// <summary>
        /// Test valid RTE Validation.
        /// </summary>
        [TestMethod]
        public void TestRteValidation()
        {
            ICollection<RteCustomTemplateSourceModelView> sources = CreateSources();
            ICollection<RTECustomTemplateQuestionAnswerModelView> answers = CreateAnswers();
			GenBOEControllerLogic sut = CreateSystem();
            
            ICollection<ValidationMessage> messages = sut.ValidateRteAnswers(answers, sources, 200);

            Assert.IsTrue(messages.None());
        }
    }
}
