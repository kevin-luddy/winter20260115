// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// The Model for an address from a section
	/// </summary>
	public class SectionAddressModelView
	{
		/// <summary>
		/// The unique ID for that address
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The title for where the address resides
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The office
		/// </summary>
		public string Office { get; set; }

		/// <summary>
		/// The agency name
		/// </summary>
		public string Agency { get; set; }

		/// <summary>
		/// The LMBA
		/// </summary>
		public string LMBA { get; set; }

		/// <summary>
		/// The Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The Street
		/// </summary>
		public string Street { get; set; }

		/// <summary>
		/// The city, ST
		/// </summary>
		public string CityST { get; set; }

		/// <summary>
		/// The phone #
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// The email
		/// </summary>
		public string Email { get; set; }
	}
}
