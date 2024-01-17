namespace IES.Common.Core.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Web;
	using HtmlAgilityPack;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Exceptions;

	/// <summary>
	/// Utilities for the HtmlAgilityPack
	/// </summary>
	public static class HtmlAgilityPackUtilities
	{
		#region HtmlDocument Queries

		/// <summary>
		/// Get all element values within all tables
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns>List of element values</returns>
		public static IEnumerable<ElementValues> GetTableElementValues(HtmlDocument doc)
		{
			return GetXPathElementValues(doc, "//table");
		}

		/// <summary>
		/// Get all drop-down element values
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns>List of element values</returns>
		public static IEnumerable<ElementValues> GetSelectElementValues(HtmlDocument doc)
		{
			return GetXPathElementValues(doc, "//select");
		}

		/// <summary>
		/// Get all textarea element values
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns>List of element values</returns>
		public static IEnumerable<ElementValues> GetTextareaElementValues(HtmlDocument doc)
		{
			return GetXPathElementValues(doc, "//textarea");
		}

		/// <summary>
		/// Get all span element values
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns>List of element values</returns>
		public static IEnumerable<ElementValues> GetSpanElementValues(HtmlDocument doc)
		{
			return GetXPathElementValues(doc, "//span[@class]");
		}

		/// <summary>
		/// Get all input element values
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns>List of element values</returns>
		public static IEnumerable<ElementValues> GetInputElementValues(HtmlDocument doc)
		{
			return GetXPathElementValues(doc, "//input[@value]");
		}

		/// <summary>
		/// Get values of all elements described by the XPath value
		/// </summary>
		/// <param name="doc">Document</param>
		/// <param name="xpath">The XPath expression</param>
		/// <returns>List of element values</returns>
		private static IEnumerable<ElementValues> GetXPathElementValues(HtmlDocument doc, string xpath)
		{
			if (doc == null)
			{
				throw new ArgumentNullException(nameof(doc));
			}

			IEnumerable<ElementValues> results;

			HtmlNodeCollection nodes;
			if ((nodes = doc.DocumentNode.SelectNodes(xpath)) != null)
			{
				results = GetElementValues(null, nodes);
			}
			else
			{
				results = new List<ElementValues>(0);
			}

			return results;
		}

		#endregion

		#region HtmlNode/HtmlNodeCollection Queries

		/// <summary>
		/// Get element values within each node in a collection
		/// </summary>
		/// <param name="idPrefix">Prefix that reflects DOM hierarchy</param>
		/// <param name="nodes">Collection of nodes</param>
		/// <returns>List of element values</returns>
		private static IEnumerable<ElementValues> GetElementValues(string idPrefix, HtmlNodeCollection nodes)
		{
			if (nodes == null)
			{
				throw new ArgumentNullException(nameof(nodes));
			}

			List<ElementValues> results = new(nodes.Count);

			foreach (HtmlNode inputElement in nodes)
			{
				ElementValues elementValues = GetElementValues(idPrefix, inputElement);
				if (!string.IsNullOrEmpty(elementValues.Value) && elementValues.Value != "$" && elementValues.Value != "%")
				{
					results.Add(elementValues);
				}
			}

			return results;
		}

		/// <summary>
		/// Get element values for a node
		/// </summary>
		/// <param name="idPrefix">Prefix that reflects DOM hierarchy</param>
		/// <param name="element">Element/node</param>
		/// <returns>Element values</returns>
		private static ElementValues GetElementValues(string idPrefix, HtmlNode element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			string tag = element.OriginalName.ToLower();
			string value = element.GetAttributeValue("value", string.Empty);

			switch (element.NodeType)
			{
				case HtmlNodeType.Element:
					if (tag.ToLower() == "span")
					{
						value = GetSpanValue(element);
					}

					break;
				case HtmlNodeType.Text:
					break;
				default:
					break;
			}

			string id;
			if (value == null || value == "$" || value == "%")
			{
				// skip currency, percent symbols
				id = string.Empty;
			}
			else
			{
				if (string.IsNullOrEmpty(element.Id))
				{
					id = idPrefix ?? string.Empty;
				}
				else
				{
					id = element.Id;
				}
			}

			return new ElementValues
			{
				Id = id,
				Name = element.Name ?? string.Empty,
				TagName = tag,
				Text = element.InnerText.Trim() ?? string.Empty,
				Value = value
			};
		}

		/// <summary>
		/// Get the value of a span element
		/// </summary>
		/// <param name="spanElement">Element</param>
		/// <returns>Value</returns>
		private static string GetSpanValue(HtmlNode spanElement)
		{
			string value = null;

			string dataType = spanElement.GetAttributeValue("dataType", string.Empty);
			string classNames = spanElement.GetAttributeValue("class", string.Empty);

			if (classNames.Contains("currency-symbol") || classNames.Contains("percent-symbol"))
			{
				value = null;
			}
			else if (!string.IsNullOrEmpty(dataType) && dataType != "number")
			{
				value = spanElement.InnerText;
			}
			else if (classNames.Contains("hide-text-overflow"))
			{
				value = spanElement.InnerText;
			}

			return value;
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Get row labels/text from the left-most cell of each row
		/// </summary>
		/// <param name="tableNode">Table node</param>
		/// <returns>List of row labels</returns>
		public static IList<string> GetRowLabels(HtmlNode tableNode)
		{
			if (tableNode == null)
			{
				throw new ArgumentNullException(nameof(tableNode));
			}

			List<string> rowLabels = new();

			HtmlNodeCollection bodyRowNodes;
			// get all rows in table
			if ((bodyRowNodes = tableNode.SelectNodes("./tbody/tr")) != null)
			{
				// make a "first pass" to determine the row "names" (i.e. the text label in the leftmost cell of each row)
				foreach (HtmlNode bodyRowNode in bodyRowNodes)
				{
					HtmlNodeCollection cellNodes;
					// get all cells in row
					if ((cellNodes = bodyRowNode.SelectNodes("./td")) != null)
					{
						HtmlNode cellNode;
						if ((cellNode = cellNodes.FirstOrDefault()) != null)
						{
							IEnumerable<ElementValues> cellValues;
							HtmlNodeCollection inputNodes;
							HtmlNodeCollection spanNodes;
							// get all /input cells
							if ((inputNodes = cellNode.SelectNodes("./input")) != null)
							{
								if ((cellValues = GetElementValues(null, inputNodes)) != null)
								{
									rowLabels.Add(cellValues.FirstOrDefault().Id);      // use the ID from the first Input cell as the row label
								}
							}
							else if ((spanNodes = cellNode.SelectNodes("./span")) != null)
							{
								if ((cellValues = GetElementValues(null, spanNodes)) != null)
								{
									rowLabels.Add(cellValues.FirstOrDefault().Value);
								}
							}
							else
							{
								rowLabels.Add(cellNode.InnerText);          // if the first cell has neither /input or /span, use inner text as row label
							}
						}
					}
				}
			}

			return rowLabels;
		}

		/// <summary>
		/// Get column labels/text from the header cell of each column
		/// </summary>
		/// <param name="tableNode">Table node</param>
		/// <returns>List of column labels</returns>
		public static IList<string> GetColumnLabels(HtmlNode tableNode)
		{
			if (tableNode == null)
			{
				throw new ArgumentNullException(nameof(tableNode));
			}

			List<string> columnLabels = new();

			HtmlNodeCollection headerRowNodes;
			if ((headerRowNodes = tableNode.SelectNodes("./thead/tr")) != null)
			{
				foreach (HtmlNode headerRowNode in headerRowNodes)
				{
					HtmlNodeCollection headerCellNodes;
					if ((headerCellNodes = headerRowNode.SelectNodes("./th")) != null)
					{
						foreach (HtmlNode headerCellNode in headerCellNodes)
						{
							ElementValues elementValues = GetElementValues(null, headerCellNode);
							columnLabels.Add(elementValues.Value);
						}
					}
				}
			}

			return columnLabels;
		}

		/// <summary>
		/// Parse and display all elements and their values on a page
		/// </summary>
		/// <param name="pageContent">Page content (View Source)</param>
		/// <param name="sharedRowLabels">Lookup table of tables (ids) which share another table's row labels</param>
		/// <returns>List of elements parsed from the page</returns>
		public static IEnumerable<ElementValues> ParseHtml(string pageContent, Dictionary<string, string> sharedRowLabels)
		{
			if (pageContent == null)
			{
				throw new ArgumentNullException(nameof(pageContent));
			}

			if (sharedRowLabels == null)
			{
				throw new ArgumentNullException(nameof(sharedRowLabels));
			}

			HtmlDocument doc = new();
			doc.LoadHtml(pageContent);

			List<ElementValues> results = new(0);
			Dictionary<string, IList<string>> rowLabelsByTable = new();

			HtmlNodeCollection tableNodes;
			// get all tables
			if ((tableNodes = doc.DocumentNode.SelectNodes("//table")) != null)
			{
				// foreach table
				foreach (HtmlNode tableNode in tableNodes)
				{
					string tableId = tableNode.Id;                                  // get the table ID

					if (string.IsNullOrEmpty(tableId))
					{
						// skip tables that do not have an ID assigned
						continue;
					}

					// determine the column "names" (i.e. the text label in the topmost [header] cell of each column)
					IList<string> tableHeaders = GetColumnLabels(tableNode);

					// determine the row "names" (i.e. the text label in the leftmost cell of each row)
					IList<string> rowLabels;
					if (sharedRowLabels.ContainsKey(tableId))
					{
						rowLabels = rowLabelsByTable[sharedRowLabels[tableId]];             // we already have row names for this table
					}
					else
					{
						rowLabels = GetRowLabels(tableNode);                                // get the row name
					}

					rowLabelsByTable[tableId] = rowLabels;                                  // we now have the row labels
																							// get all rows in the table
					HtmlNodeCollection bodyRowNodes;
					if ((bodyRowNodes = tableNode.SelectNodes("./tbody/tr")) != null)
					{
						int rowNo = 0;
						// foreach row
						foreach (HtmlNode bodyRowNode in bodyRowNodes)
						{
							if (string.IsNullOrEmpty(rowLabels[rowNo]))
							{
								rowLabels[rowNo] = "Row" + rowNo.ToString();                // if there is no row lable, use th row index
							}

							string rowPrefix = tableId + "." + (rowNo < rowLabels.Count ? rowLabels[rowNo] + "." : string.Empty);

							HtmlNodeCollection cellNodes;                                   // get all cells for the row
							if ((cellNodes = bodyRowNode.SelectNodes("./td")) != null)
							{
								int cellNo = 0;

								foreach (HtmlNode cellNode in cellNodes)
								{
									string colLabel;
									if (cellNo < tableHeaders.Count && !string.IsNullOrEmpty(tableHeaders[cellNo]))
									{
										colLabel = tableHeaders[cellNo];
									}
									else
									{
										colLabel = "Cell" + cellNo.ToString();
									}

									////       string columnPrefix = rowPrefix + ((cellNo < tableHeaders.Count) ? tableHeaders[cellNo] : string.Empty);
									string columnPrefix = rowPrefix + colLabel;

									HtmlNodeCollection inputNodes;
									if ((inputNodes = cellNode.SelectNodes("./input")) != null)
									{
										results.AddRange(GetElementValues(columnPrefix, inputNodes));
									}

									HtmlNodeCollection spanNodes;
									if ((spanNodes = cellNode.SelectNodes("./span")) != null)
									{
										results.AddRange(GetElementValues(columnPrefix, spanNodes));
									}

									if (cellNode.SelectNodes("./*") == null)
									{
										ElementValues elementValues = GetElementValues(columnPrefix, cellNode);
										if (!string.IsNullOrEmpty(elementValues.Value)
											&& elementValues.Value != "$"
											&& elementValues.Value != "%"
											&& elementValues.Value != "&nbsp;")
										{
											results.Add(elementValues);
										}
									}

									++cellNo;
								}
							}
							++rowNo;
						}
					}
				}
			}

			return results;
		}

		#endregion

		#region HtmlConverter Utilities

		private const char BLANK = ' ';

		/// <summary>
		/// Identify and remove styling and/or markup that are known to cause problems for the "HtmlConverter" third-party
		/// HTML conversion utility.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <param name="validationMessages">Repository for validation messages</param>
		/// <seealso cref="NotesFor.HtmlToOpenXml.HtmlConverter"/>
		public static string ScrubRichTextForSave(string html, ICollection<ValidationMessage> validationMessages)
		{
			return ScrubRichTextForSave(html, null, validationMessages);
		}

		/// <summary>
		/// Identify and remove styling and/or markup that are known to cause problems for the "HtmlConverter" third-party
		/// HTML conversion utility.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <param name="imageSrcConverter">Handler method for conversion of src attribute</param>
		/// <param name="validationMessages">Repository for validation messages</param>
		/// <seealso cref="NotesFor.HtmlToOpenXml.HtmlConverter"/>
		public static string ScrubRichTextForSave(string html, Func<string, string> imageSrcConverter, ICollection<ValidationMessage> validationMessages)
		{
			#region Load the document

			HtmlDocument document = new();
			document.LoadHtml(html);

			#endregion

			#region Scrub: First phase - General text-value replacements

			PreScrubRichTextForSave(document);

			#endregion

			#region Scrub: Second phase - Split multi-valued styles into individual top/bottom/right/left

			ScrubHtmlDocumentForSave(document);

			#endregion

			#region Scrub: Third phase - Convert image tag src attributes from routes to base-64

			if (imageSrcConverter != null)
			{
				ConvertImagesForSave(document, imageSrcConverter);
			}

			#endregion

			#region Validation

			ValidateRichTextForSave(document, validationMessages);

			#endregion

			// get the results
			return document.DocumentNode.InnerHtml;
		}

		/// <summary>
		/// Perform custom validation against the markup
		/// </summary>
		/// <param name="document">HTML document</param>
		/// <param name="validationMessages">Repository for validation messages</param>
		/// <returns>Validation messages</returns>
		private static ICollection<ValidationMessage> ValidateRichTextForSave(HtmlDocument document, ICollection<ValidationMessage> validationMessages)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			else if (validationMessages == null)
			{
				throw new ArgumentNullException(nameof(validationMessages));
			}

			HtmlNodeCollection allNodes = GetAllNodes(document);
			bool stop = false;

			#region Count table columns

			IEnumerable<HtmlNode> allTableNodes = allNodes.Where(n => n.Name.ToLower() == "table");
			foreach (HtmlNode tableNode in allTableNodes)
			{
				IEnumerable<HtmlNode> allRowNodes = tableNode.Descendants().Where(n => n.Name.ToLower() == "tr");
				foreach (HtmlNode rowNode in allRowNodes)
				{
					IEnumerable<HtmlNode> allCellNodes = rowNode.Descendants().Where(n => n.Name.ToLower() == "td");
					int totalCellsInRow = allCellNodes.Count();
					if (totalCellsInRow > CommonConstants.MAXIMUM_COLUMNS_PER_TABLE_MS_WORD)
					{
						validationMessages.Add(new ValidationMessage(string.Format("The number of table columns exceeds the maximum {0} supported by Microsoft Word.", CommonConstants.MAXIMUM_COLUMNS_PER_TABLE_MS_WORD)));
						stop = true;
						break;
					}
				}

				if (stop)  // only need one message
				{
					break;
				}
			}

			#endregion

			return validationMessages;
		}

		/// <summary>
		/// Identify and remove styling and/or markup that are known to cause problems for the "HtmlConverter" third-party
		/// HTML conversion utility.
		/// </summary>
		/// <param name="document">HTML document</param>
		/// <seealso cref="NotesFor.HtmlToOpenXml.HtmlConverter"/>
		private static void PreScrubRichTextForSave(HtmlDocument document)
		{
			#region Scrub: First phase - General text-value replacements

			IDictionary<string, string> straightReplacements = new Dictionary<string, string>
			{
				{ "windowtext", "black" },
				{ "0px", "0" }
			};

			HtmlNodeCollection allNodes = GetAllNodes(document);

			// if you grabbed stuff from one of our read only pages (Boes/Labor..), you are potentially holding onto some ugly stuff.. we need to clean it up
			ScrubPotentialGenBoeReadOnlyMuck(allNodes);

			foreach (HtmlNode node in allNodes)
			{

				foreach (HtmlAttribute attr in node.Attributes)
				{
					if (node.Name.ToLower() == "img" && attr.Name.ToLower() == "src")
					{
						// avoid corrupting the base-64 src attribute for images
					}
					else
					{
						foreach (KeyValuePair<string, string> replacement in straightReplacements)
						{
							attr.Value = attr.Value.Replace(replacement.Key, replacement.Value);
						}
					}
				}
			}

			#endregion
		}

		/// <summary>
		/// When our page goes into a read only mode, it does some ugly stuff to the elements. It impacts RTE in all sorts of funny ways.. So if you try to copy/paste that into RTE it freaks out.
		/// </summary>
		/// <param name="allNodes">All html nodes to scrub</param>
		internal static void ScrubPotentialGenBoeReadOnlyMuck(HtmlNodeCollection allNodes)
		{
			// Basically we normally have a text area that contains the text..
			// If we go read only, the JS hides the text area and adds a div w/ the class of replacedWidgetText, containing the text (or HTML in the RTE case).
			// So what we need to a) remove the text are, b) remove the created div and c) set the value to the parent element to make things work correctly

			// 

			/*
            
            Here is a sample of bad input that we need to clean up, keeping in mind that it may not always follow the same structure..
            
                <div id="sources-element" class="form-element">
                    <div class="wrapper">
                        <textarea id="DataSource" class="display-none" name="DataSource" maxlength="10" jquery191021047627543446945="61">&lt;p&gt;ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx&lt;/p&gt;</textarea> 
                        <div class="replacedWidgetText" title="" jquery191021047627543446945="273">
                            <p>ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx</p>
                        </div>
                    </div>
                </div> 
             
            Out of all of that, we want to get:
             
                <p>ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx</p>

            To complicate processing a little less, we can grab it inside of the parent divs, as those do not hurt us in any way:
             
                <div id=\"sources-element\" class=\"form-element\">
                    <div class=\"wrapper\"><p>ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx</p></div>
                </div>

            */

			string searchString = "class=\"replacedWidgetText\"";

			// We are now looking for text areas that have a "sibling" div div w/ the class marked above.. We are referencing this via parents, just to make things easier to follow
			List<HtmlNode> textAreasToProcess = allNodes.Where(x => x.HasChildNodes
																	&& x.ChildNodes.Any(z => z.Name.ToLower() == "textarea")
																	&& x.ChildNodes.Any(z => z.OuterHtml.ToLower().Contains(searchString.ToLower()) && z.Name.ToLower() == "div")
															).SelectMany(x => x.ChildNodes.Where(z => z.Name.ToLower() == "textarea")).ToList();

			for (int i = 0; i < textAreasToProcess.Count; i++)
			{
				// Update the parent w/ the Html Decoded contents of the text area. This will set the right value as well as remove the muck that Boe added..
				textAreasToProcess[i].ParentNode.InnerHtml = HttpUtility.HtmlDecode(textAreasToProcess[i].InnerHtml);
			}
		}

		/// <summary>
		/// Identify and remove styling and/or markup that are known to cause problems for the "HtmlConverter" third-party
		/// HTML conversion utility.
		/// </summary>
		/// <param name="document">HTML document</param>
		/// <param name="imageSrcConverter">Handler method for conversion of src attribute</param>
		private static void ConvertImagesForSave(HtmlDocument document, Func<string, string> imageSrcConverter)
		{
			if (imageSrcConverter == null)
			{
				throw new ArgumentNullException(nameof(imageSrcConverter));
			}

			HtmlNodeCollection allNodes = GetAllNodes(document);

			IEnumerable<HtmlNode> allImageNodes = allNodes.Where(n => n.Name.ToLower() == "img");

			foreach (HtmlNode imageNode in allImageNodes)
			{
				HtmlAttribute srcAttribute = imageNode.Attributes.FirstOrDefault(n => n.Name.ToLower() == "src");
				if (srcAttribute != null)
				{
					if (srcAttribute.Value.StartsWith("data:"))
					{
						srcAttribute.Value = srcAttribute.Value.Replace(' ', '+');  // replace spaces with plus-sign to ensure valid base-64 syntax
					}
					else
					{
						srcAttribute.Value = imageSrcConverter(srcAttribute.Value);

						HtmlAttribute tinyMceSrcAttribute = imageNode.Attributes.FirstOrDefault(n => n.Name.ToLower() == "data-mce-src");
						if (tinyMceSrcAttribute != null)
						{
							tinyMceSrcAttribute.Value = srcAttribute.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Identify and remove styling and/or markup that are known to cause problems for the "HtmlConverter" third-party
		/// HTML conversion utility.
		/// </summary>
		/// <param name="document">HTML document</param>
		private static void ScrubHtmlDocumentForSave(HtmlDocument document)
		{
			#region Scrub: Second phase - Split multi-valued styles into individual top/bottom/right/left

			HtmlNodeCollection allNodes = GetAllNodes(document);

			foreach (HtmlNode node in allNodes)
			{
				#region Process style attributes

				IEnumerable<HtmlAttribute> allStyleAttributes = node.Attributes.Where(a => a.Name.ToLower() == "style");

				foreach (HtmlAttribute styleAttribute in allStyleAttributes)
				{
					ICollection<string> scrubbedStyleEntries = new Collection<string>();
					bool scrubbed = false;

					string[] completeStyleEntry = styleAttribute.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (string styleEntry in completeStyleEntry)
					{
						string scrubbedStyleEntry = styleEntry.Trim(new char[] { ';', BLANK });

						string[] styleValues = scrubbedStyleEntry.Split(new char[] { ':', BLANK }, StringSplitOptions.RemoveEmptyEntries);

						if (styleValues.Any())
						{
							string styleName = styleValues[0];

							switch (styleName)
							{
								case "border-color":
								case "border-style":
								case "border-width":
									FourStyleValues fourStyleValues = InterpretStyleValues(styleValues);
									scrubbedStyleEntry = ReplaceStyleValues(fourStyleValues);
									scrubbed = true;
									break;

								case "mso-bidi-font-family":
								case "font-family":
								case "mso-bidi-font-size":
								case "font-size":
								case "line-height":
									scrubbedStyleEntry = null;  // completely remove
									scrubbed = true;
									break;

								default:
									// keep existing value
									if (string.IsNullOrWhiteSpace(scrubbedStyleEntry))
									{
										scrubbedStyleEntry = null;  // ... but skip blanks
									}
									break;

							}  // end switch
						}

						if (scrubbedStyleEntry != null)
						{
							scrubbedStyleEntries.Add(scrubbedStyleEntry);
						}

					}  // end foreach

					if (scrubbed)
					{
						System.Text.StringBuilder sb = new();

						foreach (string scrubbedEntry in scrubbedStyleEntries)
						{
							// make sure each style entry is terminated with a semicolon
							sb.Append(scrubbedEntry).Append(';');
						}

						string completeStyleEntryReplacement = sb.ToString().TrimEnd();
						styleAttribute.Value = completeStyleEntryReplacement;
					}
				}  // foreach HtmlAttribute

				#endregion

			}  // foreach HtmlNode

			#endregion
		}

		private static string ReplaceStyleValues(FourStyleValues styleValues)
		{
			System.Text.StringBuilder sb = new();

			const string STYLE_ATTR_FORMAT = "{0}:{1};";

			if (!string.IsNullOrEmpty(styleValues.Top))
			{
				string name = styleValues.Name.Replace("-", "-top-");
				sb.AppendFormat(STYLE_ATTR_FORMAT, name, styleValues.Top);
			}

			if (!string.IsNullOrEmpty(styleValues.Bottom))
			{
				string name = styleValues.Name.Replace("-", "-bottom-");
				sb.AppendFormat(STYLE_ATTR_FORMAT, name, styleValues.Bottom);
			}

			if (!string.IsNullOrEmpty(styleValues.Right))
			{
				string name = styleValues.Name.Replace("-", "-right-");
				sb.AppendFormat(STYLE_ATTR_FORMAT, name, styleValues.Right);
			}

			if (!string.IsNullOrEmpty(styleValues.Left))
			{
				string name = styleValues.Name.Replace("-", "-left-");
				sb.AppendFormat(STYLE_ATTR_FORMAT, name, styleValues.Left);
			}

			return sb.ToString().TrimEnd(';');
		}

		private static FourStyleValues InterpretStyleValues(string[] styleValues)
		{
			FourStyleValues result = new()
			{
				Name = styleValues[0]
			};

			int numberOfStyleValues = styleValues.Length - 1;

			// name: TRBL
			// name: TB  RL
			// name: T   RL   B
			// name: T   R    B   L

			if (numberOfStyleValues == 1)
			{
				result.Top = styleValues[1];
				result.Right = styleValues[1];
				result.Bottom = styleValues[1];
				result.Left = styleValues[1];
			}
			else if (numberOfStyleValues == 2)
			{
				result.Top = styleValues[1];
				result.Bottom = styleValues[1];
				result.Right = styleValues[2];
				result.Left = styleValues[2];
			}
			else if (numberOfStyleValues == 3)
			{
				result.Top = styleValues[1];
				result.Right = styleValues[2];
				result.Left = styleValues[2];
				result.Bottom = styleValues[3];
			}
			else if (numberOfStyleValues == 4)
			{
				result.Top = styleValues[1];
				result.Right = styleValues[2];
				result.Bottom = styleValues[3];
				result.Left = styleValues[4];
			}

			return result;
		}

		private class FourStyleValues
		{
			public string Name { get; set; }
			public string Top { get; set; }
			public string Right { get; set; }
			public string Bottom { get; set; }
			public string Left { get; set; }
		}

		private static void GetAllNodes(HtmlNodeCollection currentNodes, HtmlNodeCollection allNodes)
		{
			foreach (HtmlNode node in currentNodes)
			{
				allNodes.Add(node);

				GetAllNodes(node.ChildNodes, allNodes);
			}
		}

		internal static HtmlNodeCollection GetAllNodes(HtmlDocument document)
		{
			HtmlNodeCollection allNodes = new(document.DocumentNode);

			GetAllNodes(document.DocumentNode.ChildNodes, allNodes);

			return allNodes;
		}

		#endregion
	}

	/// <summary>
	/// Define various values for an element
	/// </summary>
	public class ElementValues
	{
		/// <summary>
		/// Element tag name
		/// </summary>
		public string TagName { get; set; }

		/// <summary>
		/// Element name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Element Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Element value
		/// </summary>
		private string theValue;

		/// <summary>
		/// Element value
		/// </summary>
		public string Value
		{
			get
			{
				return string.IsNullOrEmpty(theValue) ? Text : theValue;
			}

			set
			{
				theValue = value;
			}
		}

		/// <summary>
		/// Element text
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Element text
		/// </summary>
		public string Datatype { get; set; }
	}
}

