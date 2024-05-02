-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceResourcetviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceResourcetviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertWorkspaceResourceviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertWorkspaceResourceviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceResourcetviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceResourcetviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_WorkspaceResource' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_WorkspaceResource];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_WorkspaceResource] AS TABLE(
	-- the ResourceID is actually the SystemResourceID
	[ResourceID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ResourceName] [varchar](20) NOT NULL,
	[ResourceDescription] [varchar](100) NULL,
	[SegmentRegion] [varchar](50) NULL,
	[LaborType] [varchar](50) NULL,
	[SegmentID] [int] NULL,
	[ResourceListID] [int] NOT NULL,
	[CostElementID] [int] NOT NULL,
	[DeletedFlag] [bit] NOT NULL,
	[RateTypeID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertWorkspaceResourceviaTableParameter]
(
@ResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: insertWorkspaceResourceviaTableParameter
**		Desc: Bulk Insert Resource
**			
**		
**
**		Auth: Tommy Lee
**		Date: 4/29/24
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/29/24		e374897				Bulk Insert Resources
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2 = GETDATE()

DECLARE @InsertedResource AS Table (ResourceID int, ResourceListID int)


INSERT INTO [dbo].[Resource]
    ([ResourceName]
    ,[ResourceDescription]
    ,[SegmentRegion]
    ,[LaborType]
    ,[SegmentID]
    ,[ResourceListID]
    ,[CostElementID]
    ,[UpdateDT]
    ,[DeletedFlag]
    ,[RateTypeID]
    )
OUTPUT inserted.ResourceID, inserted.ResourceListID INTO @InsertedResource
SELECT ResourceName
	,ResourceDescription
	,SegmentRegion
	,LaborType
	,SegmentID
	,ResourceListID
	,CostElementID
	,@UpdateDT
	,0 /*Flag set to false for new*/
	,RateTypeID
FROM @WorkspaceResourceTableParameter

           
SELECT @ResourceID = ResourceID FROM @InsertedResource
      
      
INSERT INTO [dbo].[WorkspaceResource]
    ([SystemResourceID]
    ,[ResourceListID]
    ,[WorkspaceID])
SELECT r.ResourceID
    ,r.ResourceListID
    ,w.WorkspaceID
FROM @InsertedResource r
JOIN dbo.Workspace w on r.ResourceListID = w.ResourceListID

IF @@ERROR = 0
	SELECT ResourceID FROM @InsertedResource

GO
CREATE PROCEDURE [dbo].[deleteWorkspaceResourcetviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceResource]
**		Desc: Bulk Delete Workspace Resource
**			
**
**		Auth: Tommy Lee
**		Date: 4/29/24
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/29/24		e374897				Bulk Delete Resource
******************************************************************************/

/*
Note from Wireframe:
a.       In the System Administrator Default Resources table, 
the Resource that has a rate in the Manage Labor Resource Rates table 
that is not “In use”, will have a checkbox where it is able to be deleted.
1.       Wireframe 28 – When a Resource is selected in the 
System Administrator Default Resources table, 
the Delete button is clicked, and that Resource has a rate in the 
Manage Labor Resource Rates table, the following dialog will appear:  “The selected resource has a rate assigned in the Manage Labor Resource Rates table.  Are you sure you want to delete this resource and its corresponding resource rate?
1.       If the user clicks “Yes” the Resource is deleted from the System Administrator Default Resources table and the corresponding rate within the Manage Labor Resource Rates table will be deleted as well.
b.      In the System Administrator Default Resources table, the Resource that currently has an “In use” rate in the Manage Labor Resource Rates table, will become “In use” and cannot be deleted until the resource rate is deleted from the Manage Labor Resource Rates grid.
1.       While “In use” the Resource cannot be deleted through the UI or through the Import function.


Workspace Administrator Resources page - When a resource rate is added in the Manage Subcontractor, IWTA, and LMSI Rates grid, the resource that is now associated with a rate becomes linked to the Workspace Administrator Resources table.

a.       In the the Workspace Administrator Resources table, the Resource that has a rate in the Manage Subcontractor, IWTA, and LMSI Rates table that is not “In use”, will have a checkbox where it is able to be deleted.

                                                         i.            Wireframe 29 – When a Resource is selected in the Workspace Administrator Resources table, the Delete button is clicked, and that Resource has a rate in the Manage Subcontractor, IWTA, and LMSI Rates table, the following dialog will appear:  “The selected resource has a rate assigned in the Manage Labor Resource Rates table.  Are you sure you want to delete this resource and its corresponding resource rate?

1.       If the user clicks “Yes” the Resource is deleted from the Workspace Administrator Resources table and the corresponding rate within the Manage Subcontractor, IWTA, and LMSI Rates table will be deleted as well.

b.      In the Workspace Administrator Resources table, the Resource that currently has an “In use” rate in the Manage Subcontractor, IWTA, and LMSI Rates table, will become “In use” and cannot be deleted until the resource rate is deleted from the Manage Subcontractor, IWTA, and LMSI Rates grid.

                                                         i.            While “In use” the Resource cannot be deleted through the UI or through the Import function.


*/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

DECLARE @ResourceListIDParam TABLE (ResourceListID int)
INSERT INTO @SystemResourceListIDParam
SELECT ResourceListID
FROM @WorkspaceResourceTableParameter

DECLARE @temp TABLE (SystemResourceID int)
-- This will return a list of all system resource ids that are in use
INSERT @temp EXECUTE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter] @ResourceListIDParam


