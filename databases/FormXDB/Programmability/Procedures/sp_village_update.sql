CREATE OR ALTER PROCEDURE dbo.sp_village_update
    @tenantid  UNIQUEIDENTIFIER,
    @villageid UNIQUEIDENTIFIER,
    @blockid   UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @code      NVARCHAR(50),
    @status    TINYINT,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.blocks WHERE tenantid = @tenantid AND blockid = @blockid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid block.', 16, 1);
        RETURN;
    END;
    UPDATE dbo.villages
    SET blockid = @blockid, name = @name, code = @code, status = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND villageid = @villageid AND isdeleted = 0;
END
GO
