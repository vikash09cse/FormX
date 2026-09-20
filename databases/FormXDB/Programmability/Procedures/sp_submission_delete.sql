CREATE OR ALTER PROCEDURE dbo.sp_submission_delete
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @submissionid UNIQUEIDENTIFIER,
    @actorid      UNIQUEIDENTIFIER,
    @issuperadmin BIT = 0,
    @datascope    TINYINT = 0,
    @candelete    BIT = 1,
    @projectids   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @submittedby UNIQUEIDENTIFIER;
    DECLARE @projectid UNIQUEIDENTIFIER;

    SELECT @submittedby = submittedby, @projectid = projectid
    FROM dbo.form_submissions
    WHERE submissionid = @submissionid AND tenantid = @tenantid AND isdeleted = 0;

    IF @submittedby IS NULL
       OR dbo.fn_submission_can_mutate(@issuperadmin, @datascope, @candelete, @userid, @submittedby, @projectid, @projectids) = 0
    BEGIN
        RAISERROR('Submission not found or you cannot delete it.', 16, 1);
        RETURN;
    END;

    UPDATE dbo.form_submissions SET
        isdeleted = 1,
        updatedby = @actorid,
        updatedat = SYSUTCDATETIME()
    WHERE submissionid = @submissionid
      AND tenantid = @tenantid
      AND isdeleted = 0;
END
GO
