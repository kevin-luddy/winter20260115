// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides useful utilities for using reflection.
    /// </summary>
    public static class ReflectionHelper
    {        
        /// <summary>         
        /// Gets properties of T         
        /// </summary>         
        public static IEnumerable<PropertyInfo> GetProperties(System.Type typeToReflectOn, BindingFlags binding, PropertyReflectionOptions options = PropertyReflectionOptions.All)
        {
			PropertyInfo[] properties = typeToReflectOn.GetProperties(binding);
            bool all = (options & PropertyReflectionOptions.All) != 0;
            bool ignoreIndexer = (options & PropertyReflectionOptions.IgnoreIndexer) != 0;
            bool ignoreEnumerable = (options & PropertyReflectionOptions.IgnoreEnumerable) != 0;
            foreach (PropertyInfo property in properties)
            {
                if (!all)
                {
                    if (ignoreIndexer && IsIndexer(property))
                    {
                        continue;
                    }
                    if (ignoreIndexer && !property.PropertyType.Equals(typeof(string)) && IsEnumerable(property))
                    {
                        continue;
                    }
                }

                yield return property;
            }
        }

        /// <summary>         
        /// Check if property is indexer         
        /// </summary>         
        public static bool IsIndexer(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
			ParameterInfo[] parameters = property.GetIndexParameters();
            if (parameters != null && parameters.Length > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>         
        /// Check if property implements IEnumerable         
        /// </summary>         
        public static bool IsEnumerable(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return property.PropertyType.GetInterfaces().Any(x => x.Equals(typeof(System.Collections.IEnumerable)));
        }
    }
}
