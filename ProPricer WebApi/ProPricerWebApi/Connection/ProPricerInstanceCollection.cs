///*
//    Copyright 2016-2020 Lockheed Martin Corporation.

//    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
//    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
//    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
//    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
//*/

//namespace APTSPropricerApi.Connection
//{
//    using System;
//    using System.Configuration;

//    /// <summary>
//    /// Define the ProPricerInstanceCollection that contains the ProPricerInstanceElement elements.
//    /// </summary>
//    /// <seealso cref="System.Configuration.ConfigurationElementCollection" />
//    public class ProPricerInstanceCollection : ConfigurationElementCollection
//    {
//        /// <summary>
//        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
//        /// </summary>
//        /// <returns>
//        /// A newly created <see cref="T:System.Configuration.ConfigurationElement" />.
//        /// </returns>
//        protected override ConfigurationElement CreateNewElement()
//        {
//            return new ProPricerInstanceElement();
//        }

//        /// <summary>
//        /// Gets the element key for a specified configuration element when overridden in a derived class.
//        /// </summary>
//        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
//        /// <returns>
//        /// An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.
//        /// </returns>
//        protected override Object GetElementKey(ConfigurationElement element)
//        {
//            return ((ProPricerInstanceElement)element).Id;
//        }
//    }
//}