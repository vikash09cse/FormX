CREATE OR ALTER PROCEDURE dbo.sp_role_get_menus
    @roleid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT menukey FROM dbo.role_menus WHERE roleid = @roleid ORDER BY menukey;
END
GO
