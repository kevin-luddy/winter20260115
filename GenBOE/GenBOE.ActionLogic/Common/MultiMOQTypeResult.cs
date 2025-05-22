// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Used in conjunction with MultiMOQTypeUtility for returning results
	/// </summary>
	public class MultiMOQTypeResult
	{
		/// <summary>
		/// ctor
		/// </summary>
		public MultiMOQTypeResult()
		{
			Tasks = new List<string>();
		}

		/// <summary>
		/// Do multiple MOQ Types exist with the specified types?
		/// </summary>
		public bool DoMultiMOQTypesExist { get { return this.Tasks.Any(); } }

		/// <summary>
		/// The list of task names/titles that have the offending multiple MOQ Types
		/// </summary>
		public List<string> Tasks { get; set; }
	}
}
