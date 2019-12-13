IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteUserLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteUserLog];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteUserLog]
AS
/******************************************************************************
**		 
**		Name: [deleteUserLog]
**		Desc: Nighlt deletion of User Log
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/22/2014
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		12/31/14	dcanuso				User Log Access Reprt requested
**										Need to save off records before deleting
**		11/28/17	pattoncr			Removed domain.
*******************************************************************************/
SET NOCOUNT ON ;


MERGE [dbo].[UserAccessReport]  T
USING 
(
SELECT [LogInUpdateDT]
      ,[NTID]
  FROM [dbo].[UserLog] 
)  S 
ON	T.[NTID] = S.[NTID]
WHEN NOT MATCHED BY TARGET THEN
INSERT 
	(
		[LogInUpdateDT]
		,[NTID]
	)
     VALUES
	(
		S.[LogInUpdateDT],
		S.[NTID]
	)
WHEN MATCHED
THEN UPDATE 
SET    
	[LogInUpdateDT] = S.[LogInUpdateDT]
;

DELETE FROM [dbo].[UserLog]
WHERE 
[LogInUpdateDT] < DateAdd (HH, -12, GetDate())

GO