CREATE OR ALTER PROCEDURE dbo.sp_submission_delete
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @submissionid UNIQUEIDENTIFIER,
    @actorid      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.form_submissions SET
        isdeleted = 1,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE submissionid = @submissionid
      AND tenantid = @tenantid
      AND submittedby = @userid
      AND isdeleted = 0;
END
GO
