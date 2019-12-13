-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborTypeCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborTypeCustomFieldValueviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOELaborTypeCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOELaborTypeCustomFieldValueviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOELaborTypeCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOELaborTypeCustomFieldValueviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOELaborTypeCustomFieldValueXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOELaborTypeCustomFieldValueXREF];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOELaborTypeCustomFieldValueXREF] AS TABLE(
	[BLTCFVID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[BOELaborTypeID] [int] NOT NULL,
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

CREATE PROCEDURE [dbo].[updateBOELaborTypeCustomFieldValueviaTableParameter]
(
@BOELaborTypeCustomFieldValueXREF [dbo].[TT_BOELaborTypeCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertBOELaborTypeCustomFieldValue
**		Desc: Insert/Update the Custom Field Value for the BOELaborType
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
**		5/15/11		dcanuso				SP using the wrong key - Updated
**		5/26/11		dcanuso				Updated the use of PK and fixed PK Name
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2(7) = GetDate()
	
DECLARE @OriginalCustomFieldValue TABLE
(
	[BLTCFVID] int,
	[CustomFieldValueID] int
)
INSERT INTO @OriginalCustomFieldValue
SELECT 
	[BLTCFVID],
	[CustomFieldValueID]
FROM dbo.BOELaborTypeCustomFieldValueXREF
WHERE 
	[BLTCFVID] IN
		(
			SELECT [BLTCFVID] 
			FROM @BOELaborTypeCustomFieldValueXREF
			WHERE IsOpenEnded = 0
		)

UPDATE [dbo].[BOELaborTypeCustomFieldValueXREF]
SET 
	[CustomFieldValueID] = T.CustomFieldValueID,
	[UpdateDT] = @UpdateDT
FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X
	INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON
			X.BLTCFVID = T.BLTCFVID AND
			X.UpdateDT = T.UpdateDT

UPDATE [dbo].[CustomFieldValue]
SET [CustomFieldValueDescription] = T.CustomFieldValueDescription
	,[UpdateDT] = @UpdateDT
FROM [dbo].[CustomFieldValue] C
	INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON
			C.CustomFieldValueID = T.CustomFieldValueID
WHERE T.IsOpenEnded = 1 AND
	EXISTS (SELECT 1 FROM [dbo].[BOELaborTypeCustomFieldValueXREF]
			WHERE BLTCFVID = T.BLTCFVID AND UpdateDT = @UpdateDT)

/*Only should be looking within the scope
of the Custom Field, so if the scope is BOE
then only look in BOECustomFieldValueXREF
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
LEFT OUTER JOIN [dbo].[BOELaborTypeCustomFieldValueXREF] X ON
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
		FROM @BOELaborTypeCustomFieldValueXREF
		WHERE IsOpenEnded = 0
	)
		
IF @@ERROR = 0
	SELECT	X.BLTCFVID AS BLTCFVID,
			X.[UpdateDT]
FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X
	INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON
			X.BLTCFVID = T.BLTCFVID 
ORDER BY T.OrderID
GO
CREATE PROCEDURE [dbo].[insertBOELaborTypeCustomFieldValueviaTableParameter]
(
@BOELaborTypeCustomFieldValueXREF [dbo].[TT_BOELaborTypeCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertBOELaborTypeCustomFieldValue
**		Desc: Insert/Update the Custom Field Value for the BOELaborType
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
**		5/15/11		dcanuso				SP using the wrong key - Updated
**		5/26/11		dcanuso				Updated the use of PK and fixed PK Name
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDT datetime2 = GETDATE()
	
DECLARE @InsertedValues AS TABLE (CustomFieldValueID int, BLTCFVID int)

--Insert Open Ended CFs into CustomFieldValue table and get value IDs
--Only do this for values not already in the db, otherwise this will cause duplicates during copy workspace
MERGE INTO [dbo].[CustomFieldValue] AS cfv
USING @BOELaborTypeCustomFieldValueXREF AS xref
ON 1=0
WHEN NOT MATCHED AND xref.IsOpenEnded = 1 AND xref.CustomFieldValueID < 0  THEN 
INSERT 
	([CustomFieldValueName]
	,[CustomFieldValueDescription]
	,[CustomFieldID]
	,[CustomFieldValueInUseFlag]
	,[UpdateDT])
VALUES
	(CONVERT(varchar(10), xref.CustomFieldID) + '-' + CONVERT(varchar(10), xref.BOELaborTypeID), --Name only used when performing copy, so it needs to be unique for mapping
	xref.CustomFieldValueDescription,
	xref.CustomFieldID,
	1,
	@UpdateDT)
OUTPUT inserted.CustomFieldValueID, xref.BLTCFVID 
INTO @InsertedValues (CustomFieldValueID, BLTCFVID);

--Update Open Ended Fields that were already in the db to in use (for copy workspace)
--and then add them to the @InsertedValues table
UPDATE [dbo].[CustomFieldValue]
SET [CustomFieldValueInUseFlag] = 1
WHERE [CustomFieldValueID] in
	(
		SELECT CustomFieldValueID
		FROM @BOELaborTypeCustomFieldValueXREF
		WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0
	)

INSERT INTO @InsertedValues
SELECT CustomFieldValueID, BLTCFVID
FROM @BOELaborTypeCustomFieldValueXREF
WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0

--Insert Open Ended CFs Xrefs
INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
           ([BOELaborTypeID]
           ,[CustomFieldValueID]
           ,[UpdateDT])
SELECT
	[BOELaborTypeID]
	,iv.CustomFieldValueID
	,@UpdateDT
FROM @BOELaborTypeCustomFieldValueXREF xref INNER JOIN @InsertedValues iv 
ON xref.BLTCFVID = iv.BLTCFVID;

--Insert Standard CFs
INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
           ([BOELaborTypeID]
           ,[CustomFieldValueID]
           ,[UpdateDT])
SELECT
	[BOELaborTypeID]
	,[CustomFieldValueID]
	,@UpdateDT
FROM @BOELaborTypeCustomFieldValueXREF
WHERE [IsOpenEnded] = 0

	

UPDATE dbo.CustomFieldValue
	SET CustomFieldValueInUseFlag = 1
WHERE CustomFieldValueID IN
	(
		SELECT CustomFieldValueID
		FROM @BOELaborTypeCustomFieldValueXREF
		WHERE IsOpenEnded = 0
	)

DECLARE @ReturnTable AS TABLE (BLTCFVID int, UpdateDT Datetime2(7), OrderID int)

INSERT INTO @ReturnTable
SELECT	X.BLTCFVID AS BLTCFVID,
		X.UpdateDT,
		T.OrderID
	FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X
		INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON 
			X.BOELaborTypeID = T.BOELaborTypeID AND
			X.CustomFieldValueID = T.CustomFieldValueID
	WHERE
		X.UpdateDT = @UpdateDT

INSERT INTO @ReturnTable
	SELECT X.BLTCFVID AS BLTCFVID,
		X.UpdateDT,
		T.OrderID
	FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X
		INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON
				X.BOELaborTypeID = T.BOELaborTypeID
		INNER JOIN @InsertedValues I ON
				I.CustomFieldValueID = X.CustomFieldValueID
	WHERE X.UpdateDT = @UpdateDT

IF @@ERROR = 0
	SELECT 
		BLTCFVID AS BLTCFVID,
		UpdateDT
	FROM @ReturnTable
	ORDER BY OrderID
GO
CREATE PROCEDURE [dbo].[deleteBOELaborTypeCustomFieldValueviaTableParameter]
(
@BOELaborTypeCustomFieldValueXREF [dbo].[TT_BOELaborTypeCustomFieldValueXREF] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborTypeCustomFieldValue]
**		Desc: Deletes data from the dbo.BOELaborTypeCustomFieldValueXREF table which
**				holds the Custom Field Value for the BOELaborType
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
**		5/26/11		dcanuso				Updated the use of PK and fixed PK Name
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

		
		DELETE FROM [dbo].[BOELaborTypeCustomFieldValueXREF]
		FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X 
			INNER JOIN 	@BOELaborTypeCustomFieldValueXREF T 
				ON X.BLTCFVID = T.BLTCFVID AND
				   X.UpdateDT = T.UpdateDT

		DELETE FROM [dbo].[CustomFieldValue]
		FROM [dbo].[CustomFieldValue] C
			INNER JOIN @BOELaborTypeCustomFieldValueXREF T ON  
				C.CustomFieldValueID = T.CustomFieldValueID
		WHERE T.IsOpenEnded = 1 AND
			C.CustomFieldValueID NOT IN (SELECT CustomFieldValueID FROM [dbo].[BOELaborTypeCustomFieldValueXREF])
		
		/*Only should be looking within the scope
		of the Custom Field, so if the scope is BOE
		then only look in BOECustomFieldValueXREF
		*/
		
		UPDATE dbo.CustomFieldValue
			SET CustomFieldValueInUseFlag = CASE 
												WHEN X.[CustomFieldValueID] IS NULL THEN 0
												WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
											END,
				UpdateDT = GETDATE()
		FROM dbo.CustomFieldValue CFV 
			LEFT OUTER JOIN [dbo].[BOELaborTypeCustomFieldValueXREF] X ON
				CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
		WHERE CFV.CustomFieldValueID IN
						(
								SELECT CustomFieldValueID
								FROM @BOELaborTypeCustomFieldValueXREF
								WHERE IsOpenEnded = 0
						)
GO