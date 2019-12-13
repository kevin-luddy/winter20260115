IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSumOfBOE_WorkspaceVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSumOfBOE_WorkspaceVariable];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertSumOfBOE_WorkspaceVariable]
(
@WorkspaceVariableID int,
@CLINID int,
@WBSID int,
@BOEID int
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Inserts into SumOfBOE_WorkspaceVariableXREF rows for Workspace VariableID
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		9/4/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values
*******************************************************************************/

SET NOCOUNT ON 
DECLARE	@ErrorMessage varchar (500)

DECLARE @Inserted AS Table (ID int)



IF EXISTS	(SELECT 1 FROM [dbo].[SumOfBOE_WorkspaceVariableXREF]
				WHERE	IsNull(BOEID, -9999) = IsNull(@BOEID, -9999) AND
						IsNull(CLINID, -9999) = IsNUll(@CLINID, -9999) AND
						IsNull(WBSID, -9999) = IsNull(@WBSID, -9999) AND
						WorkspaceVariableID = @WorkspaceVariableID
			)
	BEGIN
		SET @ErrorMessage =   'There is already a matching Sum Of BOE Workspace Variable.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END
ELSE
	BEGIN
		INSERT INTO [dbo].[SumOfBOE_WorkspaceVariableXREF]
				   ([WorkspaceVariableID]
				   ,[CLINID]
				   ,[WBSID]
				   ,[BOEID])
		OUTPUT inserted.WVSumID INTO @Inserted
		VALUES
				   (@WorkspaceVariableID
				   ,@CLINID
				   ,@WBSID
				   ,@BOEID)

	END
	
IF @@ERROR = 0
SELECT ID AS WVSumID
	FROM @Inserted
GO