// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Used for calculating the Spread Curve on a BOE Task Element
    /// </summary>
    public static class SpreadCurve
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static Logger logger = new Logger(typeof(SpreadCurve));

        // spread arrays used in calculations
        // Note: the array index will be the spread curve id - 1
        private static decimal[] SC1 = new decimal[50];
        private static decimal[] SC2 = new decimal[50];
        private static decimal[] SC3 = new decimal[50];
        private static decimal[] SC4 = new decimal[50];

        /// <summary>
        /// Static constructor initializes the class
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SpreadCurve()
        {
            // initialize the spreads. This only needs to be done once since the class is static
            InitializeSpreadArrays();
        }

        /// <summary>
        /// For a given Curve, this function will calculate and spread the values over the amount of time specified
        /// </summary>
        /// <param name="inLaborSpreadRequest">the Labor Spread Request which includes Start/End Date, Curve ID, and the Hours
        /// to Spread</param>
        /// <param name="decimalPlacesAllowed">The number of decimal places allowed.</param>
        /// <returns>collection of each month in between the Start/End Date with the value that has been spread</returns>
        public static Collection<ResourceSpreadDto> CalculateLaborSpreadsBasedOnCurve(LaborSpreadRequest inLaborSpreadRequest, int decimalPlacesAllowed)
        {
            if (inLaborSpreadRequest == null)
            {
                throw new ArgumentNullException(nameof(inLaborSpreadRequest));
            }

            Collection<ResourceSpreadDto> toReturn;
            decimal[] CurveValues;

            try
            {
                if (!inLaborSpreadRequest.CurveID.HasValue)
                {
                    throw new NotImplementedException("No logic yet for null curve selection");
                }
                else if (inLaborSpreadRequest.EndDate >= inLaborSpreadRequest.StartDate)
                {
                    ManipulatedLaborSpreadRequest manipulatedRequest = new ManipulatedLaborSpreadRequest(inLaborSpreadRequest);
                    CurveValues = new decimal[manipulatedRequest.LastMonth];

                    // determine what calculation to use depending on curve ID
                    // Level Curve is Spread Curve 3
                    if (manipulatedRequest.ManipulatedCurve == SpreadCurves.SpreadCurve3 || manipulatedRequest.ManipulatedCurve == SpreadCurves.Level)
                    {
                        CurveValues = SpreadFlat((manipulatedRequest.AmountToSpread / manipulatedRequest.NumOfMonths), CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth);
                        CurveValues = Smooth(manipulatedRequest.AmountToSpread, CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth, decimalPlacesAllowed);
                        if (manipulatedRequest.HourSpread < 0)
                        {
                            CurveValues = ChangeSign(CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth);
                        }
                    }
                    else if (manipulatedRequest.ManipulatedCurve == SpreadCurves.SpreadCurve51)
                    {
                        CurveValues = ConvertBookEndResultToDoubleArray(ComputeCurve51BookEndMonths(manipulatedRequest, decimalPlacesAllowed), manipulatedRequest, decimalPlacesAllowed);
                    }
                    else if (manipulatedRequest.ManipulatedCurve == SpreadCurves.SpreadCurve52)
                    {
                        CurveValues = ConvertBookEndResultToDoubleArray(ComputeCurve52BookEndMonths(manipulatedRequest, decimalPlacesAllowed), manipulatedRequest, decimalPlacesAllowed);
                    }
                    else if (manipulatedRequest.ManipulatedCurve == SpreadCurves.SpreadCurve53)
                    {
                        CurveValues = ConvertBookEndResultToDoubleArray(ComputeCurve53BookEndMonths(manipulatedRequest, decimalPlacesAllowed), manipulatedRequest, decimalPlacesAllowed);
                    }
                    else
                    {
                        CurveValues = ComputeDistribution(manipulatedRequest.AmountToSpread, manipulatedRequest.ManipulatedCurveID, CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth);
                        CurveValues = Smooth(manipulatedRequest.AmountToSpread, CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth, decimalPlacesAllowed);
                        if (manipulatedRequest.HourSpread < 0)
                        {
                            CurveValues = ChangeSign(CurveValues, manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth);
                        }
                    }

                    toReturn = MatchMonthsWithValues(manipulatedRequest, CurveValues, decimalPlacesAllowed);
                }
                else
                {
                    // the date range is invalid so throw an exception
                    throw new GenValidationException("Start date must be before End date");
                }
            }
            catch (GenValidationException ex)
            {
                logger.Warn(ex);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }

            return toReturn;
        }

        /// <summary>
        /// The compute distribution function was created by taking the WBOE Spread Curve C++ function and converting it to C#
        /// This function should be used for spreads 1,2,4-50
        /// </summary>
        /// <param name="inAmountToSpread">the Total amount to spread</param>
        /// <param name="inCurveID">the curve number</param>
        /// <param name="inCurveValues">the spread array</param>
        /// <param name="firstMonth">The first month.</param>
        /// <param name="lastMonth">The last month.</param>
        /// <returns>
        /// array of spread values over the periods
        /// </returns>
        /// <exception cref="System.ArgumentNullException">inCurveValues</exception>
        private static decimal[] ComputeDistribution(decimal inAmountToSpread, int inCurveID, decimal[] inCurveValues, int firstMonth, int lastMonth)
        {
            if (inCurveValues == null)
            {
                throw new ArgumentNullException(nameof(inCurveValues));
            }

            // declare variables
            decimal[] toReturn;

            // Note: The WBOE ComputeDistribution uses Spread Curve ID - 1 
            int curveID = inCurveID > 0 ? inCurveID - 1 : 0;

            // establish spread coefficient
            // Compute Distribution Function - Spread Cost over Time
            decimal S1, S2, S3, S4, S5, S6, S7;
            decimal S24;
            decimal S34;

            S4 = lastMonth - firstMonth;

            decimal QQ = Convert.ToDecimal(Math.Pow((1 / (Convert.ToDouble(SC2[curveID]) + 0.001)), 4.0));
            QQ = ((1 + QQ) / (1 - QQ));

            decimal TT = SC1[curveID] - (SC4[curveID] + 1) * SC3[curveID];

            int sentinel = (int)(SC4[curveID]);

            for (int i = 0; i < sentinel; i++)
            {
                TT += 2 * SC3[curveID];

                if (TT <= 0.5m)
                {
                    S1 = 25 - 70 * TT + 40 * TT * TT;
                }
                else
                {
                    S1 = 5 + 10 * TT - 40 * TT * TT;
                }

                S5 = S1 + 8;
                S2 = 1;
                S3 = 0;

                for (int k = firstMonth; k < lastMonth; k++)
                {
                    if (i == 0)
                    {
                        inCurveValues[k] = 0.0m;     // initialize the month distribution param
                    }

                    S24 = S2 / S4;
                    S34 = S3 / S4;

                    decimal AA = Convert.ToDecimal(Math.Pow(Convert.ToDouble(SC2[curveID]) + 0.001, Convert.ToDouble(4 - S5 * S24 + S1 * S24 * S24)));
                    S6 = QQ / (1 + AA);

                    if (S34 > 0)
                    {
                        AA = Convert.ToDecimal(Math.Pow(Convert.ToDouble(SC2[curveID]) + 0.001, Convert.ToDouble(4 - S5 * S34 + S1 * S34 * S34)));
                    }
                    else
                    {
                        AA = Convert.ToDecimal(Math.Pow(Convert.ToDouble(SC2[curveID]) + 0.001, 4.0));
                    }

                    S7 = QQ / (1 + AA);

                    inCurveValues[k] += (inAmountToSpread / SC4[curveID]) * (S6 - S7);

                    S3++;
                    S2++;
                }
            }

            toReturn = inCurveValues;
            return toReturn;
        }

        /// <summary>
        /// Matches spread months with the calculated values
        /// </summary>
        /// <param name="manipulatedRequest">The <see cref="ManipulatedLaborSpreadRequest" /></param>
        /// <param name="curveValues">The calculated values for each month in the spread request</param>
        /// <param name="numberOfDecimalPlaces">The number of decimal places.</param>
        /// <returns>
        /// The <see cref="ResourceSpreadDto" /> results
        /// </returns>
        private static Collection<ResourceSpreadDto> MatchMonthsWithValues(ManipulatedLaborSpreadRequest manipulatedRequest, decimal[] curveValues, int numberOfDecimalPlaces)
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>();

            for (int y = 0; y <= manipulatedRequest.LastMonth; y++)
            {

                // Note: Since the ComputeDistribution calculation required us to add those extra months before the actual Start Month
                // need to remove those months now so ignore everything before the actual Start Month 
                if (y >= (manipulatedRequest.FirstMonth + 1))
                {
                    ResourceSpreadDto laborSpread = new ResourceSpreadDto();
                    laborSpread.LaborSpreadDate = manipulatedRequest.ListOfMonths[y - (manipulatedRequest.FirstMonth + 1)];
                    laborSpread.LaborSpreadValue = Utilities.AdjustPrecision(Convert.ToDecimal(curveValues[y - 1]), numberOfDecimalPlaces);
                    spreads.Add(laborSpread);
                }
            }

            return spreads;
        }

        /// <summary>
        /// Initialize the spread arrays
        /// </summary>
        private static void InitializeSpreadArrays()
        {
            int i = 0;
            decimal X1;
            decimal X2 = 2.5m;
            decimal X3 = -0.2m;
            decimal X4 = 0;

            for (int j = 0; j < 2; j++)
            {
                X2 -= 2;
                X3 += 0.2m;
                X4 += 1;
                for (int k = 0; k < 5; k++)
                {
                    X1 = 0.2m;
                    X2 += 0.5m;
                    for (int l = 0; l < 5; l++)
                    {
                        X1 += 0.1m;
                        SC1[i] = X1;
                        SC2[i] = X2;
                        SC3[i] = X3;
                        SC4[i] = X4;
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// For spread curve 3, apply a flat distribution
        /// </summary>
        /// <param name="inAmountToSpread">amount to spread</param>
        /// <param name="inCurveValues">spread array</param>
        /// <param name="firstMonth">The first month.</param>
        /// <param name="lastMonth">The last month.</param>
        /// <returns>The spread array.</returns>
        private static decimal[] SpreadFlat(decimal inAmountToSpread, decimal[] inCurveValues, int firstMonth, int lastMonth)
        {
            decimal[] toReturn = inCurveValues;
            decimal resid = 0;
            for (int i = firstMonth; i < lastMonth; i++)
            {
                decimal tmp = (inAmountToSpread + resid) + .5001m;
                resid += inAmountToSpread - tmp;
                toReturn[i] = tmp;
            }

            return toReturn;
        }

        /// <summary>
        /// This function smooths the data in case the full amount to spread isn't spread properly "spread out" in the distribution function
        /// there should be no delta when leaving this function
        /// </summary>
        /// <param name="inAmountToSpread">the amount to spread</param>
        /// <param name="inCurveValues">spread array</param>
        /// <param name="firstMonth">The first month.</param>
        /// <param name="lastMonth">The last month.</param>
        /// <param name="decimalPlacesAllowed">The decimal places allowed.</param>
        /// <returns>
        /// spread array with no delta
        /// </returns>
        public static decimal[] Smooth(decimal inAmountToSpread, decimal[] inCurveValues, int firstMonth, int lastMonth, int decimalPlacesAllowed)
        {
            if (inCurveValues == null)
            {
                throw new ArgumentNullException(nameof(inCurveValues));
            }

            decimal remainder = 0;

            for (int i = firstMonth; i < lastMonth; i++)
            {
                if (inCurveValues[i] < 0)
                {
                    inCurveValues[i] = 0; 
                }

                remainder += inCurveValues[i] - Utilities.AdjustPrecision(inCurveValues[i], decimalPlacesAllowed);
                inCurveValues[i] = Utilities.AdjustPrecision(inCurveValues[i], decimalPlacesAllowed);

                if (remainder >= Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed))
                {
                    inCurveValues[i] = inCurveValues[i] + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
                    remainder = remainder - Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
                }
                else if (remainder <= (-1 * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed)))
                {
                    inCurveValues[i] = inCurveValues[i] - Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
                    remainder = remainder + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
                }
            }

            if (remainder >= (0.5m * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed)))
            {
                inCurveValues[lastMonth - 1] = inCurveValues[lastMonth - 1] + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
            }
            else if (remainder <= (-0.5m * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed)))
            {
                inCurveValues[lastMonth - 1] = inCurveValues[lastMonth - 1] - Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(decimalPlacesAllowed);
            }

            // location of the largest value
            int largest = 0;
            decimal high = 0;
            decimal cum = 0;

            for (int i = firstMonth; i < lastMonth; i++)
            {
                cum += inCurveValues[i];
                if (inCurveValues[i] > high)
                {
                    largest = i;
                    high = inCurveValues[i];
                }
            }

            if (inAmountToSpread - cum != 0)
            {
                inCurveValues[largest] += inAmountToSpread - cum;
            }

            return inCurveValues;
        }

        /// <summary>
        /// If a negative amount to spread was specified, need to convert all month values to negative
        /// </summary>
        /// <param name="inCurveValues">the spread array</param>
        /// <param name="firstMonth">The first month.</param>
        /// <param name="lastMonth">The last month.</param>
        /// <returns>
        /// the spread array with negative values
        /// </returns>
        private static decimal[] ChangeSign(decimal[] inCurveValues, int firstMonth, int lastMonth)
        {
            for (int i = firstMonth; i < lastMonth; i++)
            {
                inCurveValues[i] = inCurveValues[i] * -1;
            }

            return inCurveValues;
        }

        /// <summary>
        /// Computes the first and last month values for curve 51 and prepares the data needed for the second round (curve 3)
        /// </summary>
        /// <param name="manipulatedRequest">The <see cref="ManipulatedLaborSpreadRequest" /></param>
        /// <param name="numberOfDecimalPlaces">The number of decimal places.</param>
        /// <returns>
        /// The <see cref="BookEndCurveFirstRoundResult" /> of the first round caluculation
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// manipulatedRequest - manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve51BookEndMonths when number of months to spread is less than 3.
        /// or
        /// manipulatedRequest - Infinity result after division. Possibly caused by the NumOfMonths property is less than 3.
        /// </exception>
        private static BookEndCurveFirstRoundResult ComputeCurve51BookEndMonths(ManipulatedLaborSpreadRequest manipulatedRequest, int numberOfDecimalPlaces)
        {
            BookEndCurveFirstRoundResult toReturn = new BookEndCurveFirstRoundResult(manipulatedRequest.StartDate.AddMonths(1), manipulatedRequest.EndDate.AddMonths(-1));

            if (manipulatedRequest.NumOfMonths <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(manipulatedRequest), "manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve51BookEndMonths when number of months to spread is less than 3.");
            }

            // 1. (total hours / (# months - 1)) / 2 = x (first/last month)
            // 2. y = (int)x to get truncate the decimal
            // 3. if decimal part of x >= .5 add 1 to y
            // 4. remaining = total hours - (y * 2)

            // compute the value of the book end months
            double x = (Convert.ToDouble(manipulatedRequest.AmountToSpread) / (manipulatedRequest.NumOfMonths - 1)) / 2.0;

            if (double.IsInfinity(x))
            {
                logger.Error("Infinity result after division in SpreadCurve.ComputeCurve51BookEndMonths. Possibly caused by the NumOfMonths property is less than 3.");
                throw new ArgumentOutOfRangeException(nameof(manipulatedRequest), "Infinity result after division. Possibly caused by the NumOfMonths property is less than 3.");
            }

            decimal decX = (manipulatedRequest.AmountToSpread / (manipulatedRequest.NumOfMonths - 1)) / 2;

            // cast to an int to truncate the decimal 
            decimal y = Utilities.AdjustPrecision(decX, numberOfDecimalPlaces);

            // get the decimal part of the division if any
            decimal decimalpart = decX - y;

            if (decimalpart >= (.5m * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces)))
            {
                // add the remainder to the book end months instead of letting it spread in the internal months
                // this simulates a round up instead of a round down
                y = y + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces);
            }

            toReturn.FirstMonthValue = toReturn.LastMonthValue = y;
            toReturn.AmountToSpread = manipulatedRequest.AmountToSpread - (y * 2);

            // set the new spread range
            toReturn.FirstMonth = manipulatedRequest.FirstMonth + 1;
            toReturn.LastMonth = manipulatedRequest.LastMonth - 1;

            return toReturn;
        }

        /// <summary>
        /// Computes the first and last month values for curve 52 and prepares the data needed for the second round (curve 3)
        /// </summary>
        /// <param name="manipulatedRequest">The <see cref="ManipulatedLaborSpreadRequest" /></param>
        /// <param name="numberOfDecimalPlaces">The number of decimal places.</param>
        /// <returns>
        /// The <see cref="BookEndCurveFirstRoundResult" /> of the first round caluculation
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">manipulatedRequest - manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve52BookEndMonths when number of months to spread is less than 3.</exception>
        private static BookEndCurveFirstRoundResult ComputeCurve52BookEndMonths(ManipulatedLaborSpreadRequest manipulatedRequest, int numberOfDecimalPlaces)
        {
            BookEndCurveFirstRoundResult toReturn = new BookEndCurveFirstRoundResult(manipulatedRequest.StartDate.AddMonths(1), manipulatedRequest.EndDate.AddMonths(-1));

            if (manipulatedRequest.NumOfMonths <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(manipulatedRequest), "manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve52BookEndMonths when number of months to spread is less than 3.");
            }

            decimal outx;
            decimal outy;

            ComputeBookEndMonths(manipulatedRequest, out outx, out outy, numberOfDecimalPlaces);

            // assigned the computed values to the correct months property
            toReturn.FirstMonthValue = outx;
            toReturn.LastMonthValue = outy;

            // calc remaining = total hours - total bookend months
            toReturn.AmountToSpread = manipulatedRequest.AmountToSpread - (toReturn.FirstMonthValue + toReturn.LastMonthValue);

            // set the new spread range
            toReturn.FirstMonth = manipulatedRequest.FirstMonth + 1;
            toReturn.LastMonth = manipulatedRequest.LastMonth - 1;

            return toReturn;
        }

        /// <summary>
        /// Computes the first and last month values for curve 53 and prepares the data needed for the second round (curve 3)
        /// </summary>
        /// <param name="manipulatedRequest">The <see cref="ManipulatedLaborSpreadRequest" /></param>
        /// <param name="numberOfDecimalPlaces">The number of decimal places.</param>
        /// <returns>
        /// The <see cref="BookEndCurveFirstRoundResult" /> of the first round caluculation
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">manipulatedRequest - manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve53BookEndMonths when number of months to spread is less than 3.</exception>
        private static BookEndCurveFirstRoundResult ComputeCurve53BookEndMonths(ManipulatedLaborSpreadRequest manipulatedRequest, int numberOfDecimalPlaces)
        {
            BookEndCurveFirstRoundResult toReturn = new BookEndCurveFirstRoundResult(manipulatedRequest.StartDate.AddMonths(1), manipulatedRequest.EndDate.AddMonths(-1));

            if (manipulatedRequest.NumOfMonths <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(manipulatedRequest), "manipulatedRequest.NumOfMonths property is less than 3. Do not call ComputeCurve53BookEndMonths when number of months to spread is less than 3.");
            }

            decimal outx;
            decimal outy;

            ComputeBookEndMonths(manipulatedRequest, out outx, out outy, numberOfDecimalPlaces);

            // assigned the computed values to the correct months property
            toReturn.FirstMonthValue = outy;
            toReturn.LastMonthValue = outx;

            // calc remaining = total hours - total bookend months
            toReturn.AmountToSpread = manipulatedRequest.AmountToSpread - (toReturn.FirstMonthValue + toReturn.LastMonthValue);

            // set the new spread range
            toReturn.FirstMonth = manipulatedRequest.FirstMonth + 1;
            toReturn.LastMonth = manipulatedRequest.LastMonth - 1;

            return toReturn;
        }

        /// <summary>
        /// Computes the book end months.
        /// </summary>
        /// <param name="manipulatedRequest">The manipulated request.</param>
        /// <param name="outx">The outx.</param>
        /// <param name="outy">The outy.</param>
        /// <param name="numberOfDecimalPlaces">The number of decimal places.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">manipulatedRequest - Infinity result after calculation. Possibly caused by the NumOfMonths property is less than 3.</exception>
        private static void ComputeBookEndMonths(ManipulatedLaborSpreadRequest manipulatedRequest, out decimal outx, out decimal outy, int numberOfDecimalPlaces)
        {
            // 1. (total hours / (# months - 1)) = workingvalue
            // 2. workingvalue * 75 = x (book end month a)
            // 3. workingvalue - x = y (book end month b)
            // 4. xintpart = (int)x to get truncate the decimal
            // 5. yintpart = (int)y to get truncate the decimal
            // 6. if decimal part of (x || y) >= .5 add 1 to the respective intpart

            // compute the value of the first month
            double workingvalue = Convert.ToDouble(manipulatedRequest.AmountToSpread) / (manipulatedRequest.NumOfMonths - 1);

            if (double.IsInfinity(workingvalue))
            {
                logger.Error("Infinity result after calculation in SpreadCurve.ComputeBookEndMonths. Possibly caused by the NumOfMonths property is less than 3.");
                throw new ArgumentOutOfRangeException(nameof(manipulatedRequest), "Infinity result after calculation. Possibly caused by the NumOfMonths property is less than 3.");
            }

            decimal workingValueDec = manipulatedRequest.AmountToSpread / (manipulatedRequest.NumOfMonths - 1);
            decimal x = workingValueDec * .75m;
            decimal y = workingValueDec - x;

            // cast to an int to truncate the decimal 
            decimal xPart = Utilities.AdjustPrecision(x, numberOfDecimalPlaces); 
            decimal yPart = Utilities.AdjustPrecision(y, numberOfDecimalPlaces);

            // get the decimal part of the division if any
            decimal xdecimalpart = x - xPart;
            decimal ydecimalpart = y - yPart;

            if (xdecimalpart >= (.5m * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces)))
            {
                // add the remainder to the book end months instead of letting it spread in the internal months
                // this simulates a round up instead of a round down
                xPart = xPart + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces);
            }

            if (ydecimalpart >= (.5m * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces)))
            {
                // add the remainder to the book end months instead of letting it spread in the internal months
                // this simulates a round up instead of a round down
                yPart = yPart + Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(numberOfDecimalPlaces);
            }

            outx = xPart;
            outy = yPart;
        }

        /// <summary>
        /// Converts the bookend result to a double array.
        /// </summary>
        /// <param name="firstRoundResult">The first round result.</param>
        /// <param name="manipulatedRequest">The manipulated request.</param>
        /// <param name="decimalPlacesAllowed">The decimal places allowed.</param>
        /// <returns>A double array for the bookend result.</returns>
        private static decimal[] ConvertBookEndResultToDoubleArray(BookEndCurveFirstRoundResult firstRoundResult, ManipulatedLaborSpreadRequest manipulatedRequest, int decimalPlacesAllowed)
        {
            decimal[] toReturn = new decimal[firstRoundResult.LastMonth];

            toReturn = SpreadFlat((firstRoundResult.AmountToSpread / firstRoundResult.NumOfMonths), toReturn, firstRoundResult.FirstMonth, firstRoundResult.LastMonth);
            toReturn = Smooth(firstRoundResult.AmountToSpread, toReturn, firstRoundResult.FirstMonth, firstRoundResult.LastMonth, decimalPlacesAllowed);

            ArrayList curveValueList = new ArrayList(toReturn);
            curveValueList[manipulatedRequest.FirstMonth] = firstRoundResult.FirstMonthValue;
            curveValueList.Add(firstRoundResult.LastMonthValue);

            if (manipulatedRequest.HourSpread < 0)
            {
                toReturn = ChangeSign((decimal[])curveValueList.ToArray(typeof(decimal)), manipulatedRequest.FirstMonth, manipulatedRequest.LastMonth);
            }
            else
            {
                toReturn = (decimal[])curveValueList.ToArray(typeof(decimal));
            }

            return toReturn;
        }
    }

    /// <summary>
    /// Encapsulates the data needed by the curve algorithms. Converts CurveID and Dates to the format/values expected by the algorithms.
    /// </summary>
    public class ManipulatedLaborSpreadRequest : LaborSpreadRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSpreadRequest">A populated <see cref="LaborSpreadRequest"/></param>
        public ManipulatedLaborSpreadRequest(LaborSpreadRequest inSpreadRequest)
            : base(PassThroughNonNull(inSpreadRequest).CurveID, PassThroughNonNull(inSpreadRequest).StartDate, PassThroughNonNull(inSpreadRequest).EndDate, PassThroughNonNull(inSpreadRequest).HourSpread) 
        {
            if (inSpreadRequest == null)
            {
                throw new ArgumentNullException(nameof(inSpreadRequest), "inSpreadRequest can't be null");
            }

            DateTime tempDate = this.StartDate.Normalize();

            // Normalize to make sure the end date is included in the results
            DateTime endDate = this.EndDate.Normalize();

            // Get the number of months to spread based on Start/End Date
            while (tempDate <= endDate)
            {
                this.ListOfMonths.Add(tempDate);
                tempDate = tempDate.AddMonths(1);
            }

            if (inSpreadRequest.CurveID == SpreadCurves.SpreadCurve51 ||
                inSpreadRequest.CurveID == SpreadCurves.SpreadCurve52 ||
                inSpreadRequest.CurveID == SpreadCurves.SpreadCurve53)
            {
                // if there are only 1 or 2 months to spread just use spread curve 3
                if (this.NumOfMonths <= 2)
                {
                    this.ManipulatedCurve = SpreadCurves.SpreadCurve3;
                    this.ManipulatedCurveID = Convert.ToInt32(SpreadCurves.SpreadCurve3) - 1;
                }
                else
                {
                    this.ManipulatedCurve = this.CurveID.Value;
                    this.ManipulatedCurveID = Convert.ToInt32(this.CurveID.Value) - 3;
                }
            }
            else
            {
                this.ManipulatedCurve = this.CurveID.Value;
                this.ManipulatedCurveID = Convert.ToInt32(this.CurveID.Value) - 1;
            }

            // need to determine if the amount to spread is negative since the spread calculations can only handle positive numbers
            // all calculations need to be done with postive numbers
            this.AmountToSpread = this.HourSpread < 0 ? this.HourSpread * -1 : this.HourSpread;
        }

        /// <summary>
        /// Tests the passed in <see cref="LaborSpreadRequest"/> for null.  If null throws <see cref="ArgumentNullException"/> exception
        /// </summary>
        /// <param name="request">The <see cref="LaborSpreadRequest"/> to test</param>
        /// <returns>The LaborSpreadRequest to test.</returns>
        private static LaborSpreadRequest PassThroughNonNull(LaborSpreadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request;
        }

        /// <summary>
        /// Gets the manipulated curve
        /// </summary>
        public SpreadCurves ManipulatedCurve { get; }

        /// <summary>
        /// Gets the manipulated curve ID
        /// </summary>
        public int ManipulatedCurveID { get; }

        /// <summary>
        /// Gets the list of months based on the Start/End dates
        /// </summary>
        public Collection<DateTime> ListOfMonths { get; } = new Collection<DateTime>();

        /// <summary>
        /// Gets the number of months to spread based on the Start/End dates
        /// </summary>
        public int NumOfMonths
        {
            get
            {
                return this.ListOfMonths.Count;
            }
        }

        /// <summary>
        /// Gets the manipulated first month
        /// WBOE's Spread function specifies the start month to be Jan=0, Feb=1, Dec=11, but in C# the months start as Jan=1, Feb=2..Dec=12
        /// so subtract one to get what the spread function expects
        /// </summary>
        public int FirstMonth
        {
            get
            {
                // Note: WBOE's Spread function specifies the start month to be Jan=0, Feb=1, Dec=11, but in C# the months start as Jan=1, Feb=2..Dec=12
                // so subtract one to get what the spread function expects
                return this.StartDate.Month - 1;
            }
        }

        /// <summary>
        /// Gets the manipulated last month
        /// WBOE's Spread function specifies the start month to be Jan=0, Feb=1, Dec=11, but in C# the months start as Jan=1, Feb=2..Dec=12
        /// so subtract one to get what the spread function expects
        /// </summary>
        public int LastMonth 
        {
            get
            {
                // Note: WBOE's ComputeDistribution function uses the first full year of the spread so it's necessary to add extra months
                // For instance, if the start date is in July, still need to add Jan-Jun to the index of CurveValues for the spread to be correct
                return this.NumOfMonths + this.FirstMonth;
            }
        }

        /// <summary>
        /// Gets the manipulated amount to spread
        /// </summary>
        public decimal AmountToSpread { get; }
    }

    /// <summary>
    /// Encapsulates the result of the first round spread and data needed for the second round
    /// </summary>
    public class BookEndCurveFirstRoundResult : LaborSpreadRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inStartDate">The start <see cref="DateTime"/></param>
        /// <param name="inEndDate">The end <see cref="DateTime"/></param>
        public BookEndCurveFirstRoundResult(DateTime inStartDate, DateTime inEndDate)
        {
            DateTime tempDate = this.StartDate = inStartDate;
            this.EndDate = inEndDate;

            // add a month to make sure the end date is included in the results
            DateTime endDate = inEndDate.AddMonths(1);

            // Get the number of months to spread based on Start/End Date
            while (tempDate < endDate)
            {
                this.ListOfMonths.Add(tempDate);
                tempDate = tempDate.AddMonths(1);
            }
        }

        /// <summary>
        /// Gets/Sets the manipulated first month
        /// WBOE's Spread function specifies the start month to be Jan=0, Feb=1, Dec=11, but in C# the months start as Jan=1, Feb=2..Dec=12
        /// so subtract one to get what the spread function expects
        /// </summary>
        public int FirstMonth { get; set; }

        /// <summary>
        /// Gets/Sets the manipulated last month
        /// WBOE's Spread function specifies the start month to be Jan=0, Feb=1, Dec=11, but in C# the months start as Jan=1, Feb=2..Dec=12
        /// so subtract one to get what the spread function expects
        /// </summary>
        public int LastMonth { get; set; }

        /// <summary>
        /// Gets the number of months to spread based on the Start/End dates
        /// </summary>
        public int NumOfMonths
        {
            get
            {
                return this.ListOfMonths.Count;
            }
        }

        /// <summary>
        /// Gets/Sets the calculated value for the first month of the spread
        /// </summary>
        public decimal FirstMonthValue { get; set; }

        /// <summary>
        /// Gets/Sets the calculated value for the last month of the spread
        /// </summary>
        public decimal LastMonthValue { get; set; }

        /// <summary>
        /// Gets/Sets the calculated value to spread in the second round
        /// </summary>
        public decimal AmountToSpread { get; set; }

        /// <summary>
        /// Gets the list of months based on the Start/End dates
        /// </summary>
        public Collection<DateTime> ListOfMonths { get; } = new Collection<DateTime>();
    }
}
