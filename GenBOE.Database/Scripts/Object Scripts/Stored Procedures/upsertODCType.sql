IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCType];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
 
CREATE PROCEDURE [dbo].[upsertODCType]
(
@ODCTypeID int,
@ResourceID int,
@PerformingOrganizationID int,
@ODCTypeStartDate date,
@ODCTypeEndDate date,
@SpreadCurveID int,
@ODCTypeCost decimal (12,2),
@ODCTaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: upsertODCType
**		Desc: Insert/Update data into ODC Type Section of BOE/ODC
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
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
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/11/13		dcanuso				WI14842 Redesign Performing Organization
**		4/18/13		dcanuso				WI 17644 ODCType.ODCTypeCost 
**										from decimal 9,2 to 12,2
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@InsertedODCType AS Table (ODCTypeID int)

IF @ODCTypeID < 0  /*Insert Record*/

	BEGIN

	SET @UpdateDT = GetDate()

	INSERT INTO [dbo].[ODCType]
           (
            [ResourceID]
           ,[PerformingOrganizationID]
           ,[ODCTypeStartDate]
           ,[ODCTypeEndDate]
           ,[SpreadCurveID]
           ,[ODCTypeCost]
           ,[ODCTaskElementID]
           ,[UpdateDT]
           )
     OUTPUT inserted.ODCTypeID INTO @InsertedODCType
     VALUES
           (
           @ResourceID,
		   @PerformingOrganizationID,
           @ODCTypeStartDate,
           @ODCTypeEndDate,
           @SpreadCurveID,
           @ODCTypeCost,
           @ODCTaskElementID,
           @UpdateDT
           )
           
           
	SELECT 	@ODCTypeID = ODCTypeID FROM @InsertedODCType
	
	/*Update In Use Flag*/
/*	EXECUTE [dbo].[updateResourceInUseFlagByResourceID] @ResourceID*/
/*	UPDATE [dbo].[PerformingOrganization] SET PerformingOrganizationInUseFlag = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID*/

END

ELSE
	BEGIN
			IF (SELECT UpdateDT FROM [dbo].[ODCType] WHERE ODCTypeID = @ODCTypeID) = @UpdateDT
			BEGIN	
			
			
			/*Variables Used for InUse Function*/
			DECLARE		/*@CurrentResourceID int,*/
						@CurrentPerformingOrganizationID int
			
			SELECT	/*@CurrentResourceID  = ResourceID,*/
					@CurrentPerformingOrganizationID = PerformingOrganizationID
			FROM [dbo].[ODCType]
			WHERE ODCTypeID = @ODCTypeID
			
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[ODCType]
				SET 
					[ResourceID] = @ResourceID,
					[PerformingOrganizationID] = @PerformingOrganizationID,
					[ODCTypeStartDate] = @ODCTypeStartDate,
					[ODCTypeEndDate] = @ODCTypeEndDate,
					[SpreadCurveID] = @SpreadCurveID,
					[ODCTypeCost] = @ODCTypeCost,
					[ODCTaskElementID] = @ODCTaskElementID,
					[UpdateDT] = @UpdateDT
			WHERE 
				ODCTypeID = @ODCTypeID
					
			END
			ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ODC Type with ID ' + CAST(@ODCTypeID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
			
	END

IF @@ERROR = 0
	SELECT @ODCTypeID AS ODCTypeID
GO