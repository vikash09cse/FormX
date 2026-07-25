CREATE OR ALTER PROCEDURE dbo.sp_form_get_role_ids
    @tenantid UNIQUEIDENTIFIER,
    @formid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT fr.roleid
    FROM dbo.form_roles fr
    INNER JOIN dbo.forms f ON f.formid = fr.formid
    WHERE f.tenantid = @tenantid AND f.formid = @formid AND f.isdeleted = 0;
END
GO
