// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using DataBridge.Loaders;
	using DataBridge.ModelViews;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Test Class for BOE Reports
	/// </summary>
	[TestClass]
	public class BOEReportsTest
	{
		#region Word Utilities 

		/// <summary>
		/// Update the text inside of an SDT
		/// </summary>
		[TestMethod]
		public void GetTaggedElementTest()
		{
		}

		/// <summary>
		/// Gets the last matching child by Title
		/// </summary>
		[TestMethod]
		public void GetLastMatchingChildSDTTest()
		{ }

		/// <summary>
		/// Get a matching child by Tag
		/// </summary>
		[TestMethod]
		public void GetTaggedChildElementTest()
		{ }

		/// <summary>
		/// Remove a SDT Ancestor by Tag
		/// </summary>
		[TestMethod]
		public void RemoveTaggedElementTest()
		{ }

		/// <summary>
		/// Remove Tagged Element Ancestor
		/// </summary>
		[TestMethod]
		public void RemoveTaggedElementAncestorTest()
		{ }

		/// <summary>
		/// Remove Ancestor Table Row that has a child SDT with this Tag, starting the search from element
		/// </summary>
		[TestMethod]
		public void RemoveTableRowWithTaggedElementTest()
		{ }

		/// <summary>
		/// Removes the corresponding label from the table cell for the tagged element
		/// </summary>
		[TestMethod]
		public void RemoveCellLabelWithTaggedElementTest()
		{ }

		/// <summary>
		/// Removes an entire column from a table.
		/// </summary>
		[TestMethod]
		public void RemoveColumnFromTableTest()
		{ }

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextTest()
		{}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextRunTest()
		{ }

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextCompositeNodeTest()
		{ }

		/// <summary>
		/// Sets the text of a run within a Content Element, but uses SDT as the inElement param
		/// </summary>
		[TestMethod]
		public void SetElementTextCompositeNodeTest2()
		{ }

		/// <summary>
		/// Sets text within (the FIRST of a set of) Run elements
		/// </summary>
		[TestMethod]
		public void SetElementTextMultipleRunMultipleTextInputTest()
		{ }

		/// <summary>
		/// Updates the Hours labels to EPs if needed.
		/// </summary>
		[TestMethod]
		public void UpdateHoursLabelTest()
		{

		}

		/// <summary>
		/// Resets the width of the table to the specified percentage of the page width.
		/// </summary>
		[TestMethod]

		public void ResetTableWidthPercentageTest()
		{ }

		/// <summary>
		/// Sets the text element w/ HTML formatted text (from Rich Text Editor) and appends it to the node passed in the element parameter
		/// </summary>
		[TestMethod]
		public void SetElementTextWithHTMLTest()
		{
			// Note:  Current code does not do cleanup...is that still needed?
		}

		/// <summary>
		/// Traverse the Word document XML tree and remove any content controls encountered
		/// </summary>
		[TestMethod]
		public void RemoveContentControlsTest()
		{ }

		/// <summary>
		/// Enforce valid XML formats by adding and/or removing elements
		/// All Table Cells should have a Paragraph as their last child
		/// </summary>
		[TestMethod]
		public void CleanupDocumentXmlTest()
		{ }

		/// <summary>
		/// Move the contents of the "parent" element to a node just after itself.
		/// </summary>
		[TestMethod]
		public void MoveChildrenAfterInsertionPointTest()
		{ }

		#endregion Word Utilities 

		#region BOEExportUtilities 

		/// <summary>
		/// Clone a table row
		/// </summary>
		[TestMethod]
		public void CloneMarkedTemplateRowTest()
		{ 
			// Note:  need to test if when cloning a row will the cloned SDT have the same ID or not.  
			// If so, not sure how we will fix that
		}

		/// <summary>
		/// Remove an element from the document (DOM)
		/// </summary>
		[TestMethod]
		public void RemoveElementTest()
		{ }

		/// <summary>
		/// Remove the table row containing the element
		/// If no row, just remove the element
		/// </summary>
		[TestMethod]
		public void RemoveElementRowTest()
		{ }

		/// <summary>
		/// Populate the MOQ Type Data
		/// </summary>
		[TestMethod]
		public void PopulateMOQTypeData()
		{ }

		/// <summary>
		/// Populate the Skill Mix Table
		/// </summary>
		[TestMethod]
		public void ProcessSkillMixTableTest()
		{ }

		#endregion BOEExportUtilities 


		/// <summary>
		/// Update Table Properties
		/// </summary>
		[TestMethod]
		public void UpdateTablePropertiesTest()
		{ }

		/// <summary>
		/// Update Table Row Properties
		/// </summary>
		[TestMethod]
		public void UpdateTableRowPropertiesTest()
		{ }

		/// <summary>
		/// Update Table Cell Properties
		/// </summary>
		[TestMethod]
		public void UpdateTableCellPropertiesTest()
		{ }

		/// <summary>
		/// Determine Parent Template ID (from Version tag)
		/// </summary>
		[TestMethod]
		public void DetermineParentTemplateIdTest()
		{ }

		


	}
}
