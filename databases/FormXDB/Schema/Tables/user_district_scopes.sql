IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_district_scopes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.user_district_scopes (
        userid      UNIQUEIDENTIFIER NOT NULL,
        districtid  UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_user_district_scopes PRIMARY KEY (userid, districtid),
        CONSTRAINT FK_user_district_scopes_user FOREIGN KEY (userid) REFERENCES dbo.users (userid),
        CONSTRAINT FK_user_district_scopes_district FOREIGN KEY (districtid) REFERENCES dbo.districts (districtid)
    );

    CREATE INDEX IX_user_district_scopes_district
        ON dbo.user_district_scopes (districtid);
END
GO
