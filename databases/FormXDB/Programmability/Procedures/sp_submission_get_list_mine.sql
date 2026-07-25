CREATE OR ALTER PROCEDURE dbo.sp_submission_get_list_mine
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER,
    @formid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.submissionid, s.formid, s.projectid, p.projectname,
        s.submittedby, s.submittedat, s.status, s.createdat, s.updatedat
    FROM dbo.form_submissions s
    INNER JOIN dbo.projects p ON p.projectid = s.projectid
    WHERE s.tenantid = @tenantid
      AND s.formid = @formid
      AND s.submittedby = @userid
      AND s.isdeleted = 0
    ORDER BY s.submittedat DESC;
END
GO
