// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Common;
	using System.Data.Entity;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Reflection;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;

	public static class StoredProcedureHelper
	{
		/// <summary> 
		/// Execute stored procedure with single table value parameter. The result set returned from the stored procedure call includes the id of each affected row and/or
		/// the new update date for that row. The 'updatedateIsPartOfResultSet'indicates if the update date is expected or not. If it's not, the returned collection
		/// includes a null for the date.
		/// </summary> 
		/// <param name="context">DbContext instance.</param> 
		/// <param name="dataTable">Data to store</param> 
		/// <param name="procedureName">Name of the Stored Procedure to execute</param> 
		/// <param name="paramName">the name of the Table Type Parameter in the Stored procedure</param> 
		/// <param name="typeName">Name of the user defined Table type as defined in the database</param> 
		/// <param name="updateDateIsPartOfResultSet">True indicates that the update date is part of the returned result set of the stored procedure being executed</param>
		/// <returns>Collection of KeyValuePairs in the same order as the rows in the original table. The KVP key is the id of a row (new or original) 
		/// and the value is the new update date if updateDateIsPartOfResultSet is true, otherwise this is null</returns>
		/// <exception cref="ArgumentException">Thrown if the connection string could not be determined from the context</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "no part of the sql command comes from the user - the command is created by code so it is safe to suppress this warning"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static ICollection<KeyValuePair<int, DateTime?>> ExecuteTableValueProcedure(DbContext context, DataTable dataTable, string procedureName, string paramName, string typeName, bool updateDateIsPartOfResultSet)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (dataTable == null)
			{
				throw new ArgumentNullException(nameof(dataTable));
			}

			ICollection<KeyValuePair<int, DateTime?>> idToUpdateDateXref = new Dictionary<int, DateTime?>();

			// create the table valued parameter for the stored procedure
			SqlParameter parameter = new(paramName, dataTable)
			{
				SqlDbType = SqlDbType.Structured,
				TypeName = typeName
			};

			// execute the stored procedure with the data table 
			string sql = string.Format("EXEC {0} {1};", procedureName, paramName);

			// need the entity context so we can grab the connection string   
			Database theDatabase = context.Database;
			if (theDatabase == null)
			{
				throw new ArgumentException("Context has a null Database property.", nameof(context));
			}

			DbConnection connection = theDatabase.Connection;

			if (connection == null)
			{
				throw new ArgumentException("Context's Connection property is null.", nameof(context));
			}

			string connectionString = connection.ConnectionString;

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException("The Context's connection string is null. ", nameof(context));
			}

			// this sql connection will be automatically enlisted in the current transaction
			using (SqlConnection sqlConnection = new(connectionString))
			{
				sqlConnection.Open();

				// execute sql using a SqlDataReader so we can get the returned ids and update dates 
				// in the same order we sent them - we suppress the security warning because no part of the sql command comes from the user
				// code generates it
				using (SqlCommand command = new(sql, sqlConnection))
				{
					command.CommandTimeout = ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", CommonConstants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT);

					command.Parameters.Add(parameter);

					using (SqlDataReader dataReader = command.ExecuteReader())
					{
						// now iterate thru the returned values which should include the object's id and new update date
						if (dataReader.HasRows)
						{

							while (dataReader.Read())
							{
								// grab the returned id and the new update date if the date is included
								int id = dataReader.GetInt32(0);

								/*
                                 * Bug-33719:
                                 * 
                                 * ALL of the bulk insert and update procedures ("...viaTableParameter") are written to return a table
                                 * with two columns:  an integer PKID and a datetime UpdateDT.
                                 * 
                                 * In some cases, this table can include multiple rows for a single PKID.  This scenario is handled
                                 * by the following Any clause.
                                 * 
                                 */
								if (!idToUpdateDateXref.Any(x => x.Key == id))
								{
									if (updateDateIsPartOfResultSet)
									{
										DateTime newUpdateDate = dataReader.GetDateTime(1);

										idToUpdateDateXref.Add(new KeyValuePair<int, DateTime?>(id, newUpdateDate));
									}
									else
									{
										idToUpdateDateXref.Add(new KeyValuePair<int, DateTime?>(id, null));
									}
								}
							}
						}
					}
				}
			}

			return idToUpdateDateXref;
		}

		/// <summary> 
		/// Creates data table from collection of entities in source data. This ensures that the property names match the table column names.
		/// NOTE: The propertiesToIncludeInTable should match the names and order of the 'columns' in the user-defined table type in the database.
		/// </summary> 
		/// <param name="propertiesToIncludeInTable">Names of the properties to include in the data table</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "the table var is returned to the caller so it should NOT be disposed of")]
		public static DataTable ToDataTable<TEntityType>(ICollection<TEntityType> source, ICollection<string> propertiesToIncludeInTable)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (propertiesToIncludeInTable == null)
			{
				throw new ArgumentNullException(nameof(propertiesToIncludeInTable));
			}
			if (!propertiesToIncludeInTable.Any())
			{
				throw new ArgumentException("collection must contain at least one property name", nameof(propertiesToIncludeInTable));
			}

			DataTable table = new();

			// get properties of T 
			BindingFlags binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
			PropertyReflectionOptions options = PropertyReflectionOptions.IgnoreEnumerable | PropertyReflectionOptions.IgnoreIndexer;

			List<PropertyInfo> properties = ReflectionHelper.GetProperties(typeof(TEntityType), binding, options).ToList();
			ICollection<PropertyInfo> includedProperties = new Collection<PropertyInfo>();
			Dictionary<string, object> nestedValues = new();

			// create table schema based on properties to include - the order of the property names MUST match the order of the table type columns
			foreach (string propertyName in propertiesToIncludeInTable)
			{
				//grab the corresponding property
				PropertyInfo matchingProperty = (from property in properties
												 where property.Name.Equals(propertyName)
												 select property).FirstOrDefault();

				if (matchingProperty == null)
				{
					foreach (PropertyInfo property in properties)
					{
						matchingProperty = GetNestedProperty(propertyName, property.GetValue(source.First(), null), property.PropertyType, out object value);
						if (matchingProperty != null)
						{
							nestedValues.Add(propertyName + "0", value);

							for (int i = 1; i < source.Count; i++)
							{
								value = null;
								GetNestedProperty(propertyName, property.GetValue(source.ElementAt(i)), property.PropertyType, out value);
								nestedValues.Add(propertyName + i, value);
							}
							break;
						}
					}

					if (matchingProperty == null)
					{
						throw new ArgumentException("Couldn't find matching property information for '" + propertyName + "'", nameof(propertiesToIncludeInTable));
					}
				}

				table.Columns.Add(matchingProperty.Name, Nullable.GetUnderlyingType(matchingProperty.PropertyType) ?? matchingProperty.PropertyType);
				includedProperties.Add(matchingProperty);
			}

			// now add the Order ID column - this is a standard column being added to all user-defined table types so the stored procedure 
			// can return the result set in the same order as the input data. That allows callers to match up the original
			// "row" with the returned id so the original 'row' can be updated with the new id (only in the insert case)
			table.Columns.Add("OrderID", typeof(int));

			// create table data from T instances - adding one for the DeveloperOrderId column in the DataTable type
			int numberOfPropertiesIncluded = includedProperties.Count;
			object[] values = new object[numberOfPropertiesIncluded + 1];

			int developerOrderId = 0;

			foreach (TEntityType item in source)
			{
				for (int i = 0; i < numberOfPropertiesIncluded; i++)
				{
					PropertyInfo propInfo = includedProperties.ElementAt(i);
					string nestedValueKey = propInfo.Name + developerOrderId; // developerOrderId is the same as the iteration of the foreach loop on source
					if (nestedValues.Any() && nestedValues.ContainsKey(nestedValueKey))
					{
						values[i] = nestedValues[nestedValueKey] ?? DBNull.Value;
					}
					else
					{
						values[i] = propInfo.GetValue(item, null) ?? DBNull.Value;
					}
				}

				// now add the developer order
				values[numberOfPropertiesIncluded] = developerOrderId;
				developerOrderId++;

				table.Rows.Add(values);
			}

			return table;
		}

		/// <summary>
		/// Recursively checks the nested fields for the property and gets the PropertInfo and value
		/// </summary>
		/// <param name="propname">Property to get</param>
		/// <param name="instance">Object to search for property property</param>
		/// <param name="type">Property type</param>
		/// <param name="value">Property value</param>
		/// <returns>Property Info for given property</returns>
		private static PropertyInfo GetNestedProperty(string propname, object instance, Type type, out object value)
		{
			value = null;
			PropertyInfo toReturn = null;

			if (instance != null)
			{
				PropertyInfo[] properties = type.GetProperties();

				foreach (PropertyInfo property in properties)
				{
					value = property.GetValue(instance, null);

					if (property.Name.Equals(propname))
					{
						toReturn = property;
					}
					else if (property.PropertyType.FullName != "System.String"
						&& property.PropertyType.FullName != "System.DateTime"
						&& !property.PropertyType.IsPrimitive)
					{
						// Don't need to continue checking for strings, datetimes, or primitive types, otherwise keep searching
						// DateTime in particular causes a stack overflow due to the Date property within DateTime
						toReturn = GetNestedProperty(propname, value, property.PropertyType, out value);
					}

					if (toReturn != null)
					{
						// if found, the search can stop
						break;
					}
				}
			}

			return toReturn;
		}
	}
}
