// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    /// <summary>
    /// Custom view-development helper methods for rendering C# objects and values as HTML.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Retrieve a value from ViewData
        /// </summary>
        /// <typeparam name="T">Expected type of value being retrieved</typeparam>
        /// <param name="html">Helper reference</param>
        /// <param name="key">ViewData key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value stored in ViewData, or defaultValue if key is not present in ViewData</returns>
        public static T GetViewDataObject<T>(this HtmlHelper html, string key, T defaultValue = null) where T : class
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            object viewDataObject;

            if (html.ViewData.ContainsKey(key))
            {
                viewDataObject = html.ViewData[key];
            }
            else
            {
                viewDataObject = defaultValue;
            }

            return viewDataObject as T;
        }

        /// <summary>
        /// Retrieve a value from ViewData
        /// </summary>
        /// <typeparam name="T">Expected type of value being retrieved</typeparam>
        /// <param name="html">Helper reference</param>
        /// <param name="key">ViewData key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value stored in ViewData, or defaultValue if key is not present in ViewData</returns>
        public static T GetViewDataValue<T>(this HtmlHelper html, string key, T? defaultValue = null) where T : struct
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            object viewDataObject;

            if (html.ViewData.ContainsKey(key))
            {
                viewDataObject = html.ViewData[key];
            }
            else if (defaultValue.HasValue)
            {
                viewDataObject = defaultValue.Value;
            }
            else
            {
                viewDataObject = null;
            }

            T returnValue;

            if (viewDataObject == null)
            {
                returnValue = default(T);
            }
            else
            {
                Type returnType = typeof(T);
                Type intType = typeof(int);
                Type viewDataObjectType = viewDataObject.GetType();

                // use casting for enums ...
                if ((returnType.IsEnum && viewDataObjectType.IsAssignableFrom(intType)) ||
                    (viewDataObjectType.IsEnum && returnType.IsAssignableFrom(intType)))
                {
                    returnValue = (T)viewDataObject;
                }
                else  // ... use conversion for everything else
                {
                    returnValue = (T)Convert.ChangeType(viewDataObject, returnType);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Get the JavaScript representation of the value.
        /// </summary>
        /// <param name="html">Helper reference</param>
        /// <param name="b">Server-side value</param>
        /// <returns>Client-side (JavaScript) value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "html")]
        public static MvcHtmlString GetJSValue(this HtmlHelper html, bool b)
        {
            return new MvcHtmlString(b ? "true" : "false");
        }
    }
}