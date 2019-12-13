IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOETaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOETaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOETaskElement]
(
@BOETaskElementID int,
@TaskID varchar(3),
@TaskTitle varchar(100),
@TaskDescription  varchar(max),
@TaskStartDate date,
@TaskEndDate date,
@MOQHoursEquation varchar(500),
@MOQCostEquation varchar(250),
@MOQText varchar(max),
@MOQTypeID int,
@BOEID int,
@LaborTypeWarningFlag bit,
@IMS_ID varchar(20),
@TaskElementTypeID int,
@UpdateDT datetime2,
@SortOrderID int
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElement]
**		Desc: Insert/Update data into BOE Task Elements Detailed Section of BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		9/17/10		dcanuso				Additional database changes caused 
**		3/23/11		dcanuso				Developer request to add LaborTypeWarningFlag
**		3/28/11		dcanuso				New Parameter added for IMS Importing
**		3/29/11		dcanuso				Removing @DataSource - now in BOE
**		4/18/11		dcanuso				Adding MetricID to support Historical Metrics
**										MOQ Text changed from max to 8000
**		5/3/11		dcanuso				Moving Metric to its own SP 
**		5/25/11		dcanuso				Changes:
**										@MOQHoursEquation varchar(250),
**										@MOQCostEquation varchar(250),
**										@CostElementID int
**		6/13/11		dcanuso				Cost Element removed and TaskElementTypeID added
**		3/5/13		dcanuso				WI 16730 moq text to varchar (max)
**		10/6/14		dcanuso				Sort Story
**      12/16/15    perry				Summary Boe changes
**      2/15/16		twilson3			Summary Boe changes
**		1/26/17		pattoncr			Updating MOQHoursEquation to varchar(500).
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedBOETaskElement AS Table (BOETaskElementID int)


IF @BOETaskElementID  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[BOETaskElement]
           ([TaskID]
           ,[TaskTitle]
           ,[TaskDescription]
           ,[TaskStartDate]
           ,[TaskEndDate]
           ,[MOQHoursEquation]
           ,[MOQCostEquation]
           ,[MOQText]
           ,[MOQTypeID]
           ,[BOEID]
           ,[LaborTypeWarningFlag]
           ,[IMS_ID]
           ,[TaskElementTypeID]
           ,[UpdateDT]
		   ,[SortOrderID]
		   )
     OUTPUT inserted.BOETaskElementID INTO @InsertedBOETaskElement
     VALUES
           (
            @TaskID
           ,@TaskTitle
           ,@TaskDescription
           ,@TaskStartDate
           ,@TaskEndDate
           ,@MOQHoursEquation
           ,@MOQCostEquation
           ,@MOQText
           ,@MOQTypeID
           ,@BOEID
           ,@LaborTypeWarningFlag
           ,@IMS_ID
           ,@TaskElementTypeID
           ,@UpdateDT
		   ,@SortOrderID
            ) 
            
	SELECT @BOETaskElementID = BOETaskElementID FROM @InsertedBOETaskElement
	


END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[BOETaskElement] WHERE BOETaskElementID = @BOETaskElementID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			 
			UPDATE [dbo].[BOETaskElement]
				SET 
					[TaskID] = @TaskID,
					[TaskTitle] = @TaskTitle,
					[TaskDescription] = @TaskDescription,
					[TaskStartDate] = @TaskStartDate,
					[TaskEndDate] = @TaskEndDate,
					[MOQHoursEquation] = @MOQHoursEquation,
					[MOQCostEquation] = @MOQCostEquation,
					[MOQText] = @MOQText,
					[MOQTypeID] = @MOQTypeID,
					[BOEID] = @BOEID,
					[LaborTypeWarningFlag] = @LaborTypeWarningFlag,
					[IMS_ID] = @IMS_ID,
					[TaskElementTypeID] = @TaskElementTypeID,
					[UpdateDT] = @UpdateDT,
					[SortOrderID] = @SortOrderID
			WHERE
				BOETaskElementID = @BOETaskElementID
				

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Task Element with ID ' + CAST(@BOETaskElementID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @BOETaskElementID AS BOETaskElementID
GO