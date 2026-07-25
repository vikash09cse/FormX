CREATE OR ALTER PROCEDURE dbo.sp_role_get_list
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT roleid, name, isleader, rolestatus AS status, createdat, updatedat
    FROM dbo.roles
    WHERE tenantid = @tenantid AND isdeleted = 0
    ORDER BY name;
END
GO
