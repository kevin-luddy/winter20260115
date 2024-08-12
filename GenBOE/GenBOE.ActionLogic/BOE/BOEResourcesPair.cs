// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using System.Collections.Generic;

	/// <summary>
	/// BOE Id and Resources Pair Done so GenBOE does not complain about CA: 1006
	/// </summary>
	public class BOEResourcesPair
	{
		/// <summary>
		/// BOE Id
		/// </summary>
		public int BOEId { get; set; }

		/// <summary>
		/// List of Resource Names
		/// </summary>
		public ICollection<string> Resources { get; set; }
	}
}
