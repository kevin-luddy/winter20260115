IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCommonDisclosureSkillMix]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCommonDisclosureSkillMix];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteCommonDisclosureSkillMix]
(
	@BOETaskElementID int
)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteCommonDisclosureSkillMix]
	**		Desc: Delete all parts of CommonDisclosureSkillMix Table
	**			
	**		
	**
	**		Auth: Breanne Nowicki
	**		Date: 6/2024
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**      6/16/24		e302876 			PROPH-2018 Common Disclosure Skill Mix DB Table
	**		10/02/24	e405721				PROPH-2394 Updates for Skill Mix V2
	*****************************************************************************/

	-- deleting a skill mix could delete 1-many rows in common disclosure table

	BEGIN
		DELETE FROM dbo.[CommonDisclosureSkillMix]
		WHERE [BOETaskElementID] = @BOETaskElementID

		SELECT @@ROWCOUNT AS RowsAffected;
	END
GO
