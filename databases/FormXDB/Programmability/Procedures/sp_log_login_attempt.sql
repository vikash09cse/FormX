CREATE OR ALTER PROCEDURE dbo.sp_log_login_attempt
    @tenantid       UNIQUEIDENTIFIER = NULL,
    @useridentifier NVARCHAR(100),
    @logintype      TINYINT,
    @issuccess      BIT,
    @failurereason  NVARCHAR(200) = NULL,
    @ipaddress      NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.login_audit (tenantid, useridentifier, logintype, issuccess, failurereason, ipaddress)
    VALUES (@tenantid, @useridentifier, @logintype, @issuccess, @failurereason, @ipaddress);
END
GO
