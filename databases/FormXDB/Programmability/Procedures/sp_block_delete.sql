CREATE OR ALTER PROCEDURE dbo.sp_block_delete
    @tenantid  UNIQUEIDENTIFIER,
    @blockid   UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.blocks
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND blockid = @blockid AND isdeleted = 0;
END
GO
