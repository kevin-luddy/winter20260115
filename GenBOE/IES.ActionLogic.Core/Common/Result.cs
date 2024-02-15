// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.Common
{
	using System.Collections.Generic;

	/// <summary>
	/// Result base class
	/// </summary>
	public class Result<T>
	{
		/// <summary>
		/// Data for the result
		/// </summary>
		public T Data { get; set; }

		/// <summary>
		/// List of messages that is returned from the calls. These messages treated as warnings during Save, and as errors during Export
		/// </summary>
		public IList<string> Messages { get; set; } = new List<string>();

		/// <summary>
		/// List of Warnings that are returned from the calls. 
		/// </summary>
		public IList<string> Warnings { get; set; } = new List<string>();

		/// <summary>
		/// Flag indicating if the call was successful
		/// </summary>
		public bool IsSuccessful => Status == ResultStatus.Ok;

		/// <summary>
		/// Result status enum
		/// </summary>
		public ResultStatus Status { get; set; } = ResultStatus.Ok;

		/// <summary>
		/// Correlation identifier
		/// </summary>
		public string CorrelationId { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		public Result() { }
	}
}