IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMOQTypeSelectionTableData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMOQTypeSelectionTableData];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertMOQTypeSelectionTableData]
(
	@MOQTypeSelectionTableDataId int,
	@MOQTypeSelectionId int,
	@UpdateDT datetime2,
	@Order int,
	@TableName varchar(255),
	@RepositoryName varchar(50),
	@QueryType varchar(40),
	@DateOfReport datetime2,
	@HistoricalProgramName varchar(125),
	@ContractNumber varchar(255),
	@WbsElement varchar(8000),
	@PeriodOfPerformanceStartDate datetime2,
	@PeriodOfPerformanceEndDate datetime2,
	@TotalWbsHours decimal(10,2),
	@AdditionalQueryFilters varchar(2500),
	@TotalRelevantHoursAfterQueryFilters decimal(10,2)
)
AS
/******************************************************************************
**		 
**		Name: [upsertMOQTypeSelectionTableData]
**		Desc: Insert/Update data into MOQTypeSelectionTableData
**			
**		
**
**		Auth: ranzalon
**		Date: 9/11/2020
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		10/27/22	ranzalon			IES-1951 - fix WBS Element size
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @MOQTypeSelectionTableData AS Table (MOQTypeSelectionTableDataId int)

IF @MOQTypeSelectionTableDataId  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[MOQTypeSelectionTableData]
           (
				[MOQTypeSelectionId],
				[UpdateDT],
				[Order],
				[TableName],
				[RepositoryName],
				[QueryType],
				[DateOfReport],
				[HistoricalProgramName],
				[ContractNumber],
				[WbsElement],
				[PeriodOfPerformanceStartDate],
				[PeriodOfPerformanceEndDate],
				[TotalWbsHours],
				[AdditionalQueryFilters],
				[TotalRelevantHoursAfterQueryFilters]
		   )
     OUTPUT inserted.MOQTypeSelectionTableDataId INTO @MOQTypeSelectionTableData
     VALUES
           (
				@MOQTypeSelectionId,
				@UpdateDT,
				@Order,
				@TableName,
				@RepositoryName,
				@QueryType,
				@DateOfReport,
				@HistoricalProgramName,
				@ContractNumber,
				@WbsElement,
				@PeriodOfPerformanceStartDate,
				@PeriodOfPerformanceEndDate,
				@TotalWbsHours,
				@AdditionalQueryFilters,
				@TotalRelevantHoursAfterQueryFilters
            ) 
            
	SELECT @MOQTypeSelectionTableDataId = MOQTypeSelectionTableDataId FROM @MOQTypeSelectionTableData

END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[MOQTypeSelectionTableData] WHERE MOQTypeSelectionTableDataId = @MOQTypeSelectionTableDataId) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			 
			UPDATE [dbo].[MOQTypeSelectionTableData]
				SET 
					[MOQTypeSelectionId] = @MOQTypeSelectionId,
					[UpdateDT] = @UpdateDT,
					[Order] = @Order,
					[TableName] = @TableName,
					[RepositoryName] = @RepositoryName,
					[QueryType] = @QueryType,
					[DateOfReport] = @DateOfReport,
					[HistoricalProgramName] = @HistoricalProgramName,
					[ContractNumber] = @ContractNumber,
					[WbsElement] = @WbsElement,
					[PeriodOfPerformanceStartDate] = @PeriodOfPerformanceStartDate,
					[PeriodOfPerformanceEndDate] = @PeriodOfPerformanceEndDate,
					[TotalWbsHours] = @TotalWbsHours,
					[AdditionalQueryFilters] = @AdditionalQueryFilters,
					[TotalRelevantHoursAfterQueryFilters] = @TotalRelevantHoursAfterQueryFilters
				WHERE
					[MOQTypeSelectionTableDataId] = @MOQTypeSelectionTableDataId

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The MOQ Type Selection Table Data with ID ' + CAST(@MOQTypeSelectionTableDataId AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @MOQTypeSelectionTableDataId AS MOQTypeSelectionTableDataId
GO