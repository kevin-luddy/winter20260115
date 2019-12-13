IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEPermissions]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEPermissions];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getBOEPermissions]
(
@NTID varchar(10)
)
AS
/******************************************************************************
**		 
**		Name: getBOEPermissions
**		Desc: Returns BOE  Permissions
**			
**		
**
**		Auth: Don Canuso
**		Date: 12/1/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		5/9/11		dcanuso				NTID and Domain needed to be unique		
**		11/28/17	pattoncr			Remove domain.
*******************************************************************************/
SET NOCOUNT ON 

	SELECT 
		B.BOEID AS BOEID,
		BR.RoleID AS BOERoleID,
		B.BOEStateID AS BOEStateID,
		B.WorkspaceID AS WorkspaceID
	FROM dbo.BOE B 
		INNER JOIN dbo.BOEUserRole BR ON B.BOEID = BR.BOEID
		INNER JOIN dbo.ETIuser E ON E.ETIUserID = BR.ETIUserID 		
	WHERE 
		E.NTID = @NTID

GO