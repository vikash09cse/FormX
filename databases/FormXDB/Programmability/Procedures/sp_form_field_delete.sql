CREATE OR ALTER PROCEDURE dbo.sp_form_field_delete
    @tenantid  UNIQUEIDENTIFIER,
    @fieldid   UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ff
    SET ff.isdeleted = 1, ff.updatedby = @updatedby, ff.updatedat = SYSUTCDATETIME()
    FROM dbo.form_fields ff
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND ff.fieldid = @fieldid AND ff.isdeleted = 0 AND f.isdeleted = 0;

    UPDATE o
    SET o.isdeleted = 1, o.updatedby = @updatedby, o.updatedat = SYSUTCDATETIME()
    FROM dbo.form_field_options o
    INNER JOIN dbo.form_fields ff ON ff.fieldid = o.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND o.fieldid = @fieldid AND o.isdeleted = 0;

    DELETE po
    FROM dbo.form_field_parent_options po
    INNER JOIN dbo.form_fields ff ON ff.fieldid = po.fieldid
    INNER JOIN dbo.forms f ON f.formid = ff.formid
    WHERE f.tenantid = @tenantid AND po.fieldid = @fieldid;
END
GO
