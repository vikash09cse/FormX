IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'refresh_tokens' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.refresh_tokens (
        refreshtokenid  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_refresh_tokens PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        userid          UNIQUEIDENTIFIER NOT NULL,
        tenantid        UNIQUEIDENTIFIER NULL,
        tokenhash       NVARCHAR(256)    NOT NULL,
        expiresat       DATETIME2        NOT NULL,
        isrevoked       BIT              NOT NULL CONSTRAINT DF_refresh_tokens_isrevoked DEFAULT (0),
        createdat       DATETIME2        NOT NULL CONSTRAINT DF_refresh_tokens_createdat DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_refresh_tokens_user ON dbo.refresh_tokens (userid, tenantid) WHERE isrevoked = 0;
END
GO
