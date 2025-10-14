IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSkillMixSummary]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSkillMixSummary];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSkillMixSummary]
(
	@BOETaskElementID int
)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteSkillMixSummary]
	**		Desc: Delete all parts of SkillMix summary Table
	**			
	**		
	**
	**		Auth: Oyetoro Oyeyemi
	**		Date: 9/2025
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**      9/24/25		e378233 			PROPH-3302 Skill Mix Summary DB Table
	*****************************************************************************/

	-- deleting a skill mix could delete 1-many rows in summary table

	BEGIN
		DELETE FROM dbo.[SkillMixSummary]
		WHERE [BOETaskElementID] = @BOETaskElementID

		SELECT @@ROWCOUNT AS RowsAffected;
	END
GO