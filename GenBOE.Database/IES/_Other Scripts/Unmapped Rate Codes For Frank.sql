DECLARE @revision INT;
SELECT @revision = MAX(Id) FROM Revision;

SELECT rC.RateCode, rC.Description, c.Description AS Category
	FROM RateCode rC, CategoryLU c
	WHERE 
		RevisionId = @revision
		AND c.Id = rC.CategoryId
		AND SectionId IS NULL
	;