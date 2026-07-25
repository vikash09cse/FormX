CREATE OR ALTER PROCEDURE dbo.sp_create_platform_user
    @userid       UNIQUEIDENTIFIER,
    @email        NVARCHAR(100),
    @passwordhash NVARCHAR(256),
    @firstname    NVARCHAR(100),
    @lastname     NVARCHAR(100),
    @usertype     TINYINT,
    @userstatus   TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.users (userid, tenantid, email, passwordhash, firstname, lastname, usertype, userstatus)
    VALUES (@userid, NULL, @email, @passwordhash, @firstname, @lastname, @usertype, @userstatus);

    SELECT @userid AS userid;
END
GO
