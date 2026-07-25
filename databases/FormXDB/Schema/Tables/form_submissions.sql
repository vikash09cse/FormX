IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_submissions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_submissions (
        submissionid UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_submissions PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        formid       UNIQUEIDENTIFIER NOT NULL,
        tenantid     UNIQUEIDENTIFIER NOT NULL,
        projectid    UNIQUEIDENTIFIER NOT NULL,
        submittedby  UNIQUEIDENTIFIER NOT NULL,
        submittedat  DATETIME2        NOT NULL CONSTRAINT DF_form_submissions_submittedat DEFAULT (SYSUTCDATETIME()),
        status       TINYINT          NOT NULL CONSTRAINT DF_form_submissions_status DEFAULT (1),
        isdeleted    BIT              NOT NULL CONSTRAINT DF_form_submissions_isdeleted DEFAULT (0),
        createdby    UNIQUEIDENTIFIER NULL,
        createdat    DATETIME2        NOT NULL CONSTRAINT DF_form_submissions_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby    UNIQUEIDENTIFIER NULL,
        updatedat    DATETIME2        NOT NULL CONSTRAINT DF_form_submissions_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_submissions_form FOREIGN KEY (formid) REFERENCES dbo.forms (formid),
        CONSTRAINT FK_form_submissions_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_form_submissions_project FOREIGN KEY (projectid) REFERENCES dbo.projects (projectid),
        CONSTRAINT FK_form_submissions_user FOREIGN KEY (submittedby) REFERENCES dbo.users (userid)
    );

    CREATE INDEX IX_form_submissions_form ON dbo.form_submissions (formid, submittedat DESC) WHERE isdeleted = 0;
    CREATE INDEX IX_form_submissions_tenant_project ON dbo.form_submissions (tenantid, projectid) WHERE isdeleted = 0;
    CREATE INDEX IX_form_submissions_submittedby ON dbo.form_submissions (submittedby) WHERE isdeleted = 0;
END
GO
