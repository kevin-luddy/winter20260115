/*
	### DO NOT EXECUTE AS A PART OF ANY RELEASE ###
	
	This scrubs out data from genTrac database (2021.4).
	
*/

/* <==== Remove this line, to make this script run. this is a precaution.. just in case.....

DELETE FROM ELMAH_Error;
DELETE FROM DataMart.DataMartEmployee;

DELETE FROM LineOfBusinessRoleXREF;
DELETE FROM ProposalContractTypeXREF;
DELETE FROM ProposalCostElementXREF;
DELETE FROM ProposalPARChecklistXREF;
DELETE FROM ProposalPPRChecklistXREF;

DELETE FROM ProposalChecklistComplete;
DELETE FROM ProposalChecklist;
DELETE FROM ProposalsAttachments;
DELETE FROM ProposalTrackingGenerator;

DELETE FROM ProposalUserRole;
DELETE FROM Attachment;
DELETE FROM Proposal;

DELETE FROM SystemUserRole;
DELETE FROM GenTracUser;