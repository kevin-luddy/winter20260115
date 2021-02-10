/*
	BOEJ-861 Start
	Mike Basquill
	3/22/2016

	Script delete instances where there are exact duplicates of a BOE LEVEL custom field selection.
*/

DELETE a
	FROM dbo.BOECustomFieldValueXREF a, dbo.BOECustomFieldValueXREF b
	WHERE 
		a.BOEID = b.BOEID
		AND a.CustomFieldValueID = b.CustomFieldValueID
		AND a.BCFVID < b.BCFVID; 
 GO
 
 DELETE a
	FROM version.BOECustomFieldValueXREF a, version.BOECustomFieldValueXREF b
	WHERE 
		a.BOEID = b.BOEID
		AND a.CustomFieldValueID = b.CustomFieldValueID
		AND a.BCFVID < b.BCFVID
		AND a.VersionID = b.VersionID; 
 GO

/*
	BOEJ-861 End

	BOEJ-928
	Matt Kotwicki 
	
	3/22/2016
	
	Update all NULL values for IsMultiClinWbs to false. For sanity check, make sure both the backup's and normal db's are covered.
*/

UPDATE version.boe
	SET IsMultiClinWbs = 0
	WHERE IsMultiClinWbs IS NULL;
GO

UPDATE dbo.boe
	SET IsMultiClinWbs = 0
	WHERE IsMultiClinWbs IS NULL;
GO

/*
	BOEJ-928 End
*/