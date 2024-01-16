namespace IES.Common.Core.Models
{
	using System;
	using IES.Common.Core;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Allows identification of output format template ID that either DO or DO NOT correspond to actual (defined) enumeration values
	/// </summary>
	[Serializable]
	public class ExcelReportTemplate
	{
		private int templateIdValue;
		private int? parentTemplateIdValue;

		public ExcelReportTemplate()
		{
			templateIdValue = (int)ExcelReportTemplateType.NotSet;
			parentTemplateIdValue = null;
			SetExportTemplateType((int)ExcelReportTemplateType.NotSet, null);
		}

		public ExcelReportTemplate(int templateId, int? parentTemplateId)
		{
			templateIdValue = templateId;
			parentTemplateIdValue = parentTemplateId;
			SetExportTemplateType(templateId, parentTemplateId);
		}

		public int TemplateId
		{
			set
			{
				templateIdValue = value;
				SetExportTemplateType(value, parentTemplateIdValue);
			}

			get
			{
				return templateIdValue;
			}
		}

		public int? ParentTemplateId
		{
			set
			{
				parentTemplateIdValue = value;
				SetExportTemplateType(templateIdValue, value);
			}

			get
			{
				return parentTemplateIdValue;
			}
		}

		public ExcelReportTemplateType TemplateType { get; private set; }

		private void SetExportTemplateType(int templateId, int? parentTemplateId)
		{
			if (parentTemplateId == null)
			{
				TemplateType = templateId.GetEnumeratedValue<ExcelReportTemplateType>(ExcelReportTemplateType.NotSet);
			}
			else
			{
				TemplateType = parentTemplateId.GetEnumeratedValue<ExcelReportTemplateType>(ExcelReportTemplateType.NotSet);
			}
		}
	}
}
