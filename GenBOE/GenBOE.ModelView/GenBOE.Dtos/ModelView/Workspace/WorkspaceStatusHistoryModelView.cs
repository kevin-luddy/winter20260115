// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class WorkspaceStatusHistoryModelView
    {
        public WorkspaceStatusHistoryModelView()
            : base()
        {
            OldValue = string.Empty;
            NewValue = string.Empty;
            PerformedBy = string.Empty;
            Date = DateTime.MinValue;
        }

        public string OldValue { get; set; }
        public string NewValue {get;set;}
        public string PerformedBy { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime Date { get; set; }
    }
}
