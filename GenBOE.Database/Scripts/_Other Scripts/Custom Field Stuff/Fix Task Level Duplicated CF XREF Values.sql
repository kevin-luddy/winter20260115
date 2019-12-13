-- execution.. we want to skip the first Custom Field Value XREF (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @xrefId INT, @cfValueIdToProcess INT, @previousCfValueId INT, @TaskId INT, @previousTaskId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT DISTINCT x.btecfvid, x.BOETaskElementID, x.customfieldvalueid FROM BOETaskElementCustomFieldValueXREF x
			INNER JOIN
				(SELECT BOETaskElementID, CustomFieldValueID FROM
				BOETaskElementCustomFieldValueXREF
				GROUP BY BOETaskElementID, CustomFieldValueID
				HAVING COUNT(*) > 1) g
			ON x.BOETaskElementID = g.BOETaskElementID AND x.CustomFieldValueID = g.CustomFieldValueID
			ORDER BY x.BOETaskElementID, x.customfieldvalueid DESC

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @xrefId, @TaskId, @cfValueIdToProcess;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		SET @previousTaskId = @TaskId;
		IF @count > 0
			DELETE FROM BOETaskElementCustomFieldValueXREF WHERE btecfvid = @xrefId;

		FETCH NEXT FROM @myCursor INTO @xrefId, @TaskId, @cfValueIdToProcess;

		IF @cfValueIdToProcess <> @previousCfValueId OR @TaskId <> @previousTaskId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO

-- for the 2nd pass, we need to fix picks where the values are not the same, but still duplicates are selected
DECLARE @myCursor CURSOR, @taskId INT, @customFieldId INT, @maxDate DATETIME;

SET @myCursor = CURSOR FOR 
			select BOETaskElementID, CustomFieldID, MAX(x.UpdateDT) from
			BOETaskElementCustomFieldValueXREF x
					INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
			group by BOETaskElementID, CustomFieldID
			HAVING COUNT(*) > 1;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @taskId, @customFieldId, @maxDate;

WHILE @@FETCH_STATUS = 0
	BEGIN
		DELETE
		 FROM BOETaskElementCustomFieldValueXREF 
			WHERE BOETaskElementID = @taskId
			AND CustomFieldValueID IN (SELECT CustomFieldValueID FROM CustomFieldValue WHERE CustomFieldID = @customFieldId) 
			AND UpdateDT != @maxDate;
		FETCH NEXT FROM @myCursor INTO @taskId, @customFieldId, @maxDate;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO