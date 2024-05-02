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
SELECT WorkspaceID, ResourceListID 
FROM dbo.Workspace w
    INNER JOIN @ResourceListIDParameter r ON r.ResourceListID = w.ResourceListID


-- temp BOE table to get all the BOE IDs associated with the workspace id from the @Workspace temp table
DECLARE @BOE TABLE
(
BOEID int PRIMARY KEY,
WorkspaceID int
)
INSERT INTO @BOE
SELECT B.BOEID, B.WorkspaceID 
FROM dbo.BOE B
    INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID


-- What will be returned at the end of this procedure
DECLARE @ResultSet TABLE (SystemResourceID int)


--UNION
INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT  T.ResourceID 
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
