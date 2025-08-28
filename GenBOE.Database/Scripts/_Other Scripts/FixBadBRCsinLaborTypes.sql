/*
    This file fixes the issue when BRCs are linked to the wrong Workspace's Resources.  It finds the correct Resource IDs and relinks them inside the BOE Labor Type.
	Only run if there are errors found with FindBadBRCsinLaborTypes.sql
*/
BEGIN TRANSACTION

BEGIN TRY

DECLARE @Resource TABLE
(
	[BOELaborTypeId] int NOT NULL,
	[WorkspaceId] int NOT NULL,
	[ResourceId] int not null,
	Resourcename varchar(20) NOT NULL,
	[NewResourceID] [int] NULL,
	Processed bit DEFAULT 0
)

INSERT INTO @Resource
           ([BOELaborTypeId], [WorkspaceId], [ResourceID], Resourcename, Processed)
SELECT lt.boelabortypeid, b.workspaceid,  lt.[BRCResourceID], r.Resourcename, 0 AS Processed
FROM [dbo].boelabortype lt 
  INNER JOIN boetaskelement t ON t.BOETaskElementID = lt.BOETaskElementID
  INNER JOIN BOE b on t.BOEID = b.BOEID
  INNER JOIN [Resource] r on r.ResourceID = lt.BRCResourceID
  INNER JOIN Workspace w on w.ResourceListID = r.ResourceListID
where w.workspaceid != b.workspaceid

DECLARE @ResourceID int
DECLARE @Resourcename varchar(20)
DECLARE @NewResourceID int
DECLARE @WorkspaceID int
DECLARE @BOELaborTypeId int
WHILE EXISTS (SELECT 1 FROM @Resource WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @BOELaborTypeId = BOELaborTypeId, @ResourceID = ResourceID, @Resourcename = Resourcename, @WorkspaceID = WorkspaceID FROM @Resource WHERE Processed = 0

	-- find the corresponding performing org in the correct workspace
	SELECT TOP 1 @NewResourceID = r.ResourceID 
	FROM [Resource] r 
	INNER JOIN Workspace w on r.ResourceListID = w.ResourceListID
	WHERE r.Resourcename = @Resourcename AND w.workspaceid = @WorkspaceID

	-- update all combos of bad BRC Resource/good workspaceid with the good BRC Resource
	UPDATE @Resource SET [NewResourceID] = @NewResourceID, Processed = 1 WHERE ResourceID = @ResourceID AND WorkspaceID = @WorkspaceID

END

-- now do the update on the real table
UPDATE [dbo].[boelabortype]
		SET [BRCResourceID] = r.NewResourceID
		FROM  [boelabortype] lt INNER JOIN @Resource r ON lt.boelabortypeid = r.boelabortypeid
		WHERE r.NewResourceID IS NOT NULL

IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END

END TRY


BEGIN CATCH
	ROLLBACK TRANSACTION
	

	DECLARE @ErrorMessage varchar (500)
	SELECT @ErrorMessage = ERROR_MESSAGE()
	RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)

	RETURN
	
END CATCH
GO