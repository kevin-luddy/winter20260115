// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IBOEHeaderModelView
    {
        /// <summary>
        /// Gets/sets BOEID
        /// </summary>
        int BOEID { get; set; }

        /// <summary>
        /// Gets/sets Title
        /// </summary>
        String Title { get; set; }

        /// <summary>
        /// Gets/sets CLIN
        /// </summary>
        string CLIN { get; set; }

        /// <summary>
        /// Gets/sets CustomFieldValues
        /// </summary>
        Collection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }

        /// <summary>
        /// Gets/sets DataSource
        /// </summary>
        [RichText(RichTextDbColumn.BOE_DATA_SOURCE, "BOEID")]
        string DataSource { get; set; }

        /// <summary>
        /// Gets/sets EndDate
        /// </summary>
        DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets/sets HistoricMetricDisclosureChecked
        /// </summary>
        bool HistoricMetricDisclosureChecked { get; set; }

        /// <summary>
        /// Gets/sets StartDate
        /// </summary>
        DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets/sets WBS
        /// </summary>
        string WBS { get; set; }

        // methods from parent class PersistedDataModelView
        /// <summary>
        /// Gets/sets UpdateDate
        /// </summary>
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets/sets UpdateDateLong
        /// </summary>
        string UpdateDateLong { get; set; }

        /// <summary>
        /// Indicates if the Title field is required
        /// </summary>
        Boolean IsTitleRequired { get; }

        /// <summary>
        /// Gets the Sources of data label
        /// </summary>
        String LabelSourcesOfData { get; }

        ICollection<RTECustomTemplateQuestionAnswerModelView> HeaderRteTemplateAnswers { get; }
    }
}
