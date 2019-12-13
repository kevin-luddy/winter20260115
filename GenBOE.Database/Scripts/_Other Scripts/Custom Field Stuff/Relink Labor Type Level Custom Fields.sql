-- create the temp SP
CREATE PROCEDURE dbo.TEMP_CustomFieldValueCleanup(@wsThatOwnsCf INT, @wsToWhichAssign INT, @currentlyUsedValueId INT, @assignToLaborTypeId INT) AS
	PRINT '-- About to execute CustomFieldValueCleanup, Owner WS Id: ' + CONVERT(VARCHAR(50), @wsThatOwnsCf) + ', Assign to WS Id: ' 
	+ CONVERT(VARCHAR(50), @wsToWhichAssign) + ', Current Value Id: ' + CONVERT(VARCHAR(50), @currentlyUsedValueId) + ', Assign to Labor Type Id: ' + CONVERT(VARCHAR(50), @assignToLaborTypeId)

	DECLARE @correctCustomFieldId INT;
	DECLARE @correctValueId INT;

	SELECT @correctCustomFieldId = i.CustomFieldId
		FROM CustomField ext INNER JOIN CustomField i
			ON ext.CustomFieldName = i.CustomFieldName 
				AND ext.CustomFieldName = i.CustomFieldName
			WHERE 
				ext.WorkspaceID = @wsThatOwnsCf -- cf value owner
				AND i.WorkspaceId = @wsToWhichAssign -- cf value to be assigned to
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
			PRINT '       DELETE FROM BOELaborTypeCustomFieldValueXREF WHERE ' 
						+ '      CustomFieldValueId = ' + CONVERT(VARCHAR(50), @currentlyUsedValueId)
						+ ' AND  BOELaborTypeID = ' + CONVERT(VARCHAR(50), @assignToLaborTypeId) + ';'
		END
	ELSE
		BEGIN
			-- this record needs to point at the correct value id
			PRINT '--   Correct Custom Field VALUE Id: ' + CONVERT(VARCHAR(50), @correctValueId)
			UPDATE BOELaborTypeCustomFieldValueXREF 
				SET CustomFieldValueID = @correctValueId
				WHERE BLTCFVID =
						(SELECT BLTCFVID
							FROM BOELaborTypeCustomFieldValueXREF x INNER JOIN BOELaborType bL ON x.BOELaborTypeID = bL.BOELaborTypeID
									INNER JOIN BOETaskElement bT ON bT.BOETaskElementID = bL.BOETaskElementID
									INNER JOIN BOE b ON b.BOEID = bT.BOEID
							WHERE x.CustomFieldValueId = @currentlyUsedValueId
								AND bL.BOELaborTypeID = @assignToLaborTypeId
								AND b.WorkspaceID = @wsToWhichAssign)

			UPDATE CustomFieldValue
				SET CustomFieldValueInUseFlag = 1
				WHERE CustomFieldValueId = @correctValueId
		END
	PRINT '';
GO

-- execution.. we want to skip the first Custom Field Value Id (since we are going to leave that one), and are only going to work on 2nd+
DECLARE @myCursor CURSOR, @wsToWhichAssign INT, @wsThatOwnsCf INT, @currentlyUsedValueId INT, @assignToLaborTypeId INT;

SET @myCursor = CURSOR FOR 
		SELECT TOP 2000 cV.CustomFieldValueID IncorrectValueId, b.WorkspaceID AS WsToAssignTo, cF.WorkspaceID AS wsOwnsCurrentValue, x.BoeLaborTypeId
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN BOELaborTypeCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
				INNER JOIN BOELaborType bL ON x.BOELaborTypeID = bL.BOELaborTypeID
				INNER JOIN BOETaskElement bT ON bT.BOETaskElementID = bL.BOETaskElementID
				INNER JOIN BOE b ON b.BOEID = bT.BOEID
			WHERE cF.WorkspaceID <> b.WorkspaceID
			ORDER BY WsToAssignTo, IncorrectValueId, x.BoeLaborTypeId;

OPEN @myCursor
FETCH NEXT FROM @myCursor INTO @currentlyUsedValueId, @wsToWhichAssign, @wsThatOwnsCf, @assignToLaborTypeId;

WHILE @@FETCH_STATUS = 0
	BEGIN
		-- PRINT 'dbo.TEMP_CustomFieldValueCleanup @wsThatOwnsCf = ' + CONVERT(VARCHAR(50), @wsThatOwnsCf) + ', @wsToWhichAssign = ' + CONVERT(VARCHAR(50), @wsToWhichAssign) + ', @currentlyUsedValueId = ' + CONVERT(VARCHAR(50), @currentlyUsedValueId) + ', @assignToLaborTypeId = ' + CONVERT(VARCHAR(50), @assignToLaborTypeId);
		EXEC dbo.TEMP_CustomFieldValueCleanup @wsThatOwnsCf = @wsThatOwnsCf, @wsToWhichAssign = @wsToWhichAssign, @currentlyUsedValueId = @currentlyUsedValueId, @assignToLaborTypeId = @assignToLaborTypeId;

		FETCH NEXT FROM @myCursor INTO @currentlyUsedValueId, @wsToWhichAssign, @wsThatOwnsCf, @assignToLaborTypeId;
	END;

CLOSE @myCursor;
DEALLOCATE @myCursor;

PRINT '-- DONE';

DROP PROCEDURE dbo.TEMP_CustomFieldValueCleanup;
GO