///*
//    Copyright 2016-2020 Lockheed Martin Corporation.

//    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
//    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
//    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
//    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
//*/

//namespace APTSPropricerApi.Connection
//{
//    using System.Configuration;

//    /// <summary>
//    /// A configuration section housing multiple ProPricerInstance Elements.
//    /// </summary>
//    /// <seealso cref="System.Configuration.ConfigurationSection" />
//    public class ProPricerInstancesSection : ConfigurationSection
//    {
//        /// <summary>
//        /// Gets or sets the pro pricer instances.
//        /// </summary>
//        [ConfigurationProperty("instances", IsDefaultCollection = false)]
//        [ConfigurationCollection(typeof(ProPricerInstanceCollection),
//            AddItemName = "add",
//            ClearItemsName = "clear",
//            RemoveItemName = "remove")]
//        public ProPricerInstanceCollection ProPricerInstances
//        {
//            get
//            {
//                ProPricerInstanceCollection ProPricerInstanceCollection =
//                    (ProPricerInstanceCollection)base["instances"];

//                return ProPricerInstanceCollection;
//            }

//            set
//            {
//                ProPricerInstanceCollection ProPricerInstanceCollection = value;
//            }

//        }
//    }
//}