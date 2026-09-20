CREATE OR ALTER PROCEDURE dbo.sp_user_get_district_scope_ids
    @userid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT districtid FROM dbo.user_district_scopes WHERE userid = @userid;
END
GO
