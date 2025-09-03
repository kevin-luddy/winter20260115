// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel.DataAnnotations;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;

	[ExcludeFromCodeCoverage]
	public class BOEStatusReportModelView : IComparable
	{
		public BOEStatusReportModelView()
			: base()
		{
			BOEID = -1;
			BOETitle = string.Empty;
			WBSNumber = string.Empty;
			WBSPaddedNumber = string.Empty;
			WBSTitle = string.Empty;
			CLINNumber = string.Empty;
			CLINTitle = string.Empty;
			CLINPaddedNumber = string.Empty;
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;
			TotalHours = 0;
			TotalCost = 0;
			WorkspaceID = -1;
			Authors = new Collection<string>();
			Approvers = new Collection<string>();
			Status = string.Empty;
			IsMultiClinWbs = false;
			isMaterial = false;
		}

		public int BOEID { get; set; }
		public string BOETitle { get; set; }
		public string WBSNumber { get; set; }
		public string WBSPaddedNumber { get; set; }
		public string WBSTitle { get; set; }
		public string CLINNumber { get; set; }
		public string CLINTitle { get; set; }
		public string CLINPaddedNumber { get; set; }
		public Boolean IsMultiClinWbs { get; set; }
		public Boolean isMaterial { get; set; }

		public int WorkspaceID { get; set; }

		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime StartDate { get; set; }

		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime EndDate { get; set; }

		public decimal TotalHours { get; set; }
		public decimal TotalCost { get; set; }
		public Collection<string> Authors { get; set; }
		public Collection<string> Approvers { get; set; }
		public string Status { get; set; }

		#region UCOT Variables

		/// <summary>
		/// Is UCOT Enabled for Workspace
		/// </summary>
		public bool IsUCOTEnabledForWorkspace { get; set; }

		/// <summary>
		/// Total UCOT Hours 
		/// </summary>
		public decimal TotalUCOTHours { get; set; }

		/// <summary>
		/// Gets Total UCOT Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalUCOTHoursFormatted
		{
			get => Utilities.FormatStringWithPrecision(TotalUCOTHours, ResourceDecimalPrecision);
		}

		/// <summary>
		/// Total Hours with UCOT (TotalHours + TotalUCOTHours)
		/// </summary>
		public decimal TotalHoursWithUCOT { get; set; }

		/// <summary>
		/// Gets Total Hours with UCOT Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalHoursWithUCOTFormatted
		{
			get => Utilities.FormatStringWithPrecision(TotalHoursWithUCOT, ResourceDecimalPrecision);
		}

		#endregion UCOT Variables

		/// <summary>
		/// Number of decimal places for resources.
		/// </summary>
		public int ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Gets Total Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalHoursFormatted
		{
			get => Utilities.FormatStringWithPrecision(TotalHours, ResourceDecimalPrecision);
		}

		/// <summary>
		/// String format used by the UI for decimal precision.
		/// </summary>
		public string DecimalPrecisionStringFormat
		{
			get => Utilities.PrecisionFormattingString(ResourceDecimalPrecision);
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			if (obj.GetType() == typeof(BOEStatusReportModelView))
			{
				return CompareTo((BOEStatusReportModelView)obj);
			}

			else
			{
				throw new NotImplementedException(
					String.Format(
						"Cannot compare to {0} type.",
						obj.GetType()));
			}
		}

		// Custom comparer to implement desired multi-sort order
		private int CompareTo(BOEStatusReportModelView otherModelView)
		{
			int toReturn = this.WBSPaddedNumber.CompareTo(otherModelView.WBSPaddedNumber);

			if (toReturn == 0)
			{
				toReturn = this.BOETitle.CompareTo(otherModelView.BOETitle);
			}

			if (toReturn == 0)
			{
				toReturn = this.WBSTitle.CompareTo(otherModelView.WBSTitle);
			}

			if (toReturn == 0)
			{
				toReturn = this.CLINPaddedNumber.CompareTo(otherModelView.CLINPaddedNumber);
			}

			if (toReturn == 0)
			{
				toReturn = this.CLINTitle.CompareTo(otherModelView.CLINTitle);
			}

			if (toReturn == 0)
			{
				toReturn = this.StartDate.CompareTo(otherModelView.StartDate);
			}

			if (toReturn == 0)
			{
				toReturn = this.EndDate.CompareTo(otherModelView.EndDate);
			}

			if (toReturn == 0)
			{
				toReturn = this.TotalHours.CompareTo(otherModelView.TotalHours);
			}

			if (toReturn == 0)
			{
				toReturn = this.TotalCost.CompareTo(otherModelView.TotalCost);
			}

			if (toReturn == 0)
			{
				toReturn = this.IsMultiClinWbs.CompareTo(otherModelView.IsMultiClinWbs);
			}

			if (toReturn == 0)
			{
				toReturn = this.isMaterial.CompareTo(otherModelView.isMaterial);
			}

			if (toReturn == 0)
			{
				string thisAuthorString = String.Join("", this.Authors);
				string otherAuthorString = String.Join("", otherModelView.Authors);
				toReturn = thisAuthorString.CompareTo(otherAuthorString);
			}

			if (toReturn == 0)
			{
				string thisApproverString = String.Join("", this.Approvers);
				string otherApproverString = String.Join("", otherModelView.Approvers);
				toReturn = thisApproverString.CompareTo(otherApproverString);
			}

			if (toReturn == 0)
			{
				toReturn = this.Status.CompareTo(otherModelView.Status);
			}

			return toReturn;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			return this.CompareTo(obj) == 0;
		}

		public static bool operator ==(BOEStatusReportModelView first, BOEStatusReportModelView second)
		{
			if (first == null)
			{
				if (second == null)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (second == null)
			{
				return false;
			}

			return first.CompareTo(second) == 0;
		}

		public static bool operator !=(BOEStatusReportModelView first, BOEStatusReportModelView second)
		{
			if (first == null)
			{
				if (second == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else if (second == null)
			{
				return true;
			}

			return first.CompareTo(second) != 0;
		}

		public static bool operator <(BOEStatusReportModelView first, BOEStatusReportModelView second)
		{
			if (first == null)
			{
				if (second == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else if (second == null)
			{
				return false;
			}

			return first.CompareTo(second) < 0;
		}

		public static bool operator >(BOEStatusReportModelView first, BOEStatusReportModelView second)
		{
			if (first == null)
			{
				return false;
			}
			else if (second == null)
			{
				return true;
			}

			return first.CompareTo(second) > 0;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
