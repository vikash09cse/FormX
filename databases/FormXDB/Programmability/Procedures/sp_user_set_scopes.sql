CREATE OR ALTER PROCEDURE dbo.sp_user_set_scopes
    @tenantid   UNIQUEIDENTIFIER,
    @userid     UNIQUEIDENTIFIER,
    @projectids NVARCHAR(MAX),
    @createdby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE tenantid = @tenantid AND userid = @userid AND isdeleted = 0)
        RETURN;

    DELETE FROM dbo.user_scopes WHERE userid = @userid;

    IF @projectids IS NULL OR LTRIM(RTRIM(@projectids)) = ''
        RETURN;

    INSERT INTO dbo.user_scopes (userid, projectid, createdby)
    SELECT @userid, TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER), @createdby
    FROM STRING_SPLIT(@projectids, ',')
    WHERE TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER) IS NOT NULL
      AND EXISTS (
          SELECT 1 FROM dbo.projects p
          WHERE p.projectid = TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
            AND p.tenantid = @tenantid AND p.isdeleted = 0);
END
GO
