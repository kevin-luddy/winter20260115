-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborSpreadByBOELaborTypeIDviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborSpreadByBOELaborTypeIDviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOELaborTypeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOELaborTypeviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOELaborTypeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOELaborTypeviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOELaborTypeviatableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOELaborTypeviatableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_BOELaborType' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_BOELaborType];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_BOELaborType] AS TABLE(
	[BOELaborTypeID] [int] PRIMARY KEY CLUSTERED,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ResourceID] [int] NULL,
	[PerformingOrganizationID] [int] NULL,
	[BOELaborTypeStartDate] [date] NOT NULL,
	[BOELaborTypeEndDate] [date] NOT NULL,
	[SpreadCurveID] [int] NOT NULL,
	[PercentSpread] [decimal](38, 6) NULL,
	[ValueSpread] [decimal](18, 6) NULL,
	[BOETaskElementID] [int] NOT NULL,
	[SpreadTypeID] [int] NULL,
	[PercentSpreadLocked] [bit] NOT NULL,
	[HourSpreadLocked] [bit] NOT NULL,
	[WBSID] [int] NULL,
	[CLINID] [int] NULL,
	[CanOffload] bit NULL,
	[LaborSortId] [int] NOT NULL,
	[BRCResourceID] [int] NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOELaborTypeviaTableParameter]
(
@BOELaborType [dbo].[TT_BOELaborType] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborType]
**		Desc: Delete Flag set in Labor Type Section of BOE and sub-elements (Labor Spread)
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
**		8/19/10		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft Deletes removed
**		3/29/11		dcanuso				Developer noticed Flags were not being 
**										updated - added code to correct
**		4/21/11		dcanuso				Added DELETE for Custom Field
**		8/18/11		dcanuso				Added In Use Processing of Custom Fields
**		11/25/11	dcanuso				Fixing In Use
**		12/15/11	dcanuso				WI 6166
**		12/21/11	dcanuso				WI 6021
**										Need to check if a Travel Resource Rate 
**										is being used/not used
**										and update accordingly
**		1/10/12		dcanuso				WI 
**										6470: Trip In Use Fixes
**										https://eureka.isgs.lmco.com/#activity/151789
**										6624: After WI 6622, we now need to check that 
**										the originating Labor Resource Rate is in 
**										use as we did for trips.
**										6339: Confirm in-use is working for workspace
**										and labor resource rates
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/4/13		dcanuso				WI14842 Redesign Performing Organization
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		1/23/24		e302876			    PROPH-1484 - New column BRCResourceID
*******************************************************************************/
SET NOCOUNT ON 


	
				DELETE FROM dbo.BOELaborSpread
				WHERE BOELaborTypeID IN
							(
								SELECT BOELaborTypeID
								FROM @BOELaborType
							)
			
				/*Variables Used for InUse Function*/
			DECLARE		@CurrentResourceID int,
						@CurrentPerformingOrganizationID int
			
			SELECT	@CurrentResourceID  = ResourceID,
					@CurrentPerformingOrganizationID = PerformingOrganizationID
			FROM [dbo].[BOELaborType]
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM @BOELaborType
				)
			
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @CustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @CustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM @BOELaborType
				)
	
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM @BOELaborType
				)

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @CustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
	
			DELETE FROM dbo.BOELaborType
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM @BOELaborType
				)
				
				/*	Update In Use Flag
					Fix All In Use Flags
					Check if Resource is used elsewhere
				*/
				/*EXEC  [dbo].[updateResourceInUseFlagByResourceID] @CurrentResourceID*/
					 
				/*EXECUTE [dbo].[updatePerformingOrganizationInUseFlagByPerformingOrganizationID] @CurrentPerformingOrganizationID*/

			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = B.WorkspaceID 
				FROM dbo.BOE B
					INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
					INNER JOIN dbo.BOELaborType LT ON TE.BOETaskElementID = LT.BOETaskElementID
				WHERE LT.BOELaborTypeID  IN
						(
							SELECT BOELaborTypeID
							FROM @BOELaborType
						)

GO
CREATE PROCEDURE [dbo].[deleteBOELaborSpreadByBOELaborTypeIDviaTableParameter]
(
@BOELaborType [dbo].[TT_BOELaborType] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOELaborSpreadByBOELaborTypeID]
**		Desc: Delete BOE Labor Spread row in table using BOELaborTypeID
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/13/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM dbo.BOELaborSpread
WHERE BOELaborTypeID IN
	(
		SELECT BOELaborTypeID
		FROM @BOELaborType
	)
