// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.DataAnnotations;

namespace IES.Common
{
    /// <summary>
    /// Identifies a string property as containing rich-text.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification="Accessors defined, but code analysis still complaining.")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RichTextAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="richTextDbColumn">The rich-text database column that the property corresponds to</param>
        /// <param name="pkPropertyName">The primary key associated with the rich-text database column</param>
        public RichTextAttribute(RichTextDbColumn dbColumn, string pkPropertyName)
            : base()
        {
            this.ErrorMessage = "{0} is required.";

            this.RichTextColumn = dbColumn;
            this.PKPropertyName = pkPropertyName;
        }

        /// <summary>
        /// The rich-text database column that the property corresponds to
        /// </summary>
        public RichTextDbColumn RichTextColumn { get; private set; }

        /// <summary>
        /// The primary key associated with the rich-text database column
        /// </summary>
        public string PKPropertyName { get; set; }
        
        /// <summary>
        /// Whether the property requires a value
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Determines whether the specified value of the object is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate</param>
        /// <returns>true if the specified value is valid; otherwise, false</returns>
        public override bool IsValid(object value)
        {
            bool valid = true;

            if (this.Required)
            {
                string html = value as string;

                if (string.IsNullOrWhiteSpace(html))
                {
                    valid = false;
                }
            }

            return valid;
        }
    }
}
