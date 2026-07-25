CREATE OR ALTER PROCEDURE dbo.sp_submission_user_can_access_form
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @formid       UNIQUEIDENTIFIER,
    @issuperadmin BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.forms
        WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0 AND status = 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS hasaccess;
        RETURN;
    END;

    IF @issuperadmin = 1
    BEGIN
        SELECT CAST(1 AS BIT) AS hasaccess;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.form_roles fr
        INNER JOIN dbo.user_roles ur ON ur.roleid = fr.roleid AND ur.userid = @userid
        INNER JOIN dbo.roles r ON r.roleid = ur.roleid AND r.tenantid = @tenantid AND r.isdeleted = 0 AND r.rolestatus = 1
        WHERE fr.formid = @formid)
        SELECT CAST(1 AS BIT) AS hasaccess;
    ELSE
        SELECT CAST(0 AS BIT) AS hasaccess;
END
GO
