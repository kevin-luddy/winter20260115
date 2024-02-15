IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOELaborType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOELaborType];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOELaborType]
(
@BOELaborTypeID int,
@ResourceID int,
@PerformingOrganizationID int,
@BOELaborTypeStartDate date,
@BOELaborTypeEndDate date,
@SpreadCurveID int,
@PercentSpread DECIMAL (38,6),
@ValueSpread DECIMAL (18, 6),
@BOETaskElementID int,
@SpreadTypeID int,
@UpdateDT datetime2,
@PercentSpreadLocked bit,
@HourSpreadLocked bit,
@WBSID int,
@CLINID int,
@CanOffload bit,
@LaborSortID int,
@BRCResourceID int
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
**		10/7/14		dcanuso				Precision Story
**		10/14/15	mbasquil			BOEJ-342 Resource level WBS/CLIN
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		5/22/2017	Dusan				BOEJ-2181 Add Add/Delete column
**		8/17/2017	Dusan				BOEJ-2469 Add Old Resource (2.16.1)
**		10/2/2017	twilson3			BOEJ-2520 Cleanup DB, remove old ProjectMap columns
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		9/15/2020	ranzalon			BOEJ-4825 - Added MOQTypeSelectionId
**		12/8/2020	ranzalon			BOEJ-4972 - Removed MOQTypeSelectionId
**		1/23/24		e302876			    PROPH-1484 - New column BRCResourceID
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@InsertedBOELaborType AS Table (BOELaborTypeID int)

IF @BOELaborTypeID < 0  /*Insert Record*/

	BEGIN

	SET @UpdateDT = GetDate()

	INSERT INTO [dbo].[BOELaborType]
           (
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
     OUTPUT inserted.BOELaborTypeID INTO @InsertedBOELaborType
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
		   @LaborSortID,
		   @BRCResourceID
		   )
           
           
	SELECT 	@BOELaborTypeID = BOELaborTypeID FROM @InsertedBOELaborType
	
	/*Update In Use Flag*/
/*	EXECUTE [dbo].[updateResourceInUseFlagByResourceID] @ResourceID*/
/*	UPDATE [dbo].[PerformingOrganization] SET PerformingOrganizationInUseFlag = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID*/

END

ELSE
	BEGIN
			IF (SELECT UpdateDT FROM [dbo].[BOELaborType] WHERE BOELaborTypeID = @BOELaborTypeID) = @UpdateDT
			BEGIN	
			
			
			/*Variables Used for InUse Function*/
			DECLARE		/*@CurrentResourceID int,*/
						@CurrentPerformingOrganizationID int
			
			SELECT	/*@CurrentResourceID  = ResourceID,*/
					@CurrentPerformingOrganizationID = PerformingOrganizationID
			FROM [dbo].[BOELaborType]
			WHERE BOELaborTypeID = @BOELaborTypeID
			
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[BOELaborType]
				SET 
					[ResourceID] = @ResourceID,
					[PerformingOrganizationID] = @PerformingOrganizationID,
					[BOELaborTypeStartDate] = @BOELaborTypeStartDate,
					[BOELaborTypeEndDate] = @BOELaborTypeEndDate,
					[SpreadCurveID] = @SpreadCurveID,
					[PercentSpread] = @PercentSpread,
					[ValueSpread] = @ValueSpread,
					[BOETaskElementID] = @BOETaskElementID,
					[SpreadTypeID] = @SpreadTypeID,
					[UpdateDT] = @UpdateDT,
					[PercentSpreadLocked] = @PercentSpreadLocked,
					[HourSpreadLocked] = @HourSpreadLocked,
					[WBSID] = @WBSID,
					[CLINID] = @CLINID,
					[CanOffload] = @CanOffload,
					[LaborSortId] = @LaborSortID,
					[BRCResourceID] = @BRCResourceID
			WHERE 
				BOELaborTypeID = @BOELaborTypeID
				
				/*Update In Use Flag*/
				/*EXECUTE [dbo].[updateResourceInUseFlagByResourceID] @ResourceID
				EXECUTE [dbo].[updateResourceInUseFlagByResourceID] @CurrentResourceID*/
				/*UPDATE [dbo].[PerformingOrganization] SET PerformingOrganizationInUseFlag = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID*/
				/*EXECUTE [dbo].[updatePerformingOrganizationInUseFlagByPerformingOrganizationID] @CurrentPerformingOrganizationID*/

			END
			ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Labor Type with ID ' + CAST(@BOELaborTypeID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END			
	END

IF @@ERROR = 0
	SELECT @BOELaborTypeID AS BOELaborTypeID
GO