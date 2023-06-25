// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.DeleteWorkspaces
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Data.SqlTypes;
	using System.IO;
	using System.Linq;
	using IES.Common;
	using Microsoft.VisualBasic.FileIO;

	/// <summary>
	/// Delete a list of workspaces by id
	/// </summary>
	internal class DeleteWorkspacesProgram
	{
		/// <summary>
		/// Logger
		/// </summary>
		static readonly Logger logger = new Logger(typeof(DeleteWorkspacesProgram));

		/// <summary>
		/// Batch Size for Updating Workspace
		/// </summary>
		public const int batchSize = 50;

		/// <summary>
		/// Database connections tring
		/// </summary>
		static readonly string connectionString = ConfigurationUtilities.GetConnectionString("GenBoeEntities").ConnectionString;

		/// <summary>
		/// SQL for a Soft Delete
		/// </summary>
		public const string SOFT_DELETE_SQL = "Update Workspace Set IsDeleted = 1, DateDeleted = DateAdd(dd, -61, getDate()) WHERE WorkspaceID IN ({0})";

		/// <summary>
		/// SQL for calling Stored Procedure
		/// </summary>
		public const string HARD_DELETE_SP = "deleteWorkspace_ProcessSoftDelete";

		/// <summary>
		/// Main program
		/// </summary>
		/// <param name="args">console line arguments</param>
		static void Main(string[] args)
		{
			try
			{
				List<string> workspaceIds = LoadWorkspaceIdsFromCSV();
				SoftDeleteWorkspaces(workspaceIds);
				HardDeleteWorkspaces();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error deleting workspaces: " + ex.ToString());
				logger.Error(ex);
			}

			Console.WriteLine("Done processing, hit Enter to exit");
			Console.ReadLine();
		}

		/// <summary>
		/// Run stored procedure to Hard Delete the workspaces
		/// </summary>
		private static void HardDeleteWorkspaces()
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();

				using (SqlCommand cmd = new SqlCommand(HARD_DELETE_SP, conn)
				{
					CommandTimeout = 0,
					CommandType = System.Data.CommandType.StoredProcedure,

				})
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Update the database to soft delete the workspaces
		/// </summary>
		/// <param name="workspaceIds">Workspace Ids to soft delete.</param>
		private static void SoftDeleteWorkspaces(List<string> workspaceIds)
		{
			// loop through with batch sizes of 50
			Console.WriteLine($"Processing in batches of size {batchSize}");

			int batchIndex = 1;
			foreach (IEnumerable<string> batch in workspaceIds.Batch(batchSize))
			{
				Console.WriteLine($"Updating Workspaces in batch {batchIndex++}");
				string ids = string.Join(", ", batch);
				string sqlString = string.Format(SOFT_DELETE_SQL, ids);
				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					using (SqlCommand cmd = new SqlCommand(sqlString, conn)
						{
							CommandTimeout = 0
						})
					{
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Load the workspace Ids from csv file
		/// </summary>
		/// <returns>Workspace Ids loaded from csv file</returns>
		/// <exception cref="IOException">Thrown when a row in the CSV is not an integer</exception>
		private static List<string> LoadWorkspaceIdsFromCSV()
		{
			Console.WriteLine("Loading from csv file");
			List<string> workspaceIds = new List<string>();
			string csvFile = ConfigurationUtilities.GetAppSetting("CsvFile");
			TextFieldParser parser = new TextFieldParser(csvFile);
			parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
			parser.SetDelimiters(",");

			while (!parser.EndOfData)
			{
				string row = parser.ReadLine();
				if (!string.IsNullOrWhiteSpace(row))
				{
					if (int.TryParse(row, out int result))
					{
						workspaceIds.Add(row);
					}
					else
					{
						throw new IOException("Improper line is not an integer: " + row);
					}
				}
			}

			Console.WriteLine($"Loaded {workspaceIds.Count} Workspace IDs from csv file");
			return workspaceIds;
		}
	}
}
