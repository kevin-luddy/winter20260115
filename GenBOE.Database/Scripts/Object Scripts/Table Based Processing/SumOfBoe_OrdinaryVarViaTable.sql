-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSumOfBOEByOrdinaryVariableIDviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSumOfBOEByOrdinaryVariableIDviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSumOfBOE_OrdinaryVariableviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSumOfBOE_OrdinaryVariableviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_SumOfBOE_OrdinaryVariableXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_SumOfBOE_OrdinaryVariableXREF];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_SumOfBOE_OrdinaryVariableXREF] AS TABLE(
	[OVSumID] [bigint] NOT NULL,
	[OrdinaryVariableID] [int] NOT NULL,
	[CLINID] [int] NULL,
	[WBSID] [int] NULL,
	[BOEID] [int] NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSumOfBOEByOrdinaryVariableIDviaTableParameter]
(
@SumOfBOE_OrdinaryVariableXREF [dbo].[TT_SumOfBOE_OrdinaryVariableXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Deletes the SumOfBOE_OrdinaryVariableXREF rows for OrdinaryVariableID
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/

SET NOCOUNT ON

DELETE FROM [dbo].[SumOfBOE_OrdinaryVariableXREF]
FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
	INNER JOIN @SumOfBOE_OrdinaryVariableXREF TT ON 
		X.OVSumID = TT.OVSumID
GO
CREATE PROCEDURE [dbo].[insertSumOfBOE_OrdinaryVariableviaTableParameter]
(
@SumOfBOE_OrdinaryVariableXREF [dbo].[TT_SumOfBOE_OrdinaryVariableXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Inserts into SumOfBOE_OrdinaryVariableXREF rows for OrdinaryVariableID
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		9/4/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values
**		10/3/14		dcanuso				Developer requested that the ID that is 
**										returned be an int rather than the actual
**										data type - bigint - for now this is not
**										an issue.
*******************************************************************************/

SET NOCOUNT ON 

INSERT INTO [dbo].[SumOfBOE_OrdinaryVariableXREF]
			([OrdinaryVariableID]
			,[CLINID]
			,[WBSID]
			,[BOEID])
SELECT 
	T.OrdinaryVariableID,
	T.CLINID,
	T.WBSID,
	T.BOEID
FROM @SumOfBOE_OrdinaryVariableXREF T
	LEFT OUTER JOIN [dbo].[SumOfBOE_OrdinaryVariableXREF] X ON
			IsNull(T.BOEID, -9999) = IsNull(X.BOEID, -9999) AND
			IsNull(T.CLINID, -9999) = IsNull(X.CLINID, -9999) AND
			IsNull(T.WBSID, -9999) = IsNull(X.WBSID, -9999) AND
			T.OrdinaryVariableID = X.OrdinaryVariableID
WHERE X.OVSumID IS NULL
			




SELECT CAST (X.OVSumID AS INT) AS OVSumID
FROM @SumOfBOE_OrdinaryVariableXREF T
	INNER JOIN [dbo].[SumOfBOE_OrdinaryVariableXREF] X ON
			IsNull(T.BOEID, -9999) = IsNull(X.BOEID, -9999) AND
			IsNull(T.CLINID, -9999) = IsNull(X.CLINID, -9999) AND
			IsNull(T.WBSID, -9999) = IsNull(X.WBSID, -9999) AND
			T.OrdinaryVariableID = X.OrdinaryVariableID
ORDER BY T.OrderID
GO