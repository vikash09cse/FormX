CREATE OR ALTER PROCEDURE dbo.sp_user_set_district_scopes
    @userid      UNIQUEIDENTIFIER,
    @districtids NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.user_district_scopes WHERE userid = @userid;

    IF @districtids IS NULL OR LTRIM(RTRIM(@districtids)) = N''
        RETURN;

    INSERT INTO dbo.user_district_scopes (userid, districtid)
    SELECT DISTINCT @userid, TRY_CAST(value AS UNIQUEIDENTIFIER)
    FROM STRING_SPLIT(@districtids, N',')
    WHERE TRY_CAST(value AS UNIQUEIDENTIFIER) IS NOT NULL;
END
GO
