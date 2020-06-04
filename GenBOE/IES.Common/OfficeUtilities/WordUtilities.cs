// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;

    [ExcludeFromCodeCoverage]
    public static class WordUtilities
    {
        #region Constants

        public const char NEWLINE_CHAR = '\n';

        #endregion

        #region Utility methods

        #region Find items

        internal static Tag GetTag(ICollection<Tag> tags, string tag)
        {
            return tags.Where(s => s.Val.Value.Equals(tag, StringComparison.CurrentCultureIgnoreCase)).LastOrDefault();
        }

        internal static Tag GetTag(OpenXmlElement element, string tag)
        {
            return GetTag(element.Descendants<Tag>().ToList(), tag);
        }

        /// <summary>
        /// Finds a special element in the landscape template that is used to mark the location just before the
        /// repeatable BOE details table.
        /// </summary>
        /// <param name="document">The Word document to search</param>
        /// <returns>Table element representing the spot just before the repeatable BOE details table row</returns>
        public static SdtElement GetTaggedElement(WordprocessingDocument document, string tag)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.MainDocumentPart.Document.Descendants<SdtElement>().FirstOrDefault(s => s.Descendants<Tag>().FirstOrDefault()?.Val.Value == tag);
        }

        /// <summary>
        /// Get a child element (of the given element) that has the designated tag
        /// </summary>
        /// <param name="element">Parent element</param>
        /// <param name="tag">Child element's tag</param>
        /// <returns>Child element</returns>
        public static SdtElement GetTaggedChildElement(OpenXmlElement element, string tag)
        {
            if (element == null) { throw new ArgumentNullException(nameof(element)); }

            return element.Descendants<SdtElement>().FirstOrDefault(s => s.Descendants<Tag>().FirstOrDefault()?.Val.Value == tag);
        }

        #endregion

        #region Remove items

        public static void RemoveTaggedElement(OpenXmlElement element, string tag)
        {
            RemoveTaggedElementAncestor<SdtElement>(element, tag);
        }

        internal static void RemoveTaggedElementAncestor<T>(OpenXmlElement element, string tag) where T : OpenXmlElement
        {
            Tag tagObj = GetTag(element, tag);

            if (tagObj != null)
            {
                T ancestorElement = tagObj.Ancestors<T>().FirstOrDefault();

                if (ancestorElement != null)
                {
                    ancestorElement.RemoveAllChildren();
                    ancestorElement.Remove();
                }
            }
        }

        public static void RemoveTableRowWithTaggedElement(OpenXmlElement element, string tag)
        {
            RemoveTaggedElementAncestor<TableRow>(element, tag);
        }

        /// <summary>
        /// Removes the corresponding label from the table cell for the tagged element.
        /// </summary>
        /// <param name="element">The element to be searched.</param>
        /// <param name="tag">The tag of the element which should have its label removed.</param>
        internal static void RemoveTableCellLabelWithTaggedElement(OpenXmlElement element, string tag)
        {
            Tag tagObj = GetTag(element, tag);

            if (tagObj != null)
            {
                TableRow row = tagObj.Ancestors<TableRow>().FirstOrDefault();

                if (row != null)
                {
                    foreach (Paragraph pg in row.Descendants<Paragraph>())
                    {
                        // If we find the matching tag as a descendant then we have the correct object, so remove the paragraph/label.
                        if (GetTag(pg.Descendants<Tag>().ToList<Tag>(), tag) != null)
                        {
                            pg.RemoveAllChildren<Run>();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes an entire column from a table.
        /// </summary>
        /// <param name="elementInColumnToRemove">Element contained in the table column that should be removed.</param>
        /// <param name="resizeTable">True to resize the width of the table to 100% of the page after deleting the column. False to leave the width as is.</param>
        public static void removeColumnFromTable(SdtElement elementInColumnToRemove, bool resizeTable = true)
        {
            if (elementInColumnToRemove == null)
            {
                throw new ArgumentNullException(nameof(elementInColumnToRemove));
            }
            //Find the cell and row containing the SdtElement whose column should be removed 
            TableCell cell = elementInColumnToRemove.Ancestors<TableCell>().First();
            TableRow row = cell.Ancestors<TableRow>().First();
            int columnToDelete = -1;
            int index = 0;
            
            //Determine which column number needs to be deleted
            foreach (TableCell column in row.Elements<TableCell>())
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
                Table tableElement = row.Ancestors<Table>().First();
                ICollection<TableRow> tableRows = tableElement.Elements<TableRow>().ToList();
                foreach (TableRow currentRow in tableRows)
                {
                    TableCell cellToDelete = currentRow.Elements<TableCell>().ElementAt(columnToDelete);
                    currentRow.RemoveChild(cellToDelete);
                }

                if (resizeTable)
                {
                    ResetTableWidthPercentage(tableElement, 100);
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
        public static void SetElementText(SdtElement inElement, int inValue)
        {
            SetElementText(inElement, inValue.ToString());
        }

        /// <summary>
        /// Sets the text of a run within a Content Element
        /// </summary>
        /// <param name="inElement">The element to set text on</param>
        /// <param name="inText">The text to set</param>
        public static void SetElementText(SdtElement inElement, params string[] inText)
        {
            if (inElement != null)
            {
                SetElementText(inElement.Descendants<Run>().ToList(), inText);
            }
        }

        /// <summary>
        /// Sets the text of a run within a Content Element
        /// </summary>
        /// <param name="inElement">The element to set text on</param>
        /// <param name="inText">The text to set</param>
        public static void SetElementText(OpenXmlElement inElement, params string[] inText)
        {
            if (inElement != null)
            {
                if (inElement is Run)
                {
                    SetElementText(new List<Run> { inElement as Run }, inText);
                }
                else
                {
                    SetElementText(inElement.Descendants<Run>().ToList(), inText);
                }
            }
        }

        /// <summary>
        /// Sets text within (the FIRST of a set of) Run elements
        /// </summary>
        /// <param name="runElements">Set of runs elements to set text on</param>
        /// <param name="inText">The text to set</param>
        internal static void SetElementText(ICollection<Run> runElements, params string[] inText)
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
                Text textElement = textRun.Descendants<Text>().FirstOrDefault();

                if (textElement != null)
                {
                    // Iterate through the given text values and append each one as a text Run
                    for (int ndx = 0; ndx < inText.Length; ndx++)
                    {

                        // Get the text within the Run
                        textElement.Text = inText[ndx];

                        // If this is not the last Run, add a break to the Run
                        if (ndx < inText.Length - 1)
                        {
                            Break newlineElement = new Break();
                            textElement.InsertAfterSelf<Break>(newlineElement);

                            Text nextTextElement = textElement.CloneNode(true) as Text;
                            newlineElement.InsertAfterSelf<Text>(nextTextElement);
                            textElement = nextTextElement;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Hours labels to EPs if needed.
        /// </summary>
        /// <param name="document"></param>
        public static void UpdateHoursLabel(WordprocessingDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Change all the text that has Hours to EPs (Equivalent Persons)
            var body = document.MainDocumentPart.Document.Body;
            var texts = body.Descendants<Text>();
            foreach (var text in texts)
            {
                if (text.Text.Contains("Hours"))
                {
                    text.Text = text.Text.Replace("Hours", "EPs");
                }
            }
        }

        /// <summary>
        /// Resets the width of the table to the specified percentage of the page width.
        /// </summary>
        /// <param name="tableElement"></param>
        /// <param name="percent">Percentage of the page width (0-100). Defaults to 100%</param>
        private static void ResetTableWidthPercentage(Table tableElement, int percent = 100)
        {
            percent = 50 * percent; //Width setting uses width in fiftieths of a percent
            TableWidth width = tableElement.Descendants<TableWidth>().First();
            width.Width = percent.ToString();
            width.Type = TableWidthUnitValues.Pct;
        }

        #endregion

        #region HTML

        /// <summary>
        /// Sets the text element w/ HTML formatted text (from Rich Text Editor) and appends it to the node passed in the element parameter
        /// </summary>
        /// <param name="mainPart">Main document part</param>
        /// <param name="element">Element to set</param>
        /// <param name="htmlFormattedText">Html</param>
        /// <param name="counters">Counters for altChunk processing</param>
        /// <param name="removeSpacing">Removes spacing under certain circumstances -> Bool for if the extra spacing before and after paragraphs should be removed</param>
        /// <param name="applyInternalSectionFormatting">bool to note if internal section formatting should be applied - for use with PPRD export, false by default</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void SetElementTextWithHTML(MainDocumentPart mainPart, OpenXmlElement element, string htmlFormattedText, ref ChunkCounter counters, bool removeSpacing = false, bool applyInternalSectionFormatting = false)
        {
            if (element == null) { throw new ArgumentNullException(nameof(element)); }

            // the idea for altchunks was based on http://yeshagupta.blogspot.com/2010/06/downloading-ms-word-2007-files-for-html.html
            // it's been heavily modified & adjusted beyond what it started it..

            // Clears out the original text
            SetElementText(element, null);

            if (!string.IsNullOrEmpty(htmlFormattedText))
            {
                bool skipCleanup = false;

                // do some initial prep
                htmlFormattedText = RTEUtilities.PrepareHtmlForWordExport(htmlFormattedText, removeSpacing, ref skipCleanup);

                // Get font information for the field/element into which we are inserting the HTML.
                decimal? fontSize = RTEUtilities.GetFontSizeBasedOnWordElementXml(element);
                ICollection<string> fontFamilies = RTEUtilities.GetFontFamiliesBasedOnElementXml(element);
                SpacingDetailsForRTEWordExports paragraphSizing = RTEUtilities.GetSpacingFromXml(element);

                #region Cleanup the Element, if needed

                if (!skipCleanup)
                {
                    // This is done in order to deal with how strangly the MHTML can be inserted into the document
                    // Issues this will help correct/prevent are strange spacings before/after items, styles bleeding through from the HTML into the labels and such
                    // It needs to be done in three ways, because some fields are contained inside of a paragraph, others inside of Runs, and yet others of Runs inside of StdContentRuns

                    // we are attaching properties to an existing Run/Paragraph, in order to make sure that it basically goes "invisible", as there's no fool proof way to replace it w/ the chunk

                    // 2 half points -> font size of 1, to make it as invisible as possible
                    FontSize size = new FontSize() { Val = "2" };

                    // clear out any weird spacing
                    SpacingBetweenLines spacing = new SpacingBetweenLines() { Before = "0", After = "0", Line = "0", AfterLines = 0, BeforeLines = 0 };

                    // create a run properties object, for modifying runs
                    RunProperties runProp = new RunProperties();
                    runProp.Append(size);
                    runProp.Append(spacing);

                    if (element.GetFirstChild<Run>() != null)
                    // if the element contains the Run directly, adjust it
                    {
                        element.GetFirstChild<Run>().PrependChild<RunProperties>(runProp);
                    }
                    else if (element.GetFirstChild<SdtContentRun>() != null && element.GetFirstChild<SdtContentRun>().GetFirstChild<Run>() != null)
                    // at times the runs are inside of a Standard Content Run, in which case we need to then go one level deeper
                    {
                        element.GetFirstChild<SdtContentRun>().GetFirstChild<Run>().PrependChild<RunProperties>(runProp);
                    }
                    else if (element.GetFirstChild<Paragraph>() != null)
                    {
                        // finally, if no runs exist, it's likely that a paragraph is in place, so we have to create paragraph properties instead & use those
                        // we need to clone because the original nodes are already a part of the RunProperties..
                        ParagraphProperties parProperties = new ParagraphProperties();
                        parProperties.Append(size.CloneNode(true));
                        parProperties.Append(spacing.CloneNode(true));

                        element.RemoveAllChildren<Paragraph>(); // in weird cases this may cause formatting issues, especially w/ html containing lists; so we remove it
                        element.Append(new Paragraph()); // and replace the original one w/ a new, clean one
                        element.GetFirstChild<Paragraph>().PrependChild<ParagraphProperties>(parProperties); // to which we will then append the new styles
                    }
                }

                #endregion

                // Need this in order to be able to insert the object into Word and uniquely be able to reference it
                if (counters == null) { throw new ArgumentNullException(nameof(counters)); }
                string altChunkId = String.Format("AltChunkId{0}", counters.AltChunkCounter);

                // Add the mhtml as a Mht(ml) special part, associated w/ the id
                if (mainPart == null) { throw new ArgumentNullException(nameof(mainPart)); }
                AlternativeFormatImportPart chunk = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Mht, altChunkId);

                // Write the mhtml into this newly associated AlternativeFormatImportPart
                using (Stream chunkStream = chunk.GetStream(FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(chunkStream, Encoding.UTF8)) //Encoding.UTF8 removes special characters
                    {
                        RTEUtilities.ConvertHtmlToMhtml(writer, htmlFormattedText, fontSize, fontFamilies, paragraphSizing, applyInternalSectionFormatting);
                    }
                }

                element.Append(new AltChunk() { Id = altChunkId });
            }
        }

        #endregion

        #endregion

        #region Document cleanup methods

        #region Notes

        /*
         * Note:  ONLY SdtBlock and SdtRun are of type SdtElement.  ONLY SdtElement items are "taggable".
         *        All others (including SdtContentBlock, SdtContentRun, SdtProperties) are of type OpenXmlElement.
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
         *                             TableRow
         *                                 TableCell
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
        public static void RemoveContentControls(WordprocessingDocument wordDocument)
        {
            if (wordDocument == null) { throw new ArgumentNullException(nameof(wordDocument)); }

            RemoveContentControls(wordDocument.MainDocumentPart.Document.ChildElements);
        }

        /// <summary>
        /// Check elements for the presence of content controls and remove any encountered
        /// </summary>
        /// <param name="elementsList">List of elements to be checked for the presence of content controls</param>
        private static void RemoveContentControls(OpenXmlElementList elementsList)
        {
            OpenXmlElement[] elements = elementsList.ToArray();
            int totalElements = elements.Length;

            for (int i = 0; i < totalElements; i++)
            {
                OpenXmlElement element = elements[i];

                // process children first (depth-first)
                RemoveContentControls(element.ChildElements);

                RemoveContentControls(element);
            }

            return;
        }

        /// <summary>
        /// Check element for the presence of content controls and remove any encountered
        /// </summary>
        /// <param name="element">Element to be checked for the presence of content controls</param>
        private static void RemoveContentControls(OpenXmlElement element)
        {
            // only SdtElement items need to be "cleaned"
            if (element is SdtElement)  // SdtBlock, SdtRun, SdtCell, SdtRow, SdtRunRuby
            {
                #region If element has children
                if (element.HasChildren)
                {
                    OpenXmlElement insertionPoint = element;

                    OpenXmlElement[] childElements = element.ChildElements.ToArray();
                    int totalChildElements = childElements.Length;

                    for (int i = 0; i < totalChildElements; i++)
                    {
                        OpenXmlElement child = childElements[i];
                        bool removeChild = true;

                        if (child is SdtContentBlock || child is SdtContentRun || child is SdtContentCell || child is SdtContentRow)
                        {
                            insertionPoint = MoveChildrenAfterInsertionPoint(child, insertionPoint);
                        }
                        else if (child is SdtProperties || child is SdtEndCharProperties)
                        {
                            // skip
                            removeChild = false;
                        }
                        else
                        {
                            PreCheckValidationResult xmlValidationResult = PreCheckInvalidXmlCondition(child, insertionPoint);

                            if (xmlValidationResult == PreCheckValidationResult.MoveContentOnly)
                            {
                                /*
                                 * Move the child element's CONTENTS (but NOT the element itself)
                                 * ---------------------------------
                                 * 
                                 * Example:
                                 * 
                                 *                      Paragraph1
                                 *      [element]           SdtBlock                [insertionPoint]
                                 *      [child]                Paragraph2
                                 *                          <------ Run
                                 *                          <------ Run
                                 *                          <------ Run
                                 * 
                                 *      If the child (Paragraph2) were moved after the proposed insertion point, it would become
                                 *      a child node of Paragraph1, which is NOT valid XML.
                                 *      
                                 *      Instead, move the CONTENTS of Paragraph2 WITHIN Paragraph1.
                                 *      
                                 */

                                insertionPoint = MoveChildrenAfterInsertionPoint(child, insertionPoint);
                            }
                            else if (xmlValidationResult == PreCheckValidationResult.MoveEntireElement)
                            {
                                /*
                                 * Move the entire child element
                                 * -----------------------------
                                 * 
                                 * Example:
                                 * 
                                 *                      Paragraph                   [insertionPoint #2]
                                 *      [element]           SdtBlock                [insertionPoint #1]
                                 *      [child]         <------ Table
                                 *                          <------ TableRow
                                 *                          <------ TableRow
                                 *                          <------ TableRow
                                 *                      <------ Table
                                 *                          <------ TableRow
                                 *                          <------ TableRow
                                 *                          <------ TableRow
                                 * 
                                 *      If the child (Table) were moved after the proposed insertion point (#1), it would become
                                 *      a child node of the Paragraph, which is NOT valid XML.
                                 *      
                                 *      The child needs to be moved OUTSIDE/AFTER the Paragraph - So the NEW insertion point becomes
                                 *      insertion point #2.
                                 *      
                                 *      Furthermore, to preserve ORDERING of elements, all remaining sibling nodes of the child (that
                                 *      occur AFTER it) ALSO need to be moved (WITH the child).
                                 * 
                                 */

                                insertionPoint = insertionPoint.Parent;

                                for (int j = i; j < totalChildElements; j++)  // child and every subsequent sibling element
                                {
                                    child = childElements[j];
                                    insertionPoint = insertionPoint.InsertAfterSelf(child.CloneNode(true));
                                    child.Remove();
                                }

                                i = totalChildElements;  // avoid re-processing the subsequent children
                                removeChild = false;  // removals were already done in the above loop
                            }
                            else // Valid
                            {
                                insertionPoint = insertionPoint.InsertAfterSelf(child.CloneNode(true));
                            }
                        }

                        if (removeChild)
                        {
                            child.Remove();
                        }
                    }
                }  // end element.HasChildren
                #endregion If element has children

                #region Remove the (content control) element itself

                OpenXmlElement parent = element.Parent;
                if (parent != null)
                {
                    element.Remove();  // remove the (content control) element

                    /*
                     * If removal of the element creates an "empty-nest" (for the element's parent), then delete the parent as well.
                     * This was specifically added to eliminate extraneous line-breaks, but it removes unused nodes in general.
                     * 
                     */
                    if (parent.HasChildren)
                    {
                        if (parent.ChildElements.Any(child => (!(child is ParagraphProperties) &&
                            !(child is RunProperties) &&
                            !(child is SdtProperties) &&
                            !(child is SdtEndCharProperties) &&
                            !(child is TableCellProperties) &&
                            !(child is TableProperties) &&
                            !(child is TableRowProperties) &&
                            !(child is TableStyleProperties) &&
                            !(child is CustomXmlProperties))))
                        {
                            // this child is valid content - need parent
                        }
                        else
                        {
                            parent.Remove();
                        }
                    }
                }

                #endregion
            }
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
        private static PreCheckValidationResult PreCheckInvalidXmlCondition(OpenXmlElement child, OpenXmlElement insertionPoint)
        {
            PreCheckValidationResult result = PreCheckValidationResult.Valid;

            if ((child is Paragraph && insertionPoint.Parent != null && insertionPoint.Parent is Paragraph) ||  // avoid paragraph within a paragraph (invalid XML)
                (child is Table && insertionPoint.Parent != null && insertionPoint.Parent is Paragraph))        // avoid table within a paragraph (invalid XML)
            {
                result = PreCheckValidationResult.MoveEntireElement;
            }

            return result;
        }

        /// <summary>
        /// Enforce valid XML formats by adding and/or removing elements
        /// </summary>
        /// <param name="wordDocument">Document</param>
        public static void CleanupDocumentXml(WordprocessingDocument wordDocument)
        {
            if (wordDocument == null) { throw new ArgumentNullException(nameof(wordDocument)); }
            Document xmlDocument = wordDocument.MainDocumentPart.Document;

            // make sure all table cells have a Paragraph as their last child
            ICollection<TableCell> allTableCells = xmlDocument.Descendants<TableCell>().ToList();
            foreach (TableCell tableCell in allTableCells)
            {
                if (!(tableCell.LastChild is Paragraph))
                {
                    tableCell.AppendChild<Paragraph>(new Paragraph());
                }
            }
        }

        /// <summary>
        /// Move the contents of the "parent" element to a node just after itself.
        /// </summary>
        /// <param name="parent">Parent element</param>
        /// <param name="insertionPoint">The insertion point</param>
        /// <returns>The (resulting) point at which to continue subsequent element insertion</returns>
        private static OpenXmlElement MoveChildrenAfterInsertionPoint(OpenXmlElement parent, OpenXmlElement insertionPoint)
        {
            if (insertionPoint == null)
            {
                insertionPoint = parent;
            }

            // Note: This can be resolved once/up-front because insertion points are always sibling nodes
            bool isEventualParentParagraph = (insertionPoint.Parent != null && insertionPoint.Parent is Paragraph);

            OpenXmlElement[] childElements = parent.ChildElements.ToArray();
            int totalChildElements = childElements.Length;

            for (int i = 0; i < totalChildElements; i++)
            {
                OpenXmlElement childElement = childElements[i];

                if ((childElement is Paragraph && isEventualParentParagraph) ||  // avoid paragraph within a paragraph (invalid XML)
                    (childElement is Table && isEventualParentParagraph))        // avoid table within a paragraph (invalid XML)
                {
                    // move the table (and every subsequent sibling node) to FOLLOW the paragraph node
                    insertionPoint = insertionPoint.Parent;

                    for (int j = i; j < totalChildElements; j++)
                    {
                        childElement = childElements[j];
                        insertionPoint = insertionPoint.InsertAfterSelf(childElement.CloneNode(true));
                    }

                    i = totalChildElements;  // avoid re-processing the subsequent children
                }
                else
                {
                    insertionPoint = insertionPoint.InsertAfterSelf(childElement.CloneNode(true));
                }
            }

            return insertionPoint;
        }

        #endregion

        #endregion
    }
}