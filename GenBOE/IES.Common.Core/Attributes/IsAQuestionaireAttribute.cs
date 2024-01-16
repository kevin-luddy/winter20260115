// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Attributes
{
	using System;

	/// <summary>
	/// Adds a discipline as an attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
	public sealed class IsAQuestionaireAttribute : Attribute
	{

		/// <summary>
		/// QuestionaireAttribute to indicate if a data type enum is a questionaire or not.
		/// </summary>
		/// <param name="isAQuestionaire">isAQuestionaire</param>
		public IsAQuestionaireAttribute(bool isAQuestionaire)
		{
			IsAQuestionaire = isAQuestionaire;
		}

		/// <summary>
		/// Boolean indicating if the enumeration literal represents a questionaire
		/// </summary>
		public bool IsAQuestionaire { get; }
	}
}
