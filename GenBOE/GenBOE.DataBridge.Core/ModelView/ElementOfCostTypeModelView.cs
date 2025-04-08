using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	/// <summary>
	/// This class will return the different proposal state types
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ElementOfCostTypeModelView
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ElementOfCostTypeModelView()
		{
			ElementOfCostId = 0;
			ElementOfCostName = string.Empty;
		}

		/// <summary>
		/// The ID
		/// </summary>
		public int ElementOfCostId { get; set; }

		/// <summary>
		/// the name of
		/// </summary>
		public string ElementOfCostName { get; set; }
	}
}
