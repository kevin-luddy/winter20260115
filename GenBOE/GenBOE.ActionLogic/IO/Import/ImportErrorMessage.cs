// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class ImportErrorMessage
    {

        public ImportErrorMessage()
        {
            this.Title = String.Empty;
            this.Message = String.Empty;
        }

        public String Title { get; set; }
        public String Message { get; set; }
    }
}

