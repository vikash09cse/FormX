CREATE OR ALTER FUNCTION dbo.fn_to_sql_timezone (@timezone NVARCHAR(50))
RETURNS NVARCHAR(50)
AS
BEGIN
    IF @timezone IS NULL OR LTRIM(RTRIM(@timezone)) = ''
        RETURN N'UTC';

    SET @timezone = LTRIM(RTRIM(@timezone));

    -- Already a Windows time zone name (no IANA path separator).
    IF CHARINDEX(N'/', @timezone) = 0
        RETURN @timezone;

    RETURN CASE @timezone
        WHEN N'Asia/Kolkata' THEN N'India Standard Time'
        WHEN N'Asia/Calcutta' THEN N'India Standard Time'
        WHEN N'Asia/Dubai' THEN N'Arabian Standard Time'
        WHEN N'Asia/Singapore' THEN N'Singapore Standard Time'
        WHEN N'Asia/Tokyo' THEN N'Tokyo Standard Time'
        WHEN N'Europe/London' THEN N'GMT Standard Time'
        WHEN N'America/New_York' THEN N'Eastern Standard Time'
        WHEN N'America/Chicago' THEN N'Central Standard Time'
        WHEN N'America/Denver' THEN N'Mountain Standard Time'
        WHEN N'America/Los_Angeles' THEN N'Pacific Standard Time'
        WHEN N'UTC' THEN N'UTC'
        WHEN N'Etc/UTC' THEN N'UTC'
        ELSE N'UTC'
    END;
END
GO
