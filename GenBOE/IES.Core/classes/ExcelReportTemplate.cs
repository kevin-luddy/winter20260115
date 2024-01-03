namespace IES.Core
{
    using System;

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
            this.templateIdValue = (int)ExcelReportTemplateType.NotSet;
            this.parentTemplateIdValue = null;
            SetExportTemplateType((int)ExcelReportTemplateType.NotSet, null);
        }

        public ExcelReportTemplate(int templateId, int? parentTemplateId)
        {
            this.templateIdValue = templateId;
            this.parentTemplateIdValue = parentTemplateId;
            SetExportTemplateType(templateId, parentTemplateId);
        }

        public int TemplateId
        {
            set
            {
                this.templateIdValue = value;
                SetExportTemplateType(value, this.parentTemplateIdValue);
            }

            get
            {
                return this.templateIdValue;
            }
        }

        public int? ParentTemplateId
        {
            set
            {
                this.parentTemplateIdValue = value;
                SetExportTemplateType(this.templateIdValue, value);
            }

            get
            {
                return this.parentTemplateIdValue;
            }
        }

        public ExcelReportTemplateType TemplateType { get; private set; }

        private void SetExportTemplateType(int templateId, int? parentTemplateId)
        {
            if (parentTemplateId == null)
            {
                this.TemplateType = templateId.GetEnumeratedValue<ExcelReportTemplateType>(ExcelReportTemplateType.NotSet);
            }
            else
            {
                this.TemplateType = parentTemplateId.GetEnumeratedValue<ExcelReportTemplateType>(ExcelReportTemplateType.NotSet);
            }
        }
    }
}
