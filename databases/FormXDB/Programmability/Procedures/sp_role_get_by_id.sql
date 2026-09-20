CREATE OR ALTER PROCEDURE dbo.sp_role_get_by_id
    @tenantid UNIQUEIDENTIFIER,
    @roleid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT roleid, name, isleader, datascope, cancreate, canedit, candelete,
           rolestatus AS status, createdat, updatedat
    FROM dbo.roles
    WHERE tenantid = @tenantid AND roleid = @roleid AND isdeleted = 0;
END
GO
