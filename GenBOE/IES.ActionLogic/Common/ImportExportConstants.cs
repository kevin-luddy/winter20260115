// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
	using System.Collections.Generic;

	/// <summary>
	/// Import/Export Constants
	/// </summary>
	public static class ImportExportConstants
	{
		/// <summary>
		/// Path to Rate Code template
		/// </summary>
		public static readonly string PATH_TO_RATE_CODE_TEMPLATE = "~/Templates/Export/RateCodesImportExample.xlsm";

		/// <summary>
		/// Worksheet names - Rate Codes
		/// </summary>
		public static readonly string RATE_CODES = "Rate Codes";

		/// <summary>
		/// Worksheet names - Options Lists
		/// </summary>
		public static readonly string OPTIONS_LISTS = "Options Lists";

		// Workoffline template table offsets
		// Offsets are from the top left corner of the table

		/// <summary>
		/// Rate Category Column Offset
		/// </summary>
		public const int RATE_CATEGORY_CELL_COLUMN_OFFSET = 0;

		/// <summary>
		/// Linked Section Column Offset
		/// </summary>
		public const int SECTION_CELL_COLUMN_OFFSET = 3;

		/// <summary>
		/// Resource Type Column Offset
		/// </summary>
		public const int RESOURCE_TYPE_CELL_COLUMN_OFFSET = 4;

		/// <summary>
		/// Rate Type Column Offset
		/// </summary>
		public const int RATE_TYPE_CELL_COLUMN_OFFSET = 5;

		/// <summary>
		/// Disclosure Type Column Offset
		/// </summary>
		public const int DISCLOSURE_TYPE_CELL_COLUMN_OFFSET = 6;

		/// <summary>
		/// ProPricer Resource Class Column Offset
		/// </summary>
		public const int RESOURCE_CLASS_CELL_COLUMN_OFFSET = 8;

		/// <summary>
		/// ProPricer Resource Class1 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS1_CELL_COLUMN_OFFSET = 10;

		/// <summary>
		/// ProPricer Resource Class2 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS2_CELL_COLUMN_OFFSET = 12;

		/// <summary>
		/// ProPricer Resource Class3 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS3_CELL_COLUMN_OFFSET = 14;

		/// <summary>
		/// ProPricer Resource Class4 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS4_CELL_COLUMN_OFFSET = 16;

		/// <summary>
		/// ProPricer Resource Class5 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS5_CELL_COLUMN_OFFSET = 18;

		/// <summary>
		/// ProPricer Resource Class6 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS6_CELL_COLUMN_OFFSET = 20;

		/// <summary>
		/// ProPricer Resource Class7 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS7_CELL_COLUMN_OFFSET = 22;

		/// <summary>
		/// ProPricer Resource Class8 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS8_CELL_COLUMN_OFFSET = 24;

		/// <summary>
		/// ProPricer Resource Class9 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS9_CELL_COLUMN_OFFSET = 26;

		#region 1LMX Resource Offsets
		/// <summary>
		/// ProPricer Resource Class11 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS11_CELL_COLUMN_OFFSET = 28;

		/// <summary>
		/// ProPricer Resource Class12 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS12_CELL_COLUMN_OFFSET = 30;

		/// <summary>
		/// ProPricer Resource Class13 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS13_CELL_COLUMN_OFFSET = 32;

		/// <summary>
		/// ProPricer Resource Class14 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS14_CELL_COLUMN_OFFSET = 34;

		/// <summary>
		/// ProPricer Resource Class15 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS15_CELL_COLUMN_OFFSET = 36;

		/// <summary>
		/// ProPricer Resource Class21 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS21_CELL_COLUMN_OFFSET = 38;

		/// <summary>
		/// ProPricer Resource Class22 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS22_CELL_COLUMN_OFFSET = 40;

		/// <summary>
		/// ProPricer Resource Class23 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS23_CELL_COLUMN_OFFSET = 42;

		/// <summary>
		/// ProPricer Resource Class24 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS24_CELL_COLUMN_OFFSET = 44;

		/// <summary>
		/// ProPricer Resource Class25 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS25_CELL_COLUMN_OFFSET = 46;

		/// <summary>
		/// ProPricer Resource Class31 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS31_CELL_COLUMN_OFFSET = 48;

		/// <summary>
		/// ProPricer Resource Class32 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS32_CELL_COLUMN_OFFSET = 50;

		/// <summary>
		/// ProPricer Resource Class33 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS33_CELL_COLUMN_OFFSET = 52;

		/// <summary>
		/// ProPricer Resource Class34 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS34_CELL_COLUMN_OFFSET = 54;

		/// <summary>
		/// ProPricer Resource Class35 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS35_CELL_COLUMN_OFFSET = 56;

		/// <summary>
		/// ProPricer Resource Class41 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS41_CELL_COLUMN_OFFSET = 58;

		/// <summary>
		/// ProPricer Resource Class42 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS42_CELL_COLUMN_OFFSET = 60;

		/// <summary>
		/// ProPricer Resource Class43 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS43_CELL_COLUMN_OFFSET = 62;

		/// <summary>
		/// ProPricer Resource Class44 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS44_CELL_COLUMN_OFFSET = 64;

		/// <summary>
		/// ProPricer Resource Class45 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS45_CELL_COLUMN_OFFSET = 66;

		/// <summary>
		/// ProPricer Resource Class51 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS51_CELL_COLUMN_OFFSET = 68;

		/// <summary>
		/// ProPricer Resource Class52 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS52_CELL_COLUMN_OFFSET = 70;

		/// <summary>
		/// ProPricer Resource Class53 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS53_CELL_COLUMN_OFFSET = 72;

		/// <summary>
		/// ProPricer Resource Class54 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS54_CELL_COLUMN_OFFSET = 74;

		/// <summary>
		/// ProPricer Resource Class55 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS55_CELL_COLUMN_OFFSET = 76;

		/// <summary>
		/// ProPricer Resource Class61 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS61_CELL_COLUMN_OFFSET = 78;

		/// <summary>
		/// ProPricer Resource Class62 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS62_CELL_COLUMN_OFFSET = 80;

		/// <summary>
		/// ProPricer Resource Class63 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS63_CELL_COLUMN_OFFSET = 82;

		/// <summary>
		/// ProPricer Resource Class64 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS64_CELL_COLUMN_OFFSET = 84;

		/// <summary>
		/// ProPricer Resource Class65 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS65_CELL_COLUMN_OFFSET = 86;

		/// <summary>
		/// ProPricer Resource Class71 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS71_CELL_COLUMN_OFFSET = 88;

		/// <summary>
		/// ProPricer Resource Class72 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS72_CELL_COLUMN_OFFSET = 90;

		/// <summary>
		/// ProPricer Resource Class73 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS73_CELL_COLUMN_OFFSET = 92;

		/// <summary>
		/// ProPricer Resource Class74 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS74_CELL_COLUMN_OFFSET = 94;

		/// <summary>
		/// ProPricer Resource Class75 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS75_CELL_COLUMN_OFFSET = 96;

		/// <summary>
		/// ProPricer Resource Class81 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS81_CELL_COLUMN_OFFSET = 98;

		/// <summary>
		/// ProPricer Resource Class82 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS82_CELL_COLUMN_OFFSET = 100;

		/// <summary>
		/// ProPricer Resource Class83 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS83_CELL_COLUMN_OFFSET = 102;

		/// <summary>
		/// ProPricer Resource Class84 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS84_CELL_COLUMN_OFFSET = 104;

		/// <summary>
		/// ProPricer Resource Class85 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS85_CELL_COLUMN_OFFSET = 106;

		/// <summary>
		/// ProPricer Resource Class91 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS91_CELL_COLUMN_OFFSET = 108;

		/// <summary>
		/// ProPricer Resource Class92 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS92_CELL_COLUMN_OFFSET = 110;

		/// <summary>
		/// ProPricer Resource Class93 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS93_CELL_COLUMN_OFFSET = 112;

		/// <summary>
		/// ProPricer Resource Class94 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS94_CELL_COLUMN_OFFSET = 114;

		/// <summary>
		/// ProPricer Resource Class95 Column Offset
		/// </summary>
		public const int RESOURCE_CLASS95_CELL_COLUMN_OFFSET = 116;

		#endregion

		/// <summary>
		/// Government Burden Pool Column Offset
		/// </summary>
		public const int GOVT_BURDEN_POOL_CELL_COLUMN_OFFSET = 117;

		/// <summary>
		/// Commercial Burden Pool Column Offset
		/// </summary>
		public const int COMM_BURDEN_POOL_CELL_COLUMN_OFFSET = 118;
		
		// Options Lists column headers

		/// <summary>
		/// Rate Category Column Header
		/// </summary>
		public static readonly string CATEGORY_COLUMN_HEADER = "Category";

		/// <summary>
		/// Section Column Header
		/// </summary>
		public static readonly string SECTION_COLUMN_HEADER = "Section";

		/// <summary>
		/// Resource Type Column Header
		/// </summary>
		public static readonly string RESOURCE_TYPE_COLUMN_HEADER = "Resource Type";

		/// <summary>
		/// Rate Type Column Header
		/// </summary>
		public static readonly string RATE_TYPE_COLUMN_HEADER = "Rate Type";

		/// <summary>
		/// Disclosure Type Column Header
		/// </summary>
		public static readonly string DISCLOSURE_TYPE_COLUMN_HEADER = "Disclosure Type";

		/// <summary>
		/// Resource Class Column Header
		/// </summary>
		public static readonly string RESOURCE_CLASS_COLUMN_HEADER = "Resource Class";

		/// <summary>
		/// Government Burden Pool Column Header
		/// </summary>
		public static readonly string GOVERNMENT_BURDEN_POOL_COLUMN_HEADER = "Government Burden Pool";

		/// <summary>
		/// Commercial Burden Pool Column Header
		/// </summary>
		public static readonly string COMMERCIAL_BURDEN_POOL_COLUMN_HEADER = "Commercial Burden Pool";

		// Export column headers
		/// <summary>
		/// Rate Category Column Header
		/// </summary>
		public static readonly string RATE_CATEGORY_COLUMN_HEADER = "Category";

		/// <summary>
		/// Rate Code Column Header
		/// </summary>
		public static readonly string RATE_CODE_COLUMN_HEADER = "Rate Code";

		/// <summary>
		/// Rate Description Column Header
		/// </summary>
		public static readonly string RATE_DESCRIPTION_COLUMN_HEADER = "Description";

		/// <summary>
		/// Linked Section Column Header
		/// </summary>
		public static readonly string LINKED_SECTION_COLUMN_HEADER = "Linked Section";

		/// <summary>
		/// ProPricer Description Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION_COLUMN_HEADER = "ProPricer Description";

		/// <summary>
		/// ProPricer Resource Class Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS_COLUMN_HEADER = "Resource Class";

		/// <summary>
		/// ProPricer Description1 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION1_COLUMN_HEADER = "ProPricer Description1";

		/// <summary>
		/// ProPricer Resource Class1 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS1_COLUMN_HEADER = "Resource Class1";

		/// <summary>
		/// ProPricer Description2 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION2_COLUMN_HEADER = "ProPricer Description2";

		/// <summary>
		/// ProPricer Resource Class2 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS2_COLUMN_HEADER = "Resource Class2";

		/// <summary>
		/// ProPricer Description3 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION3_COLUMN_HEADER = "ProPricer Description3";

		/// <summary>
		/// ProPricer Resource Class3 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS3_COLUMN_HEADER = "Resource Class3";

		/// <summary>
		/// ProPricer Description4 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION4_COLUMN_HEADER = "ProPricer Description4";

		/// <summary>
		/// ProPricer Resource Class4 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS4_COLUMN_HEADER = "Resource Class4";

		/// <summary>
		/// ProPricer Description5 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION5_COLUMN_HEADER = "ProPricer Description5";

		/// <summary>
		/// ProPricer Resource Class5 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS5_COLUMN_HEADER = "Resource Class5";

		/// <summary>
		/// ProPricer Description6 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION6_COLUMN_HEADER = "ProPricer Description6";

		/// <summary>
		/// ProPricer Resource Class6 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS6_COLUMN_HEADER = "Resource Class6";

		/// <summary>
		/// ProPricer Description7 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION7_COLUMN_HEADER = "ProPricer Description7";

		/// <summary>
		/// ProPricer Resource Class7 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS7_COLUMN_HEADER = "Resource Class7";

		/// <summary>
		/// ProPricer Description8 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION8_COLUMN_HEADER = "ProPricer Description8";
		/// <summary>
		/// ProPricer Resource Class8 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS8_COLUMN_HEADER = "Resource Class8";

		/// <summary>
		/// ProPricer Description9 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION9_COLUMN_HEADER = "ProPricer Description9";

		/// <summary>
		/// ProPricer Resource Class9 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS9_COLUMN_HEADER = "Resource Class9";

		#region 1LMX Descriptions/Resources

		/// <summary>
		/// ProPricer Description11 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION11_COLUMN_HEADER = "ProPricer Description11";

		/// <summary>
		/// ProPricer Resource Class11 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS11_COLUMN_HEADER = "Resource Class11";

		/// <summary>
		/// ProPricer Description12 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION12_COLUMN_HEADER = "ProPricer Description12";

		/// <summary>
		/// ProPricer Resource Class12 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS12_COLUMN_HEADER = "Resource Class12";

		/// <summary>
		/// ProPricer Description13 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION13_COLUMN_HEADER = "ProPricer Description13";

		/// <summary>
		/// ProPricer Resource Class13 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS13_COLUMN_HEADER = "Resource Class13";

		/// <summary>
		/// ProPricer Description14 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION14_COLUMN_HEADER = "ProPricer Description14";

		/// <summary>
		/// ProPricer Resource Class14 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS14_COLUMN_HEADER = "Resource Class14";

		/// <summary>
		/// ProPricer Description15 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION15_COLUMN_HEADER = "ProPricer Description15";

		/// <summary>
		/// ProPricer Resource Class15 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS15_COLUMN_HEADER = "Resource Class15";

		/// <summary>
		/// ProPricer Description21 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION21_COLUMN_HEADER = "ProPricer Description21";

		/// <summary>
		/// ProPricer Resource Class21 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS21_COLUMN_HEADER = "Resource Class21";

		/// <summary>
		/// ProPricer Description22 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION22_COLUMN_HEADER = "ProPricer Description22";

		/// <summary>
		/// ProPricer Resource Class22 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS22_COLUMN_HEADER = "Resource Class22";

		/// <summary>
		/// ProPricer Description23 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION23_COLUMN_HEADER = "ProPricer Description23";

		/// <summary>
		/// ProPricer Resource Class23 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS23_COLUMN_HEADER = "Resource Class23";

		/// <summary>
		/// ProPricer Description24 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION24_COLUMN_HEADER = "ProPricer Description24";

		/// <summary>
		/// ProPricer Resource Class24 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS24_COLUMN_HEADER = "Resource Class24";

		/// <summary>
		/// ProPricer Description25 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION25_COLUMN_HEADER = "ProPricer Description25";

		/// <summary>
		/// ProPricer Resource Class25 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS25_COLUMN_HEADER = "Resource Class25";

		/// <summary>
		/// ProPricer Description31 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION31_COLUMN_HEADER = "ProPricer Description31";

		/// <summary>
		/// ProPricer Resource Class31 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS31_COLUMN_HEADER = "Resource Class31";

		/// <summary>
		/// ProPricer Description32 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION32_COLUMN_HEADER = "ProPricer Description32";

		/// <summary>
		/// ProPricer Resource Class32 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS32_COLUMN_HEADER = "Resource Class32";

		/// <summary>
		/// ProPricer Description33 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION33_COLUMN_HEADER = "ProPricer Description33";

		/// <summary>
		/// ProPricer Resource Class33 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS33_COLUMN_HEADER = "Resource Class33";

		/// <summary>
		/// ProPricer Description34 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION34_COLUMN_HEADER = "ProPricer Description34";

		/// <summary>
		/// ProPricer Resource Class34 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS34_COLUMN_HEADER = "Resource Class34";

		/// <summary>
		/// ProPricer Description35 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION35_COLUMN_HEADER = "ProPricer Description35";

		/// <summary>
		/// ProPricer Resource Class35 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS35_COLUMN_HEADER = "Resource Class35";

		/// <summary>
		/// ProPricer Description41 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION41_COLUMN_HEADER = "ProPricer Description41";

		/// <summary>
		/// ProPricer Resource Class41 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS41_COLUMN_HEADER = "Resource Class41";

		/// <summary>
		/// ProPricer Description42 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION42_COLUMN_HEADER = "ProPricer Description42";

		/// <summary>
		/// ProPricer Resource Class42 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS42_COLUMN_HEADER = "Resource Class42";

		/// <summary>
		/// ProPricer Description43 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION43_COLUMN_HEADER = "ProPricer Description43";

		/// <summary>
		/// ProPricer Resource Class43 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS43_COLUMN_HEADER = "Resource Class43";

		/// <summary>
		/// ProPricer Description44 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION44_COLUMN_HEADER = "ProPricer Description44";

		/// <summary>
		/// ProPricer Resource Class44 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS44_COLUMN_HEADER = "Resource Class44";

		/// <summary>
		/// ProPricer Description45 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION45_COLUMN_HEADER = "ProPricer Description45";

		/// <summary>
		/// ProPricer Resource Class45 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS45_COLUMN_HEADER = "Resource Class45";

		/// <summary>
		/// ProPricer Description51 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION51_COLUMN_HEADER = "ProPricer Description51";

		/// <summary>
		/// ProPricer Resource Class51 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS51_COLUMN_HEADER = "Resource Class51";

		/// <summary>
		/// ProPricer Description52 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION52_COLUMN_HEADER = "ProPricer Description52";

		/// <summary>
		/// ProPricer Resource Class52 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS52_COLUMN_HEADER = "Resource Class52";

		/// <summary>
		/// ProPricer Description53 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION53_COLUMN_HEADER = "ProPricer Description53";

		/// <summary>
		/// ProPricer Resource Class53 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS53_COLUMN_HEADER = "Resource Class53";

		/// <summary>
		/// ProPricer Description54 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION54_COLUMN_HEADER = "ProPricer Description54";

		/// <summary>
		/// ProPricer Resource Class54 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS54_COLUMN_HEADER = "Resource Class54";

		/// <summary>
		/// ProPricer Description55 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION55_COLUMN_HEADER = "ProPricer Description55";

		/// <summary>
		/// ProPricer Resource Class55 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS55_COLUMN_HEADER = "Resource Class55";

		/// <summary>
		/// ProPricer Description61 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION61_COLUMN_HEADER = "ProPricer Description61";

		/// <summary>
		/// ProPricer Resource Class61 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS61_COLUMN_HEADER = "Resource Class61";

		/// <summary>
		/// ProPricer Description62 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION62_COLUMN_HEADER = "ProPricer Description62";

		/// <summary>
		/// ProPricer Resource Class62 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS62_COLUMN_HEADER = "Resource Class62";

		/// <summary>
		/// ProPricer Description63 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION63_COLUMN_HEADER = "ProPricer Description63";

		/// <summary>
		/// ProPricer Resource Class63 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS63_COLUMN_HEADER = "Resource Class63";

		/// <summary>
		/// ProPricer Description64 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION64_COLUMN_HEADER = "ProPricer Description64";

		/// <summary>
		/// ProPricer Resource Class64 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS64_COLUMN_HEADER = "Resource Class64";

		/// <summary>
		/// ProPricer Description65 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION65_COLUMN_HEADER = "ProPricer Description65";

		/// <summary>
		/// ProPricer Resource Class65 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS65_COLUMN_HEADER = "Resource Class65";

		/// <summary>
		/// ProPricer Description71 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION71_COLUMN_HEADER = "ProPricer Description71";

		/// <summary>
		/// ProPricer Resource Class71 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS71_COLUMN_HEADER = "Resource Class71";

		/// <summary>
		/// ProPricer Description72 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION72_COLUMN_HEADER = "ProPricer Description72";

		/// <summary>
		/// ProPricer Resource Class72 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS72_COLUMN_HEADER = "Resource Class72";

		/// <summary>
		/// ProPricer Description73 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION73_COLUMN_HEADER = "ProPricer Description73";

		/// <summary>
		/// ProPricer Resource Class73 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS73_COLUMN_HEADER = "Resource Class73";

		/// <summary>
		/// ProPricer Description74 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION74_COLUMN_HEADER = "ProPricer Description74";

		/// <summary>
		/// ProPricer Resource Class74 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS74_COLUMN_HEADER = "Resource Class74";

		/// <summary>
		/// ProPricer Description75 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION75_COLUMN_HEADER = "ProPricer Description75";

		/// <summary>
		/// ProPricer Resource Class75 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS75_COLUMN_HEADER = "Resource Class75";

		/// <summary>
		/// ProPricer Description81 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION81_COLUMN_HEADER = "ProPricer Description81";

		/// <summary>
		/// ProPricer Resource Class81 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS81_COLUMN_HEADER = "Resource Class81";

		/// <summary>
		/// ProPricer Description82 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION82_COLUMN_HEADER = "ProPricer Description82";

		/// <summary>
		/// ProPricer Resource Class82 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS82_COLUMN_HEADER = "Resource Class82";

		/// <summary>
		/// ProPricer Description83 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION83_COLUMN_HEADER = "ProPricer Description83";

		/// <summary>
		/// ProPricer Resource Class83 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS83_COLUMN_HEADER = "Resource Class83";

		/// <summary>
		/// ProPricer Description84 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION84_COLUMN_HEADER = "ProPricer Description84";

		/// <summary>
		/// ProPricer Resource Class84 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS84_COLUMN_HEADER = "Resource Class84";

		/// <summary>
		/// ProPricer Description85 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION85_COLUMN_HEADER = "ProPricer Description85";

		/// <summary>
		/// ProPricer Resource Class85 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS85_COLUMN_HEADER = "Resource Class85";

		/// <summary>
		/// ProPricer Description91 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION91_COLUMN_HEADER = "ProPricer Description91";

		/// <summary>
		/// ProPricer Resource Class91 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS91_COLUMN_HEADER = "Resource Class91";

		/// <summary>
		/// ProPricer Description92 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION92_COLUMN_HEADER = "ProPricer Description92";

		/// <summary>
		/// ProPricer Resource Class92 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS92_COLUMN_HEADER = "Resource Class92";

		/// <summary>
		/// ProPricer Description93 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION93_COLUMN_HEADER = "ProPricer Description93";

		/// <summary>
		/// ProPricer Resource Class93 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS93_COLUMN_HEADER = "Resource Class93";

		/// <summary>
		/// ProPricer Description94 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION94_COLUMN_HEADER = "ProPricer Description94";

		/// <summary>
		/// ProPricer Resource Class94 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS94_COLUMN_HEADER = "Resource Class94";

		/// <summary>
		/// ProPricer Description95 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_DESCRIPTION95_COLUMN_HEADER = "ProPricer Description95";

		/// <summary>
		/// ProPricer Resource Class95 Column Header
		/// </summary>
		public static readonly string PRO_PRICER_RESOURCE_CLASS95_COLUMN_HEADER = "Resource Class95";

		#endregion

		// data validation defined names

		/// <summary>
		/// Categories data validation
		/// </summary>
		public static readonly string CATEGORIES = "Categories";

		/// <summary>
		/// Sections data validation
		/// </summary>
		public static readonly string SECTIONS = "Sections";

		/// <summary>
		/// Resource Types data validation
		/// </summary>
		public static readonly string RESOURCE_TYPES = "ResourceTypes";

		/// <summary>
		/// Rate Types data validation
		/// </summary>
		public static readonly string RATE_TYPES = "RateTypes";

		/// <summary>
		/// Disclosure Types data validation
		/// </summary>
		public static readonly string DISCLOSURE_TYPES = "DisclosureTypes";

		/// <summary>
		/// Resource Classes data validation
		/// </summary>
		public static readonly string RESOURCE_CLASSES = "ResourceClasses";

		/// <summary>
		/// Government Burden Pools data validation
		/// </summary>
		public static readonly string GOVERNMENT_BURDEN_POOLS = "GovernmentBurdenPools";

		/// <summary>
		/// Commercial Burden Pools data validation
		/// </summary>
		public static readonly string COMMERCIAL_BURDEN_POOLS = "CommercialBurdenPools";
	}
}
