-- This file deals with issues w/ open ended custom fields, where the same value can be "selected" or referenced by multiple custom fields. this then causes all sorts of issues

-- create the temp SP
CREATE PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTaskLevel(@OldCustomFieldValueId INT, @TaskId INT) AS
	PRINT 'About to execute OpenEndedCustomFieldCleanupTaskLevel, CF ValueId: ' + CONVERT(VARCHAR(50), @OldCustomFieldValueId) + ', TaskId: ' + CONVERT(VARCHAR(50), @TaskId)

	DECLARE @NewCFValueId INT

	INSERT INTO CustomFieldValue (UpdateDT, CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, CustomFieldValueInUseFlag)
		SELECT GETDATE(), CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, 1
			FROM CustomFieldValue
			WHERE CustomFieldValueId = @OldCustomFieldValueId

	SELECT @NewCFValueId = SCOPE_IDENTITY()
	PRINT '    New CF Value Id: ' + CONVERT(VARCHAR(50), @NewCFValueId) 

	UPDATE BOETaskElementCustomFieldValueXREF
		SET CustomFieldValueID = @NewCFValueId
		WHERE BOETaskElementID = @TaskId AND CustomFieldValueId = @OldCustomFieldValueId
GO

-- execution.. we want to skip the first Custom Field Value Id (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @cfValueIdToProcess INT, @previousCfValueId INT, @TaskId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT TOP 2000 cV.CustomFieldValueID, x.BOETaskElementID
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN BOETaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
				INNER JOIN BoeTaskElement bT on x.BOETaskElementID = bT.BOETaskElementID
			WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
											FROM (
												SELECT x.*
													FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
														INNER JOIN BOETaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
													WHERE cF.IsOpenEnded = 1) z
												GROUP BY CustomFieldValueId
												HAVING COUNT(*) > 1)
			ORDER BY cF.CustomFieldId, cV.CustomFieldValueID, bT.BOEID, x.BOETaskElementID;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TaskId;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		IF @count > 0
			EXEC dbo.TEMP_OpenEndedCustomFieldCleanupTaskLevel @OldCustomFieldValueId = @cfValueIdToProcess, @TaskId = @TaskId;

		FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TaskId;	

		IF @cfValueIdToProcess <> @previousCfValueId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';

DROP PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTaskLevel;
GO