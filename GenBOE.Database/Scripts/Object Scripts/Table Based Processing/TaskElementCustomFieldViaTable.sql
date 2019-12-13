-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementCustomFieldValueviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementCustomFieldValueviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOETaskElementCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOETaskElementCustomFieldValueviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOETaskElementCustomFieldValueXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOETaskElementCustomFieldValueXREF];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOETaskElementCustomFieldValueXREF] AS TABLE(
	[BTECFVID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[CustomFieldID] [int] NULL,
	[CustomFieldValueID] [int] NOT NULL,
	[CustomFieldValueDescription] [varchar](250) NULL,
	[IsOpenEnded] [bit] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertBOETaskElementCustomFieldValueviaTableParameter]
(
@BOETaskElementCustomFieldValueXREF [dbo].[TT_BOETaskElementCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertBOETaskElementCustomFieldValue
**		Desc: Insert/Update Custom Field Value for the BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2(7) = GETDATE()

DECLARE @InsertedValues AS TABLE (CustomFieldValueID int, BTECFVID int)

--Insert Open Ended CFs into CustomFieldValue table and get value IDs
--Only do this for values not already in the db, otherwise this will cause duplicates during copy workspace
MERGE INTO [dbo].[CustomFieldValue] AS cfv
USING @BOETaskElementCustomFieldValueXREF AS xref
ON 1=0
WHEN NOT MATCHED AND xref.IsOpenEnded = 1 AND xref.CustomFieldValueID < 0 THEN 
INSERT 
	([CustomFieldValueName]
	,[CustomFieldValueDescription]
	,[CustomFieldID]
	,[CustomFieldValueInUseFlag]
	,[UpdateDT])
VALUES
	(CONVERT(varchar(10), xref.CustomFieldID) + '-' + CONVERT(varchar(10), xref.BOETaskElementID), --Name only used when performing copy, so it needs to be unique for mapping
	xref.CustomFieldValueDescription,
	xref.CustomFieldID,
	1,
	@UpdateDT)
OUTPUT inserted.CustomFieldValueID, xref.BTECFVID 
INTO @InsertedValues (CustomFieldValueID, BTECFVID);

--Update Open Ended Fields that were already in the db to in use (for copy workspace)
--and then add them to the @InsertedValues table
UPDATE [dbo].[CustomFieldValue]
SET [CustomFieldValueInUseFlag] = 1
WHERE [CustomFieldValueID] in
	(
		SELECT CustomFieldValueID
		FROM @BOETaskElementCustomFieldValueXREF
		WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0
	)

INSERT INTO @InsertedValues
SELECT CustomFieldValueID, BTECFVID
FROM @BOETaskElementCustomFieldValueXREF
WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0

--Insert Open Ended CFs Xrefs
INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
           ([BOETaskElementID]
           ,[CustomFieldValueID]
           ,[UpdateDT])
SELECT
	[BOETaskElementID]
	,iv.CustomFieldValueID
	,@UpdateDT
FROM @BOETaskElementCustomFieldValueXREF xref JOIN @InsertedValues iv 
ON xref.BTECFVID = iv.BTECFVID;

--Insert Standard CFs
INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
           ([BOETaskElementID]
           ,[CustomFieldValueID]
           ,[UpdateDT])
SELECT
           BOETaskElementID,
           CustomFieldValueID,
           @UpdateDT
FROM @BOETaskElementCustomFieldValueXREF
WHERE [IsOpenEnded] = 0            
      
UPDATE dbo.CustomFieldValue
	SET CustomFieldValueInUseFlag = 1
WHERE CustomFieldValueID IN
					
					(
						SELECT CustomFieldValueID
						FROM @BOETaskElementCustomFieldValueXREF
						WHERE IsOpenEnded = 0
					)

DECLARE @ReturnTable AS TABLE (BTECFVID int, UpdateDT Datetime2(7), OrderID int)

INSERT INTO @ReturnTable
SELECT X.BTECFVID AS BTECFVID,
		X.UpdateDT,
		T.OrderID
	FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN @BOETaskElementCustomFieldValueXREF T ON
				X.BOETaskElementID = T.BOETaskElementID AND
				X.CustomFieldValueID = T.CustomFieldValueID
	WHERE X.UpdateDT = @UpdateDT

INSERT INTO @ReturnTable
	SELECT X.BTECFVID AS BTECFVID,
		X.UpdateDT,
		T.OrderID
	FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN @BOETaskElementCustomFieldValueXREF T ON
				X.BOETaskElementID = T.BOETaskElementID
		INNER JOIN @InsertedValues I ON
				I.CustomFieldValueID = X.CustomFieldValueID
	WHERE X.UpdateDT = @UpdateDT

IF @@ERROR = 0
	SELECT 
		BTECFVID AS BTECFVID,
		UpdateDT
	FROM @ReturnTable
	ORDER BY OrderID
GO
CREATE PROCEDURE [dbo].[deleteBOETaskElementCustomFieldValueviaTableParameter]
(
@BOETaskElementCustomFieldValueXREF [dbo].[TT_BOETaskElementCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementCustomFieldValue]
**		Desc: Deletes data from the dbo.BOETaskElementCustomFieldValueXREF table which
**				holds the Custom Field Value for the BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/1/11		dcanuso				Updated order of SP to correct for logic 
**										and updated UPDATE to handle NULL
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		12/15/11	dcanuso				WI 6166
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[BOETaskElementCustomFieldValueXREF]
	FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN @BOETaskElementCustomFieldValueXREF T ON 
				X.BTECFVID = T.BTECFVID AND
				X.UpdateDT = T.UpdateDT

DELETE FROM [dbo].[CustomFieldValue]
	FROM [dbo].[CustomFieldValue] C
		INNER JOIN @BOETaskElementCustomFieldValueXREF T ON  
			C.CustomFieldValueID = T.CustomFieldValueID
	WHERE T.IsOpenEnded = 1 AND
		C.CustomFieldValueID NOT IN (SELECT CustomFieldValueID FROM [dbo].[BOETaskElementCustomFieldValueXREF])

		/*
		Only should be looking within the scope
		of the Custom Field, so if the scope is BOE
		then only look in BOECustomFieldValueXREF
		*/
		IF NOT EXISTS (SELECT [CustomFieldValueID] FROM	[BOETaskElementCustomFieldValueXREF]
						WHERE  CustomFieldValueID IN (SELECT CustomFieldValueID FROM @BOETaskElementCustomFieldValueXREF))
			BEGIN
				UPDATE dbo.CustomFieldValue
				SET CustomFieldValueInUseFlag = 0,
					UpdateDT = GETDATE()
				FROM dbo.CustomFieldValue CFV 
				WHERE CFV.CustomFieldValueID  IN (SELECT CustomFieldValueID FROM @BOETaskElementCustomFieldValueXREF WHERE IsOpenEnded = 0)
			END
GO
CREATE PROCEDURE [dbo].[updateBOETaskElementCustomFieldValueviaTableParameter]
(
@BOETaskElementCustomFieldValueXREF [dbo].[TT_BOETaskElementCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertBOETaskElementCustomFieldValue
**		Desc: Insert/Update Custom Field Value for the BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2(7) = GetDate()
			
DECLARE @OriginalCustomFieldValue TABLE
(
	[BTECFVID] int,
	[CustomFieldValueID] int
)
INSERT INTO @OriginalCustomFieldValue
SELECT 
	[BTECFVID],
	[CustomFieldValueID]
FROM dbo.BOETaskElementCustomFieldValueXREF
WHERE 
	[BTECFVID] IN
		(
			SELECT [BTECFVID] 
			FROM @BOETaskElementCustomFieldValueXREF
			WHERE IsOpenEnded = 0
		)

UPDATE [dbo].[BOETaskElementCustomFieldValueXREF]
SET 
	[CustomFieldValueID] = T.CustomFieldValueID,
	[UpdateDT] = @UpdateDT
FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
	INNER JOIN @BOETaskElementCustomFieldValueXREF T ON
			X.BTECFVID = T.BTECFVID AND
			X.UpdateDT = T.UpdateDT

UPDATE [dbo].[CustomFieldValue]
SET [CustomFieldValueDescription] = T.CustomFieldValueDescription
	,[UpdateDT] = @UpdateDT
FROM [dbo].[CustomFieldValue] C
	INNER JOIN @BOETaskElementCustomFieldValueXREF T ON
			C.CustomFieldValueID = T.CustomFieldValueID
WHERE T.IsOpenEnded = 1 AND
	EXISTS (SELECT 1 FROM [dbo].[BOETaskElementCustomFieldValueXREF]
			WHERE BTECFVID = T.BTECFVID AND UpdateDT = @UpdateDT)

/*Only should be looking within the scope
of the Custom Field, so if the scope is BOE
then only look in BOETaskElementCustomFieldValueXREF
*/
UPDATE dbo.CustomFieldValue
SET 
	CustomFieldValueInUseFlag = 
		CASE 
			WHEN X.[CustomFieldValueID] IS NULL THEN 0
			WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
		END,
		UpdateDT = @UpdateDT
FROM dbo.CustomFieldValue CFV 
LEFT OUTER JOIN [dbo].[BOETaskElementCustomFieldValueXREF] X ON
		CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
WHERE 
	CFV.CustomFieldValueID IN 
		(
			SELECT DISTINCT CustomFieldValueID
			FROM @OriginalCustomFieldValue
		)

UPDATE dbo.CustomFieldValue
	SET CustomFieldValueInUseFlag = 1,
		UpdateDT = @UpdateDT
WHERE CustomFieldValueID IN
	(
		SELECT DISTINCT CustomFieldValueID
		FROM @BOETaskElementCustomFieldValueXREF
		WHERE IsOpenEnded = 0
	)
		
IF @@ERROR = 0
	SELECT	X.BTECFVID AS BTECFVID,
			X.[UpdateDT]
FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
	INNER JOIN @BOETaskElementCustomFieldValueXREF T ON
			X.BTECFVID = T.BTECFVID 
ORDER BY T.OrderID
GO