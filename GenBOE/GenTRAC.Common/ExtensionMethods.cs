// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Extension methods for general use
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns true if the value is equal after both ToLower, and Trim
        /// </summary>
        /// <param name="str">The item being tested</param>
        /// <param name="value">The comparison value</param>
        /// <returns>Ture, if the items are equivalent; false, if not</returns>
        public static bool IsEquivalentTo(this string str, string value)
        {
            if (string.IsNullOrWhiteSpace(str) && string.IsNullOrWhiteSpace(value) ||
                (str != null && value != null && str.Trim().ToLower() == value.Trim().ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the value is contained in the string after both ToLower and Trim
        /// </summary>
        /// <param name="str">The item being tested</param>
        /// <param name="value">The comparison value</param>
        /// <returns>True, if the value is contained in the string; false, if not</returns>
        public static bool ContainsEquivalent(this string str, string value)
        {
            if (str != null && value != null && str.Trim().ToLower().Contains(value.Trim().ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a string in the MM/yyyy format to a DateTime
        /// </summary>
        /// <param name="date">String representation of a date</param>
        /// <param name="format">Format string</param>
        /// <returns>Date object, derived from string value</returns>
        public static DateTime ToDateTime(this string date, string format = "MM/yyyy")
        {
            if (date == null)
            {
                throw new ArgumentNullException("date");
            }

            // split the date
            string[] dateArray = date.Split('/');

            if (format == "MM/yyyy")
            {
                if (dateArray.Length < 2)
                {
                    throw new FormatException(string.Format("Date must be valid and in the format {0}.", format));
                }

                if (dateArray[0].Length == 1)
                {
                    dateArray[0] = "0" + dateArray[0];
                }

                if (dateArray[1].Length == 2)
                {
                    dateArray[1] = "20" + dateArray[1];
                }

                date = string.Join("/", dateArray);
            }
            else if (format == "MM/dd/yyyy")
            {
                if (dateArray.Length < 3)
                {
                    throw new FormatException(string.Format("Date must be valid and in the format {0}.", format));
                }

                if (dateArray[0].Length == 1)
                {
                    dateArray[0] = "0" + dateArray[0];
                }

                if (dateArray[1].Length == 1)
                {
                    dateArray[1] = "0" + dateArray[1];
                }

                if (dateArray[2].Length == 2)
                {
                    dateArray[2] = "20" + dateArray[2];
                }

                date = string.Join("/", dateArray);
            }

            DateTime result;

            if (!DateTime.TryParseExact(date, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            {
                throw new FormatException(string.Format("Date must be valid and in the format {0}.", format));
            }

            return result;
        }

        /// <summary>
        /// Sort a list
        /// </summary>
        /// <typeparam name="TSource">Type of the list being sorted</typeparam>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <param name="source">The list being sorted</param>
        /// <param name="keySelector">Function to determine what key to use</param>
        /// <param name="order">Sort order</param>
        /// <returns>Sorted list</returns>
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortOrder order)
        {
            IOrderedEnumerable<TSource> toReturn;

            if (order == SortOrder.Descending)
            {
                toReturn = source.OrderByDescending(keySelector);
            }
            else
            {
                toReturn = source.OrderBy(keySelector);
            }

            return toReturn;
        }

        /// <summary>
        /// Sort a list by the given property
        /// </summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="inAllItems">List to sort</param>
        /// <param name="inSortOrder">Sort order</param>
        /// <param name="inSortField">Property to sort by</param>
        /// <returns>Sorted list</returns>
        public static IEnumerable<T> PropertyNameSort<T>(this IEnumerable<T> inAllItems, SortOrder inSortOrder, string inSortField)
        {
            return _PropertyNameSort(inAllItems, inSortOrder, inSortField);
        }

        /// <summary>
        /// Sort a list (in ascending order) by the given property
        /// </summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="inAllItems">List to sort</param>
        /// <param name="inSortField">Property to sort by</param>
        /// <returns>Sorted list</returns>
        public static IEnumerable<T> PropertyNameSort<T>(this IEnumerable<T> inAllItems, string inSortField)
        {
            return PropertyNameSort(inAllItems, SortOrder.Ascending, inSortField);
        }

        /// <summary>
        /// Sort a list by the given property
        /// </summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="inAllItems">List to sort</param>
        /// <param name="inSortOrder">Sort order</param>
        /// <param name="inSortField">Property to sort by</param>
        /// <returns>Sorted list</returns>
        private static IEnumerable<T> _PropertyNameSort<T>(IEnumerable<T> inAllItems, SortOrder inSortOrder, string inSortField)
        {
            var toReturn = new List<T>();

            if (inAllItems != null)
            {
                toReturn = inAllItems.ToList();

                if (toReturn.Any())
                {
                    var firstItemProperty = typeof(T).GetProperty(inSortField);

                    if (firstItemProperty != null)
                    {
                        //// Uncomment to add support for types that don't inherit IComparable
                        ////if (firstItemProperty.PropertyType == typeof(NonComparableTypeHere))
                        ////{
                        ////  Custom Sort Delegate Here
                        ////}
                        ////else
                        if (typeof(IComparable).IsAssignableFrom(firstItemProperty.PropertyType))
                        {
                            toReturn.Sort(delegate(T left, T right)
                            {
                                IComparable leftString = firstItemProperty.GetValue(left, null) as IComparable;
                                IComparable rightString = firstItemProperty.GetValue(right, null) as IComparable;

                                return leftString.CompareTo(rightString);
                            });
                        }
                    }

                    if (inSortOrder == SortOrder.Descending)
                    {
                        toReturn.Reverse();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Return the value of the <code>Description</code> attribute for an enumerated value
        /// </summary>
        /// <typeparam name="TEnum">Type of the enumerated value</typeparam>
        /// <param name="value">The enumerated value</param>
        /// <returns>Attribute value</returns>
        public static string GetDescription<TEnum>(this TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// Returns the enum value that has "description" for its description field
        /// </summary>
        /// <typeparam name="T">Type of enumberated value.</typeparam>
        /// <param name="description">The description value.</param>
        /// <returns>The enum value.</returns>
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException();
            }

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            throw new ArgumentException("Not found.", "description");
        }

        /// <summary>
        /// Return the value of the <code>IsActive</code> attribute for an enumerated value
        /// </summary>
        /// <typeparam name="TEnum">Type of the enumerated value</typeparam>
        /// <param name="value">The enumerated value</param>
        /// <returns>Attribute value</returns>
        public static bool IsActive<TEnum>(this TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                var attributes = (IsActiveAttribute[])fi.GetCustomAttributes(typeof(IsActiveAttribute), false);
                if (attributes.Length > 0)
                {
                    return attributes[0].IsActive;
                }
            }

            // no attribute found, return default value
            return true;
        }

        /// <summary>
        /// Return the value of the <code>ContractTypes</code> attribute for an enumerated value
        /// </summary>
        /// <param name="value">The enumerated value</param>
        /// <returns>Attribute value</returns>
        public static ICollection<ContractType> GetContractTypes(this ContractTypeGroup value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                var attributes = (ContractTypesAttribute[])fi.GetCustomAttributes(typeof(ContractTypesAttribute), false);
                if (attributes.Length > 0)
                {
                    return attributes[0].ContractTypes;
                }
            }

            // no attribute found, return default value
            return new Collection<ContractType>();
        }

        /// <summary>
        /// Returns the domain of an AD object in {domain}\{object name} notation
        /// </summary>
        /// <param name="fullyQualifiedGroupName">AD object name in {domain}\{object name} notation</param>
        /// <returns>domain from {domain}\{object name}</returns>
        public static string GetDomain(this string fullyQualifiedGroupName)
        {
            if (fullyQualifiedGroupName == null)
            {
                throw new ArgumentNullException("fullyQualifiedGroupName");
            }

            int stop = fullyQualifiedGroupName.IndexOf("\\");
            return (stop > -1) ? fullyQualifiedGroupName.Substring(0, stop) : string.Empty;
        }

        /// <summary>
        /// Returns the object name of an AD object in {domain}\{object name} notation
        /// </summary>
        /// <param name="fullyQualifiedGroupName">AD object name in {domain}\{object name} notation</param>
        /// <returns>object name from {domain}\{object name}</returns>
        public static string GetObjectName(this string fullyQualifiedGroupName)
        {
            if (fullyQualifiedGroupName == null)
            {
                throw new ArgumentNullException("fullyQualifiedGroupName");
            }
            
            int stop = fullyQualifiedGroupName.IndexOf("\\");
            return fullyQualifiedGroupName.Substring(stop + 1);
        }
    }
}
