CREATE OR ALTER PROCEDURE dbo.sp_platform_user_email_exists
    @email      NVARCHAR(100),
    @excludeid  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1) AS emailcount
    FROM dbo.users
    WHERE tenantid IS NULL
      AND email = @email
      AND isdeleted = 0
      AND (@excludeid IS NULL OR userid <> @excludeid);
END
GO
