// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common
{
    using System;

    /// <summary>
    /// Set of general utility methods for enumerated types
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        /// Perform a comparison of two enumerated values.  Supports enum-type ordering when the underlying integer values
        /// of the enumeration DO NOT reflect their "intended" order (e.g. when they map to primary keys).  The developer
        /// can specify his/her OWN value-determination logic.
        /// </summary>
        /// <typeparam name="T">Enumerated type</typeparam>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>0 if values are equal; 1 if a &gt; b; -1 if a &lt; b</returns>
        public static int Compare<T>(T a, T b) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type enumType = typeof(T);

            if (enumType.BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }

            int comparison = 0;

            int aval = Convert.ToInt32(a);
            int bval = Convert.ToInt32(b);

            if (enumType == typeof(Role))
            {
                Role arole = (Role)aval;
                Role brole = (Role)bval;

                if (arole == brole)
                {
                    comparison = 0;
                }
                else
                {
                    switch (arole)
                    {
                        case Role.Admin:
                            comparison = 1;
                            break;

                        case Role.Pricer:
                            comparison = (brole == Role.Admin) ? -1 : 1;
                            break;
                        case Role.PeerReviewer:
                        case Role.ProposalSetupAdmin:
                            comparison = (brole == Role.Admin || brole == Role.Pricer) ? -1 : 1;
                            break;
                        default:
                            comparison = -1;
                            break;
                    }
                }
            }
            else
            {
                if (aval == bval)
                {
                    comparison = 0;
                }
                else if (aval > bval)
                {
                    comparison = 1;
                }
                else
                {
                    comparison = -1;
                }
            }

            return comparison;
        }
    }
}
