-- Just change the bad domain into the good domain if there is no duplicate row
UPDATE ETIuser set NTDomain = 'vflmc'
WHERE NTDomain = 'vf' AND NTID NOT IN
(
	select ntid from ETIuser 
	group by ntid
	having count(*) > 1
)

BEGIN TRANSACTION

BEGIN TRY

DECLARE @Permission_Domain TABLE 
(
	[NTID] varchar(1000) NOT NULL,
	[GoodId] int NULL,
	[BadId] int NULL,
	Processed bit DEFAULT 0
)

INSERT into @Permission_Domain (NTID) SELECT [NTID] from ETIuser 
	group by [NTID]
	having count(*) > 1


DECLARE @NTID varchar(1000)
DECLARE @GoodId int
DECLARE @BadId int
WHILE EXISTS (SELECT 1 FROM @Permission_Domain WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @NTID = [NTID] FROM @Permission_Domain WHERE Processed = 0

	SELECT @GoodId = ETIUserID FROM dbo.ETIUser where NTDomain = 'vflmc' and NTID = @NTID
	SELECT @BadId = ETIUserID FROM dbo.ETIUser where NTDomain = 'vf' and NTID = @NTID

	IF EXISTS (SELECT 1 FROM dbo.SystemUserRole WHERE ETIUserId = @BadId)
	BEGIN
		-- move over the system user role if not there already
		IF EXISTS (SELECT 1 FROM dbo.SystemUserRole WHERE ETIUserId = @GoodId)
		BEGIN
			-- duplicate rows, just delete the bad ones
			DELETE FROM dbo.SystemUserRole WHERE ETIUserId = @BadId
		END
		ELSE
		BEGIN
			-- new rows
			UPDATE dbo.SystemUserRole SET ETIUserId = @GoodId WHERE ETIUserId = @BadId
		END
	END

	-- move over the approvals
	UPDATE dbo.BOEApproval SET ApprovalETIUserId = @GoodId WHERE ApprovalETIUserId = @BadId

	-- move over the comments
	UPDATE dbo.BOEComment SET BOECommentETIUserID = @GoodId WHERE BOECommentETIUserID = @BadId
	UPDATE dbo.BOECommentHistory SET ChangedByETIUserID = @GoodId WHERE ChangedByETIUserID = @BadId

	-- move over the BOE Potential Roles
	UPDATE dbo.BOEPotentialRole SET ETIUserId = @GoodId WHERE ETIUserId = @BadId

	-- move over the state history
	UPDATE dbo.BOEStateHistory SET ChangedByETIUserID = @GoodId WHERE ChangedByETIUserID = @BadId

	-- move over the cost volume lead, created by user
	UPDATE dbo.Workspace SET CostVolumeLeadPricerUserID = @GoodId WHERE CostVolumeLeadPricerUserID = @BadId
	UPDATE dbo.Workspace SET CreatedByETIUserID = @GoodId WHERE CreatedByETIUserID = @BadId

	-- BOE User roles
	UPDATE dbo.BOEUserRole SET ETIUserId = @GoodId WHERE ETIUserId = @BadId
	UPDATE dbo.BOEUserRoleHistory SET CurrentETIUserID = @GoodId WHERE CurrentETIUserID = @BadId
	UPDATE dbo.BOEUserRoleHistory SET ChangedByETIUserID = @GoodId WHERE ChangedByETIUserID = @BadId
	UPDATE dbo.BOEUserRoleHistory SET UpdatedETIUserID = @GoodId WHERE UpdatedETIUserID = @BadId

	-- Workspace State
	UPDATE dbo.WorkspaceStateHistory SET ChangedByETIUserID = @GoodId WHERE ChangedByETIUserID = @BadId 

	-- Workspace User Roles
	UPDATE dbo.WorkspaceUserRole SET ETIUserId = @GoodId WHERE ETIUserId = @BadId

	-- Workspace Version
	UPDATE WorkspaceVersion SET CreatedByETIUserID = @GoodId WHERE CreatedByETIUserID = @BadId

	DELETE FROM ETIUSER WHERE ETIUserID = @BadId

	UPDATE @Permission_Domain 
	SET	Processed = 1, GoodId = @GoodId, BadId = @BadId
	WHERE 
		[NTID] = @NTID

END

select * from @Permission_Domain

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