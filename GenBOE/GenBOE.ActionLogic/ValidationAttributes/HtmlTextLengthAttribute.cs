using System;
using System.ComponentModel.DataAnnotations;
using IES.Common;

namespace GenBOE.ActionLogic.ValidationAttributes
{
    /// <summary>
    /// Automatically strip-out all markup before comparing the text-only string-content length of the item against the designated maximum limit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    public sealed class HtmlTextLengthAttribute : ValidationAttribute 
    {
        /// <summary>
        /// Maximum number of text-only characters allowed
        /// </summary>
        public int MaximumLength { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HtmlTextLengthAttribute()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxLength">Maximum number of text-only characters allowed</param>
        public HtmlTextLengthAttribute(int maxLength)
        {
            this.MaximumLength = maxLength;
        }

        /// <summary>
        /// Generate validation error message
        /// </summary>
        /// <param name="name">Display name</param>
        /// <returns>Error message</returns>
        public override string FormatErrorMessage(string name)
        {
            if (this.ErrorMessage == null)
            {
                return string.Format("A maximum of {0} characters are allowed for {1}.", this.MaximumLength, name);
            }
            else
            {
                return string.Format(this.ErrorMessage, name);
            }
        }

        /// <summary>
        /// Validate
        /// </summary>
        /// <param name="value">Item value</param>
        /// <returns>True if valid; false if not.</returns>
        public override bool IsValid(object value)
        {
            bool valid = true;

            if (value != null)
            {
                string html = value.ToString();
                string text = GenBOEUtilities.ConvertHtmlToText(html);
                valid = text.Length <= this.MaximumLength;
            }

            return valid;
        }
    }
}