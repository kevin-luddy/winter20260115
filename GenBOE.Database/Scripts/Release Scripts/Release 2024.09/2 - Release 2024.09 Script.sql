IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getResourceInUseFlagByResourceListIDviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter];

GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ResourceListID' AND ss.name = N'dbo')
    DROP TYPE [dbo].[TT_ResourceListID];
GO

CREATE TYPE [dbo].[TT_ResourceListID] AS TABLE(
    [ResourceListID] [int] NULL
);
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter]
(
@ResourceListIDParameter [dbo].[TT_ResourceListID] READONLY
)
AS
/******************************************************************************
**          
**          Name: [getResourceInUseFlagByResourceListIDviaTableParameter]
**          Desc: Returns Resource In Use Flag
**                
**
**          Auth: Tommy Lee
**          Date: 4/29/2024
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
**          04/29/24    e374897                 Returns resource in use flag by resource list id
*******************************************************************************/
SET NOCOUNT ON 

-- temp table to hold all the workspace involved with the worklist id
DECLARE @Workspace TABLE (WorkspaceID INT, ResourceListID int) 
INSERT INTO @Workspace
SELECT w.WorkspaceID, w.ResourceListID 
FROM dbo.Workspace w
    INNER JOIN @ResourceListIDParameter r ON r.ResourceListID = w.ResourceListID


-- temp BOE table to get all the BOE IDs associated with the workspace id from the @Workspace temp table
DECLARE @BOE TABLE
(
BOEID int PRIMARY KEY,
WorkspaceID int
)
INSERT INTO @BOE
SELECT DISTINCT B.BOEID, B.WorkspaceID 
FROM dbo.BOE B
    INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID


-- What will be returned at the end of this procedure
DECLARE @ResultSet TABLE (SystemResourceID int)


--UNION
INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT T.ResourceID 
FROM  [dbo].[ODCType] T
            INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID  
            INNER JOIN @BOE B ON TE.BOEID = B.BOEID

INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT T.ResourceID 
FROM  [dbo].[BOELaborType] T
            INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
            INNER JOIN @BOE B ON TE.BOEID = B.BOEID
WHERE T.ResourceID IS NOT NULL

INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT T.BRCResourceID 
FROM  [dbo].[BOELaborType] T
            INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
            INNER JOIN @BOE B ON TE.BOEID = B.BOEID
WHERE T.BRCResourceID IS NOT NULL


--UNION

INSERT INTO @ResultSet (SystemResourceID)
            /* Special Processing for Travel */
            SELECT DISTINCT  R.ResourceID
            FROM [dbo].[Resource] R
            INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
            INNER JOIN @Workspace W ON WR.WorkspaceID = W.WorkspaceID
            INNER JOIN 
                  (
                        SELECT BOEID, WorkspaceID
                        FROM dbo.BOE
                  ) B ON W.WorkspaceID = B.WorkspaceID
            INNER JOIN 
                  (
                        SELECT [TravelTripTaskElementID]
                        ,[BOEID]
                        FROM dbo.TravelTripTaskElement
                  ) TE ON B.BOEID = TE.BOEID
            INNER JOIN 
                  (
                        SELECT [TravelTripID]
                        ,[SegmentID]
                        ,[TravelTripTaskElementID]
                        FROM dbo.TravelTrip
            ) TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID 
      WHERE 
      
      /*
      Removing on 11/30/12 0 does not make
      sense to only have this here 
      W.WorkspaceStateID NOT IN (4,5)/*Closed/Complete*/      AND                                 
      */
      R.SegmentID = TT.SegmentID AND
      R.CostElementID = 6 /*Travel*/
      /*
      WR.ResourceListID = @ResourceListID 
      is removed because the INNER JOIN @Workspace will already have the correct ResourceListID
      So no need for INNER JOIN @ResourceListIDParameter ON ResourceListID
      */


-- UNION for INL Forms IBOE
INSERT INTO @ResultSet (SystemResourceID)
            SELECT DISTINCT  X.ResourceID
            FROM [dbo].[Resource] R
            INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
            INNER JOIN @Workspace W ON WR.WorkspaceID = W.WorkspaceID
			INNER JOIN [dbo].[BOEFormIBOE] I ON I.WorkspaceID = W.WorkspaceID
			INNER JOIN [dbo].[BOEFormIBOEResourcesXREF] X on X.[IBOEFormID] = I.[IBOEFormID]

