// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Web;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.Unity.InterceptionExtension;

    /// <summary>
    /// Call Handler
    /// </summary>
    [ConfigurationElementType(typeof(CustomCallHandlerData))]
    public class DbQueryCallHandler : ICallHandler
    {
        /// <summary>
        /// value to increment
        /// </summary>
        private int increment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="increment">increment value</param>
        public DbQueryCallHandler(int increment)
        {
            this.increment = increment;
        }

        /// <summary>
        /// Aspect Handler
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="getNext">next aspect</param>
        /// <returns>method return</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            if (getNext == null)
            {
                throw new ArgumentNullException(nameof(getNext));
            }

            // get the request
            if (HttpContext.Current != null)
            {
                var requestItems = HttpContext.Current.Items;

                // increment db calls
                if (requestItems.Contains(DbQueryConstants.CURRENT_QUERIES))
                {
                    requestItems[DbQueryConstants.CURRENT_QUERIES] = (int)requestItems[DbQueryConstants.CURRENT_QUERIES] + increment;
                }
                else
                {
                    requestItems[DbQueryConstants.CURRENT_QUERIES] = increment;
                }
            }
            
            // execute the method
            IMethodReturn methodreturn = getNext()(input, getNext);

            return methodreturn;
        }

        /// <summary>
        /// Aspect Order
        /// </summary>
        public int Order { get; set; }
    }
}
