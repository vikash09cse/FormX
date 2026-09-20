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
        stateid      UNIQUEIDENTIFIER NULL,
        districtid   UNIQUEIDENTIFIER NULL,
        blockid      UNIQUEIDENTIFIER NULL,
        villageid    UNIQUEIDENTIFIER NULL,
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

-- Parent linkage for follow-up submissions (safe on existing DBs)
IF COL_LENGTH('dbo.form_submissions', 'parentsubmissionid') IS NULL
BEGIN
    ALTER TABLE dbo.form_submissions ADD parentsubmissionid UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_submissions_parent')
   AND COL_LENGTH('dbo.form_submissions', 'parentsubmissionid') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_submissions
        ADD CONSTRAINT FK_form_submissions_parent
        FOREIGN KEY (parentsubmissionid) REFERENCES dbo.form_submissions (submissionid);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_form_submissions_parent' AND object_id = OBJECT_ID('dbo.form_submissions'))
   AND COL_LENGTH('dbo.form_submissions', 'parentsubmissionid') IS NOT NULL
BEGIN
    CREATE INDEX IX_form_submissions_parent
        ON dbo.form_submissions (parentsubmissionid, submittedat DESC)
        WHERE isdeleted = 0 AND parentsubmissionid IS NOT NULL;
END
GO

-- Location columns (safe when tables already exist without them)
IF COL_LENGTH('dbo.form_submissions', 'stateid') IS NULL
BEGIN
    ALTER TABLE dbo.form_submissions ADD stateid UNIQUEIDENTIFIER NULL;
END
GO

IF COL_LENGTH('dbo.form_submissions', 'districtid') IS NULL
BEGIN
    ALTER TABLE dbo.form_submissions ADD districtid UNIQUEIDENTIFIER NULL;
END
GO

IF COL_LENGTH('dbo.form_submissions', 'blockid') IS NULL
BEGIN
    ALTER TABLE dbo.form_submissions ADD blockid UNIQUEIDENTIFIER NULL;
END
GO

IF COL_LENGTH('dbo.form_submissions', 'villageid') IS NULL
BEGIN
    ALTER TABLE dbo.form_submissions ADD villageid UNIQUEIDENTIFIER NULL;
END
GO

-- Index only after column exists (do not put inside CREATE TABLE IF NOT EXISTS — SQL Server
-- still compiles CREATE INDEX against the live table and fails if districtid is missing).
IF COL_LENGTH('dbo.form_submissions', 'districtid') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'IX_form_submissions_tenant_district'
          AND object_id = OBJECT_ID('dbo.form_submissions'))
BEGIN
    CREATE INDEX IX_form_submissions_tenant_district
        ON dbo.form_submissions (tenantid, districtid)
        WHERE isdeleted = 0 AND districtid IS NOT NULL;
END
GO
