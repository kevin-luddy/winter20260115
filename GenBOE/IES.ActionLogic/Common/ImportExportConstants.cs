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
