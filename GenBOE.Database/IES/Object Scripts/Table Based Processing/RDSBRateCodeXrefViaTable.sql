-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRDSBRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRDSBRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRDSBRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRDSBRateCodeXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RDSBRateCodeXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RDSBRateCodeXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RDSBRateCodeXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRDSBRateCodeXrefviaTableParameter]
(
@RDSBRateCodeXref [dbo].[TT_RDSBRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRDSBRateCodeXrefviaTableParameter]
**		Desc: Insert data in RDSBRateCodeXref Table.
**
**		Auth: G. Brunworth
**		Date: 2/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/01/2018	brunworg			Created.
**		2/15/2018	brunworg			Added RDSBDocumentInformationID column.
**		2/29/2018	ranzalon			Added removal of previous xrefs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

-- Remove all previous xrefs for the RDSBDocumentInformationID
DELETE FROM [dbo].[RDSBRateCodeXref]
WHERE RDSBDocumentInformationID IN (SELECT DISTINCT RDSBDocumentInformationID FROM @RDSBRateCodeXref)

/* Declare a @TT_RDSBXref table to store the incoming table. */

DECLARE @TT_RDSBXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[RateCodeID] [int] NOT NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RDSBXref
SELECT * FROM @RDSBRateCodeXref

DECLARE @ID [int],
		@RDSBDocumentInformationID [int],
		@RateCodeID [int],
		@OrderID [int]

DECLARE @InsertedRDSBXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RDSBXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RDSBDocumentInformationID = RDSBDocumentInformationID,
		@RateCodeID = RateCodeID,		
		@OrderID = OrderID
	FROM @TT_RDSBXref
	WHERE ID < 0

	INSERT INTO [dbo].[RDSBRateCodeXref]
           ([UpdateDate]
		   ,[RDSBDocumentInformationID]
		   ,[RateCodeID]
		   )
     OUTPUT inserted.ID INTO @InsertedRDSBXref
     VALUES
           (@UpdateDate
		   ,@RDSBDocumentInformationID
		   ,@RateCodeID
           ) 
            
	SELECT @ID = ID FROM @InsertedRDSBXref
	
	UPDATE @TT_RDSBXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RDSBXref TT
		ORDER BY OrderID
GO
