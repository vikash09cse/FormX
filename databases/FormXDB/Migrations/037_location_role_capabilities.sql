-- Role capabilities (replace isleader usage)
IF COL_LENGTH('dbo.roles', 'datascope') IS NULL
BEGIN
    ALTER TABLE dbo.roles ADD datascope TINYINT NOT NULL
        CONSTRAINT DF_roles_datascope DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.roles', 'cancreate') IS NULL
BEGIN
    ALTER TABLE dbo.roles ADD cancreate BIT NOT NULL
        CONSTRAINT DF_roles_cancreate DEFAULT (1);
END
GO

IF COL_LENGTH('dbo.roles', 'canedit') IS NULL
BEGIN
    ALTER TABLE dbo.roles ADD canedit BIT NOT NULL
        CONSTRAINT DF_roles_canedit DEFAULT (1);
END
GO

IF COL_LENGTH('dbo.roles', 'candelete') IS NULL
BEGIN
    ALTER TABLE dbo.roles ADD candelete BIT NOT NULL
        CONSTRAINT DF_roles_candelete DEFAULT (1);
END
GO

-- Backfill from isleader: leaders get Project scope view-style defaults are not assumed;
-- leave datascope=Own unless already set via seed.
GO

-- Form collect location: existing rows default off (0); new inserts default on (1)
IF COL_LENGTH('dbo.forms', 'collectlocation') IS NULL
BEGIN
    ALTER TABLE dbo.forms ADD collectlocation BIT NOT NULL
        CONSTRAINT DF_forms_collectlocation DEFAULT (0);

    ALTER TABLE dbo.forms DROP CONSTRAINT DF_forms_collectlocation;
    ALTER TABLE dbo.forms ADD CONSTRAINT DF_forms_collectlocation DEFAULT (1) FOR collectlocation;
END
GO

-- Submission location FKs
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

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_submissions_state')
   AND COL_LENGTH('dbo.form_submissions', 'stateid') IS NOT NULL
   AND OBJECT_ID('dbo.states') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_submissions
        ADD CONSTRAINT FK_form_submissions_state
        FOREIGN KEY (stateid) REFERENCES dbo.states (stateid);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_submissions_district')
   AND COL_LENGTH('dbo.form_submissions', 'districtid') IS NOT NULL
   AND OBJECT_ID('dbo.districts') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_submissions
        ADD CONSTRAINT FK_form_submissions_district
        FOREIGN KEY (districtid) REFERENCES dbo.districts (districtid);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_submissions_block')
   AND COL_LENGTH('dbo.form_submissions', 'blockid') IS NOT NULL
   AND OBJECT_ID('dbo.blocks') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_submissions
        ADD CONSTRAINT FK_form_submissions_block
        FOREIGN KEY (blockid) REFERENCES dbo.blocks (blockid);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_submissions_village')
   AND COL_LENGTH('dbo.form_submissions', 'villageid') IS NOT NULL
   AND OBJECT_ID('dbo.villages') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_submissions
        ADD CONSTRAINT FK_form_submissions_village
        FOREIGN KEY (villageid) REFERENCES dbo.villages (villageid);
END
GO

IF COL_LENGTH('dbo.form_submissions', 'districtid') IS NOT NULL
   AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_form_submissions_tenant_district' AND object_id = OBJECT_ID('dbo.form_submissions'))
BEGIN
    CREATE INDEX IX_form_submissions_tenant_district
        ON dbo.form_submissions (tenantid, districtid)
        WHERE isdeleted = 0 AND districtid IS NOT NULL;
END
GO
