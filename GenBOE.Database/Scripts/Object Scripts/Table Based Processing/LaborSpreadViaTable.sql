-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOELaborSpreadviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOELaborSpreadviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOELaborSpread' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOELaborSpread];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOELaborSpread] AS TABLE(
	[BOELaborSpreadID] [int] NOT NULL,
	[BOELaborTypeID] [int] NOT NULL,
	[LaborSpreadDate] [date] NOT NULL,
	[LaborSpreadValue] [decimal](18, 6) NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertBOELaborSpreadviaTableParameter]
(
@BOELaborSpread [dbo].[TT_BOELaborSpread] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOELaborSpread]
**		Desc: Insert/Update data into LM Labor Spread Section of BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		10/13/10	dcanuso				Developer Request - Rempove optimistic locking 
**										from BOE Labor Spread table 
**		5/26/11		dcanuso				Spread Value to bigint		
*******************************************************************************/
SET NOCOUNT ON 

INSERT INTO [dbo].[BOELaborSpread]
           (
           [BOELaborTypeID]
           ,[LaborSpreadDate]
           ,[LaborSpreadValue]
           )
SELECT 	    T.BOELaborTypeID,
			T.LaborSpreadDate,
			T.LaborSpreadValue
FROM  @BOELaborSpread T



IF @@ERROR = 0
SELECT 	    X.BOELaborSpreadID
FROM  @BOELaborSpread T
	INNER JOIN [dbo].[BOELaborSpread] X ON 
		    T.BOELaborTypeID = X.BOELaborTypeID AND
			T.LaborSpreadDate = X.LaborSpreadDate AND
			T.LaborSpreadValue = X.LaborSpreadValue
ORDER BY T.OrderID
GO