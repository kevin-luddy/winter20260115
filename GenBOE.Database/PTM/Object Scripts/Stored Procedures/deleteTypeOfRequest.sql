IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTypeOfRequest]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTypeOfRequest];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteTypeOfRequest]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteTypeOfRequest]
	**		Desc:	Delete Type Of Request LU Values
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.RequestTypeLU
		WHERE RequestTypeID = @Id

GO