CREATE OR ALTER PROCEDURE dbo.sp_district_update
    @tenantid   UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER,
    @stateid    UNIQUEIDENTIFIER,
    @name       NVARCHAR(200),
    @code       NVARCHAR(50),
    @status     TINYINT,
    @updatedby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.states WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid state.', 16, 1);
        RETURN;
    END;
    UPDATE dbo.districts
    SET stateid = @stateid, name = @name, code = @code, status = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND districtid = @districtid AND isdeleted = 0;
END
GO
