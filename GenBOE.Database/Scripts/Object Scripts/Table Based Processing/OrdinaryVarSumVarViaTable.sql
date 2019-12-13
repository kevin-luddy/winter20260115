-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOrdinaryVariableResourceTypeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOrdinaryVariableResourceTypeviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteOrdinaryVariableResourceTypeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteOrdinaryVariableResourceTypeviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_OrdinaryVariableSumVariableResourceTypeXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_OrdinaryVariableSumVariableResourceTypeXREF];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_OrdinaryVariableSumVariableResourceTypeXREF] AS TABLE(
	[OVSVRTID] [int] NOT NULL,
	[OrdinaryVariableID] [int] NOT NULL,
	[SumVariableResourceTypeID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertOrdinaryVariableResourceTypeviaTableParameter]
(
@OrdinaryVariableSumVariableResourceTypeXREF [dbo].[TT_OrdinaryVariableSumVariableResourceTypeXREF] READONLY
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
				
	INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
				   ([OrdinaryVariableID]
				   ,[SumVariableResourceTypeID])
	SELECT T.OrdinaryVariableID, T.SumVariableResourceTypeID
	FROM @OrdinaryVariableSumVariableResourceTypeXREF T
		LEFT OUTER JOIN [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] X ON
			T.OrdinaryVariableID = X.OrdinaryVariableID AND
			T.SumVariableResourceTypeID = X.SumVariableResourceTypeID
	WHERE 
		X.OVSVRTID IS NULL


/* Original SP did not return values*/
SELECT X.OVSVRTID
	FROM @OrdinaryVariableSumVariableResourceTypeXREF T
		INNER JOIN [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] X ON
			T.OrdinaryVariableID = X.OrdinaryVariableID AND
			T.SumVariableResourceTypeID = X.SumVariableResourceTypeID
ORDER BY T.OrderID
GO
CREATE PROCEDURE [dbo].[deleteOrdinaryVariableResourceTypeviaTableParameter]
(
@OrdinaryVariableSumVariableResourceTypeXREF [dbo].[TT_OrdinaryVariableSumVariableResourceTypeXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteOrdinaryVariableResourceType]
**		Desc: deletes Ordinary Variable and Resource Type
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
				
DELETE FROM [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
WHERE OrdinaryVariableID IN
	(
		SELECT DISTINCT OrdinaryVariableID
		FROM  @OrdinaryVariableSumVariableResourceTypeXREF
	)
GO