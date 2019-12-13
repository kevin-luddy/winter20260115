IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProgramArea]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProgramArea]
	**		Desc:	Delete Program Area LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[ProgramAreaLU]
		WHERE [ProgramAreaID] = @Id

GO