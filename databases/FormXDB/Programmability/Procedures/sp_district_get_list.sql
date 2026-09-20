CREATE OR ALTER PROCEDURE dbo.sp_district_get_list
    @tenantid UNIQUEIDENTIFIER,
    @stateid  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.districtid, d.stateid, s.name AS statename, d.name, d.code, d.status, d.createdat, d.updatedat
    FROM dbo.districts d
    INNER JOIN dbo.states s ON s.stateid = d.stateid AND s.isdeleted = 0
    WHERE d.tenantid = @tenantid AND d.isdeleted = 0
      AND (@stateid IS NULL OR d.stateid = @stateid)
    ORDER BY s.name, d.name;
END
GO
