-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateCobraDetailviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateCobraDetailviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_CobraDetail' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_CobraDetail];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_CobraDetail] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[CobraRateSet] [varchar](50) NULL,
	[CobraCode1ID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateCobraDetailviaTableParameter]
(
@CobraDetailParam [dbo].[TT_CobraDetail] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateCobraDetailviaTableParameter]
**		Desc: Insert/Update COBRA data into RateCode Table
**
**		Auth: G. Brunworth
**		Date: 2/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/27/2018	brunworg			Created.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2 = GETDATE()

SET @UpdateDate = GetDate()
			
			 
UPDATE [dbo].[RateCode]
	SET 
		[UpdateDate] = TT.[UpdateDate],
		[CobraRateSet] = TT.[CobraRateSet],
		[CobraCode1ID] = TT.[CobraCode1ID]
FROM [dbo].[RateCode] RC
	INNER JOIN @CobraDetailParam TT ON 
		RC.ID = TT.ID 
				
IF @@ERROR = 0
	SELECT 
		RC.ID AS ID, 
		RC.UpdateDate AS UpdateDate 
	FROM @CobraDetailParam TT
		INNER JOIN dbo.RateCode RC ON TT.ID = RC.ID
	ORDER BY OrderID
GO
