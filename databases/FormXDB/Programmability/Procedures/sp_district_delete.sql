CREATE OR ALTER PROCEDURE dbo.sp_district_delete
    @tenantid   UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER,
    @updatedby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.districts
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND districtid = @districtid AND isdeleted = 0;
END
GO
