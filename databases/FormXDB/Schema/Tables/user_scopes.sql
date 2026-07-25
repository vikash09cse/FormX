IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_scopes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.user_scopes (
        userid      UNIQUEIDENTIFIER NOT NULL,
        projectid   UNIQUEIDENTIFIER NOT NULL,
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_user_scopes_createdat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_user_scopes PRIMARY KEY (userid, projectid),
        CONSTRAINT FK_user_scopes_user FOREIGN KEY (userid) REFERENCES dbo.users (userid),
        CONSTRAINT FK_user_scopes_project FOREIGN KEY (projectid) REFERENCES dbo.projects (projectid)
    );

    CREATE INDEX IX_user_scopes_projectid ON dbo.user_scopes (projectid);
END
GO
