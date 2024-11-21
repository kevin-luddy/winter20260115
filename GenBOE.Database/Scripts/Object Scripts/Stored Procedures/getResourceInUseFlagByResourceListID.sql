IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getResourceInUseFlagByResourceListID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getResourceInUseFlagByResourceListID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getResourceInUseFlagByResourceListID]
(
@ResourceListID int
)
AS
/******************************************************************************
**          
**          Name: [getResourceInUseFlagByResourceListID]
**          Desc: Returns Resource In Use Flag
**                
**
**          Auth: Don Canuso
**          Date: 7/25/2012
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
**          11/30/12    dcanuso                 WI 13466 to Fix Performance
**          4/9/13      dcanuso                 BUG 17425  WRR DOES NOT MAKE IN USE
**			11/9/16		tglick					Added rms non zone travel resources
**			5/4/2017	brunworg				BOEJ-2125 Add T&M Resource Rates
**			5/24/2017	brunworg				BOEJ-2125 Remove T&M Resource 
**                                              Rates, they don't make in use.
**			12/15/17	twilson3				BOEJ-2248 Remove Labor Rates
**		    6/25/19		twilson3			    BOEJ-3964 - Remove in-use flag, MaterialXref
**          4/3/24      e302876                 PROPH-1617 - update stored procs for BRC
*******************************************************************************/
SET NOCOUNT ON 
      
IF @ResourceListID >1  /*Not the Default List Resource*/
BEGIN

DECLARE @Workspace TABLE (WorkspaceID INT, ResourceListID int) 
INSERT INTO @Workspace
SELECT WorkspaceID, ResourceListID 
FROM dbo.Workspace 
WHERE ResourceListID = @ResourceListID



DECLARE @BOE TABLE
(
BOEID int PRIMARY KEY,
WorkspaceID int
)
INSERT INTO @BOE
SELECT B.BOEID, B.WorkspaceID 
FROM dbo.BOE B
      INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID


/*
DECLARE @WorkspaceResource TABLE 
(SystemResourceID int,ResourceListID int, WorkspaceID int)
INSERT INTO @WorkspaceResource
SELECT WR.[SystemResourceID]
      ,WR.[ResourceListID]
      ,WR.[WorkspaceID]
FROM [dbo].[WorkspaceResource] WR
      INNER JOIN @Workspace W ON WR.WorkspaceID = W.WorkspaceID
*/

DECLARE @ResultSet TABLE (SystemResourceID int)


--UNION
INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT  T.ResourceID 
FROM  [dbo].[ODCType] T
            INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID  
            INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
            INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID
            /*
            INNER JOIN @WorkspaceResource WR 
                  ON    W.WorkspaceID = WR.WorkspaceID AND
                        T.ResourceID = WR.SystemResourceID
WHERE 
WR.ResourceListID = @ResourceListID 
*/
--UNION

INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT T.ResourceID 
FROM  [dbo].[BOELaborType] T
            INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
            INNER JOIN @BOE B ON TE.BOEID = B.BOEID
            INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE T.ResourceID IS NOT NULL
            /*Regardless of whether it is in this table, the fact that is is
            used in your BOE is what is relevant so removing this
            INNER JOIN @WorkspaceResource WR 
                  ON    W.WorkspaceID = WR.WorkspaceID AND
                        T.ResourceID = WR.SystemResourceID
WHERE 
WR.ResourceListID = @ResourceListID 
*/

INSERT INTO @ResultSet (SystemResourceID)
SELECT DISTINCT T.BRCResourceID 
FROM  [dbo].[BOELaborType] T
            INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
            INNER JOIN @BOE B ON TE.BOEID = B.BOEID
            INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID
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
      R.CostElementID = 6 /*Travel*/ AND
      WR.ResourceListID = @ResourceListID 

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
	  TT.NonZoneResourceID = R.ResourceID AND
      WR.ResourceListID = @ResourceListID 
-- end added for rms travel nonzoneresourceid's 


      SELECT DISTINCT SystemResourceID FROM  @ResultSet 
      WHERE SystemResourceID IS NOT NULL
      
END


GO
