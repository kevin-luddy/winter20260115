// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Attributes
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;

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
			StartDate = string.Empty;
			EndDate = string.Empty;
			CanBeEqual = false;
			ValidIfEitherDateIsNull = true;
			ErrorMessage = "StartEndDate validation failed";
		}

		public override bool IsValid(object value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Validate with the validation context
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="validationContext">The context</param>
		/// <returns>Validation Result</returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			bool isValid = IsStartEndValid(value, validationContext);

			if (isValid)
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult(ErrorMessage, new List<string>() { StartDate });
			}
		}

		/// <summary>
		/// Is the start/end dates valid
		/// </summary>
		/// <param name="value">The parent object value</param>
		/// <param name="validationContext">validation context</param>
		/// <returns>Validation result of the start/end dates</returns>
		private bool IsStartEndValid(object value, ValidationContext validationContext)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (validationContext.ObjectType.GetProperty(StartDate) == null)
			{
				throw new ArgumentException(
					string.Format(
						"StartDate must be a valid property. '{0}' is not a property on the type '{1}'.", StartDate,
						validationContext.ObjectType.Name));
			}

			if (validationContext.ObjectType.GetProperty(EndDate) == null)
			{
				throw new ArgumentException(
					string.Format(
						"EndDate must be a valid property. '{0}' is not a property on the type '{1}'.", EndDate,
						validationContext.ObjectType.Name));
			}

			object startDateObject = validationContext.ObjectType.GetProperty(StartDate).GetValue(value, null);
			object endDateObject = validationContext.ObjectType.GetProperty(EndDate).GetValue(value, null);

			if (startDateObject == null ||
				endDateObject == null ||
				startDateObject.ToString().Trim().Length == 0 ||
				endDateObject.ToString().Trim().Length == 0)
			{
				return ValidIfEitherDateIsNull;
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
						"StartDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", StartDate,
						validationContext.ObjectType.Name));
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(
					string.Format(
						"StartDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", StartDate,
						validationContext.ObjectType.Name));
			}

			try
			{
				endDate = Convert.ToDateTime(endDateObject);
			}
			catch (FormatException)
			{
				throw new ArgumentException(
					string.Format(
						"EndDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", EndDate,
						validationContext.ObjectType.Name));
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(
					string.Format(
						"EndDate must be a valid DateTime. '{0}' is not a valid DateTime on the type '{1}'.", EndDate,
						validationContext.ObjectType.Name));
			}

			bool result = startDate < endDate || (startDate == endDate && CanBeEqual);
			return result;
		}

		/// <summary>
		/// Override to require validation context
		/// </summary>
		public override bool RequiresValidationContext => true;
	}

	//[ExcludeFromCodeCoverage]
	//public class StartEndDateValidationAttributeAdapter : DataAnnotationsModelValidator<StartEndDateValidationAttribute>
	//{
	//    public StartEndDateValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, StartEndDateValidationAttribute attribute)
	//        : base(metadata, context, attribute)
	//    {
	//    }

	//    public override IEnumerable<ModelValidationResult> Validate(object container)
	//    {
	//        if (!this.Attribute.IsValid(this.Metadata.Model))
	//        {
	//            yield return new ModelValidationResult
	//            {
	//                Message = this.ErrorMessage,
	//                MemberName = this.Attribute.StartDate
	//            };
	//        }
	//    }
	//}
}