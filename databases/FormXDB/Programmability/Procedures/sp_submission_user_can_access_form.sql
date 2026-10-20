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
        FROM dbo.forms f
        INNER JOIN dbo.user_scopes us ON us.userid = @userid AND us.projectid = f.projectid
        INNER JOIN dbo.form_roles fr ON fr.formid = f.formid
        INNER JOIN dbo.user_roles ur ON ur.roleid = fr.roleid AND ur.userid = @userid
        INNER JOIN dbo.roles r ON r.roleid = ur.roleid AND r.tenantid = @tenantid AND r.isdeleted = 0 AND r.rolestatus = 1
        WHERE f.tenantid = @tenantid
          AND f.formid = @formid
          AND f.isdeleted = 0
          AND f.status = 1)
        SELECT CAST(1 AS BIT) AS hasaccess;
    ELSE
        SELECT CAST(0 AS BIT) AS hasaccess;
END
GO
