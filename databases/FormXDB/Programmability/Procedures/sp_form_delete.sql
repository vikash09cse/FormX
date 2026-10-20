CREATE OR ALTER PROCEDURE dbo.sp_form_delete
    @tenantid  UNIQUEIDENTIFIER,
    @formid    UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.forms
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0;

    -- Clear follow-up mappings so deleted primaries do not hide other forms from My Forms.
    UPDATE dbo.form_followup_configs
    SET isdeleted = 1,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND isdeleted = 0
      AND (primaryformid = @formid OR followupformid = @formid);
END
GO
