IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelTripTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelTripTaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertTravelTripTaskElement]
(
@TravelTripTaskElementID int,
@TravelTaskID varchar(3),
@TravelTaskTitle varchar(100),
@TravelTaskDescription  varchar(max),
@BOEID int,
@UpdateDT datetime2,
@TaskStartDate date,
@TaskEndDate date,
@SortOrderID int
)
AS
/******************************************************************************
**		 
**		Name: [upsertTravelTripTaskElement]
**		Desc: Insert/Update data into Travel Task Element Detailed Section of BOE/Travel
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/7/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/6/12		dcanuso				WI 10015 Add Start and End Date
**		10/6/14		dcanuso				Sort Order Story
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)


IF @TravelTripTaskElementID  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[TravelTripTaskElement]
           ([TravelTaskID]
           ,[TravelTaskTitle]
           ,[TravelTaskDescription]
           ,[BOEID]
           ,[UpdateDT]
           ,[TaskStartDate]
           ,[TaskEndDate]
		   ,[SortOrderID]
		   )
     OUTPUT inserted.TravelTripTaskElementID INTO @Inserted
     VALUES
           (
            @TravelTaskID
           ,@TravelTaskTitle
           ,@TravelTaskDescription
           ,@BOEID
           ,@UpdateDT
           ,@TaskStartDate
		   ,@TaskEndDate
		   ,@SortOrderID
            ) 
            
	SELECT @TravelTripTaskElementID = ID FROM @Inserted
	


END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskElement] WHERE TravelTripTaskElementID = @TravelTripTaskElementID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			 
			UPDATE [dbo].[TravelTripTaskElement]
				SET [TravelTaskID] = @TravelTaskID,
					[TravelTaskTitle] = @TravelTaskTitle,
					[TravelTaskDescription] = @TravelTaskDescription,
					[BOEID] = @BOEID,
					[UpdateDT] = @UpdateDT,
					[TaskStartDate] = @TaskStartDate,
					[TaskEndDate] = @TaskEndDate,
					[SortOrderID] = @SortOrderID
			WHERE
				TravelTripTaskElementID = @TravelTripTaskElementID
				

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Travel Trip Task Element with Title ' + @TravelTaskTitle +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @TravelTripTaskElementID AS TravelTaskElementID
GO