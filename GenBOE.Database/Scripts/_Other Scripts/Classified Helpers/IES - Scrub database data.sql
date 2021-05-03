/*
	### DO NOT EXECUTE AS A PART OF ANY RELEASE ###
	
	This scrubs out data from IES database (2021.4).
	
*/

/* <==== Remove this line, to make this script run. this is a precaution.. just in case.....

DELETE FROM ELMAH_Error;
DELETE FROM Banner;
DELETE FROM OfflineApplication;

DELETE FROM RDSBSectionXref;
DELETE FROM RDSBRateCodeXref;
DELETE FROM RDSBDocumentInformation;
DELETE FROM UserLog;
