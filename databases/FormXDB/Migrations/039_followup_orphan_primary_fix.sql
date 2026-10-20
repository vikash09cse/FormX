SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Soft-delete follow-up mappings whose primary (or follow-up) form is already deleted.
-- Orphaned mappings were hiding role-assigned forms from My Forms.
UPDATE c
SET c.isdeleted = 1,
    c.updatedat = SYSUTCDATETIME()
FROM dbo.form_followup_configs c
WHERE c.isdeleted = 0
  AND (
      EXISTS (
          SELECT 1 FROM dbo.forms pf
          WHERE pf.formid = c.primaryformid AND pf.isdeleted = 1
      )
      OR EXISTS (
          SELECT 1 FROM dbo.forms ff
          WHERE ff.formid = c.followupformid AND ff.isdeleted = 1
      )
  );
GO

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
              SELECT 1
              FROM dbo.form_followup_configs c
              INNER JOIN dbo.forms pf
                  ON pf.formid = c.primaryformid
                 AND pf.tenantid = c.tenantid
                 AND pf.isdeleted = 0
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
          SELECT 1
          FROM dbo.form_followup_configs c
          INNER JOIN dbo.forms pf
              ON pf.formid = c.primaryformid
             AND pf.tenantid = c.tenantid
             AND pf.isdeleted = 0
          WHERE c.tenantid = @tenantid
            AND c.followupformid = f.formid
            AND c.isdeleted = 0)
    ORDER BY f.displayorder, f.name;
END
GO

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

    UPDATE dbo.form_followup_configs
    SET isdeleted = 1,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND isdeleted = 0
      AND (primaryformid = @formid OR followupformid = @formid);
END
GO
