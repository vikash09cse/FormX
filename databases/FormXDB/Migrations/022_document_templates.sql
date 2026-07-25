-- Seed system templates and backfill tenant copies for existing tenants.

DECLARE @seedActor UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @receiptId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
DECLARE @emailId UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';

DECLARE @receiptHtml NVARCHAR(MAX) = N'<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<title>Receipt {{ReceiptNumber}}</title>
<style>
  body { font-family: Arial, Helvetica, sans-serif; color: #111; margin: 24px; font-size: 13px; }
  .header { text-align: center; border-bottom: 2px solid #1a5f4a; padding-bottom: 12px; margin-bottom: 16px; }
  .hospital { font-size: 22px; font-weight: 700; color: #1a5f4a; letter-spacing: 0.5px; }
  .sub { color: #444; margin-top: 4px; }
  .meta { display: flex; justify-content: space-between; margin-bottom: 16px; gap: 16px; }
  .box { border: 1px solid #ddd; padding: 10px 12px; flex: 1; }
  .label { color: #666; font-size: 11px; text-transform: uppercase; }
  .value { font-weight: 600; margin-top: 2px; }
  table.fees { width: 100%; border-collapse: collapse; margin: 16px 0; }
  table.fees th, table.fees td { border-bottom: 1px solid #eee; padding: 8px 6px; text-align: left; }
  table.fees th { background: #f5f8f7; }
  table.fees td.amt, table.fees th.amt { text-align: right; }
  .total { font-size: 16px; font-weight: 700; }
  .footer { margin-top: 24px; border-top: 1px dashed #ccc; padding-top: 12px; text-align: center; color: #555; font-size: 12px; }
  @media print { body { margin: 0; } }
</style>
</head>
<body>
  <div class="header">
    <div class="hospital">{{HospitalName}}</div>
    <div class="sub">{{HospitalAddress}}</div>
    <div class="sub">Phone: {{HospitalPhone}}</div>
    <div class="sub">{{ReceiptHeader}}</div>
  </div>
  <div class="meta">
    <div class="box">
      <div class="label">Receipt</div>
      <div class="value">{{ReceiptNumber}}</div>
      <div class="label" style="margin-top:8px">Visit</div>
      <div class="value">{{VisitCode}} &middot; {{VisitDate}} {{VisitTime}}</div>
      <div class="label" style="margin-top:8px">Doctor</div>
      <div class="value">{{DoctorName}}</div>
    </div>
    <div class="box">
      <div class="label">Patient</div>
      <div class="value">{{PatientName}} ({{PatientCode}})</div>
      <div class="label" style="margin-top:8px">Age / Gender</div>
      <div class="value">{{Age}} / {{Gender}}</div>
      <div class="label" style="margin-top:8px">Phone</div>
      <div class="value">{{Phone}}</div>
      <div class="label" style="margin-top:8px">Address</div>
      <div class="value">{{Address}}</div>
    </div>
  </div>
  <table class="fees">
    <thead>
      <tr><th>Description</th><th class="amt">Amount</th></tr>
    </thead>
    <tbody>
      <tr><td>Consultation</td><td class="amt">{{ConsultationFee}}</td></tr>
      <tr><td>Procedure</td><td class="amt">{{ProcedureCharge}}</td></tr>
      <tr><td>Add-ons</td><td class="amt">{{AddonCharges}}</td></tr>
      <tr><td>Discount</td><td class="amt">-{{Discount}}</td></tr>
      <tr><td class="total">Amount paid</td><td class="amt total">{{TotalPaid}}</td></tr>
      <tr><td>Payment mode</td><td class="amt">{{PaymentMode}}</td></tr>
      <tr><td>Collected by</td><td class="amt">{{CollectedBy}}</td></tr>
    </tbody>
  </table>
  <div class="footer">{{ReceiptFooter}}</div>
</body>
</html>';

DECLARE @emailHtml NVARCHAR(MAX) = N'<p>Dear {{PatientName}},</p>
<p>Thank you for visiting <strong>{{HospitalName}}</strong>.</p>
<p>Your visit <strong>{{VisitCode}}</strong> on {{VisitDate}} has been recorded.</p>
<p>Regards,<br/>{{HospitalName}}</p>';

IF NOT EXISTS (SELECT 1 FROM dbo.global_document_templates WHERE globaldocumenttemplateid = @receiptId)
BEGIN
    INSERT INTO dbo.global_document_templates (
        globaldocumenttemplateid, templatetype, name, subject, bodyhtml, isdefault,
        createdby, updatedby)
    VALUES (
        @receiptId, 1, N'Default Receipt', NULL, @receiptHtml, 1,
        @seedActor, @seedActor);
END

IF NOT EXISTS (SELECT 1 FROM dbo.global_document_templates WHERE globaldocumenttemplateid = @emailId)
BEGIN
    INSERT INTO dbo.global_document_templates (
        globaldocumenttemplateid, templatetype, name, subject, bodyhtml, isdefault,
        createdby, updatedby)
    VALUES (
        @emailId, 2, N'Visit thank you', N'Thank you for visiting {{HospitalName}}', @emailHtml, 1,
        @seedActor, @seedActor);
END

-- Backfill: tenants with no document_templates get a full copy from global
INSERT INTO dbo.document_templates (
    documenttemplateid, tenantid, globaldocumenttemplateid,
    templatetype, name, subject, bodyhtml, isdefault,
    createdby, updatedby)
SELECT
    NEWID(), t.tenantid, g.globaldocumenttemplateid,
    g.templatetype, g.name, g.subject, g.bodyhtml, g.isdefault,
    @seedActor, @seedActor
FROM dbo.tenants t
CROSS JOIN dbo.global_document_templates g
WHERE t.isdeleted = 0
  AND g.isdeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.document_templates dt
      WHERE dt.tenantid = t.tenantid AND dt.isdeleted = 0);
GO
