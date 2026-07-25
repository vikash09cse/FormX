-- Add website to tenants; refresh receipt template (fixed header/specialties, fewer fee rows, improved layout).

IF COL_LENGTH('dbo.tenants', 'website') IS NULL
BEGIN
    ALTER TABLE dbo.tenants ADD website NVARCHAR(200) NULL;
END
GO

DECLARE @seedActor UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @receiptId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

DECLARE @receiptHtml NVARCHAR(MAX) = N'<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<title>Receipt {{ReceiptNumber}}</title>
<style>
  @page { size: A4; margin: 10mm; }
  * { box-sizing: border-box; }
  body {
    font-family: "Segoe UI", Arial, Helvetica, sans-serif;
    color: #1a1a1a;
    margin: 0;
    padding: 10px 14px 18px;
    font-size: 12px;
    line-height: 1.4;
    background: #fff;
  }
  .top {
    display: grid;
    grid-template-columns: 1.2fr 1.25fr 1fr;
    gap: 14px;
    align-items: start;
    padding-bottom: 12px;
    border-bottom: 2px solid #1a5f4a;
  }
  .brand { display: flex; gap: 12px; align-items: center; }
  .logo {
    width: 68px; height: 68px; object-fit: contain; border-radius: 50%;
    border: 1px solid #d7e5e0; background: #f7faf9;
  }
  .logo-fallback {
    width: 68px; height: 68px; border-radius: 50%;
    border: 2px solid #1a5f4a; display: flex; align-items: center; justify-content: center;
    color: #1a5f4a; font-weight: 700; font-size: 20px; flex-shrink: 0; background: #f0f7f4;
  }
  .hospital {
    font-family: Georgia, "Times New Roman", serif;
    font-size: 23px; font-weight: 700; letter-spacing: 0.3px; line-height: 1.1; color: #12382d;
  }
  .tagline-wrap {
    display: flex; align-items: center; gap: 8px; margin-top: 6px;
  }
  .tagline-wrap::before, .tagline-wrap::after {
    content: ""; flex: 1; height: 1px; background: #9bb8ae;
  }
  .tagline { font-size: 10px; color: #355a4d; white-space: nowrap; letter-spacing: 0.2px; }
  .doctor {
    border-left: 1px solid #c5d9d1; padding-left: 14px;
  }
  .doctor-name { font-weight: 700; font-size: 13.5px; color: #111; }
  .doctor-meta { font-size: 11px; color: #333; margin-top: 3px; }
  .doctor-role {
    margin-top: 8px; padding-top: 8px; border-top: 1px dashed #c5d9d1;
    font-size: 11px; font-weight: 700; color: #1a5f4a;
  }
  .doctor-spec { font-size: 10px; color: #444; margin-top: 4px; line-height: 1.45; }
  .contact { font-size: 11px; color: #2b2b2b; }
  .contact-row { display: flex; gap: 8px; margin-bottom: 7px; align-items: flex-start; }
  .contact-label {
    min-width: 18px; color: #1a5f4a; font-weight: 700; flex-shrink: 0;
  }
  .title-row {
    display: flex; align-items: center; gap: 12px; margin: 16px 0 14px;
  }
  .title-row::before, .title-row::after {
    content: ""; flex: 1; border-top: 1px solid #1a5f4a;
  }
  .title-box {
    border: 2px solid #1a5f4a; border-radius: 8px;
    padding: 5px 26px; font-weight: 700; letter-spacing: 2.5px;
    font-size: 13px; color: #12382d; background: #f4faf7;
  }
  .meta {
    display: grid; grid-template-columns: 1fr 1fr; gap: 28px; margin-bottom: 14px;
  }
  .meta-row {
    display: grid; grid-template-columns: 108px 12px 1fr; gap: 2px;
    margin-bottom: 8px; align-items: end;
  }
  .meta-label { font-weight: 600; color: #333; }
  .meta-colon { font-weight: 600; color: #666; }
  .meta-value {
    border-bottom: 1px solid #c8c8c8; min-height: 17px;
    padding: 0 2px 2px; color: #111;
  }
  .fees {
    width: 100%; border-collapse: collapse; margin-top: 2px;
  }
  .fees th, .fees td { padding: 9px 10px; }
  .fees thead th {
    background: #eef6f2; border-top: 2px solid #1a5f4a; border-bottom: 2px solid #1a5f4a;
    font-size: 11px; letter-spacing: 0.5px; text-transform: uppercase; color: #12382d;
  }
  .fees tbody td { border-bottom: 1px solid #e2e8e5; }
  .fees tbody tr:nth-child(even) td { background: #fafcfa; }
  .fees .sno { width: 70px; text-align: center; color: #555; }
  .fees .amt { width: 130px; text-align: right; font-variant-numeric: tabular-nums; }
  .total-row {
    display: grid; grid-template-columns: 1fr 130px;
    border-top: 2px solid #1a5f4a; border-bottom: 2px solid #1a5f4a;
    margin-top: 0; font-weight: 700; font-size: 14px; background: #f4faf7;
  }
  .total-row div { padding: 10px; }
  .total-row .amt { text-align: right; color: #12382d; }
  .bottom {
    display: grid; grid-template-columns: 1.1fr 1fr 1.1fr; gap: 12px;
    margin-top: 18px; align-items: stretch;
  }
  .box {
    border: 1px solid #c5d9d1; border-radius: 8px; padding: 10px 12px;
    background: #fcfefd; min-height: 96px;
  }
  .box-title {
    font-weight: 700; margin-bottom: 10px; font-size: 11px;
    letter-spacing: 0.4px; color: #1a5f4a; text-transform: uppercase;
  }
  .pay-options { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 12px; }
  .pay-item { display: flex; align-items: center; gap: 7px; }
  .mark {
    display: inline-flex; width: 14px; height: 14px; border: 1.5px solid #1a5f4a;
    border-radius: 3px; align-items: center; justify-content: center;
    font-size: 10px; font-weight: 700; color: #1a5f4a; background: #fff;
  }
  .center-block { text-align: center; padding: 4px 6px; display: flex; flex-direction: column; justify-content: center; }
  .received {
    display: grid; grid-template-columns: auto 1fr; gap: 6px;
    align-items: end; margin-bottom: 14px; text-align: left;
  }
  .received .line { border-bottom: 1px solid #9aa8a3; min-height: 20px; padding-left: 4px; }
  .thanks {
    font-family: Georgia, "Times New Roman", serif;
    font-size: 20px; font-style: italic; margin: 2px 0 6px; color: #1a5f4a;
  }
  .wish { font-size: 11px; color: #444; }
  .appt-line { margin-bottom: 8px; font-size: 11px; color: #2b2b2b; }
  .slogan-wrap {
    margin-top: 16px; text-align: center; border-top: 1px solid #c5d9d1; padding-top: 10px;
  }
  .slogan { font-style: italic; font-size: 12px; color: #1a5f4a; letter-spacing: 0.2px; }
  @media print { body { padding: 0; } .fees tbody tr:nth-child(even) td { background: transparent; } }
</style>
</head>
<body>
  <div class="top">
    <div class="brand">
      {{LogoHtml}}
      <div>
        <div class="hospital">{{HospitalName}}</div>
        <div class="tagline-wrap"><span class="tagline">Advanced Urology &amp; Stone Care Centre</span></div>
      </div>
    </div>
    <div class="doctor">
      <div class="doctor-name">{{DoctorName}}</div>
      <div class="doctor-meta">{{DoctorDesignation}}</div>
      <div class="doctor-role">Consultant Urologist</div>
      <div class="doctor-spec">
        Endourology | Laparoscopy | Kidney Stone Laser<br/>
        Prostate Care | Male Sexual Health | Uro-Oncology
      </div>
    </div>
    <div class="contact">
      <div class="contact-row"><span class="contact-label">A</span><span>{{HospitalAddress}}</span></div>
      <div class="contact-row"><span class="contact-label">P</span><span>{{HospitalPhone}}</span></div>
      <div class="contact-row"><span class="contact-label">W</span><span>{{Website}}</span></div>
    </div>
  </div>

  <div class="title-row"><div class="title-box">RECEIPT</div></div>

  <div class="meta">
    <div>
      <div class="meta-row"><span class="meta-label">Receipt No.</span><span class="meta-colon">:</span><span class="meta-value">{{ReceiptNumber}}</span></div>
      <div class="meta-row"><span class="meta-label">Patient ID</span><span class="meta-colon">:</span><span class="meta-value">{{PatientCode}}</span></div>
      <div class="meta-row"><span class="meta-label">Date</span><span class="meta-colon">:</span><span class="meta-value">{{VisitDate}}</span></div>
      <div class="meta-row"><span class="meta-label">Time</span><span class="meta-colon">:</span><span class="meta-value">{{VisitTime}}</span></div>
    </div>
    <div>
      <div class="meta-row"><span class="meta-label">Patient Name</span><span class="meta-colon">:</span><span class="meta-value">{{PatientName}}</span></div>
      <div class="meta-row"><span class="meta-label">Age / Sex</span><span class="meta-colon">:</span><span class="meta-value">{{Age}} / {{Gender}}</span></div>
      <div class="meta-row"><span class="meta-label">Mobile No.</span><span class="meta-colon">:</span><span class="meta-value">{{Phone}}</span></div>
      <div class="meta-row"><span class="meta-label">Address</span><span class="meta-colon">:</span><span class="meta-value">{{Address}}</span></div>
    </div>
  </div>

  <table class="fees">
    <thead>
      <tr>
        <th class="sno">S. No.</th>
        <th>Particulars</th>
        <th class="amt">Amount (₹)</th>
      </tr>
    </thead>
    <tbody>
      <tr><td class="sno">1</td><td>Consultation Fee</td><td class="amt">{{ConsultationFee}}</td></tr>
      <tr><td class="sno">2</td><td>Procedure Charges</td><td class="amt">{{ProcedureCharge}}</td></tr>
      <tr><td class="sno">3</td><td>Add-on Charges</td><td class="amt">{{AddonCharges}}</td></tr>
      <tr><td class="sno">4</td><td>Discount</td><td class="amt">{{Discount}}</td></tr>
    </tbody>
  </table>
  <div class="total-row">
    <div>TOTAL PAID</div>
    <div class="amt">₹ {{TotalPaid}}</div>
  </div>

  <div class="bottom">
    <div class="box">
      <div class="box-title">Payment mode</div>
      <div class="pay-options">
        <div class="pay-item"><span class="mark">{{CashMark}}</span> Cash</div>
        <div class="pay-item"><span class="mark">{{UpiMark}}</span> UPI</div>
        <div class="pay-item"><span class="mark">{{CardMark}}</span> Card</div>
        <div class="pay-item"><span class="mark">{{OtherMark}}</span> Other {{OtherPaymentDetail}}</div>
      </div>
    </div>
    <div class="center-block">
      <div class="received"><span>Received By :</span><span class="line">{{CollectedBy}}</span></div>
      <div class="thanks">Thank You!</div>
      <div class="wish">For choosing {{HospitalName}}. We wish you good health.</div>
    </div>
    <div class="box">
      <div class="appt-line"><strong>For Appointment</strong><br/>{{HospitalPhone}}</div>
      <div class="appt-line">Timings: 10:00 AM - 8:00 PM<br/>(Monday to Saturday)</div>
    </div>
  </div>

  <div class="slogan-wrap"><div class="slogan">Better Urology, Better Life.</div></div>
</body>
</html>';

UPDATE dbo.global_document_templates
SET bodyhtml = @receiptHtml,
    updatedby = @seedActor,
    updatedat = SYSUTCDATETIME()
WHERE globaldocumenttemplateid = @receiptId
   OR (templatetype = 1 AND isdefault = 1 AND isdeleted = 0);

UPDATE dt
SET bodyhtml = @receiptHtml,
    updatedby = @seedActor,
    updatedat = SYSUTCDATETIME()
FROM dbo.document_templates dt
WHERE dt.isdeleted = 0
  AND dt.templatetype = 1
  AND (
      dt.globaldocumenttemplateid = @receiptId
      OR dt.isdefault = 1
  );
GO
