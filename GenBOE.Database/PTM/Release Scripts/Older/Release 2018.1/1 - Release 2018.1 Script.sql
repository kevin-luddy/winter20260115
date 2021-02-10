/*
	## START ##
	
	1/23/2018 Dusan BOEJ-2967 PAR Checklist rows missing in the export
*/
UPDATE PARChecklistContent SET SubmissionItem = 'MATERIALS AND SERVICES'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Materials and Services</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'DIRECT LABOR'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Direct Labor</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'OTHER COSTS'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Other Costs</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'OTHER'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Other</b></p>';
/*
	1/23/2018 Dusan BOEJ-2967 PAR Checklist rows missing in the export

	## END ##
*/