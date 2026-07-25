-- Bind each form to exactly one project.

IF COL_LENGTH('dbo.forms', 'projectid') IS NULL
BEGIN
    ALTER TABLE dbo.forms ADD projectid UNIQUEIDENTIFIER NULL;
END
GO

-- Backfill: first active project per tenant (by name)
IF COL_LENGTH('dbo.forms', 'projectid') IS NOT NULL
BEGIN
    UPDATE f
    SET f.projectid = p.projectid
    FROM dbo.forms f
    CROSS APPLY (
        SELECT TOP (1) pr.projectid
        FROM dbo.projects pr
        WHERE pr.tenantid = f.tenantid AND pr.isdeleted = 0 AND pr.status = 1
        ORDER BY pr.projectname
    ) p
    WHERE f.projectid IS NULL AND f.isdeleted = 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_forms_project')
   AND COL_LENGTH('dbo.forms', 'projectid') IS NOT NULL
BEGIN
    ALTER TABLE dbo.forms
        ADD CONSTRAINT FK_forms_project
        FOREIGN KEY (projectid) REFERENCES dbo.projects (projectid);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_forms_projectid' AND object_id = OBJECT_ID('dbo.forms'))
   AND COL_LENGTH('dbo.forms', 'projectid') IS NOT NULL
BEGIN
    CREATE INDEX IX_forms_projectid ON dbo.forms (tenantid, projectid) WHERE isdeleted = 0;
END
GO
