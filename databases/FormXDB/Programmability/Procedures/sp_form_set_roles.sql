CREATE OR ALTER PROCEDURE dbo.sp_form_set_roles
    @tenantid  UNIQUEIDENTIFIER,
    @formid    UNIQUEIDENTIFIER,
    @roleids   NVARCHAR(MAX), -- comma-separated GUIDs
    @createdby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.forms WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0)
        RETURN;

    DELETE FROM dbo.form_roles WHERE formid = @formid;

    IF @roleids IS NULL OR LTRIM(RTRIM(@roleids)) = ''
        RETURN;

    INSERT INTO dbo.form_roles (formid, roleid, createdby)
    SELECT @formid, TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER), @createdby
    FROM STRING_SPLIT(@roleids, ',')
    WHERE TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER) IS NOT NULL
      AND EXISTS (
          SELECT 1 FROM dbo.roles r
          WHERE r.roleid = TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
            AND r.tenantid = @tenantid AND r.isdeleted = 0);
END
GO
