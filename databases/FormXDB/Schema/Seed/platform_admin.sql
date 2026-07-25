-- Default platform admin: admin@formx.com / Admin@123
-- usertype 0 = PlatformAdmin (platform-level; tenantid IS NULL)
IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = 'admin@formx.com' AND tenantid IS NULL)
BEGIN
    INSERT INTO dbo.users (userid, tenantid, email, passwordhash, firstname, lastname, usertype, userstatus)
    VALUES (
        '11111111-1111-1111-1111-111111111111',
        NULL,
        'admin@formx.com',
        CONCAT('$', '2a', '$', '11', '$', '0Y6ELZJUhxuJvEujJilpwOklWR3iyx235u.5WqrfUdlKYjZIim0Mi'),
        'Platform',
        'Admin',
        0,
        1
    );
END
GO
