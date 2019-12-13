IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCTaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[upsertODCTaskElement]
(
@ODCTaskElementID int,
@ODCTaskID varchar(3),
@ODCTaskTitle varchar(100),
@ODCTaskDescription  varchar(max),
@ODCMOQText varchar(max),
@BOEID int,
@UpdateDT datetime2,
@TaskStartDate date,
@TaskEndDate date,
@SortOrderID int
)
AS
/******************************************************************************
**		 
**		Name: [upsertODCTaskElement]
**		Desc: Insert/Update data into ODC Task Element Detailed Section of BOE/ODC
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/9/11		DCANUSO				WI 4503 dbo.ODCTaskElement.ODCTaskID added
**		8/6/12		dcanuso				WI 10015 Add Start and End Date
**		3/5/13		dcanuso				WI 16730 moq text to varchar (max)
**		10/6/14		dcanuso				Sort Order Story
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedODCTaskElement AS Table (ODCTaskElementID int)


IF @ODCTaskElementID  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[ODCTaskElement]
           ([ODCTaskTitle]
           ,[ODCTaskDescription]
           ,[ODCMOQText]
           ,[BOEID]
           ,[ODCTaskID]
           ,[UpdateDT]
           ,[TaskStartDate]
           ,[TaskEndDate]
		   ,[SortOrderID]
		   )
     OUTPUT inserted.ODCTaskElementID INTO @InsertedODCTaskElement
     VALUES
           (
            @ODCTaskTitle
           ,@ODCTaskDescription
           ,@ODCMOQText
           ,@BOEID
           ,@ODCTaskID
           ,@UpdateDT
           ,@TaskStartDate
           ,@TaskEndDate
		   ,@SortOrderID
            ) 
            
	SELECT @ODCTaskElementID = ODCTaskElementID FROM @InsertedODCTaskElement
	


END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[ODCTaskElement] WHERE ODCTaskElementID = @ODCTaskElementID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			 
			UPDATE [dbo].[ODCTaskElement]
				SET 
					[ODCTaskTitle] = @ODCTaskTitle,
					[ODCTaskDescription] = @ODCTaskDescription,
					[ODCMOQText] = @ODCMOQText,
					[BOEID] = @BOEID,
					[ODCTaskID] = @ODCTaskID,
					[UpdateDT] = @UpdateDT,
					[TaskStartDate] = @TaskStartDate,
					[TaskEndDate] = @TaskEndDate,
					[SortOrderID] = @SortOrderID
			WHERE
				ODCTaskElementID = @ODCTaskElementID
				

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The ODC Task Element with Title ' + @ODCTaskTitle +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @ODCTaskElementID AS ODCTaskElementID
GO