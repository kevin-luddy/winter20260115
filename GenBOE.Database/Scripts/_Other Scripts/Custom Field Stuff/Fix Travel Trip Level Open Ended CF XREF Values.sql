-- This file deals with issues w/ open ended custom fields, where the same value can be "selected" or referenced by multiple custom fields. this then causes all sorts of issues

-- create the temp SP
CREATE PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTripLevel(@OldCustomFieldValueId INT, @TripId INT) AS
	PRINT 'About to execute OpenEndedCustomFieldCleanupTripLevel, CF ValueId: ' + CONVERT(VARCHAR(50), @OldCustomFieldValueId) + ', TripId: ' + CONVERT(VARCHAR(50), @TripId)

	DECLARE @NewCFValueId INT

	INSERT INTO CustomFieldValue (UpdateDT, CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, CustomFieldValueInUseFlag)
		SELECT GETDATE(), CustomFieldValueName, CustomFieldValueDescription, CustomFieldID, 1
			FROM CustomFieldValue
			WHERE CustomFieldValueID = @OldCustomFieldValueId

	SELECT @NewCFValueId = SCOPE_IDENTITY()
	PRINT '    New CF Value Id: ' + CONVERT(VARCHAR(50), @NewCFValueId) 

	UPDATE MSTTravelTripCustomFieldValueXREF
		SET MSTCustomFieldValueID = @NewCFValueId
		WHERE MSTTravelTripID = @TripId AND MSTCustomFieldValueID = @OldCustomFieldValueId
GO

-- execution.. we want to skip the first Custom Field Value Id (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @cfValueIdToProcess INT, @previousCfValueId INT, @TripId INT, @count INT;

SET @myCursor = CURSOR FOR 
		SELECT TOP 2000 cV.CustomFieldValueID, x.MSTTravelTripID
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN MSTTravelTripCustomFieldValueXREF x ON cV.CustomFieldValueID = x.MSTCustomFieldValueID
				INNER JOIN MSTTravelTrip t ON t.MSTTravelTripID = x.MSTTravelTripID
				INNER JOIN TravelTripTaskElement tT ON t.TravelTripTaskElementID = tT.TravelTripTaskElementID
			WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
												FROM (
													SELECT x.*
														FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
															INNER JOIN MSTTravelTripCustomFieldValueXREF x ON cV.CustomFieldValueID = x.MSTCustomFieldValueID
														WHERE cF.IsOpenEnded = 1) z
													GROUP BY MSTCustomFieldValueId
													HAVING COUNT(*) > 1)
			ORDER BY cF.CustomFieldId, cV.CustomFieldValueID, tT.BOEID, t.TravelTripTaskElementID, x.MSTTravelTripID;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TripId;
SET @count = 0;

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @previousCfValueId = @cfValueIdToProcess;
		IF @count > 0
			EXEC dbo.TEMP_OpenEndedCustomFieldCleanupTripLevel @OldCustomFieldValueId = @cfValueIdToProcess, @TripId = @TripId;

		FETCH NEXT FROM @myCursor INTO @cfValueIdToProcess, @TripId;	

		IF @cfValueIdToProcess <> @previousCfValueId SET @count = 0		
		ELSE SET @count = @count + 1;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT 'DONE';

DROP PROCEDURE dbo.TEMP_OpenEndedCustomFieldCleanupTripLevel;
GO