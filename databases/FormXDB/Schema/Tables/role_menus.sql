IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'role_menus' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.role_menus (
        roleid  UNIQUEIDENTIFIER NOT NULL,
        menukey NVARCHAR(50)     NOT NULL,
        CONSTRAINT PK_role_menus PRIMARY KEY (roleid, menukey),
        CONSTRAINT FK_role_menus_role FOREIGN KEY (roleid) REFERENCES dbo.roles (roleid)
    );

    CREATE INDEX IX_role_menus_menukey ON dbo.role_menus (menukey);
END
GO
