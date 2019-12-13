IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertUserLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertUserLog];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertUserLog]
(
@NTID [varchar](1000),
@DisplayName [varchar](1000),
@Application [varchar](100)
)
AS 
	/******************************************************************************
	**		 
	**		Name:	[upsertUserLog]
	**		Desc:	Insert/Update User Log Entry
	**			
	**		
	**
	**		Auth: RJ Anzalone
	**		Date: 11/22/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**		11/27/17	ranzalon			Remove domain, add application field
	*******************************************************************************/

	SET NOCOUNT ON 
	DECLARE @Today datetime2(7) = GetDate()

	IF EXISTS (SELECT 1 FROM dbo.UserLog WHERE NTID = @NTID AND DisplayName = @DisplayName AND [Application] = @Application)
		UPDATE dbo.UserLog
		SET [LogInUpdateDT] = @Today
		WHERE 
			NTID = @NTID AND
			DisplayName = @DisplayName AND
			[Application] = @Application 		
	ELSE
		INSERT INTO [dbo].[UserLog]
				   ([LogInUpdateDT]
				   ,[NTID]
				   ,[DisplayName]
				   ,[Application])
			 VALUES
				   (@Today, @NTID, @DisplayName, @Application)
GO