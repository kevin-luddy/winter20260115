using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using IES.Common;

namespace GenBOE.ActionLogic.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class LaborSpreadHoursValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Value of a labor spread month.
        /// </summary>
        public string LaborSpreadValue { get; set; }

        /// <summary>
        /// When true, the spread value is a cost value otherwise an hour value.
        /// </summary>
        public string IsSpreadTypeCost { get; set; }

        /// <summary>
        /// Number of decimal places for hour type spreads.
        /// </summary>
        public string ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Number of decimal places for cost type spreads.
        /// </summary>
        public string CostDecimalPrecision { get; set; }

        /// <summary>
        /// Boolean about whether the rate type changed or not.
        /// </summary>
        public string RateTypeChanged { get; set; }

        /// <summary>
        /// This method will be called to validate a LaborSpread monthly value. Spread types can be cost or hours. Validation
        /// is done based on the decimal precision of the labor spread type.
        /// </summary>
        /// <param name="value">The spread value.</param>
        /// <returns>True if valid otherwise false.</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type contextType = value.GetType();

            PropertyInfo laborSpreadValueProperty = contextType.GetProperty(this.LaborSpreadValue);
            if (laborSpreadValueProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.LaborSpreadValue,
                        contextType.Name));
            }

            PropertyInfo isSpreadTypeCostProperty = contextType.GetProperty(this.IsSpreadTypeCost);
            if (isSpreadTypeCostProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "IsSpreadTypeCost must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.IsSpreadTypeCost,
                        contextType.Name));
            }

            PropertyInfo resourceDecimalPrecisionProperty = contextType.GetProperty(this.ResourceDecimalPrecision);
            if (resourceDecimalPrecisionProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "ResourceDecimalPrecision must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.ResourceDecimalPrecision,
                        contextType.Name));
            }

            PropertyInfo costDecimalPrecisionProperty = contextType.GetProperty(this.CostDecimalPrecision);
            if (costDecimalPrecisionProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostDecimalPrecision must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.CostDecimalPrecision,
                        contextType.Name));
            }

            PropertyInfo rateTypeChangedProperty = contextType.GetProperty(this.RateTypeChanged);
            if (rateTypeChangedProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateTypeChanged must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.RateTypeChanged,
                        contextType.Name));
            }

            object laborSpreadValueObject = laborSpreadValueProperty.GetValue(value, null) ?? 0;
            object isSpreadTypeCostObject = isSpreadTypeCostProperty.GetValue(value, null) ?? false;
            object resourceDecimalPrecisionObject = resourceDecimalPrecisionProperty.GetValue(value, null) ?? 0;
            object costDecimalPrecisionObject = costDecimalPrecisionProperty.GetValue(value, null) ?? 2;  // Default to 2. Dollar/cents.
            object rateTypeChangedObject = rateTypeChangedProperty.GetValue(value, null) ?? false;

            decimal laborSpreadValue;
            bool isSpreadTypeCost, rateTypeChanged;
            int resourceDecimalPrecisionValue;
            int costDecimalPrecisionValue;

            try
            {
                laborSpreadValue = Convert.ToDecimal(laborSpreadValueObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.LaborSpreadValue,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.LaborSpreadValue,
                        contextType.Name));
            }

            try
            {
                isSpreadTypeCost = Convert.ToBoolean(isSpreadTypeCostObject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "IsSpreadTypeCost must be a valid boolean. '{0}' is not a valid boolean on the type '{1}'.", this.IsSpreadTypeCost,
                        contextType.Name));
            }

            try
            {
                rateTypeChanged = Convert.ToBoolean(rateTypeChangedObject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateTypeChanged must be a valid boolean. '{0}' is not a valid boolean on the type '{1}'.", this.RateTypeChanged,
                        contextType.Name));
            }

            try
            {
                resourceDecimalPrecisionValue = Convert.ToInt32(resourceDecimalPrecisionObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "ResourceDecimalPrecision must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.ResourceDecimalPrecision,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "ResourceDecimalPrecision must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.ResourceDecimalPrecision,
                        contextType.Name));
            }

            try
            {
                costDecimalPrecisionValue = Convert.ToInt32(costDecimalPrecisionObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostDecimalPrecision must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.CostDecimalPrecision,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostDecimalPrecision must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.CostDecimalPrecision,
                        contextType.Name));
            }

            // Only check if the rate type has not changed
            if (!rateTypeChanged)
            {
                // If SpreadType is "Hours"
                if (!isSpreadTypeCost)
                {
                    // Validate number is between -9999999999.999999m and 9999999999.999999m within the limits of decimal precision for decimal places.
                    if (!Regex.IsMatch(laborSpreadValue.ToString(), @"(^[+-]?\d{0,10}([.]\d{0," + resourceDecimalPrecisionValue + "})?$)", RegexOptions.None, Constants.REGEX_TIMEOUT))
                    {
                        // Validate values is within acceptable decimal precision
                        this.ErrorMessage = "Hours Spread can only support a 10-digit number with up to " + resourceDecimalPrecisionValue + " decimal places.";
                        return false;
                    }
                }
                else // Cost type.
                {
                    // Validate number is between -9999999999.999999m and 9999999999.999999m within the limits of decimal precision for decimal places.
                    if (!Regex.IsMatch(laborSpreadValue.ToString(), @"(^[+-]?\d{0,10}([.]\d{0," + costDecimalPrecisionValue + "})?$)", RegexOptions.None, Constants.REGEX_TIMEOUT))
                    {
                        // Validate values is within acceptable decimal precision
                        this.ErrorMessage = "Costs Spreads can only support a 10-digit number with up to " + costDecimalPrecisionValue + " decimal places.";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}