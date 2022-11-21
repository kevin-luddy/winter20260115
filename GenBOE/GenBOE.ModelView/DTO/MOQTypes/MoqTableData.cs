// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// MOQ Table Data class
	/// </summary>
	[Serializable]
	public class MoqTableData : UpdateableDTO
	{
		/// <summary>
		/// Query Type Monthly
		/// </summary>
		public static readonly string MONTHLY = "Monthly";

		/// <summary>
		/// Query Type Weekly
		/// </summary>
		public static readonly string WEEKLY = "Weekly";

		/// <summary>
		/// Default constructor
		/// </summary>
		public MoqTableData()
		{
			Id = -1;
			Order = 2000;
			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
		}

		/// <summary>
		/// Table Name
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// Repository Name
		/// </summary>
		public string RepositoryName { get; set; }

		/// <summary>
		/// Selected Repository Name Option
		/// </summary>
		public string RepositoryNameSelection
		{
			get
			{
				return this.RepositoryName == IES.Common.RepositoryName.SapWebi.GetDescription() || string.IsNullOrEmpty(this.RepositoryName)
					? this.RepositoryName
					: IES.Common.RepositoryName.Other.GetDescription();
			}
		}

		/// <summary>
		/// Query Type
		/// </summary>
		public string QueryType { get; set; }

		/// <summary>
		/// Date Of Report
		/// </summary>
		public DateTime DateOfReport { get; set; }

		/// <summary>
		/// Historical Program Name
		/// </summary>
		public string HistoricalProgramName { get; set; }

		/// <summary>
		/// Contract Number
		/// </summary>
		public string ContractNumber { get; set; }

		/// <summary>
		/// Wbs Element
		/// </summary>
		public string WbsElement { get; set; }

		#region PoP Dates

		/// <summary>
		/// PoP Start Date field, to allow us the different handling of weekly dates
		/// </summary>
		private DateTime popStart { get; set; }

		/// <summary>
		/// PoP End Date field, to allow us the different handling of weekly dates
		/// </summary>
		private DateTime popEnd { get; set; }

		/// <summary>
		/// PoP Start
		/// </summary>
		public DateTime PoPStart
		{
			get
			{
				DateTime result = this.popStart;
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && this.QueryType == MoqTableData.WEEKLY)
				{
					result = GetDateFromWeekYear(this.PoPStartWeek ?? 0, this.PoPStartYear ?? 0);
				}

				return result;
			}
			set
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && this.QueryType == MoqTableData.WEEKLY)
				{
					this.PoPStartWeek = GetWeekFromDate(value);
					this.PoPStartYear = value.Year;
				}

				this.popStart = value;
			}
		}

		/// <summary>
		/// PoP End
		/// </summary>
		public DateTime PoPEnd
		{
			get
			{
				DateTime result = this.popEnd;
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && this.QueryType == MoqTableData.WEEKLY)
				{
					result = GetDateFromWeekYear(this.PoPEndWeek ?? 0, this.PoPEndYear ?? 0);
				}

				return result;
			}
			set
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && this.QueryType == MoqTableData.WEEKLY)
				{
					this.PoPEndWeek = GetWeekFromDate(value);
					this.PoPEndYear = value.Year;
				}

				this.popEnd = value;
			}
		}

		/// <summary>
		/// PoP Start Week - only used when Space and Weekly
		/// </summary>
		public int? PoPStartWeek { get; set; }

		/// <summary>
		/// PoP Start Year - only used when Space and Weekly
		/// </summary>
		public int? PoPStartYear { get; set; }

		/// <summary>
		/// PoP End Week - only used when Space and Weekly
		/// </summary>
		public int? PoPEndWeek { get; set; }

		/// <summary>
		/// PoP End Year- only used when Space and Weekly
		/// </summary>
		public int? PoPEndYear { get; set; }

		/// <summary>
		/// String version of the PoP Start date. Needed because SSC and RMS are handling things differently..
		/// </summary>
		public string PoPStartString { get { return FormatMoqTablePoPDate(this.PoPStart, this.PoPStartWeek, this.PoPStartYear, this.QueryType); } }

		/// <summary>
		/// String version of the PoP End date. Needed because SSC and RMS are handling things differently..
		/// </summary>
		public string PoPEndString { get { return FormatMoqTablePoPDate(this.PoPEnd, this.PoPEndWeek, this.PoPEndYear, this.QueryType); } }

		/// <summary>
		/// Formats the PoP Date for printing purposes, based on the Company and Query Type
		/// 
		/// RMS -> just print the date
		/// 
		/// Space -> the date is formatted based on the query Type (Weekly / Monthly)
		///         month -> MM/YYYY
		///         weeks -> FW ww/YYYY, where ww is the week value of 1-53
		/// </summary>
		/// <param name="date"></param>
		/// <param name="queryType"></param>
		/// <returns></returns>
		public static string FormatMoqTablePoPDate(DateTime date, int? week, int? year, string queryType)
		{
			return
				SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST
					? date.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR) :
					queryType == MoqTableData.MONTHLY ? $"{date.Month.ToString("00")}/{date.Year}" : $"FW {week ?? 0:00}/{year}";
		}

		/// <summary>
		/// Creates a date out of week / year. The way we split weeks is week 1-30 will fall into January 1-30. Weeks 31-53 will fall into February.
		/// </summary>
		/// <param name="week">Week</param>
		/// <param name="year">Year</param>
		/// <returns>Date representing the Week / Year</returns>
		public static DateTime GetDateFromWeekYear(int week, int year)
		{
			if (week == 0) { return DateTime.MinValue; }

			int month = week > 30 ? 2 : 1;
			int day = week > 30 ? week - 30 : week;

			return new DateTime(year, month, day);
		}

		/// <summary>
		/// Gets week from the Date. This is a reverse of GetDateFromWeekYear method
		/// </summary>
		/// <param name="date">Date representing the week</param>
		/// <returns>Week based on the date</returns>
		public static int GetWeekFromDate(DateTime date)
		{
			return date.Day + (date.Month == 2 ? 30 : 0);
		}

		#endregion

		/// <summary>
		/// Total WbsHours
		/// </summary>
		public decimal TotalWbsHours { get; set; }

		/// <summary>
		/// Additional Query Filters
		/// </summary>
		public string AdditionalQueryFilters { get; set; }

		/// <summary>
		/// Total Relevant hours
		/// </summary>
		public decimal TotalRelevantHours { get; set; }

		/// <summary>
		/// Number representing the order the MOQ Type table is displayed in when there are multiple MOQ Type tables
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// ID of the MOQ Type Selection this table data belongs to
		/// </summary>
		public int MOQTypeSelectionId { get; set; }

		/// <summary>
		/// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
		/// </summary>
		internal IEnumerable<CustomFieldValueContainer> CustomFieldValueContainersIEnum { get; set; }

		/// <summary>
		/// Custom Field Value Containers for the MOQ Table
		/// </summary>
		public ICollection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }
	}
}
