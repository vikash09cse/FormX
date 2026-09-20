IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_followup_configs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_followup_configs (
        formfollowupconfigid UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_followup_configs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid             UNIQUEIDENTIFIER NOT NULL,
        primaryformid        UNIQUEIDENTIFIER NOT NULL,
        followupformid       UNIQUEIDENTIFIER NOT NULL,
        allowmultiple        BIT              NOT NULL CONSTRAINT DF_form_followup_configs_allowmultiple DEFAULT (1),
        isdeleted            BIT              NOT NULL CONSTRAINT DF_form_followup_configs_isdeleted DEFAULT (0),
        createdby            UNIQUEIDENTIFIER NULL,
        createdat            DATETIME2        NOT NULL CONSTRAINT DF_form_followup_configs_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby            UNIQUEIDENTIFIER NULL,
        updatedat            DATETIME2        NOT NULL CONSTRAINT DF_form_followup_configs_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_followup_configs_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_form_followup_configs_primary FOREIGN KEY (primaryformid) REFERENCES dbo.forms (formid),
        CONSTRAINT FK_form_followup_configs_followup FOREIGN KEY (followupformid) REFERENCES dbo.forms (formid),
        CONSTRAINT CK_form_followup_configs_distinct CHECK (primaryformid <> followupformid)
    );

    CREATE UNIQUE INDEX UQ_form_followup_configs_primary
        ON dbo.form_followup_configs (primaryformid)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_form_followup_configs_followup
        ON dbo.form_followup_configs (followupformid)
        WHERE isdeleted = 0;

    CREATE INDEX IX_form_followup_configs_tenant
        ON dbo.form_followup_configs (tenantid)
        WHERE isdeleted = 0;
END
GO
