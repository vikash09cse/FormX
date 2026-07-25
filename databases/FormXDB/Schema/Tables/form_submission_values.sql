IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_submission_values' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_submission_values (
        submissionvalueid UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_submission_values PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        submissionid      UNIQUEIDENTIFIER NOT NULL,
        fieldid           UNIQUEIDENTIFIER NOT NULL,
        valuetext         NVARCHAR(MAX)    NULL,
        createdat         DATETIME2        NOT NULL CONSTRAINT DF_form_submission_values_createdat DEFAULT (SYSUTCDATETIME()),
        updatedat         DATETIME2        NOT NULL CONSTRAINT DF_form_submission_values_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_submission_values_submission FOREIGN KEY (submissionid) REFERENCES dbo.form_submissions (submissionid),
        CONSTRAINT FK_form_submission_values_field FOREIGN KEY (fieldid) REFERENCES dbo.form_fields (fieldid),
        CONSTRAINT UQ_form_submission_values_submission_field UNIQUE (submissionid, fieldid)
    );

    CREATE INDEX IX_form_submission_values_fieldid ON dbo.form_submission_values (fieldid);
END
GO
