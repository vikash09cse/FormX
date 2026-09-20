CREATE OR ALTER PROCEDURE dbo.sp_district_create
    @districtid UNIQUEIDENTIFIER,
    @tenantid   UNIQUEIDENTIFIER,
    @stateid    UNIQUEIDENTIFIER,
    @name       NVARCHAR(200),
    @code       NVARCHAR(50),
    @status     TINYINT,
    @createdby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.states WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0)
    BEGIN
        RAISERROR('Invalid state.', 16, 1);
        RETURN;
    END;
    INSERT INTO dbo.districts (districtid, tenantid, stateid, name, code, status, createdby, updatedby)
    VALUES (@districtid, @tenantid, @stateid, @name, @code, @status, @createdby, @createdby);
    SELECT @districtid;
END
GO
