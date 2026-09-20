CREATE OR ALTER PROCEDURE dbo.sp_submission_available_forms
    @tenantid     UNIQUEIDENTIFIER,
    @userid       UNIQUEIDENTIFIER,
    @issuperadmin BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @issuperadmin = 1
    BEGIN
        SELECT f.formid, f.name, f.description, f.displayorder, f.collectlocation
        FROM dbo.forms f
        WHERE f.tenantid = @tenantid
          AND f.isdeleted = 0
          AND f.status = 1
          AND NOT EXISTS (
              SELECT 1 FROM dbo.form_followup_configs c
              WHERE c.tenantid = @tenantid
                AND c.followupformid = f.formid
                AND c.isdeleted = 0)
        ORDER BY f.displayorder, f.name;
        RETURN;
    END;

    SELECT DISTINCT f.formid, f.name, f.description, f.displayorder, f.collectlocation
    FROM dbo.forms f
    INNER JOIN dbo.form_roles fr ON fr.formid = f.formid
    INNER JOIN dbo.user_roles ur ON ur.roleid = fr.roleid AND ur.userid = @userid
    INNER JOIN dbo.roles r ON r.roleid = ur.roleid AND r.tenantid = @tenantid AND r.isdeleted = 0 AND r.rolestatus = 1
    WHERE f.tenantid = @tenantid
      AND f.isdeleted = 0
      AND f.status = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.form_followup_configs c
          WHERE c.tenantid = @tenantid
            AND c.followupformid = f.formid
            AND c.isdeleted = 0)
    ORDER BY f.displayorder, f.name;
END
GO
