-- This should never happen, but just in case, we'll delete orphaned attachments
DELETE FROM Attachment WHERE Id NOT IN (SELECT DISTINCT AttachmentId FROM ProposalsAttachments);
GO