IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSkillMix]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSkillMix];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSkillMix]
(
	@MOQTypeSelectionID int
)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteSkillMix]
	**		Desc: Delete all parts of SkillMix Table
	**			
	**		
	**
	**		Auth: Hyun Dong Lee
	**		Date: 6/2024
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**      6/10/24		e374897 			PROPH-2017 Initial creation of delete
	*****************************************************************************/
	BEGIN
		DELETE FROM dbo.[SkillMix]
		WHERE [MOQTypeSelectionID] = @MOQTypeSelectionID

		SELECT @@ROWCOUNT AS RowsAffected;
	END
GO
