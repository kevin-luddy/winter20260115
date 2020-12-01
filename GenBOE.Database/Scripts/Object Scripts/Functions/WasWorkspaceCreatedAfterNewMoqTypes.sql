IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WasWorkspaceCreatedAfterNewMoqTypes]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [dbo].[WasWorkspaceCreatedAfterNewMoqTypes];
GO

CREATE FUNCTION dbo.WasWorkspaceCreatedAfterNewMoqTypes(@wsCreationDate DATE) RETURNS BIT AS
/******************************************************************************
**
**	Name: WasWorkspaceCreatedAfterNewMoqTypes
**	Desc: Decides if the Workspace was created once we started using new MOQ Types. 
**			This is the only place we'll store the start date.
**
*******************************************************************************
**	Change History
*******************************************************************************
**	Date:		Author:		Description:
**	--------	--------	---------------------------------------------------
**	2020-11-24	Dusan		Initial creation.
**
*******************************************************************************/

BEGIN
	DECLARE @moqStartDate DATE = '2020-10-15'; -- this may need to move / be updated?

	DECLARE @result BIT = CASE WHEN @wsCreationDate >= @moqStartDate THEN 1 ELSE 0 END

	RETURN @result;
END
GO
