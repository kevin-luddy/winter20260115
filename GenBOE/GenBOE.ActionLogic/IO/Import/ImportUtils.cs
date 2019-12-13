// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Globalization;
    using GenBOE.ActionLogic.Validation;

    public static class ImportUtils
    {
        /// <summary>
        /// Will check if the spread has 12 digits usually used to verify
        /// that X XXX XXX XXX.XX is the correct size as we convert decimals to 
        /// longs by multiplying by 100m.  
        /// </summary>
        /// <param name="spread">number to make sure isn't to large </param>
        /// <returns>True if between -999999999999L and 999999999999L if null return false. </returns>
        public static bool IsSpread12Digits(long? spread)
        {
            bool valid = false;
            if (spread.HasValue)
            {
                if (spread >= -999999999999L && spread <= 999999999999L)
                {
                    valid = true;
                }
            }
            return valid;
        }

        /// <summary>
        /// Used for verify that there are no more then 10 digits.  Hours are usualy 
        /// checked against this.  
        /// </summary>
        /// <param name="spread"></param>
        /// <returns></returns>
        public static bool IsSpread10Digits(long? spread)
        {
            bool valid = false;
            if (spread != null)
            {
                if (spread >= -9999999999L && spread <= 9999999999L)
                {
                    valid = true;
                }
            }
            return valid;
        }

        /// <summary>
        /// Check if string (number is only 2 decimal points)
        /// </summary>
        /// <param name="mystring">string to check</param>
        /// <param name="val">Value parsed</param>
        /// <returns>False if couldn't parse decimal or more then 2 decimal places</returns>
        public static bool Is2MaxDecimalPlaces(string mystring, out Decimal val)
        {
            if (mystring == null)
            {
                throw new ArgumentNullException(nameof(mystring));
            }
            bool validDecimal = decimal.TryParse(mystring, out val);
            if (!validDecimal)
            {
                return false;
            }
            var index = mystring.LastIndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            if (index == -1)
            {
                return true;
            }
            string endString = mystring.Substring(index);
            return endString.Length < 4;
        }

        /// <summary>
        /// Check if string (number is only N decimal points)
        /// </summary>
        /// <param name="mystring">string to check</param>
        /// <param name="decimalPrecision">Number of Decimal Precision</param>
        /// <param name="val">Value parsed</param>
        /// <returns>False if couldn't parse decimal or more then N decimal places</returns>
        public static bool IsMaxDecimalPlaces(string mystring, int? decimalPrecision, out Decimal val)
        {
            if (mystring == null)
            {
                throw new ArgumentNullException(nameof(mystring));
            }
            int numberOfDecimalPrecision = decimalPrecision.HasValue ? decimalPrecision.Value : 0;

            bool validDecimal = decimal.TryParse(mystring, out val);
            if (!validDecimal)
            {
                return false;
            }
            var index = mystring.LastIndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            if (index == -1)
            {
                return true;
            }
            string endString = mystring.Substring(index);
            // remove any trailing zeros.
            while (endString.EndsWith("0"))
            {
                endString = endString.Substring(0, endString.Length - 1);
            }
            return endString.Length < numberOfDecimalPrecision + 2;
        }

        /// <summary>
        /// Check if imported cost string has a valid decimal precision.
        /// </summary>
        /// <param name="mystring">Imported cost string.</param>
        /// <param name="costDecimalPrecision">Cost Decimal Precision for the workspace.</param>
        /// <param name="val">Value parsed</param>
        /// <returns>False if couldn't parse decimal or the number is not within cost precision.</returns>
        public static bool IsMaxDecimalPlacesForCost(string mystring, int costDecimalPrecision, out Decimal val)
        {
            if (mystring == null)
            {
                throw new ArgumentNullException(nameof(mystring));
            }
            bool validCost = false;

            bool validDecimal = decimal.TryParse(mystring, out val);
            if (!validDecimal)
            {
                validCost = false;
            }
            else
            {
                int index = mystring.LastIndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                if (index == -1)
                {
                    // There are no decimal places in the string so it is good no matter the precision.
                    validCost = true;
                }
                else
                {
                    // Get the string from the decimal place.
                    string endString = mystring.Substring(index);
                    // remove any trailing zeros.
                    while (endString.EndsWith("0"))
                    {
                        endString = endString.Substring(0, endString.Length - 1);
                    }
                    if (endString.Length - 1 <= costDecimalPrecision)
                    {
                        validCost = true;
                    }
                }
            }

            return validCost;
        }

        /// <summary>
        /// Verifies a cost amount is within the min/max cost allowed in genBOE.
        /// </summary>
        /// <param name="cost">Cost</param>
        /// <returns>true if the cost amount falls within the limits allowed</returns>
        public static bool isCostWithinLimits(decimal? cost)
        {
            if (cost < ValidationConstants.RESOURCE_SPREAD_COST_LOW_LIMIT || cost > ValidationConstants.RESOURCE_SPREAD_COST_HIGH_LIMIT)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Verifies an hours amount is within the min/max hours allowed in genBOE.
        /// </summary>
        /// <param name="hours">Hours</param>
        /// <returns>true if the hours amount falls within the limits allowed</returns>
        public static bool isHoursWithinLimits(decimal? hours)
        {
            if (hours < ValidationConstants.RESOURCE_SPREAD_HOURS_LOW_LIMIT || hours > ValidationConstants.RESOURCE_SPREAD_HOURS_HIGH_LIMIT)
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
