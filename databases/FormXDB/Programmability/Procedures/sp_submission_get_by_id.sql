CREATE OR ALTER PROCEDURE dbo.sp_submission_get_by_id
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @submissionid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.submissionid, s.formid, s.projectid, p.projectname,
        s.submittedby, s.submittedat, s.status, s.createdat, s.updatedat
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    WHERE s.tenantid = @tenantid
      AND s.submissionid = @submissionid
      AND s.submittedby = @userid
      AND s.isdeleted = 0;

    SELECT v.submissionvalueid, v.submissionid, v.fieldid, v.valuetext
    FROM dbo.form_submission_values v
    INNER JOIN dbo.form_submissions s ON s.submissionid = v.submissionid
    WHERE s.tenantid = @tenantid
      AND s.submissionid = @submissionid
      AND s.submittedby = @userid
      AND s.isdeleted = 0;
END
GO
