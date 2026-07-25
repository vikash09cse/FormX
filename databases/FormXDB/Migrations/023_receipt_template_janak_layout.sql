-- Refresh default Receipt HTML to match JANAK UROCARE layout.

DECLARE @seedActor UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @receiptId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

DECLARE @receiptHtml NVARCHAR(MAX) = N'<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<title>Receipt {{ReceiptNumber}}</title>
<style>
  @page { size: A4; margin: 12mm; }
  * { box-sizing: border-box; }
  body {
    font-family: Arial, Helvetica, sans-serif;
    color: #111;
    margin: 0;
    padding: 8px 12px 16px;
    font-size: 12px;
    line-height: 1.35;
  }
  .top {
    display: grid;
    grid-template-columns: 1.15fr 1.2fr 1fr;
    gap: 12px;
    align-items: start;
    padding-bottom: 10px;
    border-bottom: 1px solid #222;
  }
  .brand {
    display: flex;
    gap: 10px;
    align-items: center;
  }
  .logo {
    width: 64px;
    height: 64px;
    object-fit: contain;
    border-radius: 50%;
  }
  .logo-fallback {
    width: 64px;
    height: 64px;
    border: 2px solid #1b5e4a;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #1b5e4a;
    font-weight: 700;
    font-size: 18px;
    flex-shrink: 0;
  }
  .hospital {
    font-family: Georgia, "Times New Roman", serif;
    font-size: 22px;
    font-weight: 700;
    letter-spacing: 0.4px;
    line-height: 1.1;
  }
  .tagline-wrap {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: 4px;
  }
  .tagline-wrap::before, .tagline-wrap::after {
    content: "";
    flex: 1;
    height: 1px;
    background: #333;
  }
  .tagline {
    font-size: 10px;
    white-space: nowrap;
    color: #222;
  }
  .doctor {
    border-left: 1px solid #bbb;
    padding-left: 12px;
  }
  .doctor-name { font-weight: 700; font-size: 13px; }
  .doctor-meta { font-size: 11px; color: #222; margin-top: 2px; }
  .doctor-spec { font-size: 10px; color: #333; margin-top: 6px; }
  .contact { font-size: 11px; }
  .contact-row { display: flex; gap: 6px; margin-bottom: 6px; align-items: flex-start; }
  .contact-icon {
    width: 14px;
    flex-shrink: 0;
    text-align: center;
    font-weight: 700;
  }
  .title-row {
    display: flex;
    align-items: center;
    gap: 10px;
    margin: 14px 0 12px;
  }
  .title-row::before, .title-row::after {
    content: "";
    flex: 1;
    border-top: 1px solid #222;
  }
  .title-box {
    border: 2px double #111;
    border-radius: 6px;
    padding: 4px 22px;
    font-weight: 700;
    letter-spacing: 2px;
    font-size: 14px;
  }
  .meta {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 24px;
    margin-bottom: 14px;
  }
  .meta-row {
    display: grid;
    grid-template-columns: 110px 10px 1fr;
    gap: 4px;
    margin-bottom: 7px;
    align-items: end;
  }
  .meta-label { font-weight: 600; }
  .meta-colon { font-weight: 600; }
  .meta-value {
    border-bottom: 1px solid #111;
    min-height: 16px;
    padding: 0 2px 1px;
  }
  .fees {
    width: 100%;
    border-collapse: collapse;
    margin-top: 4px;
  }
  .fees th, .fees td {
    padding: 7px 8px;
    border-bottom: 1px solid #ccc;
  }
  .fees thead th {
    border-top: 2px solid #111;
    border-bottom: 2px solid #111;
    font-size: 12px;
    letter-spacing: 0.4px;
  }
  .fees .sno { width: 70px; text-align: center; }
  .fees .amt { width: 130px; text-align: right; }
  .fees tbody tr:last-child td { border-bottom: 2px solid #111; }
  .total-row {
    display: grid;
    grid-template-columns: 1fr 130px;
    border-top: 2px solid #111;
    border-bottom: 2px solid #111;
    margin-top: 0;
    font-weight: 700;
    font-size: 14px;
  }
  .total-row div { padding: 8px; }
  .total-row .amt { text-align: right; }
  .bottom {
    display: grid;
    grid-template-columns: 1.1fr 1fr 1.1fr;
    gap: 12px;
    margin-top: 16px;
    align-items: start;
  }
  .box {
    border: 1.5px solid #222;
    border-radius: 4px;
    padding: 8px 10px;
    min-height: 88px;
  }
  .box-title { font-weight: 700; margin-bottom: 8px; font-size: 12px; }
  .pay-options { display: grid; grid-template-columns: 1fr 1fr; gap: 6px 10px; }
  .pay-item { display: flex; align-items: center; gap: 6px; }
  .mark {
    display: inline-block;
    width: 13px;
    height: 13px;
    border: 1.5px solid #111;
    text-align: center;
    line-height: 11px;
    font-size: 11px;
    font-weight: 700;
  }
  .center-block { text-align: center; padding-top: 4px; }
  .received {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 6px;
    align-items: end;
    margin-bottom: 14px;
    text-align: left;
  }
  .received .line { border-bottom: 1px solid #111; min-height: 18px; }
  .thanks {
    font-family: "Segoe Script", "Comic Sans MS", cursive;
    font-size: 22px;
    font-style: italic;
    margin: 4px 0;
  }
  .wish { font-size: 11px; color: #333; }
  .appt-line { margin-bottom: 8px; font-size: 11px; }
  .slogan-wrap {
    margin-top: 16px;
    text-align: center;
    border-top: 1px solid #222;
    padding-top: 8px;
  }
  .slogan {
    font-style: italic;
    font-size: 12px;
  }
  @media print {
    body { padding: 0; }
  }
</style>
</head>
<body>
  <div class="top">
    <div class="brand">
      {{LogoHtml}}
      <div>
        <div class="hospital">{{HospitalName}}</div>
        <div class="tagline-wrap"><span class="tagline">{{ReceiptHeader}}</span></div>
      </div>
    </div>
    <div class="doctor">
      <div class="doctor-name">{{DoctorName}}</div>
      <div class="doctor-meta">{{DoctorDesignation}}</div>
      <div class="doctor-spec">{{DoctorSpecialties}}</div>
    </div>
    <div class="contact">
      <div class="contact-row"><span class="contact-icon">&#9679;</span><span>{{HospitalAddress}}</span></div>
      <div class="contact-row"><span class="contact-icon">&#9742;</span><span>{{HospitalPhone}}</span></div>
      <div class="contact-row"><span class="contact-icon">&#9830;</span><span>{{Website}}</span></div>
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
        <th>PARTICULARS</th>
        <th class="amt">AMOUNT (₹)</th>
      </tr>
    </thead>
    <tbody>
      <tr><td class="sno">1</td><td>Consultation Fee</td><td class="amt">{{ConsultationFee}}</td></tr>
      <tr><td class="sno">2</td><td>Registration Fee</td><td class="amt">{{RegistrationFee}}</td></tr>
      <tr><td class="sno">3</td><td>Procedure Charges</td><td class="amt">{{ProcedureCharge}}</td></tr>
      <tr><td class="sno">4</td><td>Investigation Charges</td><td class="amt">{{InvestigationCharges}}</td></tr>
      <tr><td class="sno">5</td><td>Medicine Charges</td><td class="amt">{{MedicineCharges}}</td></tr>
      <tr><td class="sno">6</td><td>Discount</td><td class="amt">{{Discount}}</td></tr>
    </tbody>
  </table>
  <div class="total-row">
    <div>TOTAL PAID</div>
    <div class="amt">₹ {{TotalPaid}}</div>
  </div>

  <div class="bottom">
    <div class="box">
      <div class="box-title">PAYMENT MODE</div>
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
      <div class="appt-line"><strong>For Appointment</strong> {{HospitalPhone}}</div>
      <div class="appt-line">{{ReceiptFooter}}</div>
    </div>
  </div>

  <div class="slogan-wrap"><div class="slogan">Better Urology, Better Life.</div></div>
</body>
</html>';

UPDATE dbo.global_document_templates
SET bodyhtml = @receiptHtml,
    name = N'Default Receipt',
    updatedby = @seedActor,
    updatedat = SYSUTCDATETIME()
WHERE globaldocumenttemplateid = @receiptId
   OR (templatetype = 1 AND isdefault = 1 AND isdeleted = 0);

-- Push layout to tenant copies that still point at the seeded global receipt (or all default receipts)
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
