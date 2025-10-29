// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;

	/// <summary>
	/// Custom validation attribute for running a Regular Expression check when a value/condition for a specified field is satisfied
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	public class RegExpConditionalAttribute : ValidationAttribute, IClientValidatable
	{
		/// <summary>
		/// The name of the property to check
		/// </summary>
		private string DependentProperty { get; set; }
		/// <summary>
		/// The value needed to satisfy the dependent property
		/// </summary>
		private object DesiredValue { get; set; }
		/// <summary>
		/// The Regular Expression pattern to match
		/// </summary>
		private string RegExp { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="dependentProperty">The dependent property</param>
		/// <param name="desiredValue">The desired value</param>
		/// <param name="regExp">The regular expression pattern</param>
		public RegExpConditionalAttribute(string dependentProperty, object desiredValue, string regExp)
		{
			DependentProperty = dependentProperty;
			DesiredValue = desiredValue;
			RegExp = regExp;
		}

		/// <summary>
		/// Checks to see if all conditions are valid
		/// </summary>
		/// <param name="value">The value being used to check against the regular expression pattern</param>
		/// <param name="validationContext">The validation context</param>
		/// <returns>True/success if all conditions are valid, false/failure if there is no regular expression match when dependent field is satisfied</returns>
		/// <exception cref="ArgumentNullException"></exception>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (validationContext == null)
			{
				throw new ArgumentNullException(nameof(validationContext));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			object dependentValue = validationContext.ObjectInstance.GetType().GetProperty(DependentProperty).GetValue(validationContext.ObjectInstance, null);

			// Check values of the dependent vs. desired
			if (Regex.IsMatch(dependentValue.ToString(), DesiredValue.ToString()))
			{
				if (!Regex.IsMatch(value.ToString(), RegExp))
				{ 
					return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.MemberName });
				}
			}

			return ValidationResult.Success;
		}

		/// <summary>
		/// Get the client validation rules
		/// </summary>
		/// <param name="metadata">Metadata container</param>
		/// <param name="context">Controller context</param>
		/// <returns>IEnumerable of all of the validation rules</returns>
		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			ModelClientValidationRule rule = new ModelClientValidationRule
			{
				ErrorMessage = ErrorMessageString,
				ValidationType = "requiredif",
			};
			rule.ValidationParameters["dependentproperty"] = GetPropertyId(metadata, context as ViewContext);
			rule.ValidationParameters["desiredvalue"] = DesiredValue is bool ? DesiredValue.ToString().ToLower() : DesiredValue;

			yield return rule;
		}

		/// <summary>
		/// Get the property ID
		/// </summary>
		/// <param name="metadata">Metadata container</param>
		/// <param name="viewContext">View context</param>
		/// <returns>The property ID</returns>
		private string GetPropertyId(ModelMetadata metadata, ViewContext viewContext)
		{
			string propertyId = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(DependentProperty);
			string parentField = metadata.PropertyName + "_";
			return propertyId.Replace(parentField, "");
		}
	}
}