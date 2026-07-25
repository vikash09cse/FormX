IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_fields' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_fields (
        fieldid                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_fields PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        formid                    UNIQUEIDENTIFIER NOT NULL,
        formgroupid               UNIQUEIDENTIFIER NOT NULL,
        controllabel              NVARCHAR(300)    NOT NULL,
        controltype               TINYINT          NOT NULL,
        controlmaxlength          INT              NULL,
        controlrequired           BIT              NOT NULL CONSTRAINT DF_form_fields_controlrequired DEFAULT (0),
        displayorder              INT              NOT NULL CONSTRAINT DF_form_fields_displayorder DEFAULT (0),
        controlnotes              NVARCHAR(1000)   NULL,
        fieldkey                  NVARCHAR(200)    NOT NULL,
        displaycontrollabel       NVARCHAR(50)     NOT NULL CONSTRAINT DF_form_fields_displaycontrollabel DEFAULT (N'visible'),
        classname                 NVARCHAR(100)    NULL,
        parentfieldid             UNIQUEIDENTIFIER NULL,
        issendemailnotification   BIT              NOT NULL CONSTRAINT DF_form_fields_issendemailnotification DEFAULT (0),
        validationregexpresetid   UNIQUEIDENTIFIER NULL,
        displayonlist             BIT              NOT NULL CONSTRAINT DF_form_fields_displayonlist DEFAULT (0),
        isdeleted                 BIT              NOT NULL CONSTRAINT DF_form_fields_isdeleted DEFAULT (0),
        createdby                 UNIQUEIDENTIFIER NULL,
        createdat                 DATETIME2        NOT NULL CONSTRAINT DF_form_fields_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby                 UNIQUEIDENTIFIER NULL,
        updatedat                 DATETIME2        NOT NULL CONSTRAINT DF_form_fields_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_fields_form FOREIGN KEY (formid) REFERENCES dbo.forms (formid),
        CONSTRAINT FK_form_fields_form_group FOREIGN KEY (formgroupid) REFERENCES dbo.form_groups (formgroupid),
        CONSTRAINT FK_form_fields_parent_field FOREIGN KEY (parentfieldid) REFERENCES dbo.form_fields (fieldid),
        CONSTRAINT FK_form_fields_validation_regex_preset FOREIGN KEY (validationregexpresetid) REFERENCES dbo.validation_regex_presets (validationregexpresetid)
    );

    CREATE UNIQUE INDEX UQ_form_fields_form_fieldkey
        ON dbo.form_fields (formid, fieldkey)
        WHERE isdeleted = 0;

    CREATE INDEX IX_form_fields_formid ON dbo.form_fields (formid, displayorder) WHERE isdeleted = 0;
    CREATE INDEX IX_form_fields_formgroupid ON dbo.form_fields (formgroupid, displayorder) WHERE isdeleted = 0;
    CREATE INDEX IX_form_fields_parentfieldid ON dbo.form_fields (parentfieldid) WHERE parentfieldid IS NOT NULL AND isdeleted = 0;
    CREATE INDEX IX_form_fields_displayonlist ON dbo.form_fields (formid) WHERE displayonlist = 1 AND isdeleted = 0;
END
GO