-- UNION for INL Forms PBOE
INSERT INTO @ResultSet (SystemResourceID)
            SELECT DISTINCT  X.ResourceID
            FROM [dbo].[Resource] R
            INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
            INNER JOIN @Workspace W ON WR.WorkspaceID = W.WorkspaceID
			INNER JOIN [dbo].[BOEFormPBOE] P ON P.WorkspaceID = W.WorkspaceID
			INNER JOIN [dbo].[BOEFormPBOEResourcesXREF] X on X.[PBOEFormID] = P.[PBOEFormID]

--for nonzone rms travel trips, add nonzoneresourceids  
INSERT INTO @ResultSet (SystemResourceID)
    SELECT DISTINCT  R.ResourceID FROM [dbo].[Resource] R
    INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
    INNER JOIN @Workspace W ON WR.resourcelistid = W.resourcelistid
    INNER JOIN dbo.BOE B ON W.WorkspaceID = B.WorkspaceID
    INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
    INNER JOIN dbo.MstTravelTrip TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID 
WHERE 
R.SegmentID = TT.SegmentID AND
R.CostElementID = 6 /*Travel*/ AND
TT.NonZoneResourceID = R.ResourceID
/*
    WR.ResourceListID = @ResourceListID 
    is removed because the INNER JOIN @Workspace will already have the correct ResourceListID
    So no need for INNER JOIN @ResourceListIDParameter ON ResourceListID
*/
-- end added for rms travel nonzoneresourceid's 


SELECT DISTINCT SystemResourceID FROM  @ResultSet 
WHERE SystemResourceID IS NOT NULL

-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceResourceviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceResourceviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertWorkspaceResourceviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertWorkspaceResourceviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceResourceviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceResourceviaTableParameter];
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
	[RateTypeID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertWorkspaceResourceviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
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
	SELECT ResourceID, @UpdateDT FROM @InsertedResource

GO
CREATE PROCEDURE [dbo].[deleteWorkspaceResourceviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceResourceviaTableParameter]
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

DECLARE @ResourceListIDParam TT_ResourceListID
INSERT INTO @ResourceListIDParam
SELECT DISTINCT ResourceListID
FROM @WorkspaceResourceTableParameter

DECLARE @temp TABLE (ResourceID int)
-- This will return a list of all system resource ids that are in use
INSERT @temp EXECUTE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter] @ResourceListIDParam


-- Create a table to set InUse Flag
DECLARE @InUseTable TABLE (
	ResourceID INT NULL,
	ResourceListID INT,
	InUse BIT NOT NULL
)
INSERT INTO @InUseTable (ResourceID, ResourceListID, InUse)
SELECT 
	ISNULL(wrp.ResourceID, t.ResourceID) AS ResourceID,
	wrp.ResourceListID,
	CASE 
		WHEN t.ResourceID IS NULL THEN 0
		ELSE 1
	END AS InUse
FROM @WorkspaceResourceTableParameter AS wrp
	FULL OUTER JOIN @temp AS t ON wrp.ResourceID = t.ResourceID

-- Delete all the non InUse ResourceID

DELETE dbo.TMResourceRate
FROM dbo.TMResourceRate tmr
	JOIN @InUseTable i ON i.ResourceID = tmr.TMResourceID
	JOIN dbo.Workspace w ON w.ResourceListID = i.ResourceListID
WHERE
	i.InUse = 0 AND
	tmr.WorkspaceID = w.WorkspaceID

DELETE dbo.WorkspaceResource 
FROM dbo.WorkspaceResource wr
	JOIN @InUseTable i ON i.ResourceID = wr.SystemResourceID AND wr.ResourceListID = i.ResourceListID
WHERE
	i.InUse = 0


DECLARE @SystemValueCheck INT;
-- If ResourceListID = 1, it will be 0, else it will be 1
-- So if @SystemValueCheck is > 0, then these are non System Resources
SELECT @SystemValueCheck = SUM(CASE WHEN ResourceListID <> 1 THEN 1 ELSE 0 END)
FROM @WorkspaceResourceTableParameter wrt
-- Checking if these are workspace resources as they are allowed to be deleted this way
IF @SystemValueCheck > 0
BEGIN
	DELETE dbo.Resource
	FROM dbo.Resource r
		JOIN @InUseTable i on i.ResourceID = r.ResourceID AND i.ResourceListID = r.ResourceListID
	WHERE 
		-- equivalent to r.ResourceListID != 1
		r.ResourceListID <> 1
	AND i.InUse = 0
