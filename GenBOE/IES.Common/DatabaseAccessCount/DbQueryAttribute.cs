// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;

    /// <summary>
    /// Marks a method as a select query
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DbQueryAttribute : HandlerAttribute
    {
        /// <summary>
        /// Increment
        /// </summary>
        int increment;

        /// <summary>
        /// Constructor
        /// </summary>
        public DbQueryAttribute()
        {
            this.increment = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="increment">increment</param>
        public DbQueryAttribute(int increment)
        {
            this.increment = increment;
        }

        /// <summary>
        /// Create the aspect handler
        /// </summary>
        /// <param name="container">unity container</param>
        /// <returns>handler</returns>
        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new DbQueryCallHandler(increment);
        }

        /// <summary>
        /// Increment Value
        /// </summary>
        public int Increment 
        { 
            get { return this.increment; } 
        }
    }
}
