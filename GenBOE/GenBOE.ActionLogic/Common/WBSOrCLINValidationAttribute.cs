using System;
using System.ComponentModel.DataAnnotations;

namespace GenBOE.ActionLogic.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class WBSOrCLINValidationAttribute : ValidationAttribute
    {
        public string WBSId { get; set; }
        public string CLINId { get; set; }

        /// <summary>
        /// Checks if WBS or CLIN is valid
        /// </summary>
        /// <param name="value">WBS or CLIN to validate</param>
        /// <returns>True if valid, false if not valid</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type contextType = value.GetType();

            if (contextType.GetProperty(this.WBSId) == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "WBSId must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.WBSId,
                        contextType.Name));
            }

            if (contextType.GetProperty(this.CLINId) == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "CLINId must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.CLINId,
                        contextType.Name));
            }

            int? wbsIdObject = contextType.GetProperty(this.WBSId).GetValue(value, null) as int?;
            int? clinIdObject = contextType.GetProperty(this.CLINId).GetValue(value, null) as int?;

            // either wbsid or clinid need to valued (or both)
            if (wbsIdObject == null && clinIdObject == null)
            {
                return false;
            }
            else if (wbsIdObject <= 0 && clinIdObject == null)
            {
                return false;
            }
            else if (wbsIdObject <= 0 && clinIdObject <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}