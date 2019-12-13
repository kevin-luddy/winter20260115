IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceLastAccessed]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceLastAccessed];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceLastAccessed]
(
@WorkspaceID int,
@ETIUserID int,
@LastAccessed datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertWorkspaceLastAccessed]
**		Desc: Insert/Update data for Workspace Last Accessed
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 6/28/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/10/2018	twilson3			BOEJ-3552 Only update on hourly increments
**		7/30/2018	twilson3			BOEJ-3710 Check for NULL before Date comparison
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2
DECLARE @CurrentTime datetime2
SET @UpdateDT = GetDate()
DECLARE @WorkspaceUserXREFID int
SELECT TOP 1 @WorkspaceUserXREFID = WorkspaceUserID, @CurrentTime = [LastAccessed] FROM [dbo].[WorkspaceUserXREF] WHERE [WorkspaceID] = @WorkspaceID AND [ETIUserId] = @ETIUserID

IF @WorkspaceUserXREFID IS NULL /*Insert Record*/
	BEGIN
		DECLARE @InsertedXREF AS Table (WorkspaceUserID int)

		INSERT INTO [dbo].[WorkspaceUserXREF]
				   ([WorkspaceID]
				   ,[ETIUserId]
				   ,[LastAccessed]
				   ,[UpdateDT])          
			 OUTPUT inserted.WorkspaceUserID INTO @InsertedXREF
			 VALUES
				   (@WorkspaceID
				   ,@ETIUserID
				   ,@LastAccessed
				   ,@UpdateDT)

			SELECT @WorkspaceUserXREFID = WorkspaceUserID FROM @InsertedXREF
	
	END
ELSE
	BEGIN
		IF (@CurrentTime IS NULL OR @CurrentTime < @LastAccessed)
			BEGIN
				UPDATE [dbo].[WorkspaceUserXREF]
				   SET [LastAccessed] = @LastAccessed
					  ,[UpdateDT] = @UpdateDT
				WHERE 
					WorkspaceUserID = @WorkspaceUserXREFID
			END
	END

IF @@ERROR = 0
	SELECT @WorkspaceUserXREFID AS WorkspaceUserXREFID
GO