-- Fixed GUIDs for stable references from form_fields.validationregexpresetid
IF NOT EXISTS (SELECT 1 FROM dbo.validation_regex_presets WHERE name = N'Email')
BEGIN
    INSERT INTO dbo.validation_regex_presets (validationregexpresetid, name, pattern, description, displayorder, isactive)
    VALUES
        ('a1000001-0000-4000-8000-000000000001', N'Email',
         N'^[^@\s]+@[^@\s]+\.[^@\s]+$',
         N'Standard email address', 1, 1),
        ('a1000001-0000-4000-8000-000000000002', N'Phone (India)',
         N'^[6-9]\d{9}$',
         N'10-digit Indian mobile number', 2, 1),
        ('a1000001-0000-4000-8000-000000000003', N'Numeric',
         N'^\d+$',
         N'Digits only', 3, 1),
        ('a1000001-0000-4000-8000-000000000004', N'Alphanumeric',
         N'^[A-Za-z0-9]+$',
         N'Letters and digits only', 4, 1),
        ('a1000001-0000-4000-8000-000000000005', N'URL',
         N'^https?:\/\/[^\s/$.?#].[^\s]*$',
         N'HTTP or HTTPS URL', 5, 1),
        ('a1000001-0000-4000-8000-000000000006', N'PIN / ZIP',
         N'^\d{4,10}$',
         N'Postal / PIN code (4–10 digits)', 6, 1);
END
GO
