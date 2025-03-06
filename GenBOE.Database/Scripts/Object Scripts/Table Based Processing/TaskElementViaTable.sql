-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOETaskElementviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOETaskElementviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOETaskElement' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOETaskElement];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOETaskElement] AS TABLE(
	[BOETaskElementID] [int] PRIMARY KEY CLUSTERED,
	[UpdateDT] [datetime2](7) NOT NULL,
	[TaskID] [varchar](3) NULL,
	[TaskTitle] [varchar](100) NOT NULL,
	[TaskDescription] [varchar](max) NULL,
	[TaskStartDate] [date] NULL,
	[TaskEndDate] [date] NULL,
	[MOQHoursEquation] [varchar](500) NULL,
	[MOQCostEquation] [varchar](250) NULL,
	[MOQText] [varchar](max) NULL,
	[MOQTypeID] [int] NULL,
	[BOEID] [int] NOT NULL,
	[LaborTypeWarningFlag] [bit] NOT NULL,
	[IMS_ID] [varchar](20) NULL,
	[TaskElementTypeID] [int] NOT NULL,
	[SortOrderID] [int] NOT NULL,
	[AuthorUserId] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateBOETaskElementviaTableParameter]
(
@BOETaskElement [dbo].[TT_BOETaskElement] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElement]
**		Desc: Insert/Update data into BOE Task Elements Detailed Section of BOE
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
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		1/26/17		pattoncr			Updating MOQHoursEquation to varchar(500).
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2 = GETDATE()

SET @UpdateDT = GetDate()
			
			 
UPDATE [dbo].[BOETaskElement]
	SET 
		[TaskID] = TT.TaskID,
		[TaskTitle] = TT.TaskTitle,
		[TaskDescription] = TT.TaskDescription,
		[TaskStartDate] = TT.TaskStartDate,
		[TaskEndDate] = TT.TaskEndDate,
		[MOQHoursEquation] = TT.MOQHoursEquation,
		[MOQCostEquation] = TT.MOQCostEquation,
		[MOQText] = TT.MOQText,
		[MOQTypeID] = TT.MOQTypeID,
		[BOEID] = TT.BOEID,
		[LaborTypeWarningFlag] = TT.LaborTypeWarningFlag,
		[IMS_ID] = TT.IMS_ID,
		[TaskElementTypeID] = TT.TaskElementTypeID,
		[UpdateDT] = @UpdateDT,
		[SortOrderID] = TT.SortOrderID,
		[AuthorUserId] = TT.AuthorUserId
FROM [dbo].[BOETaskElement] TE
	INNER JOIN @BOETaskElement TT ON 
		TE.BOETaskElementID = TT.BOETaskElementID AND
		TE.UpdateDT = TT.UpdateDT
				
IF @@ERROR = 0
	SELECT 
		T.BOETaskElementID AS BOETaskElementID, 
		T.UpdateDT AS UpdateDT 
	FROM @BOETaskElement TT
		INNER JOIN dbo.BOETaskElement T ON TT.BOETaskElementID = T.BOETaskElementID
	ORDER BY OrderID
GO
CREATE PROCEDURE [dbo].[deleteBOETaskElementviaTableParameter]
(
@BOETaskElement [dbo].[TT_BOETaskElement] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementviaTableParameter]
**		Desc: Delete Flag set in LM Task Element Section of BOE and all sub-elements (Labor Types and Labor Spread)
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
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/5/20		ranzalon			BOEJ-4658 - Update for RTE Template Answers
*******************************************************************************/
SET NOCOUNT ON 

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE TaskID IN
				(
					SELECT BOETaskElementID
					FROM @BOETaskElement
				)

			DELETE FROM dbo.BOELaborSpread
				FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
			
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM dbo.BOELaborType LT
					INNER JOIN @BOETaskElement TT ON 
					LT.BOETaskElementID = TT.BOETaskElementID
				)	
			
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
				
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @LaborTypeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM dbo.BOETaskElementWorkspaceVariableXREF X  
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM  dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			

			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
							INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


				

			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF
			WHERE BOETaskElementID IN
			(
				SELECT BOETaskElementID
				FROM @BOETaskElement
			)
				
			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
			
			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF] X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			
 
			DELETE FROM dbo.BOELaborType
				FROM dbo.BOELaborType LT
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
				
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			DELETE FROM dbo.CommonDisclosureSkillMix
				FROM dbo.CommonDisclosureSkillMix CS
					INNER JOIN dbo.BOETaskElement TE ON CS.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT

			DELETE FROM dbo.SkillMix
				FROM dbo.SkillMix S
					INNER JOIN dbo.BOETaskElement TE ON S.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT


			DELETE FROM [dbo].[BOETaskElement]
			FROM [dbo].[BOETaskElement] TE
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT



IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The BOE Task Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO
CREATE PROCEDURE [dbo].[insertBOETaskElementviaTableParameter]
(
@BOETaskElement [dbo].[TT_BOETaskElement] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElement]
**		Desc: Insert/Update data into BOE Task Elements Detailed Section of BOE
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
**		1/26/17		pattoncr			Updating MOQHoursEquation to varchar(500)
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @UpdateDT datetime2 = GETDATE()
	DECLARE @TT_BOETaskElement TABLE
	(
		[BOETaskElementID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[TaskID] [varchar](3) NULL,
		[TaskTitle] [varchar](100) NOT NULL,
		[TaskDescription] [varchar](max) NULL,
		[TaskStartDate] [date] NULL,
		[TaskEndDate] [date] NULL,
		[MOQHoursEquation] [varchar](500) NULL,
		[MOQCostEquation] [varchar](250) NULL,
		[MOQText] [varchar](max) NULL,
		[MOQTypeID] [int] NULL,
		[BOEID] [int] NOT NULL,
		[LaborTypeWarningFlag] [bit] NOT NULL,
		[IMS_ID] [varchar](20) NULL,
		[TaskElementTypeID] [int] NOT NULL,
		[SortOrderID] [int] NOT NULL,
		[AuthorUserId] [int] NULL,
		/* OrderID is automatically added in the code, so it HAS to be last */
		[OrderID] [int] NOT NULL
	)
	DECLARE @BOETaskElementID [int],
		@TaskID [varchar](3),
		@TaskTitle [varchar](100),
		@TaskDescription[varchar](max),
		@TaskStartDate [date],
		@TaskEndDate [date],
		@MOQHoursEquation [varchar](500),
		@MOQCostEquation [varchar](250),
		@MOQText [varchar](max),
		@MOQTypeID [int],
		@BOEID [int],
		@LaborTypeWarningFlag [bit],
		@IMS_ID [varchar](20),
		@TaskElementTypeID [int],
		@OrderID [int],
		@SortOrderID [int],
		@AuthorUserId [int]
	DECLARE @InsertedBOETaskElement AS Table (BOETaskElementID int)
	INSERT INTO @TT_BOETaskElement SELECT * FROM @BOETaskElement

	WHILE EXISTS (SELECT 1 FROM @TT_BOETaskElement WHERE BOETaskElementID < 0)
		BEGIN
			SELECT TOP 1 
				@BOETaskElementID = BOETaskElementID,
				@TaskID = TaskID,
				@TaskTitle = TaskTitle,
				@TaskDescription = TaskDescription,
				@TaskStartDate = TaskStartDate,
				@TaskEndDate = TaskEndDate,
				@MOQHoursEquation = MOQHoursEquation,
				@MOQCostEquation = MOQCostEquation,
				@MOQText = MOQText,
				@MOQTypeID = MOQTypeID,
				@BOEID = BOEID,
				@LaborTypeWarningFlag = LaborTypeWarningFlag,
				@IMS_ID = IMS_ID,
				@TaskElementTypeID = TaskElementTypeID,
				@OrderID =  OrderID,
				@SortOrderID = SortOrderID,
				@AuthorUserId = AuthorUserId
			FROM @TT_BOETaskElement
			WHERE BOETaskElementID < 0
			INSERT INTO [dbo].[BOETaskElement]
				   ([TaskID]
				   ,[TaskTitle]
				   ,[TaskDescription]
				   ,[TaskStartDate]
				   ,[TaskEndDate]
				   ,[MOQHoursEquation]
				   ,[MOQCostEquation]
				   ,[MOQText]
				   ,[MOQTypeID]
				   ,[BOEID]
				   ,[LaborTypeWarningFlag]
				   ,[IMS_ID]
				   ,[TaskElementTypeID]
				   ,[UpdateDT]
				   ,[SortOrderID]
				   ,[AuthorUserId]
				   )
			 OUTPUT inserted.BOETaskElementID INTO @InsertedBOETaskElement
			 VALUES
				   (
					@TaskID
				   ,@TaskTitle
				   ,@TaskDescription
				   ,@TaskStartDate
				   ,@TaskEndDate
				   ,@MOQHoursEquation
				   ,@MOQCostEquation
				   ,@MOQText
				   ,@MOQTypeID
				   ,@BOEID
				   ,@LaborTypeWarningFlag
				   ,@IMS_ID
				   ,@TaskElementTypeID
				   ,@UpdateDT
				   ,@SortOrderID
				   ,@AuthorUserId
					) 
			SELECT @BOETaskElementID = BOETaskElementID FROM @InsertedBOETaskElement
	
			UPDATE @TT_BOETaskElement
				SET BOETaskElementID = @BOETaskElementID
			WHERE 
				@OrderID = OrderID AND
				BOETaskElementID < 0
		END

	IF @@ERROR = 0
		SELECT 
			TT.BOETaskElementID AS BOETaskElementID, 
			@UpdateDT AS UpdateDT
		FROM @TT_BOETaskElement TT
			ORDER BY OrderID
GO