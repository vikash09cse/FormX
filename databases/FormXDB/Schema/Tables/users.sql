IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.users (
        userid          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_users PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid        UNIQUEIDENTIFIER NULL,
        email           NVARCHAR(100)    NOT NULL,
        passwordhash    NVARCHAR(256)    NOT NULL,
        firstname       NVARCHAR(100)    NOT NULL,
        lastname        NVARCHAR(100)    NOT NULL,
        designation     NVARCHAR(100)    NULL,
        usertype        TINYINT          NOT NULL,
        userstatus      TINYINT          NOT NULL CONSTRAINT DF_users_userstatus DEFAULT (1),
        lastloginat     DATETIME2        NULL,
        isdeleted       BIT              NOT NULL CONSTRAINT DF_users_isdeleted DEFAULT (0),
        createdby       UNIQUEIDENTIFIER NULL,
        createdat       DATETIME2        NOT NULL CONSTRAINT DF_users_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby       UNIQUEIDENTIFIER NULL,
        updatedat       DATETIME2        NOT NULL CONSTRAINT DF_users_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_users_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid)
    );

    CREATE UNIQUE INDEX UQ_users_platform_email
        ON dbo.users (email)
        WHERE tenantid IS NULL AND isdeleted = 0;

    CREATE UNIQUE INDEX UQ_users_tenant_email
        ON dbo.users (tenantid, email)
        WHERE tenantid IS NOT NULL AND isdeleted = 0;

    CREATE INDEX IX_users_tenantid ON dbo.users (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_users_usertype ON dbo.users (usertype) WHERE isdeleted = 0;
END
GO
