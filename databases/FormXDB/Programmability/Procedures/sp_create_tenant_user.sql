CREATE OR ALTER PROCEDURE dbo.sp_create_tenant_user
    @userid       UNIQUEIDENTIFIER,
    @tenantid     UNIQUEIDENTIFIER,
    @email        NVARCHAR(100),
    @passwordhash NVARCHAR(256),
    @firstname    NVARCHAR(100),
    @lastname     NVARCHAR(100),
    @designation  NVARCHAR(100) = NULL,
    @usertype     TINYINT,
    @userstatus   TINYINT,
    @createdby    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.users (
        userid, tenantid, email, passwordhash, firstname, lastname, designation, usertype, userstatus, createdby)
    VALUES (
        @userid, @tenantid, @email, @passwordhash, @firstname, @lastname, @designation, @usertype, @userstatus, @createdby);

    SELECT @userid AS userid;
END
GO
