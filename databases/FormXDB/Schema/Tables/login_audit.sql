IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'login_audit' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.login_audit (
        loginauditid    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_login_audit PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid        UNIQUEIDENTIFIER NULL,
        useridentifier  NVARCHAR(100)    NOT NULL,
        logintype       TINYINT          NOT NULL,
        issuccess       BIT              NOT NULL,
        failurereason   NVARCHAR(200)    NULL,
        ipaddress       NVARCHAR(45)     NULL,
        createdat       DATETIME2        NOT NULL CONSTRAINT DF_login_audit_createdat DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_login_audit_tenantid ON dbo.login_audit (tenantid, createdat DESC);
END
GO
