// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

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

			int aval = Convert.ToInt32(a);
			int bval = Convert.ToInt32(b);


			int comparison;
			if (enumType == typeof(PtmRole))
			{
				PtmRole arole = (PtmRole)aval;
				PtmRole brole = (PtmRole)bval;

				if (arole == brole)
				{
					comparison = 0;
				}
				else
				{
					switch (arole)
					{
						case PtmRole.Admin:
							comparison = 1;
							break;

						case PtmRole.Pricer:
							comparison = brole == PtmRole.Admin ? -1 : 1;
							break;
						case PtmRole.PeerReviewer:
						case PtmRole.ProposalSetupAdmin:
							comparison = brole is PtmRole.Admin or PtmRole.Pricer ? -1 : 1;
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

		/// <summary>
		/// Populate list items from Enum
		/// </summary>
		/// <param name="enumType">Enum type</param>
		/// <param name="required">Whether the field is required</param>
		/// <param name="selectedValue">The value that is selected, if applicable.</param>
		/// <returns>List of options for select box.  If there is a selected value provided that is not in the list, it is added.</returns>
		public static ICollection<SelectListItem> GetListItemsForEnum(Type enumType, bool required = true, string selectedValue = "")
		{
			// Attempt to parse an item from the Enum based upon the selectedValue string.
			object selectedItem = null;
			if (!string.IsNullOrEmpty(selectedValue))
			{
				try
				{
					selectedItem = Enum.Parse(enumType, selectedValue);
				}
				catch (ArgumentException)
				{
					// Do nothing.  Item was not part of the enum.
				}
			}

			ICollection<SelectListItem> items = new List<SelectListItem>();

			foreach (object item in Enum.GetValues(enumType))
			{
				// If this is an active item OR if it is an inactive item that is still selected, add it to the list.
				if (item.IsActive() || (item != null && item.Equals(selectedItem)))
				{
					items.Add(new SelectListItem() { Text = item.GetDescription(), Value = (int)item == 0 && required ? string.Empty : ((int)item).ToString() });
				}
			}

			return items;
		}

		/// <summary>
		/// Populate list items from Enum, sorted alphabetically.
		/// </summary>
		/// <param name="enumType">Enum type</param>
		/// <param name="required">Whether the field is required</param>
		/// <param name="selectedValue">The value that is selected, if applicable.</param>
		/// <returns>List of options for select box, sorted alphabetically.  If there is a selected value provided that is not in the list, it is added.</returns>
		public static ICollection<SelectListItem> GetListItemsForEnumSorted(Type enumType, bool required = true, string selectedValue = "")
		{
			// Get the list and sort it alphabetically by the Text property.
			List<SelectListItem> sortedItems = GetListItemsForEnum(enumType, required, selectedValue).OrderBy(i => i.Text).ToList();

			// If the item that is supposed to be first in the list (such as 'Please select...') is not, then move it to the first spot.
			int firstItemIndex = sortedItems.FindIndex(i => i.Value == "0" || string.IsNullOrEmpty(i.Value));
			if (firstItemIndex > 0)
			{
				SelectListItem firstItem = sortedItems.ElementAt(firstItemIndex);
				sortedItems.RemoveAt(firstItemIndex);
				sortedItems.Insert(0, firstItem);
			}

			return sortedItems;
		}
	}
}
