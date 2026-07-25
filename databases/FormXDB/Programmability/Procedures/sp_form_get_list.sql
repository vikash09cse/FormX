CREATE OR ALTER PROCEDURE dbo.sp_form_get_list
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        f.formid,
        f.projectid,
        p.projectname,
        f.name,
        f.description,
        f.status,
        f.displayorder,
        f.createdat,
        f.updatedat
    FROM dbo.forms f
    LEFT JOIN dbo.projects p ON p.projectid = f.projectid AND p.tenantid = f.tenantid AND p.isdeleted = 0
    WHERE f.tenantid = @tenantid AND f.isdeleted = 0
    ORDER BY f.displayorder, f.name;
END
GO
