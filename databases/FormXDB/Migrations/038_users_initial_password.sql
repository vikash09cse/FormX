-- Admin-visible initial password (cleared when user changes password).
IF COL_LENGTH('dbo.users', 'initialpassword') IS NULL
BEGIN
    ALTER TABLE dbo.users ADD initialpassword NVARCHAR(100) NULL;
END
GO
