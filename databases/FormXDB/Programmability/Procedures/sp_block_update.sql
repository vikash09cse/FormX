CREATE OR ALTER PROCEDURE dbo.sp_block_update
    @tenantid   UNIQUEIDENTIFIER,
    @blockid    UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER,
    @name       NVARCHAR(200),
    @code       NVARCHAR(50),
    @status     TINYINT,
    @updatedby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.districts WHERE tenantid = @tenantid AND districtid = @districtid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid district.', 16, 1);
        RETURN;
    END;
    UPDATE dbo.blocks
    SET districtid = @districtid, name = @name, code = @code, status = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND blockid = @blockid AND isdeleted = 0;
END
GO
