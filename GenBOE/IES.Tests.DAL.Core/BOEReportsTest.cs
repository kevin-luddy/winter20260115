// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Tables;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.IO.Export;
	using GenBOE.DataBridge.Core.ModelView;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.OfficeUtilities;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Test Class for BOE Reports
	/// </summary>
	[TestClass]
	public class BOEReportsTest : BOEExportUtilities
	{
		private static Document LoadTemplate()
		{
			Document doc;
			using (MemoryStream ms = new (Properties.Resources.UCOTTemplate))
			{
				doc = new Document(ms);
			}

			return doc;
		}

		#region Word Utilities 

		/// <summary>
		/// Update the text inside of an SDT
		/// </summary>
		[TestMethod]
		public void GetTaggedElementTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNotNull(sdt);
			Assert.AreEqual("Program Name", sdt.GetText());
		}

		/// <summary>
		/// Gets the last matching child by Title
		/// </summary>
		[TestMethod]
		public void GetLastMatchingChildSDTTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			StructuredDocumentTag sdt = WordUtilities.GetLastMatchingChildSDTByTag(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNotNull(sdt);
			Assert.AreEqual("Program Name", sdt.GetText());
		}

		/// <summary>
		/// Get a matching child by Tag
		/// </summary>
		[TestMethod]
		public void GetTaggedChildElementTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			StructuredDocumentTag sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNotNull(sdt);
			Assert.AreEqual("Program Name", sdt.GetText());
		}

		/// <summary>
		/// Remove a SDT Ancestor by Tag
		/// </summary>
		[TestMethod]
		public void RemoveTaggedElementTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			WordUtilities.RemoveTaggedElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			StructuredDocumentTag sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNull(sdt);

			Assert.IsFalse(boeContainer.Range.StructuredDocumentTags.Any(s => s.Title == BOEExporterConstants.FieldName_ProgramName));
		}

		/// <summary>
		/// Remove Tagged Element Ancestor
		/// </summary>
		[TestMethod]
		public void RemoveTaggedElementAncestorTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			WordUtilities.RemoveTaggedElementAncestor(sdt, BOEExporterConstants.FieldName_ProgramName, NodeType.StructuredDocumentTag);

			StructuredDocumentTag programName = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNull(programName);
		}

		/// <summary>
		/// Remove Ancestor Table Row that has a child SDT with this Tag, starting the search from element
		/// </summary>
		[TestMethod]
		public void RemoveTableRowWithTaggedElementTest()
		{ 
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);
			int numRows = table.Rows.Count;
			StructuredDocumentTag boeHeaderElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Container_BOEHeader);
			WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_ProgramName);

			// now that we removed the container row, let's get the table and see how many rows there are
			table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);
			Assert.AreEqual(numRows - 1, table.Rows.Count);

			boeHeaderElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Container_BOEHeader);
			Assert.IsNotNull(boeHeaderElement);

			StructuredDocumentTag programName = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNull(programName);
		}

		/// <summary>
		/// Removes an entire column from a table.
		/// </summary>
		[TestMethod]
		public void RemoveColumnFromTableTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);

			int numColumns = table.FirstRow.Cells.Count;
			StructuredDocumentTag sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			WordUtilities.RemoveColumnFromTable(sdt, false);

			Assert.AreEqual(numColumns - 1, table.FirstRow.Cells.Count);

		}

		/// <summary>
		/// Removes an entire column from a table.
		/// </summary>
		[TestMethod]
		public void RemoveColumnFromTableTest2()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);

			int numColumns = table.FirstRow.Cells.Count;
			StructuredDocumentTag sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			WordUtilities.RemoveColumnFromTable(sdt, true);

			Assert.AreEqual(numColumns - 1, table.FirstRow.Cells.Count);
			Assert.AreEqual(100, table.PreferredWidth.Value);
			Assert.AreEqual(PreferredWidthType.Percent, table.PreferredWidth.Type);
		}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNotNull(sdt);
			Assert.AreEqual("Program Name", sdt.GetText());

			WordUtilities.SetElementText(sdt, "Test", "two lines");

			Assert.IsTrue(sdt.GetText().StartsWith("Test"));

			// now pull the SDT from the document again and test it
			sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsTrue(sdt.GetText().StartsWith("Test"));
		}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextRunTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNotNull(sdt);
			Run? run = sdt.GetChild(NodeType.Run, 0, true) as Run;
			Assert.IsNotNull(run);

			Assert.AreEqual("Program Name", run.GetText());

			WordUtilities.SetElementText(run, "Test");

			Assert.AreEqual("Test", run.GetText());

			// now pull the SDT from the document again and test it
			sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			run = sdt.GetChild(NodeType.Run, 0, true) as Run;
			Assert.AreEqual("Test", run.GetText());
		}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		[TestMethod]
		public void SetElementTextCompositeNodeTest()
		{
			Document doc = LoadTemplate();
			CompositeNode sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);

			WordUtilities.SetElementText(sdt, "Test");

			Assert.AreEqual("Test", sdt.GetText());
		}

		/// <summary>
		/// Sets text within (the FIRST of a set of) Run elements
		/// </summary>
		[TestMethod]
		public void SetElementTextMultipleRunMultipleTextInputTest()
		{
			Document doc = LoadTemplate();
			CompositeNode sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);

			WordUtilities.SetElementText(sdt, "Test", "multiple");

			Assert.AreEqual("Test" + ControlChar.LineBreak + "multiple", sdt.GetText());
		}

		/// <summary>
		/// Updates the Hours labels to EPs if needed.
		/// </summary>
		[TestMethod]
		public void UpdateHoursLabelTest()
		{
			Document doc = LoadTemplate();
			string beforeText = doc.GetText();
			WordUtilities.UpdateHoursLabel(doc);

			string afterText = doc.GetText();

			Assert.IsFalse(afterText.Contains("Hours"));
			Assert.IsTrue(afterText.Contains("EPs"));
		}

		/// <summary>
		/// Sets the text element w/ HTML formatted text (from Rich Text Editor) and appends it to the node passed in the element parameter
		/// </summary>
		[TestMethod]
		public void SetElementTextWithHTMLTest()
		{
			// Note:  Current code does not do cleanup...is that still needed?
			string html = @"<p><strong>BoldedText</strong></p><p>NextParagraph</p><p></p>";

			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			ChunkCounter counter = new ();
			WordUtilities.SetElementTextWithHTML(doc, sdt, html, ref counter);

			// now we compare to the output
			if (sdt.ParentNode is Paragraph paragraph)
			{
				Assert.AreEqual("BoldedText", paragraph.LastChild.GetText());
				Assert.AreEqual("NextParagraph", ((Paragraph)paragraph.NextSibling).LastChild.GetText());
			}
			else
			{
				Assert.AreEqual("BoldedText NextParagraph", sdt.GetText());
				HashSet<Paragraph> paragraphs = sdt.GetChildNodes<Paragraph>(NodeType.Paragraph, true);

				Assert.AreEqual(2, paragraphs.Count);
				Assert.AreEqual("BoldedText", paragraphs.First().GetText());
				Assert.AreEqual("NextParagraph", paragraphs.Last().GetText());
			}
		}

		/// <summary>
		/// Sets the text element w/ HTML formatted text (from Rich Text Editor) and appends it to the node passed in the element parameter
		/// </summary>
		[TestMethod]
		public void SetElementTextWithHTMLTest2()
		{
			// Note:  Current code does not do cleanup...is that still needed?
			string html = @"<p>SADE 1/15/12 - 12/31/13, Chg # 345678-4321, 15,275 hours for Meeting attendance and coordination.</p>
<p>&nbsp;</p>
<table style=""width: 384px; border-collapse: collapse;"" border=""0"" cellspacing=""0"" cellpadding=""0""><colgroup> <col style=""width: 48pt;"" span=""6"" width=""64"" /></colgroup>
<tbody>
<tr style=""height: 14.4pt;"">
<td class=""xl65"" style=""width: 48pt; height: 14.4pt;"" width=""64"" height=""19"">HEADER</td>
<td class=""xl65"" style=""border-left: medium none; width: 48pt;"" width=""64"">1</td>
<td class=""xl65"" style=""border-left: medium none; width: 48pt;"" width=""64"">2</td>
<td class=""xl65"" style=""border-left: medium none; width: 48pt;"" width=""64"">3</td>
<td class=""xl65"" style=""border-left: medium none; width: 48pt;"" width=""64"">4</td>
<td class=""xl65"" style=""border-left: medium none; width: 48pt;"" width=""64"">5</td>
</tr>
<tr style=""height: 14.4pt;"">
<td class=""xl66"" style=""height: 14.4pt; border-top: medium none;"" height=""19"">TEXT</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
</tr>
<tr style=""height: 14.4pt;"">
<td class=""xl66"" style=""height: 14.4pt; border-top: medium none;"" height=""19"">TEXT</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
</tr>
<tr style=""height: 14.4pt;"">
<td class=""xl66"" style=""height: 14.4pt; border-top: medium none;"" height=""19"">TEXT</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
</tr>
<tr style=""height: 14.4pt;"">
<td class=""xl66"" style=""height: 14.4pt; border-top: medium none;"" height=""19"">TEXT</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
<td class=""xl67"" style=""border-left: medium none; border-top: medium none;""><span style=""mso-spacerun: yes;"">&nbsp;</span>$<span style=""mso-spacerun: yes;"">&nbsp;&nbsp; </span>100.0</td>
</tr>
</tbody>
</table>";

			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			ChunkCounter counter = new();
			WordUtilities.SetElementTextWithHTML(doc, sdt, html, ref counter);

			// now we compare to the output
			if (sdt.ParentNode is Paragraph paragraph)
			{
				Assert.IsTrue(paragraph.LastChild is Run);
			}
		}

		/// <summary>
		/// Traverse the Word document XML tree and remove any content controls encountered
		/// </summary>
		[TestMethod]
		public void RemoveContentControlsTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			WordUtilities.SetElementText(sdt, "TestText");

			WordUtilities.RemoveContentControls(doc);

			sdt = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNull(sdt);

			Assert.IsTrue(doc.GetText().Contains("TestText"));
		}

		/// <summary>
		/// Enforce valid XML formats by adding and/or removing elements
		/// All Table Cells should have a Paragraph as their last child
		/// </summary>
		[TestMethod]
		public void CleanupDocumentXmlTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);

			Table newTable = new (doc);
			newTable.AppendChild(new Row(doc));
			newTable.FirstRow.AppendChild(new Cell(doc));
			newTable.FirstRow.FirstCell.AppendChild(new Run(doc, "child table"));
			table.FirstRow.FirstCell.AppendChild(newTable);

			// this should add a paragraph after this new table
			WordUtilities.CleanupDocumentXml(doc);
			table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Cell firstCell = table.FirstRow.FirstCell;
			
			newTable = firstCell.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(newTable);

			NodeCollection children = firstCell.GetChildNodes(NodeType.Any, false);
			Assert.IsTrue(children.Last() is Paragraph);
		}

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

			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);
			Row firstRow = table.FirstRow;

			Row clonedRow = this.CloneMarkedTemplateRow(firstRow);

			// now test to make sure the SDT id is different inside of it
			StructuredDocumentTag firstSDT = firstRow.GetChild(NodeType.StructuredDocumentTag, 0, true) as StructuredDocumentTag;
			StructuredDocumentTag clonedSDT = clonedRow.GetChild(NodeType.StructuredDocumentTag, 0, true) as StructuredDocumentTag;

			Assert.IsNotNull(clonedSDT);
			Assert.IsNull(clonedSDT.Placeholder);
			Assert.AreNotEqual(firstSDT.Id, clonedSDT.Id);
		}

		/// <summary>
		/// Remove an element from the document (DOM)
		/// </summary>
		[TestMethod]
		public void RemoveElementTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			this.RemoveElement(boeContainer);
			boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNull(boeContainer);
		}

		/// <summary>
		/// Remove the table row containing the element
		/// If no row, just remove the element
		/// </summary>
		[TestMethod]
		public void RemoveElementRowTest()
		{
			Document doc = LoadTemplate();
			StructuredDocumentTag boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNotNull(boeContainer);

			Table? table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);
			int numRows = table.Rows.Count;
			StructuredDocumentTag sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			
			this.RemoveElementRow(sdt);

			sdt = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_ProgramName);
			Assert.IsNull(sdt);

			table = boeContainer.GetChild(NodeType.Table, 0, true) as Table;
			Assert.IsNotNull(table);
			Assert.AreEqual(numRows - 1, table.Rows.Count);

			// Now test that we can remove an element row that is not inside a table
			this.RemoveElementRow(boeContainer);
			boeContainer = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_BOE);
			Assert.IsNull(boeContainer);
		}

		/// <summary>
		/// Populate the MOQ Type Data
		/// </summary>
		[TestMethod]
		public void PopulateMOQTypeDataTest()
		{
			Document doc = LoadTemplate();
			BOEExportInputs exportInputs = GetExportInputs();
			BOEExportTaskElement task = GetTaskElement(exportInputs);

			StructuredDocumentTag moqElement = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_MOQSelection);
			ChunkCounter counter = new();
			this.PopulateMOQTypeData(task, [], doc, moqElement, false, exportInputs, ref counter);

			Assert.Fail();
		}

		/// <summary>
		/// Populate the MOQ Type Data
		/// </summary>
		[TestMethod]
		public void PopulateMOQTypeDataTest2()
		{
			Document doc = LoadTemplate();
			BOEExportInputs exportInputs = GetExportInputs();
			BOEExportTaskElement task = GetTaskElement(exportInputs);

			StructuredDocumentTag moqElement = WordUtilities.GetTaggedElement(doc, BOEExporterConstants.Container_MOQSelection);
			ChunkCounter counter = new();
			this.PopulateMOQTypeData(task, [], doc, moqElement, true, exportInputs, ref counter);

			Assert.Fail();
		}

		/// <summary>
		/// Populate the Skill Mix Table
		/// </summary>
		[TestMethod]
		public void ProcessSkillMixTableTest()
		{
			Document doc = LoadTemplate();
			BOEExportInputs exportInputs = GetExportInputs();
			BOEExportTaskElement task = GetTaskElement(exportInputs);
			StructuredDocumentTag taskContainer = WordUtilities.GetTaggedElement(doc, "TaskContainer-Labor");
			this.ProcessSkillMixTable(task, [], taskContainer, exportInputs);
			Assert.Fail();
		}

		/// <summary>
		/// Populate the Skill Mix Table
		/// </summary>
		[TestMethod]
		public void ProcessSkillMixTableTest2()
		{
			Document doc = LoadTemplate();
			BOEExportInputs exportInputs = GetExportInputs();
			BOEExportTaskElement task = GetTaskElement(exportInputs);

			StructuredDocumentTag taskContainer = WordUtilities.GetTaggedElement(doc, "TaskContainer-Labor");
			this.ProcessSkillMixTable(task, [BoeCustomReportComponent.TaskDescription, BoeCustomReportComponent.TaskMOQEquation], taskContainer, exportInputs);
			Assert.Fail();
		}

		#endregion BOEExportUtilities 	

		#region Helper Methods

		/// <summary>
		/// Generates a Test Task Element
		/// </summary>
		/// <returns>Test Task Element</returns>
		private static BOEExportTaskElement GetTaskElement(BOEExportInputs exportInputs)
		{
			return new BOEExportTaskElement
			{
				BoeID = 1,
				BOETaskDesc = "BOE Task Description test",
				TaskTitle = "This is the task title test",
				BOETaskElementID = 1,
				BOETaskID = "100",
				StartDate = new DateTime(2025, 01, 15),
				EndDate = new DateTime(2032, 01, 15),
				HasTMRates = false,
				SourceOfData = "this is the source of data test",
				ElementType = BOEExportTaskElementType.Labor,
				MOQEquation = "2000 hrs",
				MOQTotal = "2000",
				MOQType = MOQType.Historical.GetDescription(),
				BOETaskElementOrder = 1,
				MOQText = "Selected MOQ Text",
				MOQTypes = exportInputs.MOQTypes.ToList(),
				MOQVariableModelViews = [],
				WorkspaceVariables = [],
				OrdinaryVariables = [],
				taskElementLabors =
				[
					new BOEExportTaskElementLabor
					{
						StartDate = new DateTime(2025, 01, 15),
						EndDate = new DateTime(2032, 01, 15),
						Hours = 2000,
						CustomFields = [],
						ElementType = BOEExportTaskElementType.Labor,
						LaborTypeOrder = 0
					}
				]
			};
		}

		private static BOEExportInputs GetExportInputs()
		{
			ExportBoeWordRequestViewModel viewModel = new()
			{
				ExportFormatDTO = new GenBOE.DataBridge.Core.WorkspaceExportFormatDTO(),
				AllWorkspaceBoes = [],
				AssignedBoeIdsAndCustomFieldValuesMapping = [],
				BoeIdsAndLastUserToSubmitThemForApprovalMapping = [],
				BoeExportModelViews = [],
				BoeMappingWithApproverResponses = [],
				Boes = [],
				BoeSummaryGridModelViews = [],
				GetUserDataForBoesForWs = [],
				Clins = [],
				CustomFields = [],
				CustomFieldValues = [],
				IsCustomExport = true,
				PerDiemsForTravelTrips = [],
				EscalationRates = [],
				RTETemplatesOverrides = [],
				ResourcesForSystemResourceListId = [],
				ResourcesForWsResourceListId = [],
				ResourcesUsedInWsBoes = [],
				MiscTravelRatesForTravelTrips = [],
				LaborTypesMappingWithCustomFieldsValuesAndContainerIds = [],
				LocationsUsedByTrips = [],
				PerformingOrgsForWsList = [],
				Odcs = [],
				MOQTypes = [
					new() {
						DescriptionHoursRequired = "This is description test",
						SmeDurationLogic = "SME Duration logic test",
						TableData =
						[
							new() {
								AdditionalQueryFilters = "N/A",
								DateOfReport = DateTime.Now,
								ContractNumber = "Contract 1",
								RepositoryName = RepositoryName.SapWebi.GetDescription(),
								TotalWbsHours = 200m,
								TotalRelevantHours = 200m,
								TableName = "table 1",
								Id = 1,
								Order = 1,
								ResourceHours = new Collection<MOQTypeSelectionTableDataResourceHoursDTO>
								{

								},
								CustomFieldValueContainers = [],
								MOQTypeSelectionId = 1,
								HistoricalProgramName = "Historical Program Test",
								WbsElement = "WBS12345678",
								PoPStart = new DateTime(2020, 1, 15),
								PoPEnd = new DateTime(2021, 1, 15)
							}
						],
						BoeId = 1,
						TaskId = 1,
						HistoricalReferenceExplanation = "Historical Ref Test",
						Id = 1,
						Order = 0,
						SelectedMOQType = MOQType.Historical,
						Rationale = "Rationale test",
						SkillMixRationale = "Skill Mix MOQ Rationale test"
					}
				],
				Materials = [],
				TaskElementsMappingWithCustomFieldsValuesAndContainerIds = [],
				TaskElements = [],
				Travels = [],
				TravelTrips = [],
				PerformingOrgsUsedInBoes = [],
				SegmentedOutput = false,
				SelectedComponents = [],
				SummarizeByCustomField = "CLIN",
				WbsElements = [],
				Workspace = new ()
				{

				},
				WorkspaceHistory = [],
				WorkspaceVariables = [],
			};
			BOEExportInputs exportInputs = new (viewModel);

			return exportInputs;
		}

		#endregion Helper Methods
	}
}
