IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getSystemPermissions]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getSystemPermissions];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getSystemPermissions]
(
@NTID varchar(10)
)
AS
/******************************************************************************
**		 
**		Name: getSystemPermissions
**		Desc: Returns System Permissions
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
		SR.RoleID AS SystemRoleID
	FROM  dbo.SystemUserRole  SR
		INNER JOIN dbo.ETIuser E  ON E.ETIUserID = SR.ETIUserID
	WHERE 
		E.NTID = @NTID
GO