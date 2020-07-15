// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class SystemSettingDTO
    {
        public SystemSettingDTO()
        {
            Key = string.Empty;
            Value = string.Empty;
        }

        /// <summary>
        /// System setting key
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Key { get; set; }

        /// <summary>
        /// System setting value
        /// </summary>
        [StringLength(4000)]
        public string Value { get; set; }
    }
}
