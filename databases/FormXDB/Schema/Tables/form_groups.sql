IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_groups' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_groups (
        formgroupid           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_groups PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        formid                UNIQUEIDENTIFIER NOT NULL,
        groupname             NVARCHAR(200)    NOT NULL,
        groupdisplaynamekey   NVARCHAR(200)    NULL,
        displayorder          INT              NOT NULL CONSTRAINT DF_form_groups_displayorder DEFAULT (0),
        isdeleted             BIT              NOT NULL CONSTRAINT DF_form_groups_isdeleted DEFAULT (0),
        createdby             UNIQUEIDENTIFIER NULL,
        createdat             DATETIME2        NOT NULL CONSTRAINT DF_form_groups_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby             UNIQUEIDENTIFIER NULL,
        updatedat             DATETIME2        NOT NULL CONSTRAINT DF_form_groups_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_groups_form FOREIGN KEY (formid) REFERENCES dbo.forms (formid)
    );

    CREATE UNIQUE INDEX UQ_form_groups_form_name
        ON dbo.form_groups (formid, groupname)
        WHERE isdeleted = 0;

    CREATE INDEX IX_form_groups_formid ON dbo.form_groups (formid, displayorder) WHERE isdeleted = 0;
END
GO
