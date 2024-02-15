// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.IO.Export
{
	/// <summary>
	/// Constants for PPRDExporter
	/// </summary>
	public static class PPRDExporterConstants
	{
		#region Containers

		/// <summary>
		/// Document Details container tag
		/// </summary>
		internal const string CONTAINER_DOCUMENT_DETAILS = "DocumentDetailsContainer";

		/// <summary>
		/// Introduction container tag
		/// </summary>
		internal const string CONTAINER_INTRODUCTION = "IntroductionContainer";

		/// <summary>
		/// Section container tag
		/// </summary>
		internal const string CONTAINER_SECTION = "SectionContainer";

		/// <summary>
		/// Section Title container tag
		/// </summary>
		internal const string CONTAINER_SECTIONTITLE = "SectionTitleContainer";

		/// <summary>
		/// Subsection Title container tag
		/// </summary>
		internal const string CONTAINER_SUBSECTIONTITLE = "SubsectionTitleContainer";

		/// <summary>
		/// Subsubsection Title container tag
		/// </summary>
		internal const string CONTAINER_SUBSUBSECTIONTITLE = "SubsubsectionTitleContainer";

		/// <summary>
		/// Text Element container tag
		/// </summary>
		internal const string CONTAINER_TEXTANDTABLES = "TextAndTablesContainer";

		#endregion

		#region Fields

		/// <summary>
		/// Revision Number tag
		/// </summary>
		internal const string FIELDNAME_REVISIONNUMBER = "RevisionNumber";

		/// <summary>
		/// Publish Date tag
		/// </summary>
		internal const string FIELDNAME_PUBLISHDATE = "PublishDate";

		/// <summary>
		/// Section Number tag
		/// </summary>
		internal const string FIELDNAME_SECTIONNUMBER = "SectionNumber";

		/// <summary>
		/// Section Title tag
		/// </summary>
		internal const string FIELDNAME_SECTIONTITLE = "SectionTitle";

		/// <summary>
		/// Subsection Number tag
		/// </summary>
		internal const string FIELDNAME_SUBSECTIONNUMBER = "SubsectionNumber";

		/// <summary>
		/// Subsection Title tag
		/// </summary>
		internal const string FIELDNAME_SUBSECTIONTITLE = "SubsectionTitle";

		/// <summary>
		/// Subsubsection Number tag
		/// </summary>
		internal const string FIELDNAME_SUBSUBSECTIONNUMBER = "SubsubsectionNumber";

		/// <summary>
		/// Subsubsection Title tag
		/// </summary>
		internal const string FIELDNAME_SUBSUBSECTIONTITLE = "SubsubsectionTitle";

		/// <summary>
		/// Text Element tag
		/// </summary>
		internal const string FIELDNAME_TEXTELEMENT = "TextElement";

		/// <summary>
		/// Rate Code tag
		/// </summary>
		internal const string FIELDNAME_RATECODE = "RateCode";

		/// <summary>
		/// Rate Code Description tag
		/// </summary>
		internal const string FIELDNAME_RATECODEDESCRIPTION = "RateCodeDescription";

		/// <summary>
		/// Rate Code Value tag
		/// </summary>
		internal const string FIELDNAME_RATECODEVALUE = "RateCodeValue";

		/// <summary>
		/// Introduction rate details tag
		/// </summary>
		internal const string FIELDNAME_HISTORY = "History";

		/// <summary>
		/// Introduction change list tag
		/// </summary>
		internal const string FIELDNAME_RELEASENOTES = "ReleaseNotes";

		/// <summary>
		/// Address Office tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSOFFICE = "AddressOffice";

		/// <summary>
		/// Address Agency tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSAGENCY = "AddressAgency";
		/// <summary>
		/// Address LM BA tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSLMBA = "AddressLMBA";
		/// <summary>
		/// Address Name tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSNAME = "AddressName";
		/// <summary>
		/// Address Street tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSSTREET = "AddressStreet";
		/// <summary>
		/// Address City tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSCITY = "AddressCity";
		/// <summary>
		/// Address Phone tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSPHONE = "AddressPhone";
		/// <summary>
		/// Address Email tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSEMAIL = "AddressEmail";
		/// <summary>
		/// Address Other tag
		/// </summary>
		internal const string FIELDNAME_ADDRESSOTHER = "AddressOther";

		#endregion

		#region Tables

		/// <summary>
		/// Rates table tag
		/// </summary>
		internal const string TABLE_RATES = "RatesTable";

		/// <summary>
		/// Address table tag
		/// </summary>
		internal const string TABLE_ADDRESS = "AddressTable";

		#endregion

		#region Labels and markers

		/// <summary>
		/// Rate Code label tag
		/// </summary>
		internal const string LABEL_RATECODE = "RateCodeLabel";

		/// <summary>
		/// Description label tag
		/// </summary>
		internal const string LABEL_DESCRIPTION = "DescriptionLabel";

		/// <summary>
		/// Year label tag
		/// </summary>
		internal const string LABEL_YEAR = "YearLabel";

		/// <summary>
		/// Data Row Marker tag
		/// </summary>
		internal const string MARKER_DATAROW = "Marker-DataRow";

		#endregion

		#region MIME Constants

		/// <summary>
		/// Content header name constant
		/// </summary>
		public const string CONTENT_HEADER_NAME = "Content-Disposition";

		/// <summary>
		/// Content header format string constant
		/// </summary>
		public const string CONTENT_HEADER_FORMAT_STRING = "attachment;filename={0}";

		#endregion

		#region Misc

		/// <summary>
		/// Work In Progress revision string
		/// </summary>
		internal const string WORKINPROGRESSID = "WIP";

		/// <summary>
		/// Hex code for blue text for internal sections
		/// </summary>
		internal const string INTERNALSECTIONTEXTCOLOR = "0000FF";

		/// <summary>
		/// Start tag for html paragraph
		/// No closing bracket to catch tags with attributes
		/// </summary>
		internal const string P_START_TAG = "<p";

		/// <summary>
		/// End tag for html paragraph
		/// </summary>
		internal const string P_END_TAG = "</p>";

		/// <summary>
		/// Start tag for html div
		/// No closing bracket to not lose attributes
		/// </summary>
		internal const string DIV_START_TAG = "<div";

		/// <summary>
		/// End tag for html div
		/// </summary>
		internal const string DIV_END_TAG = "</div>";

		/// <summary>
		/// Tag for the page break
		/// </summary>
		internal const string PAGE_BREAK = "PageBreak";
		#endregion

		#region Rate Table Constants

		/// <summary>
		/// Maximum number of years per row in the Rate Table
		/// </summary>
		internal const int MAX_TABLE_ROW_YEARS = 7;

		/// <summary>
		/// Width for description column in inches when there is no rate code column
		/// </summary>
		internal const decimal DESCRIPTION_NO_RATE_CODE_COLUMN_WIDTH = 2.59m;

		#endregion
	}
}
