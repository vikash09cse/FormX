CREATE OR ALTER PROCEDURE dbo.sp_form_group_delete
    @tenantid    UNIQUEIDENTIFIER,
    @formgroupid UNIQUEIDENTIFIER,
    @updatedby   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE g
    SET g.isdeleted = 1, g.updatedby = @updatedby, g.updatedat = SYSUTCDATETIME()
    FROM dbo.form_groups g
    INNER JOIN dbo.forms f ON f.formid = g.formid
    WHERE f.tenantid = @tenantid AND g.formgroupid = @formgroupid AND g.isdeleted = 0 AND f.isdeleted = 0;
END
GO
