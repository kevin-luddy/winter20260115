// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Attribute for defining what Contract Types belong to this Contract Type Group
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ContractTypesAttribute : Attribute
    {
        /// <summary>
        /// Contract Types
        /// </summary>
        private ICollection<ContractType> contractTypes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contractTypes">Contract Types that belong to this group</param>
        public ContractTypesAttribute(params ContractType[] contractTypes)
        {
            this.contractTypes = new Collection<ContractType>();

            if (contractTypes != null)
            {
                foreach (ContractType group in contractTypes)
                {
                    this.contractTypes.Add(group);
                }
            }
        }

        /// <summary>
        /// Contract Types
        /// </summary>
        public ICollection<ContractType> ContractTypes
        {
            get
            {
                return this.contractTypes;
            }
        }
    }
}
