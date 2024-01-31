// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Enums
{
	/// <summary>
	/// The set of rich-text DB columns that an image may be associated with
	/// </summary>
	public enum RichTextDbColumn
	{
		None = 0,
		BOE_BOE_DESCRIPTION,
		BOE_DATA_SOURCE,
		BOE_TASK_ELEMENT_TASK_DESCRIPTION,
		BOE_TASK_ELEMENT_MOQ_TEXT,
		MATERIAL_TASK_ELEMENT_TASK_DESCRIPTION,
		MATERIAL_TASK_ELEMENT_MATERIAL_MOQ_TEXT,
		ODC_TASK_ELEMENT_ODC_TASK_DESCRIPTION,
		ODC_TASK_ELEMENT_ODC_MOQ_TEXT,
		TRAVEL_TRIP_TASK_ELEMENT_TRAVEL_TASK_DESCRIPTION,
		PBOE_DESCRIPTION,
		PBOE_BASIS_RATIONALE,
		PBOE_SOURCE_SELECTION_DESC,
		PBOE_COMMERCIALITY_DESC,
		PBOE_TECHNICAL_EVAL_DESC,
		PBOE_PRICE_ANALYSIS_DESC,
		PBOE_COST_ANALYSIS_DESC,
		PBOE_RATIONALE_VALUE_SUMMARY
	}
}
