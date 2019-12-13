IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteLOB]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteLOB];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteLOB]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteLOB]
	**		Desc:	Delete LOB LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[LineOfBusiness]
		WHERE [LineOfBusinessID] = @Id

GO