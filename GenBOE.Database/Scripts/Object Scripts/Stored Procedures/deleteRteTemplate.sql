IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRteTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRteTemplate];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRteTemplate]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRteTemplate]
	**		Desc:	Delete a Template, Questions, and Answers 
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

	DELETE FROM [dbo].[RteTemplateAnswer]
	FROM [dbo].[RteTemplateAnswer] RTA 
		INNER JOIN [RteTemplateQuestion] RTQ ON RTQ.[QuestionID] = RTA.[QuestionID]
	WHERE RTQ.TemplateID = @Id

	DELETE FROM [dbo].[RteTemplateAssigned]
	WHERE TemplateID = @Id

	DELETE FROM dbo.[RteTemplateQuestion]
		WHERE TemplateID = @Id

	DELETE FROM dbo.[RteTemplate]
		WHERE TemplateID = @Id

GO