GO
CREATE PROCEDURE [dbo].[updateBOELaborTypeviaTableParameter]
(
@BOELaborType [dbo].[TT_BOELaborType] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertBOELaborType
**		Desc: Insert/Update data into Labor Type Section of BOE
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
**		9/3			dcanuso				Added Reource, data type changes for spread
**										Removed:
**										@LaborTypeID int,
**										@SegmentRegionID int,
**										They are associated with a single Resource ID not a BOE
**		9/13/10		dcanuso				Soft deletes removed
**		12/14/10	dcanuso				Changed percent spread parameter to decimal (6,3)
**		1/20/11		dcanuso				Resource Redesign called for changes in SP
**		2/10/11		dcanuso				Adding InUse functionality for Resource and 
**										Performing Organization
**		2/14/11		dcanuso				Added Updating Update DT
**		5/26/11		dcanuso				Value Spread changed and added @SpreadTypeID
**		11/25/11	dcanuso				InUse Updated
**		12/13/11	dcanuso				WI 6166
**		12/21/11	dcanuso				WI 6021
**										Need to check if a Travel Resource Rate 
**										is being used/not used
**										and update accordingly
**		1/10/12		dcanuso				WI 
**										6470: Trip In Use Fixes
**										https://eureka.isgs.lmco.com/#activity/151789
**										6624: After WI 6622, we now need to check that 
**										the originating Labor Resource Rate is in 
**										use as we did for trips.
**										6339: Confirm in-use is working for workspace
**										and labor resource rates
**		2/6/12		dcanuso				More changes to InUse Processing
**		4/12/12		dcanuso				WI 8398 Locking Spread
**		8/3/12		dcanuso				WI 10410 % Spread to 38,12
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		8/9/12		dcanuso				WI 10410 changed @PercentSpread DECIMAL (38,3)
**		2/11/13		dcanuso				WI14842 Redesign Performing Organization
**		10/14/15	mbasquil			BOEJ-342 Resource level WBS/CLIN
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		5/22/2017	Dusan				BOEJ-2181 Add Add/Delete column
**		8/17/2017	Dusan				BOEJ-2469 Add Old Resource (2.16.1)
**		10/2/2017	twilson3			BOEJ-2520 Cleanup DB, remove old ProjectMap columns
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		9/15/20		ranzalon			BOEJ-4825 - Added MOQTypeSelectionId
**		12/8/2020	ranzalon			BOEJ-4972 - Removed MOQTypeSelectionId
**		1/23/24		e302876			    PROPH-1484 - New column BRCResourceID
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2(7) = GetDate()
			
UPDATE [dbo].[BOELaborType]
SET 
	[ResourceID] = T.ResourceID,
	[PerformingOrganizationID] = T.PerformingOrganizationID,
	[BOELaborTypeStartDate] = T.BOELaborTypeStartDate,
	[BOELaborTypeEndDate] = T.BOELaborTypeEndDate,
	[SpreadCurveID] = T.SpreadCurveID,
	[PercentSpread] = T.PercentSpread,
	[ValueSpread] = T.ValueSpread,
	[BOETaskElementID] = T.BOETaskElementID,
	[SpreadTypeID] = T.SpreadTypeID,
	[UpdateDT] = @UpdateDT,
	[PercentSpreadLocked] = T.PercentSpreadLocked,
	[HourSpreadLocked] = T.HourSpreadLocked,
	[WBSID]	= T.WBSID,
	[CLINID] = T.CLINID,
	[CanOffload] = T.CanOffload,
	[BRCResourceID] = T.BRCResourceID,
	[LaborSortId] = T.LaborSortId
FROM [dbo].[BOELaborType] L
	INNER JOIN @BOELaborType T ON 
		L.[BOELaborTypeID] = T.[BOELaborTypeID] AND
		L.[UpdateDT] = T.[UpdateDT] 
				
IF @@ERROR = 0
SELECT	L.BOELaborTypeID AS BOELaborTypeID,
		L.[UpdateDT]
FROM [dbo].[BOELaborType] L
	INNER JOIN @BOELaborType T ON 
		L.[BOELaborTypeID] = T.[BOELaborTypeID] 
ORDER BY T.OrderID
GO
CREATE PROCEDURE [dbo].[insertBOELaborTypeviatableParameter] (@BOELaborType [dbo].[TT_BOELaborType] READONLY)
AS
/******************************************************************************
**		 
**		Name: upsertBOELaborType
**		Desc: Insert/Update data into Labor Type Section of BOE
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
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		5/22/2017	Dusan				BOEJ-2181 Add Add/Delete column
**		8/17/2017	Dusan				BOEJ-2469 Add Old Resource (2.16.1)
**		8/30/2017	Dusan				Rewrite... Hoping to improve performance... Hoping....
**		10/2/2017	twilson3			BOEJ-2520 Cleanup DB, remove old ProjectMap columns
**		6/5/2017	twilson3			BOEJ-3510 Updated insert order to match the input
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		9/15/20		ranzalon			BOEJ-4825 - Added MOQTypeSelectionId
**		12/8/2020	ranzalon			BOEJ-4972 - Removed MOQTypeSelectionId
**		1/23/24		e302876			    PROPH-1484 - New column BRCResourceID
*******************************************************************************/
	SET NOCOUNT ON 

	DECLARE @UpdateDT datetime2(7) = GETDATE()
	DECLARE @TT_BOELaborType TABLE
	(
		[BOELaborTypeID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[ResourceID] [int] NULL,
		[PerformingOrganizationID] [int] NULL,
		[BOELaborTypeStartDate] [date] NOT NULL,
		[BOELaborTypeEndDate] [date] NOT NULL,
		[SpreadCurveID] [int] NOT NULL,
		[PercentSpread] [decimal](38, 6) NULL,
		[ValueSpread] [decimal](18, 6) NULL,
		[BOETaskElementID] [int] NOT NULL,
		[SpreadTypeID] [int] NULL,
		[PercentSpreadLocked] [bit] NOT NULL,
		[HourSpreadLocked] [bit] NOT NULL,
		[WBSID] [int] NULL,
		[CLINID] [int] NULL,
		[CanOffload] bit NULL,
		[LaborSortId] [int] NOT NULL,
		[BRCResourceID] [int] NULL,
		[OrderID] [int] NOT NULL
	)
	DECLARE @BOELaborTypeID [int],
		@ResourceID [int],
		@PerformingOrganizationID [int],
		@BOELaborTypeStartDate [date],
		@BOELaborTypeEndDate [date],
		@SpreadCurveID [int],
		@PercentSpread [decimal](38, 6),
		@ValueSpread [decimal](18, 6),
		@BOETaskElementID [int],
		@SpreadTypeID [int],
		@PercentSpreadLocked [bit],
		@HourSpreadLocked [bit],
		@WBSID [int],
		@CLINID [int],
		@CanOffload bit,
		@LaborSortId [int],
		@BRCResourceID [int],
		@OrderID [int]
	DECLARE @InsertedItem AS Table (Id int)

	INSERT INTO @TT_BOELaborType SELECT * FROM @BOELaborType

	WHILE EXISTS (SELECT 1 FROM @TT_BOELaborType WHERE BOELaborTypeID < 0)
		BEGIN
			SELECT TOP 1 
				@ResourceID = ResourceID,
				@PerformingOrganizationID = PerformingOrganizationID,
				@BOELaborTypeStartDate = BOELaborTypeStartDate,
				@BOELaborTypeEndDate = BOELaborTypeEndDate,
				@SpreadCurveID = SpreadCurveID,
				@PercentSpread = PercentSpread,
				@ValueSpread = ValueSpread,
				@BOETaskElementID = BOETaskElementID,
				@SpreadTypeID = SpreadTypeID,
				@PercentSpreadLocked = PercentSpreadLocked,
				@HourSpreadLocked = HourSpreadLocked,
				@WBSID = WBSID,
				@CLINID = CLINID,
				@CanOffload = CanOffload,
				@LaborSortId = LaborSortId,
				@BRCResourceID = BRCResourceID,
				@OrderID = OrderID
			FROM @TT_BOELaborType
			WHERE BOELaborTypeID < 0
			ORDER BY OrderID ASC

			INSERT INTO [dbo].[BOELaborType] (
					[ResourceID]
				   ,[PerformingOrganizationID]
				   ,[BOELaborTypeStartDate]
				   ,[BOELaborTypeEndDate]
				   ,[SpreadCurveID]
				   ,[PercentSpread]
				   ,[ValueSpread]
				   ,[BOETaskElementID]
				   ,[SpreadTypeID]/*Spread Type is Labor and Hours
									to determine if the BOE Labor Type row
									is one of those types*/
				   ,[UpdateDT]
				   ,[PercentSpreadLocked]
				   ,[HourSpreadLocked]
				   ,[WBSID]
				   ,[CLINID]
				   ,[CanOffload]
				   ,[LaborSortId]
				   ,[BRCResourceID]
				   )
			 OUTPUT inserted.BOELaborTypeID INTO @InsertedItem
			 VALUES
				   (
					@ResourceID,
					@PerformingOrganizationID,
					@BOELaborTypeStartDate,
					@BOELaborTypeEndDate,
					@SpreadCurveID,
					@PercentSpread,
					@ValueSpread,
					@BOETaskElementID,
					@SpreadTypeID,
					@UpdateDT,
					@PercentSpreadLocked,
					@HourSpreadLocked,
					@WBSID,
					@CLINID,
					@CanOffload,
					@LaborSortId,
					@BRCResourceID) 

			SELECT @BOELaborTypeID = Id FROM @InsertedItem
	
			UPDATE @TT_BOELaborType
				SET BOELaborTypeID = @BOELaborTypeID
			WHERE 
				@OrderID = OrderID AND
				BOELaborTypeID < 0
		END

	IF @@ERROR = 0
		SELECT 
			TT.BOELaborTypeID AS BOELaborTypeID, 
			@UpdateDT AS UpdateDT
		FROM @TT_BOELaborType TT
			ORDER BY OrderID
GO