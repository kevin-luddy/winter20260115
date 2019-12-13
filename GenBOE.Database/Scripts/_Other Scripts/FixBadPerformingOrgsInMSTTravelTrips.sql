BEGIN TRANSACTION

BEGIN TRY

DECLARE @PerformingOrganization TABLE
(
	[MSTTravelTripId] int NOT NULL,
	[WorkspaceId] int NOT NULL,
	[PerformingOrganizationId] int not null,
	PerformingOrganizationName varchar(20) NOT NULL,
	[NewPerformingOrganizationID] [int] NULL,
	Processed bit DEFAULT 0
)

INSERT INTO @PerformingOrganization
           ([MSTTravelTripId], [WorkspaceId], [PerformingOrganizationId], PerformingOrganizationName, Processed)
SELECT tt.msttraveltripid, b.workspaceid,  tt.[PerformingOrganizationId], po.PerformingOrganizationName, 0 AS Processed
FROM [dbo].[MSTTravelTrip] tt
  INNER JOIN TravelTripTaskElement t ON t.TravelTripTaskElementID = tt.TravelTripTaskElementID
  INNER JOIN BOE b on t.BOEID = b.BOEID
  INNER JOIN PerformingOrganization po on po.performingorganizationid = tt.[PerformingOrganizationID]
  INNER JOIN Workspace w on w.PerformingOrganizationListID = po.PerformingOrganizationListID
where w.workspaceid != b.workspaceid

DECLARE @PerformingOrganizationID int
DECLARE @PerformingOrganizationName varchar(20)
DECLARE @NewPerformingOrganizationID int
DECLARE @WorkspaceID int
DECLARE @MSTTravelTripId int
WHILE EXISTS (SELECT 1 FROM @PerformingOrganization WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @MSTTravelTripId = MSTTravelTripId, @PerformingOrganizationID = PerformingOrganizationID, @PerformingOrganizationName = PerformingOrganizationName, @WorkspaceID = WorkspaceID FROM @PerformingOrganization WHERE Processed = 0

	-- find the corresponding performing org in the correct workspace
	SELECT TOP 1 @NewPerformingOrganizationID = p.PerformingOrganizationID 
	FROM performingorganization p 
	INNER JOIN Workspace w on p.performingorganizationlistID = w.performingorganizationlistID
	WHERE p.performingorganizationname = @PerformingOrganizationName AND w.workspaceid = @WorkspaceID

	-- update all combos of bad perfOrg/good workspaceid with the good perfOrg
	UPDATE @PerformingOrganization SET [NewPerformingOrganizationID] = @NewPerformingOrganizationID, Processed = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID AND WorkspaceID = @WorkspaceID

END

-- now do the update on the real table
UPDATE [dbo].[MSTTravelTrip]
		SET [PerformingOrganizationID] = P.NewPerformingOrganizationID
		FROM  [MSTTravelTrip] tt INNER JOIN @PerformingOrganization P ON tt.MSTTravelTripId = P.MSTTravelTripId
		WHERE P.NewPerformingOrganizationID IS NOT NULL

IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END

END TRY


BEGIN CATCH
	ROLLBACK TRANSACTION
	

	DECLARE @ErrorMessage varchar (500)
	SELECT @ErrorMessage = ERROR_MESSAGE()
	RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)

	RETURN
	
END CATCH
GO