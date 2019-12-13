DECLARE @numberOfBadRecords INT;
SELECT @numberOfBadRecords = SUM(Count) FROM (
SELECT -1 * COUNT(1) AS Count FROM (
SELECT 
	DISTINCT BOETaskElementId, OrdinaryVariableName
	FROM OrdinaryVariable) x
UNION
SELECT COUNT(1) AS Count FROM (
SELECT 
	BOETaskElementId, OrdinaryVariableName
	FROM OrdinaryVariable) y
)z;
PRINT 'Number of ordinary variable records to fix: ' + CONVERT(VARCHAR(10), @numberOfBadRecords);


-- execution.. we want to skip the first copy and then trash the rest
DECLARE @myCursor CURSOR, 
@id INT, -- ids of items that we'll be checking
@BOETaskElementId INT, @previousBOETaskElementId INT, -- matching condition 1
@OrdinaryVariableName VARCHAR(50), @previousOrdinaryVariableName VARCHAR(50), -- matching condition 2
@count INT = 0, @overallCount INT = -1;

SET @myCursor = CURSOR FOR 
SELECT OrdinaryVariableID, BOETaskElementId, OrdinaryVariableName
	FROM OrdinaryVariable
	WHERE BOETaskElementId IN (
			SELECT BOETaskElementId FROM OrdinaryVariable GROUP BY BOETaskElementId, OrdinaryVariableName
				HAVING COUNT(*) > 1)
	ORDER BY OrdinaryVariableName, BOETaskElementId;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @id, @BOETaskElementId, @OrdinaryVariableName;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousBOETaskElementId = @BOETaskElementId;
		SET @previousOrdinaryVariableName = @OrdinaryVariableName;
		IF @count > 0
			DELETE FROM OrdinaryVariable WHERE OrdinaryVariableID = @id;

		FETCH NEXT FROM @myCursor INTO @id, @BOETaskElementId, @OrdinaryVariableName;

		IF @previousBOETaskElementId <> @BOETaskElementId OR @previousOrdinaryVariableName <> @OrdinaryVariableName SET @count = 0		
		ELSE BEGIN SET @count += 1; SET @overallCount += 1; END
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'Number of ordinary variable records fixed: ' + CONVERT(VARCHAR(10), @overallCount);
IF @numberOfBadRecords <> @overallCount
	PRINT '!!!! ERROR - didn'' fix all of the bad ordinary variables !!!!';
PRINT '!! DONE !!';
GO
