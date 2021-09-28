// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.GeneralHelper
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Sorting Class
    /// </summary>
    public class SortHelper<T>
    {
        /// <summary>
        /// Sorts the data based on the Sort By and Order
        /// </summary>
        /// <param name="dataToSort">Data which will be sorted</param>
        /// <param name="sortBy">Property by which we should sort</param>
        /// <param name="orderDirection">Order direction</param>
        /// <returns>Sorted Data</returns>
        public ICollection<T> Sort(ICollection<T> dataToSort, string sortBy, SortOrder orderDirection)
        {
            if (dataToSort == null)
            {
                throw new ArgumentNullException(nameof(dataToSort));
            }

            IOrderedQueryable<T> result = (IOrderedQueryable<T>)dataToSort.AsQueryable();

            if (!string.IsNullOrEmpty(sortBy))
            {
                ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), string.Empty) };
                PropertyInfo pi = typeof(T).GetProperty(sortBy);
                IOrderedQueryable<T> data = (IOrderedQueryable<T>)dataToSort.AsQueryable();
                Type[] dataTypes = new Type[] { typeof(T), pi.PropertyType };

                string orderCommand = "OrderBy";
                if (orderDirection == SortOrder.Descending)
                {
                    orderCommand = "OrderByDescending";
                }

                result = (IOrderedQueryable<T>)data.Provider.CreateQuery(
                        Expression.Call(
                            typeof(Queryable),
                            orderCommand,
                            dataTypes,
                            data.Expression,
                            Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams)));
            }

            return result.AsEnumerable().ToList();
        }
    }
}
