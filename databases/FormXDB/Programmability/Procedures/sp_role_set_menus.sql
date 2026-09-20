CREATE OR ALTER PROCEDURE dbo.sp_role_set_menus
    @roleid   UNIQUEIDENTIFIER,
    @menukeys NVARCHAR(MAX) -- comma-separated
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.role_menus WHERE roleid = @roleid;

    IF @menukeys IS NULL OR LTRIM(RTRIM(@menukeys)) = N''
        RETURN;

    INSERT INTO dbo.role_menus (roleid, menukey)
    SELECT DISTINCT @roleid, LTRIM(RTRIM(value))
    FROM STRING_SPLIT(@menukeys, N',')
    WHERE LTRIM(RTRIM(value)) <> N'';
END
GO
