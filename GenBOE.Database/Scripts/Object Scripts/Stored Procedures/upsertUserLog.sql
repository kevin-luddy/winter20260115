IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertUserLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertUserLog];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertUserLog]
(
@NTID [varchar](1000)
)
AS 
/******************************************************************************
**		 
**		Name:	[upsertUserLog]
**		Desc:	Insert/Update User Log Entry
**			
**		
**
**		Auth: Don Canuso
**		Date: 05/22/2014
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		5/22/14		dcanuso				Initial creation.
**		11/28/17	pattoncr			Remove domain.
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @Today datetime2(7) = GetDate()

IF EXISTS (SELECT 1 FROM dbo.UserLog WHERE NTID = @NTID)
	UPDATE dbo.UserLog
	SET [LogInUpdateDT] = @Today
	WHERE 
		NTID = @NTID
ELSE
	INSERT INTO [dbo].[UserLog]
			   ([LogInUpdateDT]
			   ,[NTID])
		 VALUES
			   (@Today, @NTID)
GO