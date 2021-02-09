EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.4';
GO

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'genTracUser' AND C.name = 'IsUsPerson')
BEGIN
       ALTER TABLE [dbo].genTracUser ADD IsUsPerson bit;
       ALTER TABLE [dbo].genTracUser ADD IsSubcontractor bit;
END
GO

-- One time data cleanup to remove OTA contract type and create a group out of it
DECLARE @otaGroupId AS INT = (SELECT ContractTypeGroupId FROM  ContractTypeGroupLU WHERE ContractTypeGroup = 'OTA');
DECLARE @OtaContractTypeId AS INT = (SELECT ContractTypeID FROM ContractTypeLU WHERE ContractType = 'OTA');

IF @otaGroupId IS NULL
BEGIN
	-- Create a new Group, called OTA. 
	INSERT INTO ContractTypeGroupLU (ContractTypeGroup, IsActive) VALUES ('OTA', 1) SELECT @otaGroupId = Scope_Identity();

	-- Add into it the same contract types as are in the Hybrid group.
	INSERT INTO ContractTypeGroupXREF (ContractTypeID, ContractTypeGroupID)
		SELECT x.ContractTypeID, @otaGroupId
			FROM ContractTypeGroupXREF x 
			WHERE x.ContractTypeGroupID = (SELECT ContractTypeGroupID FROM ContractTypeGroupLU WHERE ContractTypeGroup = 'Hybrid');
END

-- Move data from OTA contract type into OTA group.
	-- Update the contract group in the proposals
	UPDATE Proposal
		SET ContractTypeGroupId = @otaGroupId
		WHERE ProposalId IN (SELECT ProposalId FROM ProposalContractTypeXREF WHERE ContractTypeId = @OtaContractTypeId);

	-- No contract types are to be selected. -> delete xrefs
	DELETE FROM ProposalContractTypeXREF WHERE ContractTypeId = @OtaContractTypeId;

-- Delete OTA contract type.
	-- delete all xrefs between types and groups
	DELETE FROM ContractTypeGroupXREF WHERE ContractTypeId = (SELECT ContractTypeId FROM ContractTypeLU WHERE ContractType = 'OTA');
	-- delete the type -- need to actually make it inactive, due to integration w/ BOE
	UPDATE ContractTYpeLU SET IsActive = 0 WHERE ContractType = 'OTA';

-- Move data from “Other” group into OTA group.
	-- Update the group in the proposals - the above should have taken care of this, but just in case, we want to be sure..
	UPDATE Proposal
		SET ContractTypeGroupId = @otaGroupId
		FROM Proposal
		WHERE ContractTypeGroupId = (SELECT ContractTypeGroupID FROM ContractTypeGroupLU WHERE ContractTypeGroup = 'Other');
	-- No contract type selected. -> do not need actually need to delete xrefs, because the "Other" group only contained 1 contract type - OTA, 
		-- and those XREFs were deleted above

-- Delete Other group, including all of the contract types inside of it.
	-- delete the group (nothing else needed, as we deleted the OTA type already above, and that was the only type in the group
	DELETE FROM ContractTypeGroupLU WHERE ContractTypeGroup = 'Other';
GO

-- BOEJ-4244 - Fix Proposal Statuses where not properly updated to complete/submitted
UPDATE [dbo].[ProposalChecklistComplete]
SET SubmitDate = P.LOBEstimatingLeadSignedDT
FROM [dbo].[ProposalChecklistComplete] PCC
INNER JOIN [dbo].[Proposal] P
ON P.ProposalID = PCC.ProposalID
WHERE P.LOBEstimatingLeadSignedDT is not null and P.ProposalStatusID = 1

UPDATE [dbo].[Proposal]
SET ProposalStatusID = 6
WHERE LOBEstimatingLeadSignedDT is not null and ProposalStatusID = 1 and CCPDRequired = 1

UPDATE [dbo].[Proposal]
SET ProposalStatusID = 2
WHERE LOBEstimatingLeadSignedDT is not null and ProposalStatusID = 1 and CCPDRequired = 0

GO