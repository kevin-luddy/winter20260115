IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteOutputFormatTemplateWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteOutputFormatTemplateWorkspace];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteOutputFormatTemplateWorkspace]
(
@WorkspaceID int,
@TemplateID int
)
AS
/******************************************************************************
**		 
**		Name: deleteOutputFormatTemplateWorkspace
**		Desc: Deletes the record in the OutputFormatTemplateWorkspaceXREF
**				table.  Note: Wireframe state closed Workspaces are not shown
**				So, global delete is not correct.
**		
**
**		Auth: Don Canuso
**		Date: 3/30/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/

SET NOCOUNT ON

DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF
WHERE 
TemplateID = @TemplateID AND
WorkspaceID = @WorkspaceID 

GO