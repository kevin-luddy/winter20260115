IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTMResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTMResourceRate];

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertTMResourceRate]
(
@TMResourceRateID [int],
@WorkspaceID int,
@TMResourceID INT,
@TMResourceRateStartDate [date],
@TMResourceRateEndDate [date],
@TMResourceRate [decimal](7, 2),
@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: [upsertTMResourceRate]
**		Desc: Insert/Update T&M Resource Rate
**
**		Auth: Greg Brunworth
**		Date: 5/03/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

IF @TMResourceRateID < 0 /*Insert Record*/
	BEGIN
		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[TMResourceRate]
			   ([UpdateDT]
			   ,[WorkspaceID]
			   ,[TMResourceID]
			   ,[TMResourceRateStartDate]
			   ,[TMResourceRateEndDate]
			   ,[TMResourceRate]
			   )
		 OUTPUT inserted.TMResourceRateID INTO @Inserted
		 VALUES
			   (@UpdateDT
			   ,@WorkspaceID
			   ,@TMResourceID
			   ,@TMResourceRateStartDate
			   ,@TMResourceRateEndDate
			   ,@TMResourceRate
			   )

		SELECT @TMResourceRateID = ID FROM @Inserted
	END
ELSE /*Update Record*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TMResourceRate] WHERE TMResourceRateID = @TMResourceRateID) = @UpdateDT
			BEGIN	
				SET @UpdateDT = GETDATE()
				
				UPDATE [dbo].[TMResourceRate]
			   SET [UpdateDT] = @UpdateDT
				  ,[WorkspaceID] = @WorkspaceID
				  ,[TMResourceID] = @TMResourceID
				  ,[TMResourceRateStartDate] = @TMResourceRateStartDate
				  ,[TMResourceRateEndDate] = @TMResourceRateEndDate
				  ,[TMResourceRate] = @TMResourceRate
				WHERE [TMResourceRateID] = @TMResourceRateID
			END
		ELSE
		BEGIN
			SET @ErrorMessage =   'The T&M Resource Rate with ID ' + CAST(@TMResourceRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
	END
IF @@ERROR = 0
	SELECT	@TMResourceRateID AS TMResourceRateID

GO