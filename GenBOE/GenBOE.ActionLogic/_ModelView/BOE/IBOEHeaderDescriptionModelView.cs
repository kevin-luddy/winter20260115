// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView.BOE
{
	using GenBOE.Dtos;
	using System.Collections.Generic;

	public interface IBOEHeaderDescriptionModelView
    {
		/// <summary>
		/// Gets/sets BOEID
		/// </summary>
		int BOEID { get; set; }

		/// <summary>
        /// Gets/sets Description
        /// </summary>
		string Description { get; set; }

		/// <summary>
		/// Gets/sets EndDate
		/// </summary>
		string EndDate { get; set; }

		/// <summary>
		/// Gets/sets StartDate
		/// </summary>
		string StartDate { get; set; }

		/// <summary>
		/// Gets or sets the RTE Custom Template Answers at BOE level.
		/// </summary>
		ICollection<RTECustomTemplateQuestionAnswerModelView> RteTemplateAnswers { get; }

	}
}
