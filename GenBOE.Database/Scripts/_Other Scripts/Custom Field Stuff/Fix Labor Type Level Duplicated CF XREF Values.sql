-- execution.. we want to skip the first Custom Field Value XREF (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @xrefId INT, @cfValueIdToProcess INT, @previousCfValueId INT, @LaborTypeId INT, @previousLaborTypeId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT DISTINCT x.bltcfvid, x.BOELaborTypeID, x.customfieldvalueid FROM BOELaborTypeCustomFieldValueXREF x
			INNER JOIN
				(SELECT BOELaborTypeID, CustomFieldValueID FROM
				BOELaborTypeCustomFieldValueXREF
				GROUP BY BOELaborTypeID, CustomFieldValueID
				HAVING COUNT(*) > 1) g
			ON x.BOELaborTypeID = g.BOELaborTypeID AND x.CustomFieldValueID = g.CustomFieldValueID
			ORDER BY x.BOELaborTypeID, x.customfieldvalueid DESC

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @xrefId, @LaborTypeId, @cfValueIdToProcess;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		SET @previousLaborTypeId = @LaborTypeId;
		IF @count > 0
			DELETE FROM BOELaborTypeCustomFieldValueXREF WHERE bltcfvid = @xrefId;

		FETCH NEXT FROM @myCursor INTO @xrefId, @LaborTypeId, @cfValueIdToProcess;

		IF @cfValueIdToProcess <> @previousCfValueId OR @LaborTypeId <> @previousLaborTypeId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO

-- for the 2nd pass, we need to fix picks where the values are not the same, but still duplicates are selected
DECLARE @myCursor CURSOR, @laborResourceId INT, @customFieldId INT, @maxDate DATETIME;

SET @myCursor = CURSOR FOR 
				select BOELaborTypeID, CustomFieldID, MAX(x.UpdateDT) as cnt from
				BOELaborTypeCustomFieldValueXREF x
						INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
				group by BOELaborTypeID, CustomFieldID
				HAVING COUNT(*) > 1

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @laborResourceId, @customFieldId, @maxDate;

WHILE @@FETCH_STATUS = 0
	BEGIN
		DELETE FROM BOELaborTypeCustomFieldValueXREF 
			WHERE BOELaborTypeID = @laborResourceId
			AND CustomFieldValueID IN (SELECT CustomFieldValueID FROM CustomFieldValue WHERE CustomFieldID = @customFieldId) 
			AND UpdateDT != @maxDate;
		FETCH NEXT FROM @myCursor INTO @laborResourceId, @customFieldId, @maxDate;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';
GO