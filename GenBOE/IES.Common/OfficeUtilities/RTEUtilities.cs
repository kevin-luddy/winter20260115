// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using HtmlAgilityPack;
    using NotesFor.HtmlToOpenXml;

    public static class RTEUtilities
    {
        // Regex that will match an element (with some optional styles) that contains nothing but whitespace as it's "content", or InnerText
        private static string regexToRemoveEmptyElement =
            // opening tag of the element (with an optional style; RTE only sets text-align and color, and the specified characters take care of that
            "<ELEMENT(( ){0,}style=\"[A-Za-z0-9: #=; -]*\"){0,1}>"
            // any combination/order/number of ONLY "HTML white space" => regular space or &nbsp.. we do not need to test for line breaks, as those show up in new <p> objects
            + "(( ){0,}&nbsp;){0,}( ){0,}"
            // closing tag of said element
            + "<\\/ELEMENT>";

        // Create static Regex objects.
        // bold font
        private static Regex regexStrong = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "strong"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // italics font
        private static Regex regexEm = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "em"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // subscript
        private static Regex regexSub = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "sub"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // superscript
        private static Regex regexSup = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "sup"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // spans are used when you set either a color or alignment to a partial element (partial line, but also partial "strong" and so on
        private static Regex regexSpan = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "span"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // every line is wrapped in a paragraph, so empty lines will be always wrapped in this; putting it at the end, to have this go last, for efficiency
        private static Regex regexP = new Regex(regexToRemoveEmptyElement.Replace("ELEMENT", "p"), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        // next we need to remove text that starts w/ a "mso-" above and ends with ";" - this is Microsoft-specific formatting.
        private static Regex regexMicrosoft = new Regex("( ){0,1}mso-[A-Za-z0-9:.% #='?-]*;", RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

        #region These are used in Excel, when we need to display just text, and no other markup or images

        /// <summary>
        /// Turns HTML input into plain text
        /// </summary>
        /// <param name="htmlToProcess">HTML To Process</param>
        /// <returns>Plain text version of HTML stuff</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static ICollection<string> TurnHTMLIntoPlainText(ICollection<string> htmlToProcess)
        {
            if (htmlToProcess == null)
            {
                throw new ArgumentNullException(nameof(htmlToProcess));
            }

            List<string> plainTextFromHtml = new List<string>();

            if (htmlToProcess.Any())
            {
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (WordprocessingDocument document = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                        {
                            document.AddMainDocumentPart();

                            HtmlConverter converter = new HtmlConverter(document.MainDocumentPart);
                            StringBuilder temp = new StringBuilder();

                            foreach (string html in htmlToProcess)
                            {   
                                IList<OpenXmlCompositeElement> paragraphs;

                                try
                                {
                                    paragraphs = converter.Parse(html);
                                }
                                catch
                                {
                                    /*
                                     * If a user enters rich-text that HtmlConverter cannot handle, the converter will throw an exception.
                                     * The error handling for HtmlConverter does not seem very robust, so the set of possible exceptions
                                     * is fairly indeterminate (e.g. in one case, a null-argument exception was thrown).
                                     * 
                                     * As a work-around remedy, just strip out the markup tags here and returning the raw text.
                                     * 
                                     */
                                    HtmlDocument htmldoc = new HtmlDocument();
                                    htmldoc.LoadHtml(html);
                                    paragraphs = converter.Parse(htmldoc.DocumentNode.InnerText);
                                }

                                for (int i = 0; i < paragraphs.Count; i++)
                                {
                                    temp.Append(paragraphs[i].InnerText);
                                }

                                plainTextFromHtml.Add(temp.ToString());
                                temp.Clear();
                            }
                        }
                    }
                }
            }

            return plainTextFromHtml;
        }

        /// <summary>
        /// Turns HTML input into plain text
        /// </summary>
        /// <param name="htmlToProcess">HTML To Process</param>
        /// <returns>Plain text version of HTML stuff</returns>
        public static string TurnHTMLIntoPlainText(string htmlToProcess)
        {
            string result = string.Empty;

            ICollection<string> tempResult = TurnHTMLIntoPlainText(new List<string>() { htmlToProcess });

            if (tempResult.Any())
            {
                result = tempResult.ElementAt(0);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Cleans up html, preparing it for exporting to MS word
        /// </summary>
        /// <param name="htmlFormattedText">HTML formatted text</param>
        /// <param name="removeSpacing">Indicates whether spacing should be removed</param>
        /// <param name="skipCleanup">Reference variable that indicates whether Element XML cleanup should be skipped (for certain elements we add a new line and so cleanup is not needed)</param>
        /// <returns>HTML formatted text after all the "bad" stuff was stripped out</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        public static string PrepareHtmlForWordExport(string htmlFormattedText, bool removeSpacing, ref bool skipCleanup)
        {
            skipCleanup = false;

            if (!string.IsNullOrEmpty(htmlFormattedText))
            {
                // If the extra spacing should be removed, we do so
                if (removeSpacing)
                {
                    htmlFormattedText = RemoveEmptyLinesFromHtmlString(htmlFormattedText);
                }

                //<p> tag changes the font of the label to Times New Roman if it is on the same line - some templates use other fonts
                //removing the first <p> and </p> fixes the issue, but can't remove any others to preserve line breaks
                if(htmlFormattedText.StartsWith("<p>"))
                {
                    htmlFormattedText = htmlFormattedText.Remove(0, 3);
                    int closeTagIndex = htmlFormattedText.IndexOf("</p>");
                    htmlFormattedText = closeTagIndex < 0 ? htmlFormattedText : htmlFormattedText.Remove(closeTagIndex, 4);
                }
                
                // Remove styles that could have been introduced when copy/paste from MS word is done
                htmlFormattedText = RemoveMsFormattingFromHtmlString(htmlFormattedText);

                if (htmlFormattedText.StartsWith("<ol")
                    || htmlFormattedText.StartsWith("<ul")
                    || htmlFormattedText.StartsWith("<img")
                    || htmlFormattedText.StartsWith("<table"))
                {
                    // if we start w/ those 4 options, we need to throw in an empty line up front, to prevent style bleed through
                    htmlFormattedText = "&nbsp;" + htmlFormattedText;
                    skipCleanup = true;
                }

                // clean up invalid characters.. These are a few that we ran into
                htmlFormattedText = htmlFormattedText.Replace("•", "&bull;");
                htmlFormattedText = htmlFormattedText.Replace("–", "-");

                // an additional catch all, to replace any other invalid characters with a dash -
                htmlFormattedText = Encoding.ASCII.GetString(
                    Encoding.Convert(
                        Encoding.UTF8,
                        Encoding.GetEncoding(
                            Encoding.ASCII.EncodingName,
                            new EncoderReplacementFallback("-"),
                            new DecoderExceptionFallback()
                            ),
                        Encoding.UTF8.GetBytes(htmlFormattedText)
                    )
                );
            }

            return htmlFormattedText;
        }

        #region Helpers for Html PreProcessing

        /// <summary>
        /// Removes empty spacing or new lines from HTML code. This should be coming from RTE, so we are only accounting for RTE generated HTML
        /// </summary>
        /// <param name="htmlFormattedText">Html to adjust</param>
        /// <returns>Cleaned HTML</returns>
        internal static string RemoveEmptyLinesFromHtmlString(string htmlFormattedText)
        {
            if (!string.IsNullOrEmpty(htmlFormattedText))
            {
                // Build comparers for all of the html elements - built from static Regex objects for performance reasons.
                List<Regex> comparers = new List<Regex>();
                comparers.Add(regexStrong);
                comparers.Add(regexEm);
                comparers.Add(regexSub);
                comparers.Add(regexSup);
                comparers.Add(regexSpan);
                comparers.Add(regexP);

                // Remove the lines
                int lengthAtTheStartOfLoop;
                while (true)
                {
                    lengthAtTheStartOfLoop = htmlFormattedText.Length;

                    foreach (Regex comparer in comparers)
                    {
                        htmlFormattedText = comparer.Replace(htmlFormattedText, string.Empty);
                    }

                    // if no changes were made during this pass, we can exit
                    if (lengthAtTheStartOfLoop == htmlFormattedText.Length)
                    {
                        break;
                    }
                }
            }

            return htmlFormattedText;
        }

        /// <summary>
        /// Removes unwanted MS formatting that could have been introduced if the user copy/pasted things directly from word
        /// 
        /// This is in response to BOEJ93
        /// </summary>
        /// <param name="htmlFormattedText">Html Formatted Text</param>
        /// <returns>Scrubbed text</returns>
        internal static string RemoveMsFormattingFromHtmlString(string htmlFormattedText)
        {
            if (!string.IsNullOrEmpty(htmlFormattedText))
            {
                // remove based on regex
                htmlFormattedText = regexMicrosoft.Replace(htmlFormattedText, string.Empty);
            }

            return htmlFormattedText;
        }

        #endregion

        /// <summary>
        /// Converts HTML input (from RTE) into an Mht(ml) string that we can then feed into Word
        /// This is done so that way we can display images and raw HTML in Word, thus preserving RTE's formatting
        /// </summary>
        /// <param name="htmlContent">Rich Text in HTML form</param>
        /// <param name="desiredFontSize">Optional font size requirement</param>
        /// <param name="desiredFontFamilies">Optional font family requirement</param>
        /// <returns>Mhtml formatted string</returns>
        internal static string ConvertHtmlToMhtml(string htmlContent, decimal? desiredFontSize, ICollection<string> desiredFontFamilies, SpacingDetailsForRTEWordExports paragraphSpacing)
        {
            using (StringWriter writer = new StringWriter())

            {

                ConvertHtmlToMhtml(writer, htmlContent, desiredFontSize, desiredFontFamilies, paragraphSpacing);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Converts HTML input (from RTE) into an Mht(ml) string that we can then feed into Word
        /// This is done so that way we can display images and raw HTML in Word, thus preserving RTE's formatting
        /// </summary>
        /// <param name="writer">Text writer to receive the formatted MHTML content.</param>
        /// <param name="htmlContent">Rich Text in HTML form</param>
        /// <param name="desiredFontSize">Optional font size requirement</param>
        /// <param name="desiredFontFamilies">Optional font family requirement</param>
        /// <param name="applyInternalSectionFormatting">bool to note if internal section formatting should be applied - for use with PPRD export, false by default</param>
        internal static void ConvertHtmlToMhtml(TextWriter writer, string htmlContent, decimal? desiredFontSize, ICollection<string> desiredFontFamilies, SpacingDetailsForRTEWordExports paragraphSpacing, bool applyInternalSectionFormatting = false)
        {
            if (htmlContent == null) { throw new ArgumentNullException(nameof(htmlContent)); }
            if (paragraphSpacing == null) { throw new ArgumentNullException(nameof(paragraphSpacing)); }

            // MHTML formatting/processing strings
            string BOUNDARY_STRING = "dataBoundary";
            string SEPARATION_BOUNDARY = "\n" + "--" + BOUNDARY_STRING + "\n";
            string CLOSING_BOUNDARY = "\n" + "--" + BOUNDARY_STRING + "--";


            // write header
            writer.Write("MIME-Version: 1.0\nContent-Type: multipart/related;\n	type=\"text/html\";\n	boundary=\"");
            writer.Write(BOUNDARY_STRING);
            writer.Write("\"\n");

            // write HTML portion
            // Note: The body style resets the defaults out and set the appropriate font styles as well
            // Then we set paragraph/div/... styles to correctly follow what was set in the template
            writer.Write(SEPARATION_BOUNDARY);
            writer.Write("Content-Type: text/html;\nContent-Transfer-Encoding: 8bit\n\n<!DOCTYPE html><HTML><HEAD> <style type=\"text/css\">body { line-spacing:100%; line-height: 100%; margin-top: 0; margin-bottom: 0; ");
            if (applyInternalSectionFormatting)
            {
                writer.Write("color: blue; font-style: italic; ");
            }

            GetCSSFontStyles(writer, desiredFontSize, desiredFontFamilies);
            writer.Write(" } p, div, span { ");
            if (paragraphSpacing.Above.HasValue)
            {
                writer.Write("margin-top: {0}pt; ", paragraphSpacing.Above.Value / 20);
            }
            if (paragraphSpacing.Below.HasValue)
            {
                writer.Write("margin-bottom: {0}pt; ", paragraphSpacing.Below.Value / 20);
            }
            if (paragraphSpacing.FirstLineIndent.HasValue)
            {
                writer.Write("text-indent: {0}pt; ", paragraphSpacing.FirstLineIndent.Value / 20);
            }
            writer.Write(" } </style>" + " </HEAD><BODY>");
            ICollection<ImageDataForMhtml> imagesToProcess = ProcessImagesForMhtml(writer, htmlContent);
            writer.Write("</BODY></HTML>\n");

            // write images
            foreach (ImageDataForMhtml imageToProcess in imagesToProcess)
            {
                writer.Write(SEPARATION_BOUNDARY);
                writer.Write("Content-Type: {0}\nContent-Transfer-Encoding: base64\nContent-Location: {1}\n\n", imageToProcess.ImageType, imageToProcess.NumberedLocation);
                writer.Write(imageToProcess.Base64EncodingOfImage);
                writer.Write('\n');
            }

            // write closing boundary
            writer.Write(CLOSING_BOUNDARY);
        }

        #region Helpers to generate Mhtml based on HTML and element formatting

        /// <summary>
        /// Generates Font Styles based on the inputs
        /// </summary>
        /// <param name="writer">Text writer to write the font style information to.</param>
        /// <param name="desiredFontSize">Font size</param>
        /// <param name="desiredFontFamilies">Font Families</param>
        /// <returns>CSS Formatted style</returns>
        private static void GetCSSFontStyles(TextWriter writer, decimal? desiredFontSize, ICollection<string> desiredFontFamilies)
        {
            if (desiredFontSize.HasValue)
            {
                writer.Write("font-size:{0}pt;", desiredFontSize.Value);
            }

            if (desiredFontFamilies != null && desiredFontFamilies.Any())
            {
                // Valid CSS requires one family per statement, so in order to handle multiples, multiple statements are needed
                foreach (string fontFamily in desiredFontFamilies)
                {
                    writer.Write(" font-family:'{0}';", fontFamily);
                }
            }
        }

        /// <summary>
        /// Tears out base64 encoded images from html and puts them into the separate collection. This is used when generating the Mhtml string
        /// </summary>
        /// <param name="writer">Text writer to write all the HTML other than the images themselves to.</param>
        /// <param name="html">Html potentially containing the images</param>
        /// <returns>A collection of images ready for further processing</returns>
        private static ICollection<ImageDataForMhtml> ProcessImagesForMhtml(TextWriter writer, string html)
        {
            var imagesForProcessing = new List<ImageDataForMhtml>();

            // here is a sample of a base64 encoded image
            // <img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==\" alt=\"Red dot\" >
            // our goal is to extract the data type (image/png) and the base64 text and replace it with a "fake" location
            // this helps word display the images correctly

            const string IMAGE_TAG_START = "<img";
            const string IMAGE_TAG_END = ">";
            const string IMAGE_SRC_ATTR_START = "src=\"";
            const string IMAGE_DATA_START = IMAGE_SRC_ATTR_START + "data:";
            const string IMAGE_TYPE_AND_DATA_SEPARATOR = ";base64,";
            const string IMAGE_SRC_ATTR_END = "\"";

            int searchStartIndex = 0;
            int lastCopiedIndex = 0;
            int imageStartIndex;
            int imageCounter = 0;

            var compare = CultureInfo.CurrentCulture.CompareInfo;

            // continue while there is another image
            while ((imageStartIndex = compare.IndexOf(html, IMAGE_TAG_START, searchStartIndex, CompareOptions.IgnoreCase)) >= 0)
            {
                // find end of tag to limit the scope of the subsequent searches
                int imageEndIndex = compare.IndexOf(html, IMAGE_TAG_END, imageStartIndex, CompareOptions.IgnoreCase);

                // data type starts with imageDataStartingString and ends with imageTypeAndBase64Separator ( | is used to show boundaries)
                // src=\"data:|image/png|;base64,
                int srcAttrStartIndex = compare.IndexOf(html, IMAGE_DATA_START, imageStartIndex + IMAGE_TAG_START.Length,
                    imageEndIndex - (imageStartIndex + IMAGE_TAG_START.Length), CompareOptions.IgnoreCase);
                if (srcAttrStartIndex < 0)
                {
                    // src attribute not found at all or doesn't contain data (maybe contains a path or URL) - leave this image as-is
                    searchStartIndex = imageEndIndex + IMAGE_TAG_END.Length;
                    continue;
                }
                int imgTypeStartIndex = srcAttrStartIndex + IMAGE_DATA_START.Length;
                int imgTypeEndIndex = compare.IndexOf(html, IMAGE_TYPE_AND_DATA_SEPARATOR, imgTypeStartIndex, imageEndIndex - imgTypeStartIndex, CompareOptions.IgnoreCase);

                // extract the image type
                string imageType = html.Substring(imgTypeStartIndex, imgTypeEndIndex - imgTypeStartIndex);

                // base64 string starts with imageTypeAndBase64Separator and ends with imageBase64DataEnding ( | is used to show boundaries)
                // ;base64,|iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyb|\"
                int imgDataStartIndex = imgTypeEndIndex + IMAGE_TYPE_AND_DATA_SEPARATOR.Length;
                int imgDataEndIndex = compare.IndexOf(html, IMAGE_SRC_ATTR_END, imgDataStartIndex, CompareOptions.IgnoreCase);

                // extract the image data and add to list
                string imageData = html.Substring(imgDataStartIndex, imgDataEndIndex - imgDataStartIndex);
                ImageDataForMhtml newImage = new ImageDataForMhtml() { ImageNumber = imageCounter++, ImageType = imageType, Base64EncodingOfImage = imageData };
                imagesForProcessing.Add(newImage);

                // Now we need to tear out the base64 image information & replace it with a fake location.. ( | is used to show boundaries)
                // so we start wiht something like this: src=\"|data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxg|\"
                // and want to end with something like this: src=\"http:\\someLocation\"

                // copy everything up to the start of the image data to the stream
                writer.Write(html.Substring(lastCopiedIndex, srcAttrStartIndex + IMAGE_SRC_ATTR_START.Length - lastCopiedIndex));
                // write the image number
                writer.Write(newImage.NumberedLocation);

                // move the starting point and copy point further
                lastCopiedIndex = searchStartIndex = imgDataEndIndex;
            }

            // write any remaining content
            if (lastCopiedIndex < html.Length)
            {
                writer.Write(lastCopiedIndex > 0 ? html.Substring(lastCopiedIndex) : html);
            }

            return imagesForProcessing;
        }

        #endregion

        #region Helpers to extract formatting from the XML/Elements

        /// <summary>
        /// Attempts to figure out the font size for the field into which we are going to insert the HTML, based on the XML element
        /// </summary>
        /// <param name="element">Element into which we are inserting the data</param>
        /// <returns>Font size (if any) specified, to be used to correctly deal with the HTML formatted text</returns>
        public static decimal? GetFontSizeBasedOnWordElementXml(OpenXmlElement element)
        {
            if (element == null) { return null; }

            return GetFontSizeBasedOnWordElementXml(element.InnerXml);
        }

        /// <summary>
        /// Attempts to figure out the font size for the field into which we are going to insert the HTML, based on the innerXml of an element
        /// This was needed for testing purposes
        /// </summary>
        /// <param name="innerXml">Inner Xml from an Element into which we are inserting the data</param>
        /// <returns>Font size (if any) specified, to be used to correctly deal with the HTML formatted text</returns>
        internal static decimal? GetFontSizeBasedOnWordElementXml(string innerXml)
        {
            decimal? result = null;

            // need to figure out the correct font size, so we can set it for the entire html document..
            // if we do get anything, the size in the word document is stored as "half points", so 20 actually means a font size of 10.

            string sizeElementConstant = "<w:sz w:val=\"";
            string sizeCsElementConstant = "<w:szCs w:val=\"";

            int sizeIndex = innerXml.LastIndexOf(sizeElementConstant);
            int sizeCsIndex = innerXml.LastIndexOf(sizeCsElementConstant);

            if (sizeIndex == sizeCsIndex) { return result; } // they are both not present

            string pickedElement = sizeIndex > sizeCsIndex ? sizeElementConstant : sizeCsElementConstant;

            string sizeElementValue = GetValueFromXml(innerXml, pickedElement, "\"");

            decimal decimalSize;
            if (decimal.TryParse(sizeElementValue, out decimalSize)) { result = decimalSize / 2; }

            return result;
        }

        /// <summary>
        /// Attempts to figure out the font families for the field into which we are going to insert the HTML, based on the XML element
        /// </summary>
        /// <param name="element">Element into which we are inserting the data</param>
        /// <returns>Font families (if any) specified, to be used to correctly deal with the HTML formatted text</returns>
        public static ICollection<string> GetFontFamiliesBasedOnElementXml(OpenXmlElement element)
        {
            if (element == null) { return new List<string>(); }

            return GetFontFamiliesBasedOnElementXml(element.InnerXml);
        }

        /// <summary>
        /// Attempts to figure out the font families for the field into which we are going to insert the HTML, based on the innerXml of an element
        /// This was needed for testing purposes
        /// </summary>
        /// <param name="innerXml">Inner Xml from an Element into which we are inserting the data</param>
        /// <returns>Font families (if any) specified, to be used to correctly deal with the HTML formatted text</returns>
        internal static ICollection<string> GetFontFamiliesBasedOnElementXml(string innerXml)
        {
            // there are multiple ways the font families are specified, based on the encoding, so we need to look for all
            string asciiFont = GetValueFromXml(innerXml, "w:ascii=\"", "\"");
            string hAnsiFont = GetValueFromXml(innerXml, "w:hAnsi=\"", "\"");
            string complexScriptFont = GetValueFromXml(innerXml, "w:cs=\"", "\"");

            // want to remove any empty strings (no fonts specified), and duplicates (same font specified under different categories)
            List<string> result = new List<string>() { asciiFont, hAnsiFont, complexScriptFont }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            return result;
        }

        /// <summary>
        /// Gets spacing data from XML
        /// </summary>
        /// <param name="element">Element to process</param>
        /// <returns>A list of spacing items, above spacing first, below spacing next</returns>
        internal static SpacingDetailsForRTEWordExports GetSpacingFromXml(OpenXmlElement element)
        {
            if (element == null) { return new SpacingDetailsForRTEWordExports(); }

            return GetSpacingFromXml(element.InnerXml);
        }

        /// <summary>
        /// Gets spacing data from XML
        /// </summary>
        /// <param name="innerXml">InnerXML to process</param>
        /// <returns>A list of spacing items, above spacing first, below spacing next</returns>
        internal static SpacingDetailsForRTEWordExports GetSpacingFromXml(string innerXml)
        {
            // doing replace of spaces because sometimes these can be as w:before, other times they can be w : before, same with = signs
            string aboveSpacing = GetValueFromXml(innerXml.Replace(" ", string.Empty), "w:before=\"", "\"");
            string belowSpacing = GetValueFromXml(innerXml.Replace(" ", string.Empty), "w:after=\"", "\"");
            string lineIndent = GetValueFromXml(innerXml.Replace(" ", string.Empty), "w:firstLine=\"", "\"");

            SpacingDetailsForRTEWordExports spacingOptions = new SpacingDetailsForRTEWordExports();

            int value;
            if (int.TryParse(aboveSpacing, out value)) { spacingOptions.Above = value; }
            if (int.TryParse(belowSpacing, out value)) { spacingOptions.Below = value; }
            if (int.TryParse(lineIndent, out value)) { spacingOptions.FirstLineIndent = value; }

            return spacingOptions;
        }

        /// <summary>
        /// A private helper to cut down on code reuse. Simply extracts a substring based on the start and end strings
        /// </summary>
        /// <param name="element">XML element which we'll be using as the source</param>
        /// <param name="startingString">string that is right before our value</param>
        /// <param name="endingString">string that is right after our value</param>
        /// <returns>Substring</returns>
        private static string GetValueFromXml(string innerXml, string startingString, string endingString)
        {
            string textResult = string.Empty;

            int startingPosition = innerXml.LastIndexOf(startingString);

            if (startingPosition > 0)
            {
                int startingIndex = startingPosition + startingString.Length;
                int endingPosition = innerXml.IndexOf(endingString, startingIndex);

                if (endingPosition > 0)
                {
                    textResult = innerXml.Substring(startingIndex, endingPosition - startingIndex);
                }
            }

            return textResult;
        }

        #endregion
    }

    #region internal classes

    /// <summary>
    /// Helper for image processing for Mhtml generation
    /// </summary>
    internal class ImageDataForMhtml
    {
        /// <summary>
        /// This is a 'fake' location that is used in the mhtml string. since it's pretend, the file loads the data from the base64 encoding.. so really, it's just used as an id
        /// </summary>
        private const string IMG_LOCATION_BASE_STRING = "http://{0}";

        /// <summary>
        /// Number, used to adjust the location to make sure things are unique
        /// </summary>
        public int ImageNumber { private get; set; }

        /// <summary>
        /// Location to be used in the Mhtml string, combining the IMG_LOCATION_BASE_STRING and the image number
        /// </summary>
        public string NumberedLocation
        {
            get
            {
                return string.Format(IMG_LOCATION_BASE_STRING, this.ImageNumber);
            }
        }

        /// <summary>
        /// Image type, as defined in the base64 encoding/representation
        /// e.g. image/png or image/jpeg
        /// </summary>
        public string ImageType { get; set; }

        /// <summary>
        /// Base64 string of the image
        /// </summary>
        public string Base64EncodingOfImage { get; set; }
    }

    internal class SpacingDetailsForRTEWordExports
    {
        public int? Above { get; set; }

        public int? Below { get; set; }

        public int? FirstLineIndent { get; set; }
    }

    #endregion
}