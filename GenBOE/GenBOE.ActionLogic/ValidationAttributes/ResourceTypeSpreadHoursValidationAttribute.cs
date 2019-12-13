using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using IES.Common;

namespace GenBOE.ActionLogic.ValidationAttributes
{
    /// <summary>
    /// Validates HoursSpread property of LMLaborTypeModelView class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class ResourceTypeSpreadHoursValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Name of the Hours Spread property.
        /// </summary>
        public string HourSpread { get; set; }

        /// <summary>
        /// Name of the Resource Hours Decimal Precision property.
        /// </summary>
        public string ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Name of the Cost spread property.
        /// </summary>
        public string CostSpread { get; set; }

        /// <summary>
        /// Name of the Resource Cost Decimal Precision property.
        /// </summary>
        public string CostDecimalPrecision { get; set; }

        /// <summary>
        /// Hours or Cost.
        /// </summary>
        public string RateType { get; set; }

        /// <summary>
        /// Gets or sets the previous rate type (Hours or Cost).
        /// </summary>
        public string PreviousRateType { get; set; }

        /// <summary>
        /// This method will be called to validate a HourSpread "Hrs Spread" value of the Resource Type.
        /// </summary>
        /// <param name="value">LMLaborTypeModelView</param>
        /// <returns>True if Hrs Spread is valid otherwise false.</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            bool isValid = true;

            // Use reflection to get the property values from the view model.
            Type contextType = value.GetType();
            PropertyInfo hourSpreadProperty = contextType.GetProperty(this.HourSpread);
            if (hourSpreadProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "HourSpread must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.HourSpread,
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

            PropertyInfo costSpreadProperty = contextType.GetProperty(this.CostSpread);
            if (costSpreadProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostSpread must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.HourSpread,
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

            PropertyInfo rateTypeProperty = contextType.GetProperty(this.RateType);
            if (rateTypeProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateType must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.RateType,
                        contextType.Name));
            }

            PropertyInfo previousRateTypeProperty = contextType.GetProperty(this.PreviousRateType);
            if (previousRateTypeProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Previous RateType must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.PreviousRateType,
                        contextType.Name));
            }

            object hourSpreadObject = hourSpreadProperty.GetValue(value, null) ?? 0;
            object resourceDecimalPrecisionObject = resourceDecimalPrecisionProperty.GetValue(value, null) ?? 0;
            object costSpreadObject = costSpreadProperty.GetValue(value, null) ?? 0;
            object costDecimalPrecisionObject = costDecimalPrecisionProperty.GetValue(value, null) ?? 0;
            object rateTypeObject = rateTypeProperty.GetValue(value, null) ?? 0;
            object previousRateTypeObject = previousRateTypeProperty.GetValue(value, null) ?? 0;

            decimal hourSpreadValue;
            int resourceDecimalPrecisionValue;
            decimal costSpreadValue;
            int costDecimalPrecisionValue;
            RateType rateTypeValue, previousRateTypeValue;

            try
            {
                hourSpreadValue = Convert.ToDecimal(hourSpreadObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.HourSpread,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.HourSpread,
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
                costSpreadValue = Convert.ToDecimal(costSpreadObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.HourSpread,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "CostSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.HourSpread,
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

            try
            {
                rateTypeValue = (RateType)Convert.ToInt32(rateTypeObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateType must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.RateType,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateType must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.RateType,
                        contextType.Name));
            }

            try
            {
                previousRateTypeValue = (RateType)Convert.ToInt32(previousRateTypeObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "Previous RateType must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.RateType,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "Previous RateType must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.RateType,
                        contextType.Name));
            }

            // only validate if the rate type has not changed between Hours <--> Cost.  If it has, the spread curve will be reset to nothing and the hours emptied later
            if (previousRateTypeValue == rateTypeValue || previousRateTypeValue == IES.Common.RateType.NotSet)
            {

                // Validate number is between -9999999999.999999m and 9999999999.999999m within the limits of decimal precision for decimal places.
                Regex expression = new Regex(@"(^[+-]?\d{0,10}([.]\d{0," + resourceDecimalPrecisionValue + "})?$)", RegexOptions.None, Constants.REGEX_TIMEOUT); ;

                if (!expression.IsMatch(hourSpreadValue.ToString()))
                {
                    // Validate values is within acceptable decimal precision
                    this.ErrorMessage = "Hours Spread can only support a 10-digit number with up to " + resourceDecimalPrecisionValue + " decimal places.";
                    isValid = false;
                }

                if (isValid)
                {
                    // Validate number is between -9999999999.99m and 9999999999.99m within the limits of decimal precision for decimal places
                    // If hours always allow for 2 decimal digits. If cost factor in the cost precision.
                    int precision = rateTypeValue == IES.Common.RateType.Cost ? costDecimalPrecisionValue : 2;
                    expression = new Regex(@"(^[+-]?\d{0,10}([.]\d{0," + precision + "})?$)", RegexOptions.None, Constants.REGEX_TIMEOUT);

                    if (!expression.IsMatch(costSpreadValue.ToString()))
                    {
                        // Validate values is within acceptable decimal precision
                        this.ErrorMessage = "Costs Spread can only support a 10-digit number with up to " + precision + " decimal places.";
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}