-- Create a table to set InUse Flag
DECLARE @InUseTable TABLE (
	ResourceID INT NOT NULL,
	WorkspaceID INT NOT NULL,
	ResourceListID INT,
	InUse BIT NOT NULL
)
INSERT INTO @InUseTable (ResourceID, WorkspaceID, InUse)
SELECT 
	ISNULL(wrp.ResourceID, t.ResourceID) AS ResourceID,
	ISNULL(wrp.WorkspaceID, t.WorkspaceID) AS WorkspaceID,
	wrp.ResourceListID,
	CASE 
		WHEN wrp.ResourceID IS NULL THEN 0
		ELSE 1
	END AS InUse
FROM @WorkspaceResourceTableParameter AS wrp
	FULL OUTER JOIN @temp AS t ON wrp.ResourceID = t.SystemResourceID AND wrp.WorkspaceID = t.WorkspaceID
WHERE 
	t.SystemResourceID IS NULL

-- Delete all the non InUse ResourceID

DELETE dbo.TMResourceRate
FROM dbo.TMResourceRate tmr
	JOIN @InUseTable i ON i.ResourceID = tmr.TMResourceID AND tmr.WorkspaceID = i.WorkspaceID
WHERE
	i.InUse = 0

DELETE dbo.WorkspaceResource 
FROM dbo.WorkspaceResource wr
	JOIN @InUseTable i ON i.ResourceID = wr.SystemResourceID AND i.WorkspaceID = wr.WorkspaceID
WHERE
	i.InUse = 0


IF @ResourceType > 1
BEGIN
	DELETE dbo.Resource
	FROM dbo.Resource r
		JOIN @InUseTable i on i.ResourceID = r.ResourceID AND i.ResourceListID = r.ResourceListID
	WHERE 
		-- equivalent to r.ResourceListID != 1
		r.ResourceListID <> 1
END

