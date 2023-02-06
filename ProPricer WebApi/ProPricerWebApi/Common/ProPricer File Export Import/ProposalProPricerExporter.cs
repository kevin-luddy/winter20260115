// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2015 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
	using System;
	using System.IO;
	using System.Text;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.ImportExport;
	using EBS.ProPricer.ImportExport.Ascii;
	using APTSPropricerApi.Connection;
	using EBS.Core;

	/// <summary>
	/// ProPricerProposalExporter handles exporting task and resource data directly to a ProPricer proposal.
	/// </summary>
	public class ProPricerProposalExporter
	{
		#region private fields / constants

		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger Logger;

		/// <summary>
		/// File extension name for the task export file.
		/// </summary>
		private const string TASK_EXPORT_FILE_EXTENSION = "tsk";

		/// <summary>
		/// File extension name for the resource export file.
		/// </summary>
		private const string RESOURCE_EXPORT_FILE_EXTENSION = "res";

		/// <summary>
		/// When errors occur with and export, ProPricer creates an exception file with a .exc extension. This file
		/// contains a list of export records that are in error. It needs to be mapped to a corresponding .log file.
		/// </summary>
		private const string PROPRICER_ERROR_FILE_EXTENSION = "exc";

		/// <summary>
		/// When errors occur with an export, ProPricer creates an exception file with a .exc extension. This file
		/// contains a list of export records that are in error. It needs to be mapped to a corresponding .log file. The
		/// log file contains the actual error message. So record 1 in exc file maps to record 1 in the log file.
		/// </summary>
		private const string PROPRICER_LOG_FILE_EXTENSION = "log";

		#endregion

		/// <summary>
		/// #ctor
		/// </summary>
		/// <param name="logger">Logger</param>
		/// <param name="poolManagerList">Pool Manager</param>
		public ProPricerProposalExporter(ILogger<ProPricerProposalExporter> logger, PoolManagerList poolManagerList)
		{
			this.Logger = logger;
			this.poolManagerList = poolManagerList;
		}

		/// <summary>
		/// Exports genBOE task and resource data to a ProPricer proposal.
		/// </summary>
		/// <param name="inTempPathFile">Temporary file path to store task and resoure export files.</param>
		/// <param name="proposalId">Id of the Proposal to export to.</param>
		/// <param name="instanceId">PP Instance Id</param>
		/// <param name="taskData">task data to export.</param>
		/// <param name="resourceData">resource data to export.</param>
		/// <param name="taskExportOption">Task data export option.</param>
		/// <param name="resourceExportOption">Resource Hours/Cost export option.</param>
		/// <returns>The results of the Task and resource export.</returns>
		public ProPricerExportResults ExportToProPricerProposal(string inTempPathFile, string proposalId, int instanceId,
			string taskData, string resourceData, ProPricerExportOption taskExportOption,
			ProPricerExportOption resourceExportOption)
		{
			ProPricerExportResults exportResults = new();

			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Guid id = new(proposalId);
				Proposal proposal = ppc.Workspace.Proposals.Find(id).Value();
				if (proposal != null)
				{
					proposal.Open();
					string taskFileName = CreateTemporaryExportFile(inTempPathFile, TASK_EXPORT_FILE_EXTENSION, taskData);
					string exceptionFileName = GetExceptionOrLogFileName(taskFileName, PROPRICER_ERROR_FILE_EXTENSION);
					exportResults.TaskExportResults = ExportTasksToProposal(proposal, taskFileName, exceptionFileName, taskExportOption);

					// Export resources. This needs to be done after the Tasks due to dependency.
					string resourceFileName = CreateTemporaryExportFile(inTempPathFile, RESOURCE_EXPORT_FILE_EXTENSION, resourceData);
					exceptionFileName = GetExceptionOrLogFileName(resourceFileName, PROPRICER_ERROR_FILE_EXTENSION);
					exportResults.ResourceExportResults = ExportResourcesToProposal(proposal, resourceFileName, exceptionFileName, resourceExportOption);
					proposal.Close();
				}
				else
				{
					throw new ArgumentException("Proposal was not found or could not be opened in the workspace.", nameof(proposalId));
				}
			}

			return exportResults;
		}

		#region Private Helpers

		/// <summary>
		/// Exports genBOE task data to a ProPricer ProPricer proposal.
		/// </summary>
		/// <param name="proposal">ProPricer Proposal.</param>
		/// <param name="taskExportFileName">Path to task file to be exported.</param>
		/// <param name="exceptionFileName">Name of the file to contain exceptions for exports. This file along with a .log file with
		/// the same name and path can be use to report errors.</param>
		/// <param name="taskExportOption">Task export option.</param>
		/// <returns>The results of the export.</returns>
		private ProPricerExportResult ExportTasksToProposal(Proposal proposal, string taskExportFileName, string exceptionFileName, ProPricerExportOption taskExportOption)
		{
			ProPricerExportResult exportResults = new();

			try
			{
				// Instantiate, Initialize and Execute export using ProPricer ASCII "Task Data" import API.
				TaskDataImport taskDataImport = new(proposal.Tasks);

				if (taskExportOption == ProPricerExportOption.ReplaceAllExisting)
				{
					taskDataImport.ReplaceAllExisting = true;
				}
				else
				{
					taskDataImport.DuplicateOption = this.ConvertExportOption(taskExportOption);
				}

				taskDataImport.FileName = taskExportFileName;
				taskDataImport.ExceptionFilePath = exceptionFileName;
				taskDataImport.IgnoreFirstLine = false;
				taskDataImport.Delimiter = ',';
				taskDataImport.Process();
				exportResults = GetExportResults(taskDataImport.Statistics, exceptionFileName, this.GetExceptionOrLogFileName(taskExportFileName, PROPRICER_LOG_FILE_EXTENSION));
			}
			finally
			{
				// Clean up export files.
				DeleteFile(taskExportFileName);
				DeleteFile(exceptionFileName);
				DeleteFile(GetExceptionOrLogFileName(taskExportFileName, PROPRICER_LOG_FILE_EXTENSION));
			}

			return exportResults;
		}

		/// <summary>
		/// Exports genBOE task data to a ProPricer ProPricer proposal.
		/// </summary>
		/// <param name="proposal">ProPricer Proposal.</param>
		/// <param name="resourceExportFile">Path to resource file to be exported.</param>
		/// <param name="exceptionFileName">Name of the file to contain exceptions for exports. This file along with a .log file with
		/// the same name and path can be use to report errors.</param>
		/// <param name="resourceExportOption">Task export option.</param>
		/// <returns>The results of the export.</returns>
		private ProPricerExportResult ExportResourcesToProposal(Proposal proposal, string resourceExportFile, string exceptionFileName, ProPricerExportOption resourceExportOption)
		{
			ProPricerExportResult exportResults = new();

			try
			{
				// Instantiate, Initialize and Execute export using ProPricer ASCII "Resource Hours/Cost" import API.
				ResourceHoursCostImport hoursCostImport = new(proposal.Tasks);

				if (resourceExportOption == ProPricerExportOption.ReplaceAllExisting)
				{
					hoursCostImport.ReplaceAllExisting = true;
				}
				else
				{
					hoursCostImport.DuplicateOption = this.ConvertExportOption(resourceExportOption);
				}

				hoursCostImport.Delimiter = ',';
				hoursCostImport.FileName = resourceExportFile;
				hoursCostImport.ExceptionFilePath = exceptionFileName;
				hoursCostImport.IgnoreFirstLine = false;
				hoursCostImport.Process();
				exportResults = GetExportResults(hoursCostImport.Statistics, exceptionFileName, this.GetExceptionOrLogFileName(resourceExportFile, PROPRICER_LOG_FILE_EXTENSION));
			}
			finally
			{
				DeleteFile(resourceExportFile);
				DeleteFile(exceptionFileName);
				DeleteFile(GetExceptionOrLogFileName(resourceExportFile, PROPRICER_LOG_FILE_EXTENSION));
			}

			return exportResults;
		}

		/// <summary>
		/// Converts a ProPricerExportOption to an EBS.ProPricer.ImportExport.DuplicateOption that is required for the export.
		/// </summary>
		/// <param name="exportOption">ProPricerExportOption</param>
		/// <returns>Converted DuplicateOption.</returns>
		private DuplicateOption ConvertExportOption(ProPricerExportOption exportOption)
		{
			DuplicateOption convertedExportOption = DuplicateOption.Skip;

			switch (exportOption)
			{
				case (ProPricerExportOption.OverwriteDuplicates):
					{
						// TODO: What is the difference between Overwrite, SafeOverwrite, and ReleaseOverwrite?
						// Overwrites duplicate records.
						convertedExportOption = DuplicateOption.Overwrite;
						break;
					}
				case (ProPricerExportOption.DoNotOverwriteDuplicates):
					{
						// TODO: Does DuplicateOption.Skip equate to Do not overwrite???
						convertedExportOption = DuplicateOption.Skip;
						break;
					}
				case (ProPricerExportOption.AddValueToDuplicate):
					{
						// Accumulates resource hours if a resource already exists.
						convertedExportOption = DuplicateOption.AddToExisting;
						break;
					}
			}

			return convertedExportOption;
		}

		/// <summary>
		/// Creates a temporary physical file from a memory stream. The ProPricer API requires a path to a physical file for export. This file
		/// should be deleted after the export completes.
		/// </summary>
		/// <param name="folderPath">Path to the folder for storing the physical file.</param>
		/// <param name="fileExtension">File extension to use.</param>
		/// <param name="data">The data to write out.</param>
		/// <returns>The fully qualified path to the file.</returns>
		private string CreateTemporaryExportFile(string folderPath, string fileExtension, string data)
		{
			// Create a new unique file name for saving the data.
			string fullFileName = $"{Path.GetDirectoryName(folderPath)}\\{Path.GetRandomFileName()}.{fileExtension}";
			File.WriteAllText(fullFileName, data);

			return fullFileName;
		}

		/// <summary>
		/// Gets a user readable string that is the result of an export.
		/// </summary>
		/// <param name="importStats">EBS import statistics object.</param>
		/// <param name="exceptionFileName">Exception File Name</param>
		/// <param name="logFileName">Log File Name</param>
		/// <returns>Export results string.</returns>
		private ProPricerExportResult GetExportResults(Statistics importStats, string exceptionFileName, string logFileName)
		{
			ProPricerExportResult exportResults = new()
			{
				TotalRecordsInException = importStats.Exceptioned,
				TotalRecordsInImportFile = importStats.All,
				TotalRecordsSkipped = importStats.Skipped,
				TotalRecordsSuccessfullyExported = importStats.Imported
			};

			if (exportResults.TotalRecordsInException > 0)
			{
				ProcessExceptionFile(exportResults, exceptionFileName, logFileName);
			}

			return exportResults;
		}

		/// <summary>
		/// Processes errors for the exception and log files. Theses files have corresponding 1:1 record where the exception file
		/// contains records we attempted to export and the log file contains error messages resulting in the failed export of
		/// those records.
		/// </summary>
		/// <param name="exportResults">The Export Results</param>
		/// <param name="exceptionFile">Fully qualified path to the exception file.</param>
		/// <param name="logFile">Fully qualified path to the log file.</param>
		public void ProcessExceptionFile(ProPricerExportResult exportResults, string exceptionFile, string logFile)
		{
			if (File.Exists(exceptionFile) && File.Exists(logFile))
			{
				StreamReader exceptionFileHandle = null;
				StreamReader logFileHandle = null;
				try
				{
					exceptionFileHandle = new StreamReader(exceptionFile);
					logFileHandle = new StreamReader(logFile);
					string exceptionRecord;
					while ((exceptionRecord = exceptionFileHandle.ReadLine()) != null)
					{
						// exceptionRecord is one of the export records from the csv file we tried to export. The
						// first field is the TaskId. Strip it out of the record.
						string taskId = exceptionRecord.Remove(exceptionRecord.IndexOf(","));
						string errorMessage;
						// Get the associated error message for the above task.
						if ((errorMessage = logFileHandle.ReadLine()) != null)
						{
							if (!exportResults.TaskIdErrorMessages.ContainsKey(taskId))
							{
								exportResults.TaskIdErrorMessages.Add(taskId, errorMessage);
							}
						}
						else
						{
							// There should be a 1:1 corresponding records in each file. If we get here, that was 
							// not the case, so just break out.
							break;
						}
					}
				}
				catch (Exception ex)
				{
					StringBuilder error = new(ex.Message);
					error.Append(" Please Contact your system administrator. ");
					exportResults.ProcessExceptionFileError = error.ToString();
					error.Append(ex.StackTrace);
					this.Logger.LogError(error.ToString());
				}
				finally
				{
					if (exceptionFileHandle != null)
					{
						exceptionFileHandle.Close();
					}
					if (logFileHandle != null)
					{
						logFileHandle.Close();
					}
				}
			}
		}

		/// <summary>
		/// Gets a user readable string that is the result of an export.
		/// </summary>
		/// <param name="importStats">EBS import statistics object.</param>
		/// <returns>Export results string.</returns>
		private string GetImportStatistics(Statistics importStats)
		{
			StringBuilder stats = new();
			stats.Append("IMPORTING COMPLETED" + Environment.NewLine + Environment.NewLine);
			stats.Append("Records in the Import File:       " + importStats.All + Environment.NewLine);
			stats.Append("Records in the Exception File:    " + importStats.Exceptioned + Environment.NewLine);
			stats.Append("Duplicates Skipped as Requested:  " + importStats.Skipped + Environment.NewLine + Environment.NewLine);
			stats.Append("Records Successfully Imported:    " + importStats.Imported + Environment.NewLine);
			return stats.ToString();
		}

		/// <summary>
		/// Gets the name of the .exc or .log file used for writing errors that occur during the export process. The .exc file
		/// contains a list export records that were in error and the .log file contains a list of corresponding error messages 
		/// for the records in error. The file names will be the same as the export file name with .exc or .log extensions.
		/// </summary>
		/// <param name="baseFileName">The export file name.</param>
		/// <param name="fileExtension">File extension string.</param>
		/// <returns>Name of .exc or log file.</returns>
		private string GetExceptionOrLogFileName(string baseFileName, string fileExtension)
		{
			string exceptionFileName = string.Empty;

			if (baseFileName == null)
			{
				throw new ArgumentNullException(nameof(baseFileName));
			}
			exceptionFileName = String.Format("{0}{1}{2}{3}", Path.GetDirectoryName(baseFileName), @"\", Path.GetFileNameWithoutExtension(baseFileName), "." + fileExtension);
			return exceptionFileName;
		}

		/// <summary>
		/// Deletes temporary files created by the export process.
		/// </summary>
		/// <param name="fileName">Fully qualified file name.</param>
		private void DeleteFile(string fileName)
		{
			try
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning(String.Format("Failed to delete ProPricer export file: {0} Exception: {1}", fileName, ex.Message));
			}
		}

		#endregion
	}
}