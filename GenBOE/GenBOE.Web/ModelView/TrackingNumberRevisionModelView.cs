// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System;
	public class TrackingNumberRevisionModelView
	{
		public TrackingNumberRevisionModelView()
		{
			TrackingNumber = String.Empty;
		}

		public TrackingNumberRevisionModelView(string trackingNumber)
		{
			TrackingNumber = trackingNumber;
		}

		/// <summary>
		/// Tracking Number
		/// </summary>
		public string TrackingNumber { get; set; }
	}
}