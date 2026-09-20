CREATE OR ALTER PROCEDURE dbo.sp_form_followup_config_save
    @tenantid        UNIQUEIDENTIFIER,
    @primaryformid   UNIQUEIDENTIFIER,
    @followupformid  UNIQUEIDENTIFIER = NULL, -- NULL clears the mapping
    @allowmultiple   BIT = 1,
    @actorid         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.forms
        WHERE tenantid = @tenantid AND formid = @primaryformid AND isdeleted = 0 AND status = 1)
    BEGIN
        RAISERROR('Primary form not found or inactive.', 16, 1);
        RETURN;
    END;

    -- Clear mapping
    IF @followupformid IS NULL
    BEGIN
        UPDATE dbo.form_followup_configs
        SET isdeleted = 1,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE tenantid = @tenantid
          AND primaryformid = @primaryformid
          AND isdeleted = 0;

        SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS formfollowupconfigid;
        RETURN;
    END;

    IF @primaryformid = @followupformid
    BEGIN
        RAISERROR('Follow-up form must be different from the primary form.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.forms
        WHERE tenantid = @tenantid AND formid = @followupformid AND isdeleted = 0 AND status = 1)
    BEGIN
        RAISERROR('Follow-up form not found or inactive.', 16, 1);
        RETURN;
    END;

    -- Primary cannot already be a follow-up for another form
    IF EXISTS (
        SELECT 1 FROM dbo.form_followup_configs
        WHERE tenantid = @tenantid
          AND followupformid = @primaryformid
          AND isdeleted = 0)
    BEGIN
        RAISERROR('This form is already used as a follow-up form and cannot have its own follow-up.', 16, 1);
        RETURN;
    END;

    -- Follow-up cannot itself be a primary that has a follow-up (no chains)
    IF EXISTS (
        SELECT 1 FROM dbo.form_followup_configs
        WHERE tenantid = @tenantid
          AND primaryformid = @followupformid
          AND isdeleted = 0)
    BEGIN
        RAISERROR('Selected follow-up form already has its own follow-up configured.', 16, 1);
        RETURN;
    END;

    -- Follow-up cannot already be used by a different primary
    IF EXISTS (
        SELECT 1 FROM dbo.form_followup_configs
        WHERE tenantid = @tenantid
          AND followupformid = @followupformid
          AND primaryformid <> @primaryformid
          AND isdeleted = 0)
    BEGIN
        RAISERROR('Selected follow-up form is already linked to another primary form.', 16, 1);
        RETURN;
    END;

    IF @allowmultiple IS NULL SET @allowmultiple = 1;

    BEGIN TRANSACTION;

    DECLARE @existingid UNIQUEIDENTIFIER;

    SELECT @existingid = formfollowupconfigid
    FROM dbo.form_followup_configs
    WHERE tenantid = @tenantid
      AND primaryformid = @primaryformid
      AND isdeleted = 0;

    IF @existingid IS NOT NULL
    BEGIN
        UPDATE dbo.form_followup_configs
        SET followupformid = @followupformid,
            allowmultiple = @allowmultiple,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE formfollowupconfigid = @existingid;

        COMMIT TRANSACTION;
        SELECT @existingid AS formfollowupconfigid;
        RETURN;
    END;

    DECLARE @newid UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.form_followup_configs
        (formfollowupconfigid, tenantid, primaryformid, followupformid, allowmultiple, createdby, updatedby)
    VALUES
        (@newid, @tenantid, @primaryformid, @followupformid, @allowmultiple, @actorid, @actorid);

    COMMIT TRANSACTION;
    SELECT @newid AS formfollowupconfigid;
END
GO
