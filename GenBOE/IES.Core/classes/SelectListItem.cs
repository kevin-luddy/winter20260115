using System;
using System.Collections.Generic;
using System.Text;

namespace IES.Core
{
	//
	// Summary:
	//     Represents the selected item in an instance of the System.Web.Mvc.SelectList
	//     class.
	public class SelectListItem
	{
		//
		// Summary:
		//     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListItem
		//     is disabled.
		public bool Disabled { get; set; }

		//
		// Summary:
		//     Represents the optgroup HTML element this item is wrapped into. In a select list,
		//     multiple groups with the same name are supported. They are compared with reference
		//     equality.
		public SelectListGroup Group { get; set; }

		//
		// Summary:
		//     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListItem
		//     is selected.
		//
		// Returns:
		//     true if the item is selected; otherwise, false.
		public bool Selected { get; set; }

		//
		// Summary:
		//     Gets or sets the text of the selected item.
		//
		// Returns:
		//     The text.
		public string Text { get; set; }

		//
		// Summary:
		//     Gets or sets the value of the selected item.
		//
		// Returns:
		//     The value.
		public string Value { get; set; }

		//
		// Summary:
		//     Initializes a new instance of the System.Web.Mvc.SelectListItem class.
		public SelectListItem()
		{
		}
	}

	// Summary:
	//     Represents the optgroup HTML element and its attributes. In a select list, multiple
	//     groups with the same name are supported. They are compared with reference equality.
	public class SelectListGroup
	{
		//
		// Summary:
		//     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListGroup
		//     is disabled.
		public bool Disabled { get; set; }

		//
		// Summary:
		//     Represents the value of the optgroup's label.
		public string Name { get; set; }

		public SelectListGroup()
		{
		}
	}
}
