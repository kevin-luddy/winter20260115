IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRteTemplateQuestion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRteTemplateQuestion];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRteTemplateQuestion]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRteTemplateQuestion]
	**		Desc:	Delete an RTE Question
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 12/5/2019
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[RteTemplateAnswer]
		WHERE QuestionID = @Id

	DELETE FROM dbo.[RteTemplateQuestion]
		WHERE QuestionID = @Id

GO