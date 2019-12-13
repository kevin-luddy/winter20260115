IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOrdinaryVariableResourceType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOrdinaryVariableResourceType];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertOrdinaryVariableResourceType]
(
@OrdinaryVariableID int,
@SumVariableResourceTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [insertOrdinaryVariableResourceType]
**		Desc: Insert Ordinary Variable and Resource Type
**				No updates
**				Only inserts/deletes
**			
**		
**
**		Auth: Don Canuso
**		Date: 09/13/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
				
IF NOT EXISTS 
	(SELECT 1 FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF 
		WHERE	OrdinaryVariableID = @OrdinaryVariableID AND 
				SumVariableResourceTypeID = @SumVariableResourceTypeID
	)
	INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
				   ([OrdinaryVariableID]
				   ,[SumVariableResourceTypeID])
	SELECT @OrdinaryVariableID, @SumVariableResourceTypeID

GO