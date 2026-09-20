CREATE OR ALTER PROCEDURE dbo.sp_form_followup_config_get
    @tenantid  UNIQUEIDENTIFIER,
    @formid    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Config where this form is the primary
    SELECT
        c.formfollowupconfigid,
        c.tenantid,
        c.primaryformid,
        c.followupformid,
        pf.name AS primaryformname,
        ff.name AS followupformname,
        c.allowmultiple,
        c.createdat,
        c.updatedat
    FROM dbo.form_followup_configs c
    INNER JOIN dbo.forms pf ON pf.formid = c.primaryformid AND pf.isdeleted = 0
    INNER JOIN dbo.forms ff ON ff.formid = c.followupformid AND ff.isdeleted = 0
    WHERE c.tenantid = @tenantid
      AND c.primaryformid = @formid
      AND c.isdeleted = 0;

    -- Also return if this form is used as someone else's follow-up
    SELECT
        c.formfollowupconfigid,
        c.primaryformid,
        pf.name AS primaryformname,
        c.followupformid
    FROM dbo.form_followup_configs c
    INNER JOIN dbo.forms pf ON pf.formid = c.primaryformid AND pf.isdeleted = 0
    WHERE c.tenantid = @tenantid
      AND c.followupformid = @formid
      AND c.isdeleted = 0;
END
GO
