// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class StartEndDateValidationAttribute : ValidationAttribute
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool CanBeEqual { get; set; }
        public bool ValidIfEitherDateIsNull { get; set; }

        public StartEndDateValidationAttribute()
            : base()
        {
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;
            this.CanBeEqual = false;
            this.ValidIfEitherDateIsNull = true;
            this.ErrorMessage = "StartEndDate validation failed";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type contextType = value.GetType();

            if (contextType.GetProperty(this.StartDate) == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "StartDate must be a valid property. '{0}' is not a property on the type '{1}'.", this.StartDate,
                        contextType.Name));
            }

            if (contextType.GetProperty(this.EndDate) == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "EndDate must be a valid property. '{0}' is not a property on the type '{1}'.", this.EndDate,
                        contextType.Name));
            }

            object startDateObject = contextType.GetProperty(this.StartDate).GetValue(value, null);
            object endDateObject = contextType.GetProperty(this.EndDate).GetValue(value, null);

            if (startDateObject == null ||
                endDateObject == null ||
                startDateObject.ToString().Trim().Length == 0 ||
                endDateObject.ToString().Trim().Length == 0)
            {
                return this.ValidIfEitherDateIsNull;
            }

            DateTime startDate;
            DateTime endDate;

            try
            {
                startDate = Convert.ToDateTime(startDateObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "StartDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", this.StartDate,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "StartDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", this.StartDate,
                        contextType.Name));
            }

            try
            {
                endDate = Convert.ToDateTime(endDateObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "EndDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", this.EndDate,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "EndDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", this.EndDate,
                        contextType.Name));
            }

            bool result = (startDate < endDate || (startDate == endDate && this.CanBeEqual));
            return result;
        }
    }

    [ExcludeFromCodeCoverage]
    public class StartEndDateValidationAttributeAdapter : DataAnnotationsModelValidator<StartEndDateValidationAttribute>
    {
        public StartEndDateValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, StartEndDateValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            if (!this.Attribute.IsValid(this.Metadata.Model))
            {
                yield return new ModelValidationResult
                {
                    Message = this.ErrorMessage,
                    MemberName = this.Attribute.StartDate
                };
            }
        }
    }
}