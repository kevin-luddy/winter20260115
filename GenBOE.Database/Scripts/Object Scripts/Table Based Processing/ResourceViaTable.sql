-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteResourcetviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteResourcetviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertResourceviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertResourceviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceResourcetviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceResourcetviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_OrdinaryVariable' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_Resource];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_Resource] AS TABLE(
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

CREATE PROCEDURE [dbo].[insertResourceviaTableParameter]
(
@ResourceTableParameter [dbo].[TT_Resource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertWorkspaceResource
**		Desc: Insert/Update into Resource Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/4/11		dcanuso				Added Flag Processing:
**										Add: Workspace.ResourceChangeFlag
**										IF SA Changes a Global Resource – Set All Workspace.ResourceChangeFlag = 1
**										IF WS Admin changes a Workspace Resource, set Workspace.ResourceChangeFlag = 1  
**										for specific workspace
**										Added Resource In Use Flag		
**		3/23/11		dcanuso				Added Error when editing In Use row	
**		5/23/11		dcanuso				Added 2 new variables
**		6/2/11		dcanuso				Burden Pool removed
**		6/13/11		dcanuso				New Resource Table columns added
**		8/2/11		dcanuso				WI 4436: remove the column Company 
**										from the Resource table and any SPs.
**		8/2/11		dcanuso				WI TBD: remove the column Labor Category 
**										from the Resource table and any SPs.
**		9/15/11		dcanuso				Resource.ResourceName and Segment Region
**										updated to varchar (30)
**		9/27/11		dcanuso				Resource.ResourceDescription, Segment Region,
**										Labor Type updated to varchar (50)
**		2/7/2012	dcanuso				WI 7145 - Resource Update allowed
**										When In Use, ResourceName and CostElementID 
**										can not be updated
**										When NOT in use, all can be updated
**										InUse Updates
**		7/24/12		dcanuso				WI8960: SP will be used for non-System Resources
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		11/1/12		dcanuso				Meetings with SE caused some Business 
**										Rule changes - Rqmt doc will be created
**		1/28/13		dcanuso				WI 14779 Remove Workspace.ResourceChangeFlag
**		7/25/13		dcanuso				WI 19369 Changes for Labor Tab
**      1/25/16     kotwickm			Updating desc to 100 for SSC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
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
CREATE PROCEDURE [dbo].[deleteResourcetviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_Resource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteResourceByResourceID]
**		Desc: Delete Resource based on its ResourceID
**			
**
**		Auth: Don Canuso
**		Date: 1/24/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/3/11		dcanuso				Adding addition WHERE to only allow
**										deletions of non-InUse Resource
**		3/23/11		dcanuso				Added Flag Processing:
**										Add: Workspace.ResourceChangeFlag
**										IF SA Changes a Global Resource – Set All Workspace.ResourceChangeFlag = 1
**										IF WS Admin changes a Workspace Resource, set Workspace.ResourceChangeFlag = 1  
**										for specific workspace
**										Added Resource In Use Flag			
**										Added Error if editing In Use row
**		11/30/11	dcanuso				See Note Below
**		1/18/12		dcanuso				Updates to WRR/LRR/Locking
**		2/7/12		dcanuso				Additional InUse Changes
**		7/24/12		dcanuso				WI8960
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/5/13		dcanuso				WI 14477
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
******************************************************************************/

/*
11/30/11 Note from Wireframe:
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

DECLARE @temp TABLE (ResourceID int)
INSERT @temp EXECUTE [dbo].[getWorkspaceResourceInUseFlag]

-- Create a table to set InUse Flag for all resource ids
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
FULL OUTER JOIN 
	@temp AS t
ON 
	wrp.ResourceID = t.ResourceID
	AND wrp.WorkspaceID = t.WorkspaceID
WHERE 
	t.ResourceID IS NULL

-- Delete all the non InUse ResourceID

DELETE FROM dbo.TMResourceRate
WHERE
	TMResourceID IN (SELECT ResourceID FROM @InUseTable WHERE InUse = 0)
	AND
	WorkspaceID IN (SELECT WorkspaceID FROM @InUseTable WHERE InUse = 0)

DELETE FROM dbo.WorkspaceResource 
WHERE
	TMResourceID IN (SELECT ResourceID FROM @InUseTable WHERE InUse = 0)
	AND
	WorkspaceID IN (SELECT WorkspaceID FROM @InUseTable WHERE InUse = 0)


IF @ResourceType > 1
BEGIN
	DELETE FROM [dbo].[Resource] 
	WHERE 
	TMResourceID IN (SELECT ResourceID FROM @InUseTable WHERE InUse = 0)
	AND
	WorkspaceID IN (SELECT WorkspaceID FROM @InUseTable WHERE InUse = 0)
	AND
	ResourceListID IN (SELECT ResourceListID FROM @InUseTable WHERE InUse = 0)
END

GO
CREATE PROCEDURE [dbo].[updateWorkspaceResourcetviaTableParameter]
(
@WorkspaceResourceTableParameter [dbo].[TT_Resource] READONLY
)
AS
/******************************************************************************
**		 
**		Name: upsertWorkspaceResource
**		Desc: Insert/Update into Resource Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/4/11		dcanuso				Added Flag Processing:
**										Add: Workspace.ResourceChangeFlag
**										IF SA Changes a Global Resource – Set All Workspace.ResourceChangeFlag = 1
**										IF WS Admin changes a Workspace Resource, set Workspace.ResourceChangeFlag = 1  
**										for specific workspace
**										Added Resource In Use Flag		
**		3/23/11		dcanuso				Added Error when editing In Use row	
**		5/23/11		dcanuso				Added 2 new variables
**		6/2/11		dcanuso				Burden Pool removed
**		6/13/11		dcanuso				New Resource Table columns added
**		8/2/11		dcanuso				WI 4436: remove the column Company 
**										from the Resource table and any SPs.
**		8/2/11		dcanuso				WI TBD: remove the column Labor Category 
**										from the Resource table and any SPs.
**		9/15/11		dcanuso				Resource.ResourceName and Segment Region
**										updated to varchar (30)
**		9/27/11		dcanuso				Resource.ResourceDescription, Segment Region,
**										Labor Type updated to varchar (50)
**		2/7/2012	dcanuso				WI 7145 - Resource Update allowed
**										When In Use, ResourceName and CostElementID 
**										can not be updated
**										When NOT in use, all can be updated
**										InUse Updates
**		7/24/12		dcanuso				WI8960: SP will be used for non-System Resources
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		11/1/12		dcanuso				Meetings with SE caused some Business 
**										Rule changes - Rqmt doc will be created
**		1/28/13		dcanuso				WI 14779 Remove Workspace.ResourceChangeFlag
**		7/25/13		dcanuso				WI 19369 Changes for Labor Tab
**      1/25/16     kotwickm			Updating desc to 100 for SSC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2 = GETDATE()

DECLARE @InsertedResource AS Table (ResourceID int, ResourceListID int, WorkspaceID int)

DECLARE @temp TABLE (ResourceID int, ResourceListID int)

INSERT INTO @temp (ResourceID, ResourceListID, WorkspaceID)
SELECT r.ResourceID, r.ResourceListID, w.WorkspaceID FROM [dbo].[Resource] r
JOIN [dbo].[Workspace] w ON r.[ResourceListID] = w.[ResourceListID]


/*
	If Resource belongs to Resource List is 1, 
	Delete WSR, 
	Add Resource, 
	Add WSR
*/

DELETE wr
FROM dbo.WorkspaceResource wr
JOIN @temp t ON wr.ResourceID = t.ResourceID
WHERE wr.ResourceListID = 1


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
OUTPUT inserted.ResrouceID INTO @InsertedResource
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
FROM @InsertedResource


UPDATE lt
SET lt.ResourceID = ir.ResourceID
FROM dbo.BOELaborType lt
JOIN @InsertedResource ir ON lt.ResourceID = ir.ResourceID AND lt.ResourceListID = ir.ResourceListID
JOIN dbo.BOETaskElement te ON lt.BOETaskElementID = te.BOETaskElementID
JOIN dbo.BOE b ON te.BOEID = b.BOEID
WHERE lt.ResourceID = ir.ResourceID AND b.WorkspaceID = ir.WorkspaceID
			

UPDATE ot
SET ot.ResourceID = ir.ResourceID
FROM dbo.ODCType ot
JOIN @InsertedResource ir ON ot.ResourceID = ir.ResourceID AND ot.ResourceListID = ir.ResourceListID
JOIN dbo.ODCTaskElement te ON ot.ODCTaskElementID = te.ODCTaskElementID
JOIN dbo.BOE b ON te.BOEID = b.BOEID
WHERE ot.ResourceID = ir.ResourceID AND b.WorkspaceID = ir.WorkspaceID	