END

GO
CREATE PROCEDURE [dbo].[updateWorkspaceResourceviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_WorkspaceResource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: updateWorkspaceResourceviaTableParameter
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
DECLARE @InsertedSystemResource AS Table (ResourceID int, ResourceListID int)

DECLARE @WorkspaceTemp TABLE (ResourceID int, ResourceListID int, WorkspaceID int)
INSERT INTO @WorkspaceTemp (ResourceID, ResourceListID, WorkspaceID)
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
DECLARE @SystemValueCheck INT;
-- If ResourceListID = 1, it will be 0, else it will be 1
-- So if @SystemValueCheck is > 0, then these are non System Resources
SELECT @SystemValueCheck = SUM(CASE WHEN ResourceListID <> 1 THEN 1 ELSE 0 END)
FROM @WorkspaceTemp wt

-- then we do a sum of @SystemValueCheck and if @SystemValueCheck = 0, then we are working with System Resources
IF @SystemValueCheck = 0
	BEGIN 
		DELETE wr
		FROM dbo.WorkspaceResource wr
			JOIN @WorkspaceTemp wt ON wr.SystemResourceID = wt.ResourceID


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
		OUTPUT inserted.ResourceID, inserted.ResourceListID INTO @InsertedSystemResource
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
		SELECT isr.ResourceID
			,isr.ResourceListID
			,wt.WorkspaceID
		FROM @InsertedSystemResource isr
		JOIN @WorkspaceTemp wt ON isr.ResourceID = wt.ResourceID AND isr.ResourceListID = wt.ResourceListID


		UPDATE dbo.BOELaborType
		SET ResourceID = isr.ResourceID
		FROM dbo.BOELaborType lt
			JOIN @InsertedSystemResource iSr ON lt.ResourceID = isr.ResourceID
			JOIN @WorkspaceTemp wt ON isr.ResourceID = wt.ResourceID AND isr.ResourceListID = wt.ResourceListID
			JOIN dbo.BOETaskElement te ON lt.BOETaskElementID = te.BOETaskElementID
			JOIN dbo.BOE b ON te.BOEID = b.BOEID
		WHERE lt.ResourceID = isr.ResourceID AND b.WorkspaceID = wt.WorkspaceID
			

		UPDATE dbo.ODCType
		SET ResourceID = isr.ResourceID
		FROM dbo.ODCType ot
			JOIN @InsertedSystemResource isr ON ot.ResourceID = isr.ResourceID
			JOIN @WorkspaceTemp wt ON isr.ResourceID = wt.ResourceID AND isr.ResourceListID = wt.ResourceListID
			JOIN dbo.ODCTaskElement te ON ot.ODCTaskElementID = te.ODCTaskElementID
			JOIN dbo.BOE b ON te.BOEID = b.BOEID
		WHERE ot.ResourceID = isr.ResourceID AND b.WorkspaceID = wt.WorkspaceID
	END

ELSE -- All ResourceIDList > 1
	BEGIN
		IF NOT EXISTS (
			SELECT *
			FROM dbo.[Resource] r
				JOIN @WorkspaceResourceTableParameter wr ON wr.ResourceID = r.ResourceID
			WHERE wr.UpdateDT <> r.UpdateDT
		)
		BEGIN
			DECLARE @ResourceListIDParam TT_ResourceListID
			INSERT INTO @ResourceListIDParam
			SELECT ResourceListID
			FROM @WorkspaceResourceTableParameter

			DECLARE @SystemTemp TABLE (ResourceID int)
			INSERT @SystemTemp EXECUTE [dbo].[getResourceInUseFlagByResourceListIDviaTableParameter] @ResourceListIDParam

			-- Create a table to set InUse Flag
			DECLARE @InUseTable TABLE (
				ResourceID INT NULL,
				ResourceListID INT,
				InUse BIT NOT NULL
			)
			INSERT INTO @InUseTable (ResourceID, ResourceListID, InUse)
			SELECT 
				ISNULL(wrp.ResourceID, t.ResourceID) AS ResourceID,
				wrp.ResourceListID,
				CASE 
					WHEN t.ResourceID IS NULL THEN 0
					ELSE 1
				END AS InUse
			FROM @WorkspaceResourceTableParameter AS wrp
				FULL OUTER JOIN @SystemTemp AS t ON wrp.ResourceID = t.ResourceID

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
	SELECT ResourceID, @UpdateDT FROM @WorkspaceResourceTableParameter
GO
