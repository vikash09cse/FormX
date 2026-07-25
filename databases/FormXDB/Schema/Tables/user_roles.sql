IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_roles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.user_roles (
        userid      UNIQUEIDENTIFIER NOT NULL,
        roleid      UNIQUEIDENTIFIER NOT NULL,
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_user_roles_createdat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_user_roles PRIMARY KEY (userid, roleid),
        CONSTRAINT FK_user_roles_user FOREIGN KEY (userid) REFERENCES dbo.users (userid),
        CONSTRAINT FK_user_roles_role FOREIGN KEY (roleid) REFERENCES dbo.roles (roleid)
    );

    CREATE INDEX IX_user_roles_roleid ON dbo.user_roles (roleid);
END
GO
