// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.Common.Core.Utilities
{
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Net;
	using HtmlAgilityPack;
	using IES.Common.Core.Models;

	/// <summary>
	/// Utilities for image conversion and manipulation
	/// </summary>
	public static class GenImageUtilities
	{
		/// <summary>
		/// Generate the application-route-based URL for an image.
		/// </summary>
		/// <param name="workspaceShortName">Workspace</param>
		/// <param name="boeID">BOE (if images are "scoped" under a BOE)</param>
		/// <param name="imageName">Logical ("pseudo") file name for the image</param>
		/// <returns>URL</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GenerateImageUrl(string workspaceShortName, int? boeID, string imageName)
		{
			string url;

			if (boeID.HasValue)
			{
				// BOE route for image retrieval - also see:  WebConstants.URL_PATTERN_BOE_IMAGE
				url = string.Format("/{0}/BOE/GetImage/boe/{1}/image/{2}", workspaceShortName, boeID.Value, imageName);
			}
			else
			{
				// workspace route for image retrieval - also see:  WebConstants.URL_PATTERN_WORKSPACE_IMAGE
				url = string.Format("/{0}/Workspace/GetImage/image/{1}", workspaceShortName, imageName);
			}

			return url;
		}

		/// <summary>
		/// Get the format and file-extension for an image corresponding to its MIME type.
		/// </summary>
		/// <param name="mimeType">MIME type</param>
		/// <returns>Image format and file-extension</returns>
		public static Tuple<ImageFormat, string> GetImageFormat(string mimeType)
		{
			ImageFormat format;
			string extension;

			switch (mimeType)
			{
				case "image/jpeg":
					format = ImageFormat.Jpeg;
					extension = "jpeg";
					break;
				case "image/gif":
					format = ImageFormat.Gif;
					extension = "gif";
					break;
				case "image/bmp":
					format = ImageFormat.Bmp;
					extension = "bmp";
					break;
				case "image/png":
					format = ImageFormat.Png;
					extension = "png";
					break;
				default:
					format = ImageFormat.Bmp;
					extension = "bmp";
					break;
			}

			return new Tuple<ImageFormat, string>(format, extension);
		}

		/// <summary>
		/// Get the MIME type for an image given its file name
		/// </summary>
		/// <param name="imageFileName">Image file name</param>
		/// <returns>MIME type</returns>
		public static string GetImageMIMETypeFromExtension(string imageFileName)
		{
			if (imageFileName == null)
			{
				throw new ArgumentNullException(nameof(imageFileName));
			}

			string mimeType;

			int pos = imageFileName.LastIndexOf('.');
			if (pos < 0)
			{
				mimeType = "image/bmp";
			}
			else
			{
				string extension = imageFileName.Substring(pos + 1);

				switch (extension)
				{
					case "jpg":
					case "jpeg":
						mimeType = "image/jpeg";
						break;
					case "gif":
						mimeType = "image/gif";
						break;
					case "bmp":
						mimeType = "image/bmp";
						break;
					case "png":
						mimeType = "image/png";
						break;
					default:
						mimeType = "image/bmp";
						break;
				}
			}

			return mimeType;
		}

		/// <summary>
		/// Convert a Base64-encoded image to binary
		/// </summary>
		/// <param name="base64String">Base64-encoded image</param>
		/// <param name="format">Image format to convert to</param>
		/// <returns>Converted image, as binary</returns>
		public static byte[] ConvertBase64StringToBytes(string base64String, ImageFormat format)
		{
			byte[] outbuffer = null;

			MemoryStream msin = null;  // for the base64-to-bitmap conversion
			MemoryStream msout = null;  // for save-as-format conversion

			/*
			 * Note: While the (two) image conversions are going on (i.e. UNTIL the byte array is actually captured), BOTH of the
			 * memory streams MUST remain open.
			 * 
			 */
			try
			{
				// convert base-64 string to binary
				byte[] inbuffer = Convert.FromBase64String(base64String);

				// copy binary into memory
				msin = new MemoryStream(inbuffer)
				{
					Position = 0
				};

				// convert to a bitmap image
				using (Bitmap bmp = new(Image.FromStream(msin)))
				{
					msout = new MemoryStream
					{
						Position = 0
					};

					// now convert the image to the designated format ...
					bmp.Save(msout, format);

					// ... and capture it as binary
					outbuffer = msout.ToArray();
				}

				msin.Close();
				msout.Close();

				msin = null;
				msout = null;
			}
			finally
			{
				if (msin != null)
				{
					msin.Close();
				}

				if (msout != null)
				{
					msout.Close();
				}
			}

			return outbuffer;
		}

		/// <summary>
		/// Return a stream that can be used to read the designated image
		/// </summary>
		/// <param name="emailImageInfo">Image data</param>
		/// <returns>A stream that can used to read the image contents</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream ConvertBase64StringToStream(EmailImageInfo emailImageInfo)
		{
			if (emailImageInfo == null)
			{
				throw new ArgumentNullException(nameof(emailImageInfo));
			}

			MemoryStream msin = null;  // for the base64-to-bitmap conversion
			MemoryStream msout = null;  // for save-as-format conversion

			try
			{
				// convert base-64 string to binary
				byte[] inbuffer = Convert.FromBase64String(emailImageInfo.Base64String);

				// copy binary into memory
				msin = new MemoryStream(inbuffer)
				{
					Position = 0
				};

				// convert to a bitmap image
				Bitmap bmp;
				if (string.IsNullOrEmpty(emailImageInfo.Height) || string.IsNullOrEmpty(emailImageInfo.Width))
				{
					bmp = new Bitmap(Image.FromStream(msin));
				}
				else  // if height and width are specified, then apply them when creating the image (bitmap) object
				{
					int width;
					int height;

					if (int.TryParse(emailImageInfo.Height, out height) && int.TryParse(emailImageInfo.Width, out width))
					{
						bmp = new Bitmap(Image.FromStream(msin), new Size(width, height));
					}
					else
					{
						bmp = new Bitmap(Image.FromStream(msin));
					}
				}

				msout = new MemoryStream();

				// now convert the image to the designated format ...
				Tuple<ImageFormat, string> formatMapping = GetImageFormat(emailImageInfo.MimeType);
				bmp.Save(msout, formatMapping.Item1);

				msin.Close();
				msin = null;
			}
			finally
			{
				if (msin != null)
				{
					msin.Close();
				}

				msout.Position = 0;  // reset stream to beginning so it can be read in its entirety
			}

			return msout;
		}

		/// <summary>
		/// Convert a binary image to its Base64-encoded string representation
		/// </summary>
		/// <param name="imageBytes">Image binary, as a byte array</param>
		/// <returns>Base64-encoded image</returns>
		public static string ConvertImageToBase64String(byte[] imageBytes)
		{
			if (imageBytes == null)
			{
				throw new ArgumentNullException(nameof(imageBytes));
			}

			return Convert.ToBase64String(imageBytes);
		}

		/// <summary>
		/// Issue a web request to download an image given its URL.
		/// </summary>
		/// <param name="uri">URL to the image</param>
		/// <param name="proxy">LMI Proxy</param>
		/// <returns>Image, as bytes</returns>
		public static byte[] DownloadImage(Uri uri, string proxy)
		{
			byte[] totalImageBytes;

			using (WebClient webClient = new())
			{
				webClient.Proxy = new WebProxy(proxy);
				webClient.UseDefaultCredentials = false;
				webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

				totalImageBytes = webClient.DownloadData(uri);
			}

			return totalImageBytes;
		}

		/// <summary>
		/// Pre-process and convert the given HTML, replacing each img-tag src attribute with its email-compatible CID representation
		/// and storing the associated image attibutes into the output collection.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <param name="images">Image output collection</param>
		/// <returns>Converted HTML</returns>
		public static string SeparateImagesForEmail(string html, ICollection<EmailImageInfo> images)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return html;
			}
			else if (images == null)
			{
				throw new ArgumentNullException(nameof(images));
			}

			string convertedHtml = html;  // initialize

			HtmlDocument document = new();
			document.LoadHtml(html);

			HtmlNodeCollection nodes;
			if ((nodes = document.DocumentNode.SelectNodes("//img")) != null)  // locate img tags
			{
				int imageCounter = 0;

				foreach (HtmlNode imgNode in nodes)  // for each img tag ...
				{
					IList<HtmlAttribute> attributesToRemove = new List<HtmlAttribute>();

					EmailImageInfo imageInfo = new();

					foreach (HtmlAttribute attribute in imgNode.Attributes)
					{
						string attributeName = attribute.Name.ToLower();

						switch (attributeName)
						{
							case "src":

								string src = attribute.Value;

								if (src.StartsWith("data:image"))  // if it is a base-64 embedded image ...
								{
									// ... generate the (unique) content ID
									string imageContentId = string.Format("image{0}", ++imageCounter);

									// ... parse out the src attribute "meta data" components
									string[] components = src.Split(',', ';', ':');

									// ... store the results
									imageInfo.EmailContentId = imageContentId;
									imageInfo.MimeType = components[1];
									imageInfo.Base64String = components[3];

									// ... replace the html src value with the content-id version
									attribute.Value = string.Format("cid:{0}", imageContentId);
								}

								break;

							case "height":

								imageInfo.Height = attribute.Value;
								attributesToRemove.Add(attribute);
								break;

							case "width":

								imageInfo.Width = attribute.Value;
								attributesToRemove.Add(attribute);
								break;

							default:

								attributesToRemove.Add(attribute);
								break;

						}  // end switch
					}  // end foreach attribute

					foreach (HtmlAttribute attr in attributesToRemove)
					{
						imgNode.Attributes.Remove(attr);
					}

					images.Add(imageInfo);

				}  // end foreach

				/*
				 * Finally, grab the resulting HTML (with all of the src attribute URL substitutions completed)
				 * 
				 */
				MemoryStream ms = null;
				try
				{
					ms = new MemoryStream();

					document.Save(ms);

					convertedHtml = document.DocumentNode.InnerHtml;

					ms.Close();
					ms = null;
				}
				finally
				{
					if (ms != null)
					{
						ms.Close();
					}
				}

			}  // end if nodes

			return convertedHtml;
		}
	}
}
