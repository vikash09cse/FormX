IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_roles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_roles (
        formid      UNIQUEIDENTIFIER NOT NULL,
        roleid      UNIQUEIDENTIFIER NOT NULL,
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_form_roles_createdat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_form_roles PRIMARY KEY (formid, roleid),
        CONSTRAINT FK_form_roles_form FOREIGN KEY (formid) REFERENCES dbo.forms (formid),
        CONSTRAINT FK_form_roles_role FOREIGN KEY (roleid) REFERENCES dbo.roles (roleid)
    );

    CREATE INDEX IX_form_roles_roleid ON dbo.form_roles (roleid);
END
GO
