CREATE OR ALTER PROCEDURE dbo.sp_submission_email_notify_fields
    @tenantid     UNIQUEIDENTIFIER,
    @formid       UNIQUEIDENTIFIER,
    @submissionid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ff.fieldid, ff.controllabel, v.valuetext
    FROM dbo.form_fields ff
    INNER JOIN dbo.form_submission_values v ON v.fieldid = ff.fieldid AND v.submissionid = @submissionid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid
      AND ff.formid = @formid
      AND ff.isdeleted = 0
      AND ff.controltype = 8
      AND ff.issendemailnotification = 1
      AND v.valuetext IS NOT NULL
      AND LTRIM(RTRIM(v.valuetext)) <> '';
END
GO
