/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Microsoft.AspNetCore.Mvc.Rendering;

	/// <summary>
	/// Extension Methods
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Create static Regex object for Indices.
		/// </summary>
		private static readonly Regex regexIndices = new("\\[\\d+\\]", RegexOptions.None, Constants.REGEX_TIMEOUT);

		/// <summary>
		/// Create static Regex object for CarriageReturns.
		/// </summary>
		private static readonly Regex regexCarriageReturns = new(@"\n+", RegexOptions.None, Constants.REGEX_TIMEOUT);

		/// <summary>
		/// Returns true if the value is equal after both ToLower, and Trim
		/// </summary>
		/// <param name="str">The item being tested</param>
		/// <param name="value">The comparison value</param>
		/// <returns>True, if the items are equivalent; false, if not</returns>
		public static bool IsEquivalentTo(this string str, string value)
		{
			if ((string.IsNullOrWhiteSpace(str) && string.IsNullOrWhiteSpace(value)) ||
				(str != null && value != null && str.Trim().ToLower() == value.Trim().ToLower()))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether [is not equivalent to] [the specified value].
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///   <c>true</c> if [is not equivalent to] [the specified value]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNotEquivalentTo(this string str, string value)
		{
			return !IsEquivalentTo(str, value);
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
		/// Truncates the specified maximum length.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="maxLength">The maximum length.</param>
		/// <returns></returns>
		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) { return value; }
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		/// <summary>
		/// Adds the range.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="destination">The destination.</param>
		/// <param name="source">The source.</param>
		public static void AddRange<T>(this ICollection<T> destination,
			IEnumerable<T> source)
		{
			if (source != null && destination != null)
			{
				foreach (T item in source)
				{
					destination.Add(item);
				}
			}
		}

		#region Date/Time methods

		/// <summary>
		/// To the month string.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static string ToMonthString(this DateTime date)
		{
			return date.ToString("MM/yyyy");
		}

		/// <summary>
		/// Determines whether [is in range] [the specified dt start].
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="dtStart">The dt start.</param>
		/// <param name="dtEnd">The dt end.</param>
		/// <returns>
		///   <c>true</c> if [is in range] [the specified dt start]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInRange(this DateTime dt, DateTime dtStart, DateTime dtEnd)
		{
			return dt.CompareTo(dtStart) >= 0 && dt.CompareTo(dtEnd) <= 0;
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
				throw new ArgumentNullException(nameof(date));
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


			if (!DateTime.TryParseExact(date, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
			{
				throw new FormatException(string.Format("Date must be valid and in the format {0}.", format));
			}

			return result;
		}

		/// <summary>
		/// Converts a string in the MM/yyyy format to a DateTime set to the 15th of the month
		/// </summary>
		/// <param name="date">date string</param>
		/// <param name="format"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.FormatException"></exception>
		public static DateTime ToDateTimeMidMonth(this string date, string format = "MM/yyyy")
		{
			if (date == null)
			{
				throw new ArgumentNullException(nameof(date));
			}

			// append leading 0 in case the date is missing it.
			if (date.Length == 6)
			{
				date = "0" + date;
			}

			// set the time to the 15th of the month at noon (12PM)
			DateTime dateFirstOfMonth = DateTime.ParseExact(date, format, CultureInfo.CurrentCulture).AddDays(14).AddHours(12);

			return dateFirstOfMonth;
		}

		/// <summary>
		/// Provides the month difference between the start and end dates. Will return a negative value if you put in the dates the wrong way.
		/// </summary>
		/// <param name="startDate">Start Date</param>
		/// <param name="endDate">End Date</param>
		/// <returns>Month difference</returns>
		public static int MonthDifference(this DateTime startDate, DateTime endDate)
		{
			int startYear = startDate.Year;
			int endYear = endDate.Year;

			int numberOfMonths = 12 * (endYear - startYear);
			numberOfMonths += endDate.Month - startDate.Month;

			return numberOfMonths;
		}

		#endregion Date/Time methods

		/// <summary>
		/// Gets the index.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="occurrence">The occurrence.</param>
		/// <returns></returns>
		public static int GetIndex(this string value, int occurrence = 1)
		{
			int index = -1;  // no match found

			IList<int> indices = value.GetIndexes();
			if (indices.Count >= occurrence)
			{
				index = indices[occurrence - 1];
			}

			return index;
		}

		/// <summary>
		/// Gets the indexes.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static IList<int> GetIndexes(this string value)
		{
			MatchCollection matches = regexIndices.Matches(value);
			IList<int> results = new List<int>(matches.Count);
			foreach (Match match in matches)
			{
				string indexString = match.Value.TrimStart('[').TrimEnd(']');
				results.Add(int.Parse(indexString));
			}
			return results;
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
		/// Orders a list by this after an order by..
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="order">The order.</param>
		/// <returns></returns>
		public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortOrder order)
		{
			IOrderedEnumerable<TSource> toReturn;

			if (order == SortOrder.Descending)
			{
				toReturn = source.ThenByDescending(keySelector);
			}
			else
			{
				toReturn = source.ThenBy(keySelector);
			}

			return toReturn;
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
		private static IEnumerable<T> PropertyNameSort<T>(IEnumerable<T> inAllItems, SortOrder inSortOrder, string inSortField)
		{
			List<T> toReturn = new();

			if (inAllItems != null)
			{
				toReturn = inAllItems.ToList();

				if (toReturn.Any())
				{
					PropertyInfo firstItemProperty = typeof(T).GetProperty(inSortField);

					if (firstItemProperty != null)
					{
						//// Uncomment to add support for types that don't inherit IComparable
						////if (firstItemProperty.PropertyType == typeof(NonComparableTypeHere))
						////{
						////  Custom Sort Delegate Here
						////}
						////*else*/
						if (typeof(IComparable).IsAssignableFrom(firstItemProperty.PropertyType))
						{
							toReturn.Sort(delegate (T left, T right)
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
		/// Converts a generic list to a generic collection
		/// </summary>
		/// <param name="list">The list to convert</param>
		/// <returns>A collection containing the items contained in the passed in list</returns>
		public static Collection<T> ToCollection<T>(this List<T> list)
		{
			Collection<T> toReturn = new();
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list), "ToCollection conversion");
			}

			foreach (T t in list)
			{
				toReturn.Add(t);
			}
			return toReturn;
		}

		/// <summary>
		/// Converts an IEnumerable T to a generic collection
		/// </summary>
		/// <param name="items">The IEnumerable to convert</param>
		/// <returns>A collection containing the items contained in the passed in list</returns>
		public static Collection<T> ToCollection<T>(this IEnumerable<T> items)
		{
			Collection<T> toReturn = new();
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items), "ToCollection conversion");
			}

			foreach (T t in items)
			{
				toReturn.Add(t);
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
				DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
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
			Type type = typeof(T);
			if (!type.IsEnum)
			{
				throw new InvalidOperationException();
			}

			foreach (FieldInfo field in type.GetFields())
			{
				if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
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

			throw new ArgumentException("Not found.", nameof(description));
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
				throw new ArgumentNullException(nameof(fullyQualifiedGroupName));
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
				throw new ArgumentNullException(nameof(fullyQualifiedGroupName));
			}

			int stop = fullyQualifiedGroupName.IndexOf("\\");
			return fullyQualifiedGroupName.Substring(stop + 1);
		}

		/// <summary>
		/// Get the name property for the enumerated value
		/// </summary>
		/// <param name="value">Enumerated value</param>
		/// <returns>Name</returns>
		public static string GetName(this Enum value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Enum.GetName(value.GetType(), value).ToString();
		}

		/// <summary>
		/// Checks if the string can be converted to DateTime
		/// </summary>
		/// <param name="date">string date</param>
		/// <returns>true if string can be date, false if not</returns>
		public static bool IsValidDate(this string date)
		{
			bool isValidDate = DateTime.TryParse(date, out _);
			return isValidDate;

		}

		/// <summary>
		/// To the description.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public static string ToDescription<T>(this T source)
		{
			FieldInfo fi = source.GetType().GetField(source.ToString());

			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
				typeof(DescriptionAttribute), false);

			if (attributes != null && attributes.Length > 0)
			{
				return attributes[0].Description;
			}
			else
			{
				return source.ToString();
			}
		}

		/// <summary>
		/// Extract the inner-most exception from an exception and build a display-ready string of the message and stack trace for it.
		/// </summary>
		/// <param name="ex">The exception</param>
		/// <returns>String for display</returns>
		public static string ToDisplayString(this Exception ex)
		{
			Exception x = ex;
			while (x.InnerException != null)
			{
				x = x.InnerException;
			}

			string display =
				"Message : " + (x.Message ?? "No Message") + "\n" +
				"Stack : " + (x.StackTrace ?? "No StackTrace");

			return display;
		}

		#region Enumerated types

		/// <summary>
		/// Get all values for an enumerated type.
		/// </summary>
		/// <typeparam name="T">Enumerated type</typeparam>
		/// <returns>Strongly-typed collection of enum values</returns>
		public static ICollection<T> GetEnumValues<T>() where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			if (enumType.BaseType != typeof(Enum))
			{
				throw new ArgumentException("T must be of type System.Enum");
			}

			return new List<T>(Enum.GetValues(enumType) as IEnumerable<T>);
		}

		/// <summary>
		/// Gets the select items for an HTML Dropdown.
		/// </summary>
		/// <typeparam name="T">An enumeration type</typeparam>
		/// <returns>Collection of items.</returns>
		public static Collection<SelectListItem> GetSelectItems<T>() where T : struct, IComparable, IFormattable, IConvertible
		{
			Collection<SelectListItem> items = new();
			foreach (T propType in ExtensionMethods.GetEnumValues<T>())
			{
				items.Add(new SelectListItem() { Value = propType.ToString(), Text = propType.GetDescription() });
			}

			return items;
		}

		/// <summary>
		/// Returns the integer value of the target enum, or null if the target enum is one of the values specified
		/// </summary>
		/// <typeparam name="T">The enumerated type</typeparam>
		/// <param name="value">The target enum</param>
		/// <param name="nullValues">Optional list of enum values that will yield a null integer value</param>
		/// <returns>Integer equivalent value of the enum, or null</returns>
		public static int? GetNullableIntValue<T>(this T value, params T[] nullValues) where T : struct, IComparable, IFormattable, IConvertible
		{
			int? intValue;

			Type enumType = typeof(T);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("T must be of type System.Enum");
			}
			else if (nullValues != null && nullValues.Length > 0 && nullValues.Contains(value))
			{
				intValue = null;
			}
			else
			{
				intValue = Convert.ToInt32(value);
			}

			return intValue;
		}

		/// <summary>
		/// Conversion from string to enumerated type.
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="value">The string value to be converted</param>
		/// <param name="defaultValue">Optional default value to be assigned if conversion fails</param>
		/// <returns>Enumerated value</returns>
		public static T GetEnumeratedValue<T>(this string value, T? defaultValue) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T enumValue;

			if (!enumType.IsEnum || string.IsNullOrEmpty(value))
			{
				enumValue = defaultValue ?? default;
			}
			else if (Enum.IsDefined(enumType, value))
			{
				enumValue = (T)Enum.Parse(typeof(T), value);
			}
			else
			{
				T? enumValueNullable = value.GetEnumeratedValueFromDescription<T>();

				if (enumValueNullable.HasValue)
				{
					enumValue = enumValueNullable.Value;
				}
				else if (defaultValue.HasValue)
				{
					enumValue = defaultValue.Value;
				}
				else
				{
					enumValue = default;
				}
			}

			return enumValue;
		}

		/// <summary>
		/// Conversion from integer to enumerated type.
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="value">The nullable integer value to be converted</param>
		/// <param name="defaultValue">Optional default value to be assigned if conversion fails</param>
		/// <returns>Enumerated value</returns>
		public static T GetEnumeratedValue<T>(this int? value, T? defaultValue) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T enumValue;

			if (!enumType.IsEnum)
			{
				enumValue = defaultValue ?? default;
			}
			else if (!value.HasValue)
			{
				enumValue = defaultValue ?? default;
			}
			else
			{
				object enumObject = Enum.ToObject(enumType, value.Value);

				if (Enum.IsDefined(enumType, enumObject))
				{
					enumValue = (T)enumObject;
				}
				else
				{
					enumValue = defaultValue ?? default;
				}
			}

			return enumValue;
		}

		/// <summary>
		/// Conversion from integer to enumerated type.
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="value">The integer value to be converted</param>
		/// <param name="defaultValue">Optional default value to be assigned if conversion fails</param>
		/// <returns>Enumerated value</returns>
		public static T GetEnumeratedValue<T>(this int value, T? defaultValue) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T enumValue;

			if (!enumType.IsEnum)
			{
				enumValue = defaultValue ?? default;
			}
			else
			{
				object enumObject = Enum.ToObject(enumType, value);

				if (Enum.IsDefined(enumType, enumObject))
				{
					enumValue = (T)enumObject;
				}
				else
				{
					enumValue = defaultValue ?? default;
				}
			}

			return enumValue;
		}

		/// <summary>
		/// Conversion from string description to enumerated type having the corresponding DescriptionAttribute defined
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="description">The description to be converted</param>
		/// <returns>Enumerated value, or null if description unmatched</returns>
		private static T? GetEnumeratedValueFromDescription<T>(this string description) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T? enumValue = null;

			if (enumType.IsEnum && !string.IsNullOrEmpty(description))
			{
				T[] allEnumValues = enumType.GetEnumValues() as T[];

				IDictionary<string, T> descriptionToEnumLookup = new Dictionary<string, T>();
				foreach (T ev in allEnumValues)
				{
					descriptionToEnumLookup[ev.GetDescription()] = ev;
				}

				if (descriptionToEnumLookup.ContainsKey(description))
				{
					enumValue = descriptionToEnumLookup[description];
				}
			}

			return enumValue;
		}

		/// <summary>
		/// Conversion from string to enumerated type.
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="value">The string value to be converted</param>
		/// <returns>Enumerated value</returns>
		public static T? GetEnumeratedValueNullable<T>(this string value) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T? enumValue;

			if (!enumType.IsEnum || string.IsNullOrEmpty(value))
			{
				enumValue = null;
			}
			else if (Enum.IsDefined(enumType, value))
			{
				enumValue = (T)Enum.Parse(typeof(T), value);
			}
			else
			{
				T? enumValueNullable = value.GetEnumeratedValueFromDescription<T>();

				if (enumValueNullable.HasValue)
				{
					enumValue = enumValueNullable.Value;
				}
				else
				{
					enumValue = null;
				}
			}

			return enumValue;
		}

		/// <summary>
		/// Conversion from int to enumerated type.
		/// </summary>
		/// <typeparam name="T">The enumerated type to convert to</typeparam>
		/// <param name="value">The int value to be converted</param>
		/// <returns>Enumerated value, or null if unmatched</returns>
		public static T? GetEnumeratedValueNullable<T>(this int value) where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);

			T? enumValue = null;

			if (enumType.IsEnum)
			{
				object enumObject = Enum.ToObject(enumType, value);

				if (Enum.IsDefined(enumType, enumObject))
				{
					enumValue = (T)enumObject;
				}
			}

			return enumValue;
		}

		#endregion

		/// <summary>
		/// Removes the carriage returns.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns></returns>
		public static string RemoveCarriageReturns(this string str)
		{
			return !string.IsNullOrEmpty(str)
				? regexCarriageReturns.Replace(str, " ")
				: string.Empty;
		}

		/// <summary>
		/// Opposite of Linq Any() operator
		/// </summary>
		/// <typeparam name="TSource">Type contained in source collection</typeparam>
		/// <param name="source">Source collection to check</param>
		/// <returns>true if a collection is empty</returns>
		public static bool None<TSource>(this IEnumerable<TSource> source)
		{
			return !source.Any();
		}
	}
}
