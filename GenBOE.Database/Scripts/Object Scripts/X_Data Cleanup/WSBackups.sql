-- Remove backups older than 6 months
DECLARE @date DATETIME = DATEADD(d, -180, getdate());
DECLARE @i INT;
DECLARE @wsId INT;
DECLARE @vId INT;
DECLARE @msg VARCHAR(100);

SELECT @i = COUNT(*)
		FROM version.Workspace
		WHERE WorkspaceId IN (
			SELECT DISTINCT(WorkspaceId)
				FROM WorkspaceStateHistory o
				WHERE o.UpdatedWorkspaceStateID >= 3
					AND EXISTS (SELECT 1
									FROM Workspace w
									WHERE w.WorkspaceID = o.WorkspaceID and w.WorkspaceStateID >= 3)
					AND o.UpdateDT IN (SELECT MAX(x.UpdateDT) 
										FROM WorkspaceStateHistory x 
										WHERE o.WorkspaceID = x.WorkspaceID)
					AND o.UpdateDT < @date);
SELECT @msg = @i; 
RAISERROR(@msg, 0, 1) WITH NOWAIT;

SELECT @i = 0;
DECLARE c CURSOR for
    SELECT WorkspaceId, VersionId
		FROM version.Workspace
		WHERE WorkspaceId IN (
			SELECT DISTINCT(WorkspaceId)
				FROM WorkspaceStateHistory o
				WHERE o.UpdatedWorkspaceStateID >= 3
					AND EXISTS (SELECT 1
									FROM Workspace w
									WHERE w.WorkspaceID = o.WorkspaceID and w.WorkspaceStateID >= 3)
					AND o.UpdateDT IN (SELECT MAX(x.UpdateDT) 
										FROM WorkspaceStateHistory x 
										WHERE o.WorkspaceID = x.WorkspaceID)
					AND o.UpdateDT < @date);
OPEN c;
FETCH NEXT FROM c INTO @wsId, @vId;
WHILE @@FETCH_STATUS = 0 BEGIN
    EXEC [dbo].[deleteWorkspaceVersion] @wsId, @vId;
	
	SELECT @msg = 'FINISHED #' + CONVERT(VARCHAR, @i) + ' WorkspaceId: ' + CONVERT(VARCHAR, @wsId) + ', Version Id: ' + CONVERT(VARCHAR, @vId);
	RAISERROR(@msg, 0, 1) WITH NOWAIT;
	SET @i = @i + 1;
FETCH NEXT FROM c INTO @wsId, @vId
END;
CLOSE c;
DEALLOCATE c;
SELECT @msg = 'FINISHED'; RAISERROR(@msg, 0, 1) WITH NOWAIT;
GO