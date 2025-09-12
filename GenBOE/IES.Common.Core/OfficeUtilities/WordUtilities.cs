// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.OfficeUtilities
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using Aspose.Words;
	using Aspose.Words.Drawing;
	using Aspose.Words.Layout;
	using Aspose.Words.Markup;
	using Aspose.Words.Tables;
	using DocumentFormat.OpenXml;
	using IES.Common.Core.Constants;

	[ExcludeFromCodeCoverage]
	public static class WordUtilities
	{
		#region CommonConstants

		public const char NEWLINE_CHAR = '\n';

		private static Regex CONTROL_CHAR_REPLACE  = new Regex(@"![\P{Cc}\P{Cn}\P{Cs}]");

		#endregion

		#region Utility methods

		#region Find items

		/// <summary>
		/// Finds a special element in the landscape template that is used to mark the location just before the
		/// repeatable BOE details table.
		/// </summary>
		/// <param name="document">The Word document to search</param>
		/// <returns>Table element representing the spot just before the repeatable BOE details table row</returns>
		public static StructuredDocumentTag GetTaggedElement(Document document, string tag)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}

			return document.Range.StructuredDocumentTags.GetByTag(tag) as StructuredDocumentTag;
		}

		/// <summary>
		/// Override to convert from NodeType to real class Type
		/// </summary>
		/// <typeparam name="T">The Class Type</typeparam>
		/// <param name="compositeNode">The node ancestor to search inside</param>
		/// <param name="nodeType">The node type to find</param>
		/// <param name="isDeep">Do we search children's children recursively?</param>
		/// <returns>Hash Set of the found Nodes</returns>
		/// <exception cref="ArgumentException">Thrown if the NodeType does not match Type T</exception>
		public static HashSet<T> GetChildNodes<T>(this CompositeNode compositeNode, NodeType nodeType, bool isDeep) where T : Node
		{
			HashSet<T> result = new HashSet<T>();

			NodeCollection coll = compositeNode.GetChildNodes(nodeType, isDeep);
			foreach (Node node in coll)
			{
				if (node is T)
				{
					result.Add((T)node);
				}
				else
				{
					throw new ArgumentException("NodeType does not match T");
				}
			}

			return result;
		}

		/// <summary>
		/// Get Last matching Child by the SDT's Title
		/// </summary>
		/// <param name="compositeNode">The node to search</param>
		/// <param name="match">The Title to match</param>
		/// <param name="comparison">How will the Comparison work</param>
		/// <returns>The last matching child of the composite node that matches the sdt by Tag or Title.  Or null.</returns>
		public static StructuredDocumentTag GetLastMatchingChildSDTByTag(this CompositeNode compositeNode, string match, StringComparison comparison = StringComparison.CurrentCulture)
		{
			NodeCollection collection = compositeNode.GetChildNodes(NodeType.StructuredDocumentTag, true);
			StructuredDocumentTag found = collection.LastOrDefault(compositeNode => compositeNode is StructuredDocumentTag sdt && (match.Equals(sdt.Tag, comparison) || match.Equals(sdt.Title, comparison))) as StructuredDocumentTag;

			return found;
		}

		/// <summary>
		/// Returns if there are any matching child by the SDT's Title
		/// </summary>
		/// <param name="compositeNode">The node to search</param>
		/// <param name="match">The Title to match</param>
		/// <param name="comparison">How will the Comparison work</param>
		/// <returns>True if any child is SDT and matches the title/tag</returns>
		public static bool AnyMatchingChildContainsSDT(this CompositeNode compositeNode, ICollection<string> match, StringComparison comparison = StringComparison.CurrentCulture)
		{
			NodeCollection collection = compositeNode.GetChildNodes(NodeType.StructuredDocumentTag, true);
			return collection.Any(compositeNode => compositeNode is StructuredDocumentTag sdt && match.Any(m => (sdt.Title.Contains(m, comparison) || sdt.Tag.Contains(m, comparison))));
		}

		/// <summary>
		/// Get Last matching Child by the SDT's Title
		/// </summary>
		/// <param name="compositeNode">The node to search</param>
		/// <param name="match">The Title to match</param>
		/// <param name="comparison">How will the Comparison work</param>
		/// <returns>Gets the last matching child that is SDT</returns>
		public static StructuredDocumentTag GetLastMatchingChildSDT(this CompositeNode compositeNode, ICollection<string> match, StringComparison comparison)
		{
			NodeCollection collection = compositeNode.GetChildNodes(NodeType.StructuredDocumentTag, true);
			StructuredDocumentTag found = collection.LastOrDefault(compositeNode => compositeNode is StructuredDocumentTag sdt && match.Any(m => sdt.Title.Equals(m, comparison) || sdt.Tag.Equals(m, comparison))) as StructuredDocumentTag;

			return found;
		}

		/// <summary>
		/// Get a child element (of the given element) that has the designated tag
		/// </summary>
		/// <param name="element">Parent element</param>
		/// <param name="tag">Child element's tag</param>
		/// <returns>Child element</returns>
		public static StructuredDocumentTag GetTaggedChildElement(CompositeNode element, string tag)
		{
			if (element == null) { throw new ArgumentNullException(nameof(element)); }

			return element.GetChildNodes(NodeType.StructuredDocumentTag, true).FirstOrDefault(s => s is StructuredDocumentTag && ((StructuredDocumentTag)s).Tag == tag) as StructuredDocumentTag;
		}

		#endregion

		#region Remove items

		/// <summary>
		/// Remove SDT Ancestor by Tag
		/// </summary>
		/// <param name="element">The element to start the search</param>
		/// <param name="tag">The matching tag</param>
		public static void RemoveTaggedElement(Node element, string tag)
		{
			RemoveTaggedElementAncestor(element, tag, NodeType.StructuredDocumentTag);
		}

		/// <summary>
		/// Remove Tagged Element Ancestor
		/// </summary>
		/// <param name="element">The element to start the search</param>
		/// <param name="tag">The matching tag</param>
		/// <param name="nodeType">The type of Node to remove</param>
		internal static void RemoveTaggedElementAncestor(Node element, string tag, NodeType nodeType)
		{
			StructuredDocumentTag tagObj = element.Range.StructuredDocumentTags.GetByTag(tag) as StructuredDocumentTag;
			
			if (tagObj != null)
			{
				CompositeNode ancestorElement = nodeType == NodeType.StructuredDocumentTag ? tagObj : tagObj.GetAncestor(nodeType);

				if (ancestorElement != null)
				{
					ancestorElement.RemoveAllChildren();
					ancestorElement.Remove();
				}
			}
		}

		/// <summary>
		/// Remove Ancestor Table Row that has a child SDT with this Tag, starting the search from element
		/// </summary>
		/// <param name="element">The element to start the search</param>
		/// <param name="tag">The matching tag</param>
		public static void RemoveTableRowWithTaggedElement(Node element, string tag)
		{
			RemoveTaggedElementAncestor(element, tag, NodeType.Row);
		}

		/// <summary>
		/// Removes an entire column from a table.
		/// </summary>
		/// <param name="elementInColumnToRemove">Element contained in the table column that should be removed.</param>
		/// <param name="resizeTable">True to resize the width of the table to 100% of the page after deleting the column. False to leave the width as is.</param>
		public static void RemoveColumnFromTable(StructuredDocumentTag elementInColumnToRemove, bool resizeTable = true)
		{
			if (elementInColumnToRemove == null)
			{
				throw new ArgumentNullException(nameof(elementInColumnToRemove));
			}
			//Find the cell and row containing the StructuredDocumentTag whose column should be removed 
			Cell cell = elementInColumnToRemove.GetAncestor(NodeType.Cell) as Cell;
			Row row = cell.ParentRow;
			int columnToDelete = -1;
			int index = 0;

			//Determine which column number needs to be deleted
			foreach (Cell column in row.Cells)
			{
				if (column == cell)
				{
					columnToDelete = index;
					break;
				}
				index++;
			}
			
			//Cycle through each row in table and delete the appropriate cell number
			//Note that this assumes every row has the same number of cells.
			if (columnToDelete >= 0)
			{
				Table table = row.ParentTable;
				
				foreach (Row currentRow in table.Rows)
				{
					if (currentRow.Cells.Count >= columnToDelete + 1)
					{
						Cell cellToDelete = currentRow.Cells[columnToDelete];
						currentRow.RemoveChild(cellToDelete);
					}
				}

				if (resizeTable)
				{
					ResetTableWidthPercentage(table, 100);
				}
			}
		}

		#endregion

		#region Add items

		#endregion

		#region Get values

		public static Tuple<T, T> GetEarliestAndLatestItems<T>(ICollection<T> items, Func<T, bool> startDateCriteria, Func<T, DateTime> startDateSelector, Func<T, bool> endDateCriteria, Func<T, DateTime> endDateSelector) where T : class
		{
			T earliestItem = null;
			T latestItem = null;

			if (items != null)
			{
				earliestItem = items.Where(startDateCriteria).OrderBy(startDateSelector).FirstOrDefault();
				latestItem = items.Where(endDateCriteria).OrderByDescending(endDateSelector).FirstOrDefault();
			}

			return new Tuple<T, T>(earliestItem, latestItem);
		}

		#endregion

		#region Set values

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		/// <param name="inElement">The element to set text on</param>
		/// <param name="inValue">The value to set</param>
		public static void SetElementText(StructuredDocumentTag inElement, int inValue)
		{
			SetElementText(inElement, inValue.ToString());
		}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		/// <param name="run">The run to set</param>
		/// <param name="inText">The text to set</param>
		public static void SetElementText(Run run, string inText)
		{
			SetElementText(new Run[] { run }, inText);
		}

		/// <summary>
		/// Sets the text of a run within a Content Element
		/// </summary>
		/// <param name="inElement">The element to set text on</param>
		/// <param name="inText">The text to set</param>
		public static void SetElementText(CompositeNode inElement, params string[] inText)
		{
			if (inElement != null)
			{
				SetElementText(inElement.GetChildNodes(NodeType.Run, true).Cast<Run>(), inText);
			}
		}

		/// <summary>
		/// Sets text within (the FIRST of a set of) Run elements
		/// </summary>
		/// <param name="runElements">Set of runs elements to set text on</param>
		/// <param name="inText">The text to set</param>
		internal static void SetElementText(IEnumerable<Run> runElements, params string[] inText)
		{
			if (inText == null || inText.Length == 0)
			{
				// if inputs are null or empty
				inText = new string[] { string.Empty };
			}
			else
			{
				// disregard null inputs
				inText = inText.Where(t => t != null).ToArray();

				if (inText.Length == 0)
				{
					// if inputs are empty
					inText = new string[] { string.Empty };
				}
			}

			// Break each text value at the line breaks (newline character)
			inText = (from text in inText
					  where text != null
					  from brokenText in text.Split(NEWLINE_CHAR)
					  select brokenText).ToArray();

			// Keep ONLY the first text run in the element; remove others
			Run[] allRunElements = runElements.ToArray();
			Run textRun = runElements.Any() ? allRunElements[0] : null;

			for (int i = 1; i < allRunElements.Length; i++)
			{
				allRunElements[i].Remove();
			}

			// If a text run was found
			if (textRun != null)
			{
				textRun.Text = string.Join(ControlChar.LineBreak, inText);
			}
		}

		/// <summary>
		/// Updates the Hours labels to EPs if needed.
		/// </summary>
		/// <param name="document"></param>
		public static void UpdateHoursLabel(Document document)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}

			// Change all the text that has Hours to EPs (Equivalent Persons)
			document.Range.Replace("Hours", "EPs");
		}

		/// <summary>
		/// Resets the width of the table to the specified percentage of the page width.
		/// </summary>
		/// <param name="tableElement"></param>
		/// <param name="percent">Percentage of the page width (0-100). Defaults to 100%</param>
		private static void ResetTableWidthPercentage(Table tableElement, int percent = 100)
		{
			tableElement.PreferredWidth = PreferredWidth.FromPercent(percent);
		}

		#endregion

		#region HTML

		/// <summary>
		/// Sets the text element w/ HTML formatted text (from Rich Text Editor) and appends it to the node passed in the element parameter
		/// </summary>
		/// <param name="document">Main document part</param>
		/// <param name="element">Element to set</param>
		/// <param name="htmlFormattedText">Html</param>
		/// <param name="removeSpacing">Removes spacing under certain circumstances -> Bool for if the extra spacing before and after paragraphs should be removed</param>
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static void SetElementTextWithHTML(Document document, StructuredDocumentTag element, string htmlFormattedText, bool removeSpacing = false, bool keepParagraphs = true)
		{
			if (element != null)
			{
				// Clears out the original text
				SetElementText(element, ControlChar.LineBreak);

				if (!string.IsNullOrEmpty(htmlFormattedText))
				{
					bool skipCleanup = false;

					// do some initial prep
					htmlFormattedText = RTEUtilities.PrepareHtmlForWordExport(htmlFormattedText, removeSpacing, ref skipCleanup);

					// remove any children of the SDT
					element.RemoveAllChildren();

					Node insertionPoint = element;

					if (element.Level == MarkupLevel.Block)
					{
						element.AppendChild(new Paragraph(document));
					}
					else if (element.Level == MarkupLevel.Inline)
					{
						element.AppendChild(new Run(document));
					}

					// Aspose HACK inserting paragraphs inside of SDT that is buried inside other SDT(s) inside a Paragraph causes issues
					//     instead, we will convert any paragraphs in the SDT to line breaks
					if (!keepParagraphs)
					{
						htmlFormattedText = ReplaceParagraphTagsWithLineBreaks(htmlFormattedText);
					}

					DocumentBuilder builder = new DocumentBuilder(document);
					builder.MoveTo(insertionPoint);
					HtmlInsertOptions options = HtmlInsertOptions.RemoveLastEmptyParagraph;

					if (keepParagraphs && !removeSpacing)
					{
						// If we are keeping paragraphs and not removing spacing, then insert the last empty paragraph
						options = HtmlInsertOptions.None;
					}

					try
					{
						builder.InsertHtml(htmlFormattedText, options);
					}
					catch (InvalidOperationException)
					{
						// Error inserting the paragraph, we need to remove them
						options = HtmlInsertOptions.RemoveLastEmptyParagraph;
						htmlFormattedText = ReplaceParagraphTagsWithLineBreaks(htmlFormattedText) + ControlChar.LineBreak;
						
						element.RemoveAllChildren();
						builder.MoveToStructuredDocumentTag(element, 0);
						builder.InsertHtml(htmlFormattedText, options);
					}
				}
			}
		}

		#endregion

		#endregion

		#region Document cleanup methods

		#region Notes

		/*
         * Note:  ONLY SdtBlock and SdtRun are of type StructuredDocumentTag.  ONLY StructuredDocumentTag items are "taggable".
         *        All others (including SdtContentBlock, SdtContentRun, SdtProperties) are of type Node.
         * 
         * SdtBlock                                                         [tagged: BOEContainer]
         *     SdtProperties
         *         Tag = BOEContainer
         *     SdtContentBlock
         *         SdtBlock                                                 [tagged: TaskContainer-Labor]
         *             SdtProperties
         *                 Tag = TaskContainer-Labor
         *             SdtContentBlock
         *                 SdtBlock                                         [tagged: LaborHoursRollupTable]
         *                     SdtProperties
         *                         Tag = LaborHoursRollupTable
         *                     SdtContentBlock
         *                         Table
         *                             Row
         *                                 Cell
         *                                     Paragraph
         *                                         SdtRun                   [tagged: Resource]
         *                                             SdtProperties
         *                                                 Tag = Resource
         *                                             SdtContentRun
         *                                                 Run ==> DATA
         */

		/*
         * Note: The "element" variable is the same tagged element returned from GetTaggedChildElement()
         * 
         * Each tagged element has a child "content" (block) element:
         *     SdtContentBibliography       SdtContentBlock         SdtContentCell          SdtContentCitation
         *     SdtContentComboBox           SdtContentDate          SdtContentDocPartList   SdtContentDocPartObject
         *     SdtContentDropDownList       SdtContentEquation      SdtContentGroup         SdtContentPicture
         *     SdtContentRichText           SdtContentRow           SdtContentRun           SdtContentRunRuby
         *     SdtContentText
         * 
         * The CONTENTS of each content block must be MOVED (preserved) before the content block itself can be REMOVED.
         * In addition to the content block, the SdtProperties (that contain the TAG) must ALSO be removed.
         * 
         */

		#endregion

		#region Remove Content Controls

		/// <summary>
		/// Traverse the Word document XML tree and remove any content controls encountered
		/// </summary>
		/// <param name="wordDocument">The Word document</param>
		public static void RemoveContentControls(Document wordDocument)
		{
			if (wordDocument == null) { throw new ArgumentNullException(nameof(wordDocument)); }

			RemoveContentControls(wordDocument.GetChildNodes(NodeType.StructuredDocumentTag, true));
		}

		/// <summary>
		/// Check elements for the presence of content controls and remove any encountered
		/// </summary>
		/// <param name="elementsList">List of elements to be checked for the presence of content controls</param>
		private static void RemoveContentControls(NodeCollection elementsList)
		{
			foreach (StructuredDocumentTag node in elementsList)
			{
				RemoveContentControls(node);
			}
			
			return;
		}

		/// <summary>
		/// Check element for the presence of content controls and remove any encountered
		/// </summary>
		/// <param name="element">Element to be checked for the presence of content controls</param>
		private static void RemoveContentControls(StructuredDocumentTag element)
		{
			element.RemoveSelfOnly();
		}

		/// <summary>
		/// Defines different results/direction for the XML pre-check validation
		/// </summary>
		private enum PreCheckValidationResult
		{
			Valid = 0,
			MoveEntireElement = 1,
			MoveContentOnly = 2
		}

		/// <summary>
		/// Call this method prior to moving a child element to make sure the move would not cause invalid XML in the document.
		/// </summary>
		/// <param name="child">THe child element being moved</param>
		/// <param name="insertionPoint">The (proposed) destination of the move</param>
		/// <returns>Either valid, or an indication of how to avoid invalid XML</returns>
		private static PreCheckValidationResult PreCheckInvalidXmlCondition(Node child, Node insertionPoint)
		{
			PreCheckValidationResult result = PreCheckValidationResult.Valid;

			if ((child is Paragraph paragraph && paragraph.ParentNode != null && insertionPoint.ParentNode is Paragraph) ||  // avoid paragraph within a paragraph (invalid XML)
				(child is Table table && insertionPoint.ParentNode != null && insertionPoint.ParentNode is Paragraph))        // avoid table within a paragraph (invalid XML)
			{
				result = PreCheckValidationResult.MoveEntireElement;
			}

			return result;
		}

		/// <summary>
		/// Enforce valid XML formats by adding and/or removing elements
		/// </summary>
		/// <param name="wordDocument">Document</param>
		public static void CleanupDocumentXml(Document wordDocument)
		{
			if (wordDocument == null) { throw new ArgumentNullException(nameof(wordDocument)); }

			// make sure all table cells have a Paragraph as their last child
			ICollection<Node> allCells = wordDocument.GetChildNodes(NodeType.Cell, true).ToList();
			foreach (Cell Cell in allCells)
			{
				if (Cell.LastChild is not Paragraph)
				{
					Cell.AppendChild<Paragraph>(new Paragraph(wordDocument));
				}
			}
			DocumentBuilder builder = new DocumentBuilder(wordDocument);
			// make sure all images are not wider than the page
			ICollection<Node> shapes = wordDocument.GetChildNodes(NodeType.Shape, true).ToList();
			foreach (Shape shape in shapes)
			{
				builder.MoveTo(shape);
				PageSetup ps = builder.CurrentSection.PageSetup;
				// make sure the Height has extra space for a section header
				double targetHeight = ps.PageHeight - ps.TopMargin - ps.BottomMargin - ps.FooterDistance - ps.HeaderDistance - ConvertUtil.InchToPoint(0.5);
				double targetWidth = ps.PageWidth - ps.LeftMargin - ps.RightMargin;

				if (shape.Height > targetHeight || shape.Width > targetWidth)
				{
					double ratioX = targetWidth / shape.Width;
					double ratioY = targetHeight / shape.Height;
					double ratio = Math.Min(ratioX, ratioY);

					double newWidth = shape.Width * ratio;
					double newHeight = shape.Height * ratio;

					shape.Width = newWidth;
					shape.Height = newHeight;
				}
			}

			// make sure all tables are not wider than the page
			ICollection<Node> tables = wordDocument.GetChildNodes(NodeType.Table, true).ToList();
			LayoutCollector collector = new LayoutCollector(wordDocument);
			LayoutEnumerator enumerator = new LayoutEnumerator(wordDocument);
			foreach (Table table in tables)
			{
				// Skip nodes in the header and footer. LayoutCollector Does not work with them.
				if (table.GetAncestor(NodeType.HeaderFooter) != null)
					continue;

				// Move to first paragraph in the table
				enumerator.Current = collector.GetEntity(table.FirstRow.FirstCell.FirstParagraph);
				// And move to the row entity.
				while (enumerator.Type != LayoutEntityType.Row)
					enumerator.MoveParent();

				
				builder.MoveTo(table);
				PageSetup ps = builder.CurrentSection.PageSetup;
				// make sure the Height has extra space for a section header
				double targetWidth = ps.PageWidth - ps.LeftMargin - ps.RightMargin;
				
				// Now we can get the calculated rectangle of the row.
				if (enumerator.Rectangle.Width > targetWidth)
				{
					table.AutoFit(AutoFitBehavior.AutoFitToContents);
					
					// resize font to be smaller to fit 
					foreach (Run run in table.GetChildNodes(NodeType.Run, true))
					{
						run.Font.Size = table.Style.Font.Size / 1.5;
					}
				}
			}
		}

		/// <summary>
		/// Move the contents of the "parent" element to a node just after itself.
		/// </summary>
		/// <param name="parent">Parent element</param>
		/// <param name="insertionPoint">The insertion point</param>
		/// <returns>The (resulting) point at which to continue subsequent element insertion</returns>
		private static Node MoveChildrenAfterInsertionPoint(CompositeNode parent, Node insertionPoint)
		{
			if (insertionPoint == null)
			{
				insertionPoint = parent;
			}

			// Note: This can be resolved once/up-front because insertion points are always sibling nodes
			bool isEventualParentParagraph = (insertionPoint.ParentNode is not null and Paragraph);

			Node[] childElements = parent.GetChildNodes(NodeType.Any, false).ToArray();
			int totalChildElements = childElements.Length;

			for (int i = 0; i < totalChildElements; i++)
			{
				Node childElement = childElements[i];

				if ((childElement is Paragraph && isEventualParentParagraph) ||  // avoid paragraph within a paragraph (invalid XML)
					(childElement is Table && isEventualParentParagraph))        // avoid table within a paragraph (invalid XML)
				{
					// move the table (and every subsequent sibling node) to FOLLOW the paragraph node
					insertionPoint = insertionPoint.ParentNode;

					for (int j = i; j < totalChildElements; j++)
					{
						childElement = childElements[j];
						insertionPoint = insertionPoint.ParentNode.InsertAfter(childElement.Clone(true), insertionPoint);
					}

					i = totalChildElements;  // avoid re-processing the subsequent children
				}
				else
				{
					insertionPoint = insertionPoint.ParentNode.InsertAfter(childElement.Clone(true), insertionPoint);
				}
			}

			return insertionPoint;
		}

		#endregion

		/// <summary>
		/// Replaces paragraph tags in html text with div tags
		/// Fixes line spacing issues in export
		/// </summary>
		/// <param name="htmlText">HTML text to replace tags in</param>
		/// <returns>string with paragraph tags replaced with div tags</returns>
		public static string ReplaceParagraphTags(string htmlText)
		{
			return htmlText.Replace(CommonConstants.P_START_TAG, CommonConstants.DIV_START_TAG).Replace(CommonConstants.P_END_TAG, CommonConstants.DIV_END_TAG).Replace("\r\n", ControlChar.LineFeed).Replace(ControlChar.ParagraphBreakChar, ControlChar.LineFeedChar);
		}

		/// <summary>
		/// Replace Paragraph Tags with Line Breaks
		/// This fixes Aspose issue with inserting Paragraphs/Block elements inside Paragraph
		/// </summary>
		/// <param name="htmlText"></param>
		/// <returns></returns>
		public static string ReplaceParagraphTagsWithLineBreaks(string htmlText)
		{
			htmlText = htmlText.Replace(CommonConstants.H1_START_TAG, CommonConstants.SPAN_START_TAG).Replace(CommonConstants.H1_END_TAG, CommonConstants.SPAN_END_TAG_WITH_NewLine).Replace(CommonConstants.DIV_START_TAG, CommonConstants.SPAN_START_TAG).Replace(CommonConstants.DIV_END_TAG, CommonConstants.SPAN_END_TAG_WITH_NewLine).Replace(CommonConstants.P_START_TAG, CommonConstants.SPAN_START_TAG).Replace(CommonConstants.P_END_TAG, CommonConstants.SPAN_END_TAG_WITH_NewLine).Replace("\r\n", ControlChar.LineFeed).Replace(ControlChar.ParagraphBreakChar, ControlChar.LineFeedChar).Replace(ControlChar.LineBreakChar, ControlChar.LineFeedChar);
			
			return CONTROL_CHAR_REPLACE.Replace(ReplaceParagraphTags(htmlText), string.Empty);
		}

		#endregion
	}
}