// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    ///  Xml Extensions pulled from https://stackoverflow.com/questions/1138414/can-i-serialize-xml-straight-to-a-string-instead-of-a-stream-with-c
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Converts To XML string.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>An Xml String.</returns>
        public static string ToXmlString<T>(this T input)
        {
            using (var writer = new StringWriter())
            {
                input.ToXml(writer);
                return writer.ToString();
            }
        }
        /// <summary>
        /// Convert To XML.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="writer">The writer.</param>
        public static void ToXml<T>(this T objectToSerialize, TextWriter writer)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
        }
    }
}
