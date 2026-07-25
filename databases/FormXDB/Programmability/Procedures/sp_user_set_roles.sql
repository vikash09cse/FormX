CREATE OR ALTER PROCEDURE dbo.sp_user_set_roles
    @tenantid  UNIQUEIDENTIFIER,
    @userid    UNIQUEIDENTIFIER,
    @roleids   NVARCHAR(MAX),
    @createdby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE tenantid = @tenantid AND userid = @userid AND isdeleted = 0)
        RETURN;

    DELETE FROM dbo.user_roles WHERE userid = @userid;

    IF @roleids IS NULL OR LTRIM(RTRIM(@roleids)) = ''
        RETURN;

    INSERT INTO dbo.user_roles (userid, roleid, createdby)
    SELECT @userid, TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER), @createdby
    FROM STRING_SPLIT(@roleids, ',')
    WHERE TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER) IS NOT NULL
      AND EXISTS (
          SELECT 1 FROM dbo.roles r
          WHERE r.roleid = TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
            AND r.tenantid = @tenantid AND r.isdeleted = 0);
END
GO
