-- execution.. we want to skip the first Custom Field Value XREF (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @xrefId INT, @cfValueIdToProcess INT, @previousCfValueId INT, @BoeId INT, @previousBoeId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT DISTINCT x.bcfvid, x.boeid, x.customfieldvalueid FROM BOECustomFieldValueXREF x
			INNER JOIN
				(SELECT BOEID, CustomFieldValueID FROM
				BOECustomFieldValueXREF
				GROUP BY BOEID, CustomFieldValueID
				HAVING COUNT(*) > 1) g
			ON x.BOEID = g.BOEID AND x.CustomFieldValueID = g.CustomFieldValueID
			ORDER BY x.boeid, x.customfieldvalueid DESC

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @xrefId, @BoeId, @cfValueIdToProcess;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		SET @previousBoeId = @BoeId;
		IF @count > 0
			DELETE FROM BOECustomFieldValueXREF WHERE bcfvid = @xrefId;

		FETCH NEXT FROM @myCursor INTO @xrefId, @BoeId, @cfValueIdToProcess;

		IF @cfValueIdToProcess <> @previousCfValueId OR @BoeId <> @previousBoeId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO

-- for the 2nd pass, we need to fix picks where the values are not the same, but still duplicates are selected
DECLARE @myCursor CURSOR, @boeId INT, @customFieldId INT, @maxDate DATETIME;
SET @myCursor = CURSOR FOR 
	select BOEID, CustomFieldID, MAX(x.UpdateDT)
		from BOECustomFieldValueXREF x
			INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
		group by BOEID, CustomFieldID
		HAVING COUNT(*) > 1;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @BoeId, @customFieldId, @maxDate;

WHILE @@FETCH_STATUS = 0
	BEGIN
		DELETE
		 FROM BOECustomFieldValueXREF 
			WHERE BOEID = @boeId 
			AND CustomFieldValueID IN (SELECT CustomFieldValueID FROM CustomFieldValue WHERE CustomFieldID = @customFieldId) 
			AND UpdateDT != @maxDate;
		FETCH NEXT FROM @myCursor INTO @BoeId, @customFieldId, @maxDate;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO
