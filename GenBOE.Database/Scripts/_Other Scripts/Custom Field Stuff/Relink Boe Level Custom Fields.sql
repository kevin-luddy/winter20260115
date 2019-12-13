-- create the temp SP
CREATE PROCEDURE dbo.TEMP_CustomFieldValueCleanup(@wsThatOwnsCf INT, @wsToWhichAssign INT, @currentlyUsedValueId INT, @BoeToAssignTo INT) AS
	PRINT '-- About to execute CustomFieldValueCleanup, Owner WS Id: ' + CONVERT(VARCHAR(50), @wsThatOwnsCf) + ', Assign to WS Id: ' 
			+ CONVERT(VARCHAR(50), @wsToWhichAssign) + ', Current Value Id: ' + CONVERT(VARCHAR(50), @currentlyUsedValueId)
			+ ', BoeId: ' + CONVERT(VARCHAR(50), @BoeToAssignTo)

	DECLARE @correctCustomFieldId INT;
	DECLARE @correctValueId INT;

	SELECT @correctCustomFieldId = i.CustomFieldId
		FROM CustomField ext INNER JOIN CustomField i
			ON ext.CustomFieldName = i.CustomFieldName 
				AND ext.CustomFieldName = i.CustomFieldName
			WHERE 
				ext.WorkspaceID = @wsThatOwnsCf
				AND i.WorkspaceId = @wsToWhichAssign
				AND ext.CustomFieldId = (SELECT CustomFieldId FROM CustomFieldValue WHERE CustomFieldValueId = @currentlyUsedValueId)

	PRINT '--   Correct Custom Field Id: ' + CONVERT(VARCHAR(50), @correctCustomFieldId)

	-- from value ID, need to find other similar values
	SELECT @correctValueId = i.CustomFieldValueID
		FROM CustomFieldValue ext INNER JOIN CustomFieldValue i
			ON ext.CustomFieldValueDescription = i.CustomFieldValueDescription 
				AND ext.CustomFieldValueName = i.CustomFieldValueName
				AND ext.CustomFieldValueID = @currentlyUsedValueId
				AND i.CustomFieldID = @correctCustomFieldId

	IF(@correctValueId IS NULL)
		BEGIN
			PRINT '--   !!! We couldn''t properly match a value. The record above should be manually deleted ..'
			PRINT '       DELETE FROM BOECustomFieldValueXREF WHERE ' 
						+ '      CustomFieldValueId = ' + CONVERT(VARCHAR(50), @currentlyUsedValueId)
						+ ' AND  BOEID = ' + CONVERT(VARCHAR(50), @BoeToAssignTo) + ';'
		END
	ELSE
		BEGIN
			-- this record needs to point at the correct value id
			PRINT '--   Correct Custom Field VALUE Id: ' + CONVERT(VARCHAR(50), @correctValueId)
			UPDATE BOECustomFieldValueXREF 
				SET CustomFieldValueID = @correctValueId
				WHERE BCFVID =
						(SELECT x.BCFVID
							FROM BOECustomFieldValueXREF x
							WHERE 
								x.CustomFieldValueID = @currentlyUsedValueId
								AND x.BOEID = @BoeToAssignTo)
			UPDATE CustomFieldValue
				SET CustomFieldValueInUseFlag = 1
				WHERE CustomFieldValueId = @correctValueId
		END
	PRINT '';
GO

-- execution.. we want to skip the first Custom Field Value Id (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @wsToWhichAssign INT, @wsThatOwnsCf INT, @currentlyUsedValueId INT, @boeId INT;

SET @myCursor = CURSOR FOR 
		SELECT TOP 2000 x.CustomFieldValueID IncorrectValueId, b.WorkspaceID WsToAssignTo, cF.WorkspaceID AS wsOwnsCurrentValue, x.BOEID
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN BOECustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
				INNER JOIN BOE b ON b.BOEID = x.BOEID
			WHERE cF.WorkspaceID <> b.WorkspaceID;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @currentlyUsedValueId, @wsToWhichAssign, @wsThatOwnsCf, @boeId;

WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC dbo.TEMP_CustomFieldValueCleanup @wsToWhichAssign = @wsToWhichAssign, @currentlyUsedValueId = @currentlyUsedValueId, @wsThatOwnsCf = @wsThatOwnsCf, @BoeToAssignTo = @boeId;

		FETCH NEXT FROM @myCursor INTO @currentlyUsedValueId, @wsToWhichAssign, @wsThatOwnsCf, @boeId;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT '--DONE';

DROP PROCEDURE dbo.TEMP_CustomFieldValueCleanup;
GO