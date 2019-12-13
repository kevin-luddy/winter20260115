// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// Adds a discipline as an attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class IsAQuestionaireAttribute : Attribute
    {
        /// <summary>
        /// Boolean indicating if the data type is a questionaire
        /// </summary>
        private bool isAQuestionaire;

        /// <summary>
        /// QuestionaireAttribute to indicate if a data type enum is a questionaire or not.
        /// </summary>
        /// <param name="isAQuestionaire">isAQuestionaire</param>
        public IsAQuestionaireAttribute(bool isAQuestionaire)
        {
            this.isAQuestionaire = isAQuestionaire;
        }

        /// <summary>
        /// Boolean indicating if the enumeration literal represents a questionaire
        /// </summary>
        public bool IsAQuestionaire
        {
            get
            {
                return this.isAQuestionaire;
            }
        }
    }
}