GO
CREATE PROCEDURE [dbo].[updateWorkspaceResourcetviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: updateWorkspaceResourcetviaTableParameter
**		Desc: Bulk Update Workspace Resources
**			
**		
**
**		Auth: Tommy Lee
**		Date: 4/29/24
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/29/24		e374897				Bulk Update Resources
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

DECLARE @UpdateDT datetime2 = GETDATE()

-- @InsertedSystemResource will only have ResourceListID = 1 because of WHERE ResourceListID = 1
DECLARE @InsertedSystemResource AS Table (ResourceID int, ResourceListID int, WorkspaceID int)

DECLARE @temp TABLE (ResourceID int, ResourceListID int, WorkspaceID int)
INSERT INTO @temp (ResourceID, ResourceListID, WorkspaceID)
SELECT r.ResourceID, r.ResourceListID, w.WorkspaceID FROM [dbo].[Resource] r
JOIN [dbo].[Workspace] w ON r.[ResourceListID] = w.[ResourceListID]

/*
	If Resource belongs to Resource List is 1, 
	Delete WSR, 
	Add Resource, 
	Add WSR
*/

-- Setting up to check if the Resources being updated are System Resources
-- ResourceListID will be uniform throughout resources as these are set by the Workspace
DECLARE @IsSystem INT;
-- If ResourceListID = 1, it will be 0, else it will be 1
SELECT @IsSystem = SUM(CASE WHEN ResourceListID <> 1 THEN 1 ELSE 0 END)
FROM @temp t

-- then we do a sum of @IsSystem and if @IsSystem = 0, then we are working with System Resources
IF @IsSystem = 0
	BEGIN 
		DELETE wr
		FROM dbo.WorkspaceResource wr
			JOIN @temp t ON wr.ResourceID = t.ResourceID


		INSERT INTO [dbo].[Resource]
			([ResourceName]
			,[ResourceDescription]
			,[SegmentRegion]
			,[LaborType]
			,[SegmentID]
			,[ResourceListID]
			,[CostElementID]
			,[UpdateDT]
			,[RateTypeID]
			)
		OUTPUT inserted.ResourceID INTO @InsertedSystemResource
		SELECT ResourceName
			,ResourceDescription
			,SegmentRegion
			,LaborType
			,SegmentID
			,ResourceListID
			,CostElementID
			,@UpdateDT
			,RateTypeID
		FROM @WorkspaceResourceTableParameter


		INSERT INTO [dbo].[WorkspaceResource]
			([SystemResourceID]
			,[ResourceListID]
			,[WorkspaceID])
		SELECT ResourceID
			,ResourceListID
			,WorkspaceID
		FROM @InsertedSystemResource


		UPDATE dbo.BOELaborType
		SET lt.ResourceID = isr.ResourceID
		FROM dbo.BOELaborType lt
			JOIN @InsertedSystemResource iSr ON lt.ResourceID = isr.ResourceID AND lt.ResourceListID = isr.ResourceListID
			JOIN dbo.BOETaskElement te ON lt.BOETaskElementID = te.BOETaskElementID
			JOIN dbo.BOE b ON te.BOEID = b.BOEID
		WHERE lt.ResourceID = isr.ResourceID AND b.WorkspaceID = isr.WorkspaceID
			

		UPDATE dbo.ODCType
		SET ot.ResourceID = isr.ResourceID
		FROM dbo.ODCType ot
			JOIN @InsertedSystemResource isr ON ot.ResourceID = isr.ResourceID AND ot.ResourceListID = isr.ResourceListID
			JOIN dbo.ODCTaskElement te ON ot.ODCTaskElementID = te.ODCTaskElementID
			JOIN dbo.BOE b ON te.BOEID = b.BOEID
		WHERE ot.ResourceID = isr.ResourceID AND b.WorkspaceID = isr.WorkspaceID
	END

ELSE -- All ResourceIDList > 1
	BEGIN
		IF NOT EXISTS (
			SELECT *
			FROM dbo.[Resource] r
				JOIN @WorkspaceResourceTableParameter wr ON wr.ResourceID = r.SystemResourceID
			WHERE r.UpdateDT <> @UpdateDT
		)
		BEGIN
			DECLARE @ResourceListIDParam TABLE (ResourceListID int)
			INSERT INTO @SystemResourceListIDParam
			SELECT ResourceListID
			FROM @WorkspaceResourceTableParameter

			DECLARE @temp TABLE (SystemResourceID int)
			INSERT @temp EXECUTE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter] @ResourceListIDParam

			-- Create a table to set InUse Flag
			DECLARE @InUseTable TABLE (
				ResourceID INT NOT NULL,
				WorkspaceID INT NOT NULL,
				ResourceListID INT,
				InUse BIT NOT NULL
			)
			INSERT INTO @InUseTable (ResourceID, WorkspaceID, InUse)
			SELECT 
				ISNULL(wrp.ResourceID, t.ResourceID) AS ResourceID,
				ISNULL(wrp.WorkspaceID, t.WorkspaceID) AS WorkspaceID,
				wrp.ResourceListID,
				CASE 
					WHEN wrp.ResourceID IS NULL THEN 0
					ELSE 1
				END AS InUse
			FROM 
				@WorkspaceResourceTableParameter AS wrp
				FULL OUTER JOIN @temp AS t ON wrp.ResourceID = t.SystemResourceID AND wrp.WorkspaceID = t.WorkspaceID
			WHERE 
				t.SystemResourceID IS NULL

			/* Updates for those that are InUse */
			UPDATE r
			SET r.ResourceDescription = wr.ResourceDescription
				,r.SegmentRegion = wr.SegmentRegion
				,r.LaborType = wr.LaborType
				,r.ResourceListID = wr.ResourceListID
				,r.UpdateDt = @UpdateDT
				,r.RateTypeID = wr.RateTypeID
			FROM [dbo].[Resource] r
				JOIN @WorkspaceResourceTableParameter wr ON r.ResourceID = wr.ResourceID AND r.ResourceListID = wr.ResourceListId
				JOIN @InUseTable i ON wr.ResourceID = i.ResourceID
			WHERE 
				wr.ResourceListID > 1
				AND i.InUse = 1

			/* Updates for those that are NOT InUse */
			UPDATE r
			SET r.ResourceName = wr.ResourceName
				,r.ResourceDescription = wr.ResourceDescription
				,r.SegmentRegion = wr.SegmentRegion
				,r.LaborType = wr.LaborType
				,r.SegmentID = wr.SegmentID
				,r.ResourceListID = wr.ResourceListID
				,r.CostElementID = wr.CostElementID
				,r.UpdateDt = @UpdateDT
				,r.RateTypeID = wr.RateTypeID
			FROM [dbo].[Resource] r
				JOIN @WorkspaceResourceTableParameter wr ON r.ResourceID = wr.ResourceID AND r.ResourceListID = wr.ResourceListId
				JOIN @InUseTable i ON wr.ResourceID = i.ResourceID
			WHERE 
				wr.ResourceListID > 1
				AND i.InUse = 0
		END
		ELSE
			BEGIN
				SET @ErrorMessage = 'There are resources that have been updated and is out of sync with the data in your browser. Please refresh your data.'
				RAISERROR (
					@ErrorMessage,
					11,
					1
				)
				RETURN
			END
		END
IF @@ERROR = 0
	SELECT ResourceID FROM @WorkspaceResourceTableParameter
GO


