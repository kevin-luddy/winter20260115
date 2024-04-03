// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Text.Json;
	using System.Text.RegularExpressions;
	using IES.Common.Core.Attributes;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;

	public static class ExtensionMethods
	{
		/// <summary>
		/// Create static Regex object for Indices.
		/// </summary>
		private static readonly Regex regexIndices = new("\\[\\d+\\]", RegexOptions.None, CommonConstants.REGEX_TIMEOUT);

		/// <summary>
		/// Create static Regex object for CarriageReturns.
		/// </summary>
		private static readonly Regex regexCarriageReturns = new(@"[\n\r]+", RegexOptions.None, CommonConstants.REGEX_TIMEOUT);

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

		public static bool IsNotEquivalentTo(this string str, string value)
		{
			return !str.IsEquivalentTo(value);
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

		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) { return value; }
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

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

		public static string ToMonthString(this DateTime date)
		{
			return date.ToString("MM/yyyy");
		}

		public static DateTime Normalize(this DateTime dt)
		{
			return GenBOEUtilities.AdjustDateTimePrecision(dt);
		}

		public static DateTime Normalize(this DateTime dt, DateTimePrecision precision)
		{
			return GenBOEUtilities.AdjustDateTimePrecision(dt, precision);
		}

		public static DateTime? Normalize(this DateTime? dt)
		{
			return dt.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(dt.Value) : null;
		}

		public static DateTime? Normalize(this DateTime? dt, DateTimePrecision precision)
		{
			return dt.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(dt.Value, precision) : null;
		}

		public static bool IsInRange(this DateTime dt, DateTime dtStart, DateTime dtEnd)
		{
			return dt.CompareTo(dtStart) >= 0 && dt.CompareTo(dtEnd) <= 0;
		}

		public static ICollection<DateTime> CreateSpreadDateRange(this DateTime spreadStartDate, DateTime spreadEndDate)
		{
			ICollection<DateTime> spreadDates = new List<DateTime>();

			// display the header (all dates across the spread)
			for (DateTime dt = spreadStartDate; dt <= spreadEndDate; dt = dt.AddMonths(1))
			{
				spreadDates.Add(dt.Normalize());
			}

			return spreadDates;
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
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="FormatException"></exception>
		public static DateTime ToDateTimeMidMonth(this string date, string format = "MM/yyyy")
		{
			_ = date ?? throw new ArgumentNullException(nameof(date));

			// append leading 0 in case the date is missing it.
			if (date.Length == 6)
			{
				date = "0" + date;
			}

			// set the time to the 15th of the month at noon (12PM)
			DateTime dateFirstOfMonth = DateTime.TryParseExact(date, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateFirstOfMonth) ? dateFirstOfMonth.AddDays(14).AddHours(12)
				: throw new FormatException($"Date must be valid and in the format {format}. The entered string was {date}.");

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

		/// <summary>
		/// Provides the month (30 day) difference between start and end dates with decimal precision, taking days into account
		/// </summary>
		/// <param name="startDate">Start Date</param>
		/// <param name="endDate">End Date</param>
		/// <returns>Month difference</returns>
		public static decimal MonthDifferenceDecimal(this DateTime? startDate, DateTime? endDate)
		{
			if (startDate == null || endDate == null)
			{
				return 0;
			}

			return endDate.Value.Subtract(startDate.Value).Days / CommonConstants.POP_MONTHS_DIVISOR;
		}

		#endregion Date/Time methods

		public static int GetIndex(this string value, int occurence = 1)
		{
			int index = -1;  // no match found

			IList<int> indices = value.GetIndexes();
			if (indices.Count >= occurence)
			{
				index = indices[occurence - 1];
			}

			return index;
		}

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
		/// Sort a list by the given property
		/// </summary>
		/// <typeparam name="T">Type of list elements</typeparam>
		/// <param name="inAllItems">List to sort</param>
		/// <param name="inSortOrder">Sort order</param>
		/// <param name="inSortField">Property to sort by</param>
		/// <returns>Sorted list</returns>
		public static IEnumerable<T> PropertyNameSort<T>(this IEnumerable<T> inAllItems, SortOrder inSortOrder, string inSortField)
		{
			List<T> toReturn = new();

			if (inAllItems != null)
			{
				toReturn = inAllItems.ToList();

				if (toReturn.Count > 0)
				{
					PropertyInfo firstItemProperty = typeof(T).GetProperty(inSortField);

					if (firstItemProperty != null)
					{
						// Uncomment to add support for types that don't inherit IComparable
						//if (firstItemProperty.PropertyType == typeof(NonComparableTypeHere))
						//{
						//  Custom Sort Delegate Here
						//}
						/*else*/
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
		/// Sort a list (in ascending order) by the given property
		/// </summary>
		/// <typeparam name="T">Type of list elements</typeparam>
		/// <param name="inAllItems">List to sort</param>
		/// <param name="inSortField">Property to sort by</param>
		/// <returns>Sorted list</returns>
		public static IEnumerable<T> PropertyNameSort<T>(this IEnumerable<T> inAllItems, string inSortField)
		{
			return inAllItems.PropertyNameSort(SortOrder.Ascending, inSortField);
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
		/// Converts an IEnumerable<T> to a generic collection
		/// </summary>
		/// <param name="list">The IEnumerable to convert</param>
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
		/// Makes a deep copy of a Serializable object using the <see cref="JsonSerializer"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The object to deep copy</param>
		/// <returns>The copied object</returns>
		public static T DeepClone<T>(this T source) where T: class 
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", nameof(source));
			}

			// Don't serialize a null object, simply return the default for that object
			if (ReferenceEquals(source, null))
			{
				return default;
			}

			Type S = source.GetType();

			if (S == typeof(object))
			{
				throw new NotSupportedException();
			}

			string serializedObject = JsonSerializer.Serialize(source, S);
			object deserializedObject = JsonSerializer.Deserialize(serializedObject, S);
			return deserializedObject as T;
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
				DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
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

			throw new ArgumentException("Not found.", nameof(description));
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
				IsActiveAttribute[] attributes = (IsActiveAttribute[])fi.GetCustomAttributes(typeof(IsActiveAttribute), false);
				if (attributes.Length > 0)
				{
					return attributes[0].IsActive;
				}
			}

			// no attribute found, return default value
			return true;
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
			return stop > -1 ? fullyQualifiedGroupName.Substring(0, stop) : string.Empty;
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
		/// Gets the options for an HTML Select.
		/// </summary>
		/// <typeparam name="T">An enumeration type</typeparam>
		/// <returns>Collection of options.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static Collection<OptionModelView> GetOptions<T>() where T : struct, IComparable, IFormattable, IConvertible
		{
			Collection<OptionModelView> options = new();
			foreach (T propType in GetEnumValues<T>())
			{
				options.Add(new OptionModelView() { Id = Convert.ToInt32(propType), Label = propType.GetDescription() });
			}

			return options;
		}

		/// <summary>
		/// Gets the select items for an HTML Dropdown.
		/// </summary>
		/// <typeparam name="T">An enumeration type</typeparam>
		/// <returns>Collection of items.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static Collection<SelectListItem> GetSelectItems<T>() where T : struct, IComparable, IFormattable, IConvertible
		{
			Collection<SelectListItem> items = new();
			foreach (T propType in GetEnumValues<T>())
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

		public static string GetResourceTypeCategory(this ElementOfCostType elementOfCost)
		{
			string resourceTypeCategory;

			switch (elementOfCost)
			{
				case ElementOfCostType.IWTA:
					resourceTypeCategory = "IWTA";
					break;
				case ElementOfCostType.Sub:
					resourceTypeCategory = "Subcontractor";
					break;
				case ElementOfCostType.Materials:
					resourceTypeCategory = "Materials";
					break;
				case ElementOfCostType.ODC:
					resourceTypeCategory = "ODC";
					break;
				case ElementOfCostType.Travel:
					resourceTypeCategory = "Travel";
					break;
				case ElementOfCostType.LMLabor:
				default:
					resourceTypeCategory = null;
					break;
			}

			return resourceTypeCategory;
		}

		#endregion

		public static string RemoveCarriageReturns(this string str)
		{
			return !string.IsNullOrEmpty(str)
				? regexCarriageReturns.Replace(str, " ")
				: string.Empty;
		}

		/// <summary>
		/// Removes illegal excel characters from a string
		/// </summary>
		/// <param name="str">The string to work on</param>
		/// <returns>The original string minus the illegel excel characters</returns>
		public static string RemoveIllegalExcelCharacters(this string str)
		{
			// TODO Holding off on replacement for now, just returning string or empty if null
			return str ?? string.Empty;
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

		/// <summary>
		/// Replace only the first instance of a string instead of every instance like in string.Replace
		/// based on https://stackoverflow.com/questions/141045/how-do-i-replace-the-first-instance-of-a-string-in-net
		/// </summary>
		/// <param name="text">original string</param>
		/// <param name="search">text to be replaced</param>
		/// <param name="replace">text to replace it with</param>
		/// <returns>Text with only first instance of 'search' replaced</returns>
		public static string ReplaceFirst(this string text, string search, string replace)
		{
			_ = text ?? throw new ArgumentNullException(nameof(text));
			_ = search ?? throw new ArgumentNullException(nameof(search));

			int i = text.IndexOf(search);

			if (i < 0)
			{
				return text;
			}

			return $"{text.Substring(0, i)}{replace}{text.Substring(i + search.Length)}";
		}

		/// <summary>
		/// Batches the specified the list into batches of max size.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="items">The items.</param>
		/// <param name="maxItems">The maximum size for a batch of items.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
		{
			return items.Select((item, inx) => new { item, inx })
					.GroupBy(x => x.inx / maxItems)
					.Select(g => g.Select(x => x.item));
		}
	}
}
