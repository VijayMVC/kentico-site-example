CREATE PROCEDURE [Proc_CMS_Attachment_MoveAttachmentUp]
	@AttachmentGUID uniqueidentifier,
	@DocumentID int
AS
BEGIN
	/* Get Attachment ID */
	DECLARE @AttachmentID int;
	IF @DocumentID > 0 
		SET @AttachmentID = (SELECT TOP 1 AttachmentID FROM CMS_Attachment WHERE (AttachmentGUID = @AttachmentGUID) AND (AttachmentDocumentID=@DocumentID));
	ELSE
		SET @AttachmentID = (SELECT TOP 1 AttachmentID FROM CMS_Attachment WHERE (AttachmentGUID = @AttachmentGUID) AND (AttachmentDocumentID IS NULL));
	/* Get ID of group */
	DECLARE @AttachmentGroupGUID uniqueidentifier;
	SET @AttachmentGroupGUID = (SELECT TOP 1 AttachmentGroupGUID FROM CMS_Attachment WHERE AttachmentID = @AttachmentID);
	
	/* Get GUID of form */
	DECLARE @AttachmentFormGUID uniqueidentifier;
	SET @AttachmentFormGUID = (SELECT TOP 1 AttachmentFormGUID FROM CMS_Attachment WHERE AttachmentID = @AttachmentID);
	
	/* Move the previous attachment down */
	IF @AttachmentFormGUID IS NOT NULL
		IF @AttachmentGroupGUID IS NOT NULL
			UPDATE CMS_Attachment SET AttachmentOrder = AttachmentOrder + 1 WHERE AttachmentOrder = ((SELECT AttachmentOrder FROM CMS_Attachment WHERE AttachmentID = @AttachmentID) - 1 ) AND AttachmentFormGUID = @AttachmentFormGUID AND AttachmentGroupGUID = @AttachmentGroupGUID
		ELSE 
			UPDATE CMS_Attachment SET AttachmentOrder = AttachmentOrder + 1 WHERE AttachmentOrder = ((SELECT AttachmentOrder FROM CMS_Attachment WHERE AttachmentID = @AttachmentID) - 1 ) AND AttachmentFormGUID = @AttachmentFormGUID AND AttachmentGroupGUID IS NULL
	ELSE
		IF @AttachmentGroupGUID IS NOT NULL
			UPDATE CMS_Attachment SET AttachmentOrder = AttachmentOrder + 1 WHERE AttachmentOrder = ((SELECT AttachmentOrder FROM CMS_Attachment WHERE AttachmentID = @AttachmentID) - 1 ) AND AttachmentDocumentID = @DocumentID AND AttachmentGroupGUID = @AttachmentGroupGUID
		ELSE 
			UPDATE CMS_Attachment SET AttachmentOrder = AttachmentOrder + 1 WHERE AttachmentOrder = ((SELECT AttachmentOrder FROM CMS_Attachment WHERE AttachmentID = @AttachmentID) - 1 ) AND AttachmentDocumentID = @DocumentID AND AttachmentGroupGUID IS NULL
		
	/* Move the current attachment up */
	UPDATE CMS_Attachment SET AttachmentOrder = AttachmentOrder - 1 WHERE (AttachmentID = @AttachmentID) AND (AttachmentOrder > 1)
END
