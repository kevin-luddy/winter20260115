EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.7';
GO

/*
	## START ##

	4/19/2021 [Dusan] - BOEJ-5210: Additional options for Additional Estimating Resources
*/
IF NOT EXISTS (SELECT 1 FROM RoleTypeLU WHERE RoleTypeId = 5)
BEGIN
	SET IDENTITY_INSERT RoleTypeLU ON;
	INSERT INTO RoleTypeLU (RoleTypeId, RoleType)
		VALUES	(5, 'Parametric Estimator'),
				(6, 'Other');
	SET IDENTITY_INSERT ProposalAdequacyReview OFF;
END
GO
/*
	4/19/2021 [Dusan] - BOEJ-5210: Additional options for Additional Estimating Resources

	## END ##
*/
