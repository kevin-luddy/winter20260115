IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteOutputFormatTemplateForAllWorkspaces]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteOutputFormatTemplateForAllWorkspaces];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteOutputFormatTemplateForAllWorkspaces]
(
@TemplateID int
)
AS
/******************************************************************************
**		 
**		Name: deleteOutputFormatTemplateForAllWorkspaces
**		Desc: Deletes the record in the OutputFormatTemplateWorkspaceXREF
**				table for all workspaces.		
**
**		Auth: RJ Anzalone
**		Date: 5/1/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/

SET NOCOUNT ON

DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF
WHERE 
TemplateID = @TemplateID

GO