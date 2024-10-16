EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.14';
GO

-- Author: Katie Pham (e309214)
-- JIRA Story: PROPH-2306
-- Replace all proposals with "Letter Contract" Contract Action Types with the "Other" value, populate the text box to "Letter Contract",
-- and remove "Letter Contract" type from the Contract Action Type dropdown

BEGIN
  -- Set Other Text
  UPDATE [dbo].[Proposal]
  SET ContractActionTypeOtherText = 'Letter Contract'
  FROM [dbo].[Proposal] AS p
  INNER JOIN [dbo].[ContractActionTypeLU] AS cat
  ON p.ContractActionType = cat.ID
  WHERE cat.ContractActionType = 'Letter Contract';

  -- Set ContractActionType
  UPDATE [dbo].[Proposal]
  SET [dbo].[Proposal].ContractActionType = (
	SELECT ID FROM [dbo].[ContractActionTypeLU]
	WHERE ContractActionType = 'Other'
  )
  WHERE [dbo].[Proposal].ContractActionTypeOtherText = 'Letter Contract'

  -- Delete Letter Contract from LU table
  DELETE FROM [dbo].[ContractActionTypeLU]
  WHERE ContractActionType = 'Letter Contract';
END
GO