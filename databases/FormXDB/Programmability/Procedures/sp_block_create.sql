CREATE OR ALTER PROCEDURE dbo.sp_block_create
    @blockid    UNIQUEIDENTIFIER,
    @tenantid   UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER,
    @name       NVARCHAR(200),
    @code       NVARCHAR(50),
    @status     TINYINT,
    @createdby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.districts WHERE tenantid = @tenantid AND districtid = @districtid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid district.', 16, 1);
        RETURN;
    END;
    INSERT INTO dbo.blocks (blockid, tenantid, districtid, name, code, status, createdby, updatedby)
    VALUES (@blockid, @tenantid, @districtid, @name, @code, @status, @createdby, @createdby);
    SELECT @blockid;
END
GO
