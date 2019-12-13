-- This file deals with issues w/ open ended custom fields, where the same value can be "selected" or referenced by multiple custom fields. this then causes all sorts of issues

-- create the temp SP
CREATE PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTravelTaskLevel(@OldCustomFieldValueId INT, @TravelTaskId INT) AS
	PRINT 'About to execute OpenEndedCustomFieldCleanupTravelTaskLevel, CF ValueId: ' + CONVERT(VARCHAR(50), @OldCustomFieldValueId) + ', TravelTaskId: ' + CONVERT(VARCHAR(50), @TravelTaskId)

	DECLARE @NewCFValueId INT

	INSERT INTO CustomFieldValue (UpdateDT, CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, CustomFieldValueInUseFlag)
		SELECT GETDATE(), CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, 1
			FROM CustomFieldValue
			WHERE CustomFieldValueId = @OldCustomFieldValueId

	SELECT @NewCFValueId = SCOPE_IDENTITY()
	PRINT '    New CF Value Id: ' + CONVERT(VARCHAR(50), @NewCFValueId) 

	UPDATE TravelTripTaskElementCustomFieldValueXREF
		SET CustomFieldValueID = @NewCFValueId
		WHERE TravelTripTaskElementID = @TravelTaskId AND CustomFieldValueId = @OldCustomFieldValueId
GO

-- execution.. we want to skip the first Custom Field Value Id (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @cfValueIdToProcess INT, @previousCfValueId INT, @TravelTaskId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT TOP 2000 cV.CustomFieldValueID, x.TravelTripTaskElementID
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN TravelTripTaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
				INNER JOIN TravelTripTaskElement t on x.TravelTripTaskElementID = t.TravelTripTaskElementID
			WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
											FROM (
												SELECT x.*
													FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
														INNER JOIN TravelTripTaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
													WHERE cF.IsOpenEnded = 1) z
												GROUP BY CustomFieldValueId
												HAVING COUNT(*) > 1)
			ORDER BY cF.CustomFieldId, cV.CustomFieldValueID, t.BOEID, x.TravelTripTaskElementID;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TravelTaskId;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		IF @count > 0
			EXEC dbo.TEMP_OpenEndedCustomFieldCleanupTravelTaskLevel @OldCustomFieldValueId = @cfValueIdToProcess, @TravelTaskId = @TravelTaskId;

		FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TravelTaskId;	

		IF @cfValueIdToProcess <> @previousCfValueId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';

DROP PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTravelTaskLevel;
GO