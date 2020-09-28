EXEC dbo.[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.5.1';
GO

/*
	## START ##

	9/25/2020 [ranzalon] - BOEJ-4837 - Update text for PTM checklist question 15 
*/

IF EXISTS (SELECT 1 
		FROM dbo.[PARChecklistContent]
		WHERE [ProposalAdequacyReviewID] = 13 AND [ChecklistText] = '<p>Reference Subcontract Summary Table, if the cost volume includes subcontracts. </p><p><a href=''__BASE_URL__Instruction_15.docx'' target=''_blank''>Additional Instructions</a></p>')
BEGIN

UPDATE dbo.[PARChecklistContent]
SET [ChecklistText] = '<p>Reference Subcontractor Summary Table and PBOE for subcontractors over the CCoPD threshold where field pricing assistance has been or will be requested. </p><p><a href=''__BASE_URL__Instruction_15.docx'' target=''_blank''>Additional Instructions</a></p>'
WHERE [ProposalAdequacyReviewID] = 13 AND [ChecklistText] = '<p>Reference Subcontract Summary Table, if the cost volume includes subcontracts. </p><p><a href=''__BASE_URL__Instruction_15.docx'' target=''_blank''>Additional Instructions</a></p>'

END 

/*
	9/25/2020 [ranzalon] - BOEJ-4837 - Update text for PTM checklist question 15

	## END ##
*/