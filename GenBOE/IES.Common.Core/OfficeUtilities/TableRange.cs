// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.OfficeUtilities
{
	using System;
	using System.Linq;
	using System.Text;
	using DocumentFormat.OpenXml.Spreadsheet;

	public class TableRange
	{
		public TableRange(String sheetName, Table table)
		{
			if (sheetName == null)
			{
				throw new ArgumentException("sheetName can't be null", nameof(sheetName));
			}
			if (table == null)
			{
				throw new ArgumentException("table can't be null", nameof(table));
			}

			Table = table;
			Begin = getRangeBegin();
			End = getRangeEnd();
			SheetName = sheetName;
			TableName = table.Name.Value;
			RowBegin = getBeginningRow();
			RowEnd = getEndingRow();
			ColumnBegin = getBeginningColumn();
			ColumnEnd = getEndingColumn();
			ColumnBeginIndex = GetColumnNumber(ColumnBegin);
			ColumnEndIndex = GetColumnNumber(ColumnEnd);
		}

		/// <summary>
		/// Gets the worksheet name where the table is located
		/// </summary>
		public string SheetName { get; private set; }

		/// <summary>
		/// Gets the original Table definition
		/// </summary>
		public Table Table { get; }

		/// <summary>
		/// Gets the table name that this TableRange object describes
		/// </summary>
		public string TableName { get; private set; }

		/// <summary>
		/// Gets the cell reference of the top left most cell in the table
		/// </summary>
		public string Begin { get; private set; }

		/// <summary>
		/// Gets the cell reference of the bottom right most cell in the table
		/// </summary>
		public string End { get; private set; }

		/// <summary>
		/// Gets the top most row reference in the table
		/// </summary>
		public uint RowBegin { get; private set; }

		/// <summary>
		/// Gets the bottom most row reference in the table
		/// </summary>
		public uint RowEnd { get; private set; }

		/// <summary>
		/// Gets the left most column reference in the table
		/// </summary>
		public string ColumnBegin { get; private set; }

		/// <summary>
		/// Gets the right most column reference in the table
		/// </summary>
		public string ColumnEnd { get; private set; }

		/// <summary>
		/// Gets the left most column index reference in the table
		/// </summary>
		public long ColumnBeginIndex { get; private set; }

		/// <summary>
		/// Gets the right most column reference in the table
		/// </summary>
		public long ColumnEndIndex { get; private set; }

		private string getRangeBegin()
		{
			return Table.Reference.Value.Split(new char[] { ':' })[0];
		}

		private string getRangeEnd()
		{
			return Table.Reference.Value.Split(new char[] { ':' })[1];
		}

		private uint getBeginningRow()
		{
			return ExcelUtilities.ParseRowIndex(getRangeBegin());
		}

		private uint getEndingRow()
		{
			return ExcelUtilities.ParseRowIndex(getRangeEnd());
		}

		private string getBeginningColumn()
		{
			return ExcelUtilities.ParseColumnName(getRangeBegin());
		}

		private string getEndingColumn()
		{
			return ExcelUtilities.ParseColumnName(getRangeEnd());
		}

		/// <summary>
		/// Takes a column name (like A, B, C, ... AA, AB, AC, etc) and 
		/// returns the one-based index number for that column (1, 2, 3, etc)
		/// </summary>
		/// <param name="name">The column name, e.g. A, B, C, ... AA, AB, AC, etc.</param>
		/// <returns>The 1-based column index.</returns>
		public static long GetColumnNumber(string name)
		{
			if (name == null)
			{
				throw new ArgumentException("name can't be null", nameof(name));
			}

			char[] chars = name.ToUpper().ToCharArray();

			return (long)(Math.Pow(26, chars.Count() - 1)) *
				(System.Convert.ToInt32(chars[0]) - 64) +
				((chars.Count() > 2) ? GetColumnNumber(name.Substring(1, name.Length - 1)) :
				((chars.Count() == 2) ? (System.Convert.ToInt32(chars[chars.Count() - 1]) - 64) : 0));
		}

		/// <summary>
		/// Takes a one-based index number for a column (1, 2, 3, etc) and
		/// returns the character column name (like A, B, C, ... AA, AB, AC, etc)
		/// </summary>
		/// <param name="number">The 1-based column index</param>
		/// <returns>The column name, e.g. A, B, C, ... AA, AB, AC, etc.</returns>
		public static String GetColumnName(long number)
		{
			StringBuilder retVal = new StringBuilder();
			int x = 0;

			for (int n = (int)(Math.Log(25 * (number + 1)) / Math.Log(26)) - 1; n >= 0; n--)
			{
				x = (int)((Math.Pow(26, (n + 1)) - 1) / 25 - 1);
				if (number > x)
				{
					retVal.Append(System.Convert.ToChar((int)(((number - x - 1) / Math.Pow(26, n)) % 26 + 65)));
				}
			}

			return retVal.ToString();
		}
	}
}
