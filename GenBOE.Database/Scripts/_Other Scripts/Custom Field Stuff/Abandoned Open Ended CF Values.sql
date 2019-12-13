-- This file would display all open ended custom field values which are not beign used by any custom fields (i.e. are not in use)

-- Abandoned values
SELECT cF.CustomFieldID, cF.CustomFieldName, cV.CustomFieldValueID, cV.CustomFieldValueDescription
	FROM CustomField cF RIGHT OUTER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
	WHERE cF.IsOpenEnded = 1
		AND
			(SELECT COUNT(*) 
				FROM BOELaborTypeCustomFieldValueXREF x
				WHERE x.CustomFieldValueID = cV.CustomFieldValueID) < 1
		AND 
			(SELECT COUNT(*) 
				FROM BOETaskElementCustomFieldValueXREF x
				WHERE x.CustomFieldValueID = cV.CustomFieldValueID) < 1
		AND
			(SELECT COUNT(*) 
				FROM BOECustomFieldValueXREF x
				WHERE x.CustomFieldValueID = cV.CustomFieldValueID) < 1;

SELECT cF.CustomFieldID, cF.CustomFieldName, cV.CustomFieldValueID, cV.CustomFieldValueDescription
	FROM CustomField cF RIGHT OUTER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
	WHERE cF.IsOpenEnded = 1
		AND (cV.CustomFieldValueDescription = '' OR CustomFieldValueDescription IS NULL);