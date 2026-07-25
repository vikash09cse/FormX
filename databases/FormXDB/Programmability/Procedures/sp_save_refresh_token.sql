CREATE OR ALTER PROCEDURE dbo.sp_save_refresh_token
    @userid     UNIQUEIDENTIFIER,
    @tenantid   UNIQUEIDENTIFIER = NULL,
    @tokenhash  NVARCHAR(256),
    @expiresat  DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.refresh_tokens (userid, tenantid, tokenhash, expiresat)
    VALUES (@userid, @tenantid, @tokenhash, @expiresat);
END
GO
