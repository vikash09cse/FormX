CREATE OR ALTER PROCEDURE dbo.sp_role_name_exists
    @tenantid  UNIQUEIDENTIFIER,
    @name      NVARCHAR(100),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.roles
    WHERE tenantid = @tenantid AND isdeleted = 0 AND name = @name
      AND (@excludeid IS NULL OR roleid <> @excludeid);
END
GO
