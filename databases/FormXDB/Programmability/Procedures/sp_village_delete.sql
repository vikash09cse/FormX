CREATE OR ALTER PROCEDURE dbo.sp_village_delete
    @tenantid  UNIQUEIDENTIFIER,
    @villageid UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.villages
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND villageid = @villageid AND isdeleted = 0;
END
GO
