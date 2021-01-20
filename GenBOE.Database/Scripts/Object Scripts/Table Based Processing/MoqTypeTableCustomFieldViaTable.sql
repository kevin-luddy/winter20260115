-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMoqTypeTableCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.deleteMoqTypeTableCustomFieldValueviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMoqTypeTableCustomFieldValueviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.insertMoqTypeTableCustomFieldValueviaTableParameter;
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_MoqTypeTableCustomFieldValueXREF' AND ss.name = N'dbo')
	DROP TYPE dbo.TT_MoqTypeTableCustomFieldValueXREF;
GO

-- Recreate types 3rd
CREATE TYPE dbo.TT_MoqTypeTableCustomFieldValueXREF AS TABLE(
	Id							INT NOT NULL,
	UpdateDT					DATETIME2(7) NOT NULL,
	MoqTypeTableDataId			INT NOT NULL,
	CustomFieldID				INT,
	CustomFieldValueID			INT NOT NULL,
	CustomFieldValueDescription VARCHAR(250),
	IsOpenEnded					BIT NOT NULL,
	OrderID						INT NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE dbo.insertMoqTypeTableCustomFieldValueviaTableParameter (@MoqTypeTableCustomFieldValueXREF dbo.TT_MoqTypeTableCustomFieldValueXREF READONLY)
AS
	/******************************************************************************
	**		Name: insertMoqTypeTableCustomFieldValue
	**		Desc: Insert the Custom Field Value for the MoqTypeTable
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		1/6/21		Dusan				Initial Release
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @UpdateDT datetime2 = GETDATE()	
	DECLARE @InsertedValues AS TABLE (CustomFieldValueID int, Id int)

	--Insert Open Ended CFs into CustomFieldValue table and get value IDs, Only do this for values not already in the db, otherwise this will cause duplicates during copy workspace
	MERGE INTO dbo.CustomFieldValue AS cfv USING @MoqTypeTableCustomFieldValueXREF AS xref ON 1 = 0 WHEN NOT MATCHED AND xref.IsOpenEnded = 1 AND xref.CustomFieldValueID < 0  
		THEN INSERT (CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, CustomFieldValueInUseFlag, UpdateDT)
				VALUES (CONVERT(VARCHAR(10), xref.CustomFieldID) + '-' + CONVERT(VARCHAR(10), xref.MoqTypeTableDataId), xref.CustomFieldValueDescription, xref.CustomFieldID, 1, @UpdateDT)
				OUTPUT inserted.CustomFieldValueID, xref.Id INTO @InsertedValues (CustomFieldValueID, Id);

	--Update Open Ended Fields that were already in the db to in use (for copy workspace) and then add them to the @InsertedValues table
	UPDATE dbo.CustomFieldValue
		SET CustomFieldValueInUseFlag = 1
			WHERE CustomFieldValueID IN (SELECT CustomFieldValueID FROM @MoqTypeTableCustomFieldValueXREF WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0)

	INSERT INTO @InsertedValues
		SELECT CustomFieldValueID, Id
			FROM @MoqTypeTableCustomFieldValueXREF
			WHERE IsOpenEnded = 1 AND CustomFieldValueID > 0

	--Insert Open Ended CFs Xrefs
	INSERT INTO dbo.MoqTypeTableCustomFieldValueXREF (MoqTypeTableDataId, CustomFieldValueID, UpdateDT)
		SELECT MoqTypeTableDataId, iv.CustomFieldValueID, @UpdateDT
		FROM @MoqTypeTableCustomFieldValueXREF xref 
			INNER JOIN @InsertedValues iv ON xref.Id = iv.Id

	--Insert Standard CFs
	INSERT INTO dbo.MoqTypeTableCustomFieldValueXREF(MoqTypeTableDataId, CustomFieldValueID, UpdateDT)
		SELECT MoqTypeTableDataId, CustomFieldValueID, @UpdateDT
			FROM @MoqTypeTableCustomFieldValueXREF
			WHERE IsOpenEnded = 0	

	UPDATE dbo.CustomFieldValue
		SET CustomFieldValueInUseFlag = 1
			WHERE CustomFieldValueID IN (SELECT CustomFieldValueID FROM @MoqTypeTableCustomFieldValueXREF WHERE IsOpenEnded = 0)

	DECLARE @ReturnTable AS TABLE (Id INT, UpdateDT DATETIME2(7), OrderID int)

	INSERT INTO @ReturnTable
		SELECT	X.Id AS Id, X.UpdateDT, T.OrderID
			FROM dbo.MoqTypeTableCustomFieldValueXREF X 
				INNER JOIN @MoqTypeTableCustomFieldValueXREF T ON X.MoqTypeTableDataId = T.MoqTypeTableDataId AND X.CustomFieldValueID = T.CustomFieldValueID
			WHERE X.UpdateDT = @UpdateDT

	INSERT INTO @ReturnTable
		SELECT X.Id AS Id, X.UpdateDT, T.OrderID
			FROM dbo.MoqTypeTableCustomFieldValueXREF X 
				INNER JOIN @MoqTypeTableCustomFieldValueXREF T ON X.MoqTypeTableDataId = T.MoqTypeTableDataId 
				INNER JOIN @InsertedValues I ON I.CustomFieldValueID = X.CustomFieldValueID
			WHERE X.UpdateDT = @UpdateDT

	IF @@ERROR = 0
		SELECT Id AS Id, UpdateDT
			FROM @ReturnTable
			ORDER BY OrderID
GO
CREATE PROCEDURE dbo.deleteMoqTypeTableCustomFieldValueviaTableParameter (@MoqTypeTableCustomFieldValueXREF dbo.TT_MoqTypeTableCustomFieldValueXREF READONLY)
AS
	/******************************************************************************
	**		Name: deleteMoqTypeTableCustomFieldValue
	**		Desc: Delete the Custom Field Value for the MoqTypeTable
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		1/6/21		Dusan				Initial Release
	*******************************************************************************/
	SET NOCOUNT ON 
		
	DELETE FROM dbo.MoqTypeTableCustomFieldValueXREF
		FROM dbo.MoqTypeTableCustomFieldValueXREF X 
			INNER JOIN 	@MoqTypeTableCustomFieldValueXREF T ON X.Id = T.Id AND X.UpdateDT = T.UpdateDT

	DELETE FROM dbo.CustomFieldValue
		FROM dbo.CustomFieldValue C 
			INNER JOIN @MoqTypeTableCustomFieldValueXREF T ON C.CustomFieldValueID = T.CustomFieldValueID
		WHERE T.IsOpenEnded = 1 AND C.CustomFieldValueID NOT IN (SELECT CustomFieldValueID FROM dbo.MoqTypeTableCustomFieldValueXREF)
		
	/* Only should be looking within the scope of the Custom Field, so if the scope is MoqTypeTable then only look in MoqTypeTableCustomFieldValueXREF */		
	UPDATE dbo.CustomFieldValue
		SET CustomFieldValueInUseFlag = CASE WHEN X.CustomFieldValueID IS NULL THEN 0 WHEN X.CustomFieldValueID IS NOT NULL THEN 1 END, UpdateDT = GETDATE()
		FROM dbo.CustomFieldValue CFV 
			LEFT OUTER JOIN dbo.MoqTypeTableCustomFieldValueXREF X ON CFV.CustomFieldValueID = X.CustomFieldValueID
		WHERE CFV.CustomFieldValueID IN (SELECT CustomFieldValueID FROM @MoqTypeTableCustomFieldValueXREF WHERE IsOpenEnded = 0)
GO