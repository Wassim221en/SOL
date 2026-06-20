using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SOL.Application.Common.Interfaces;

namespace Template.Infrastructe.Services;

public class EmailService : IEmailService
{
  private readonly IConfiguration _configuration;

  public EmailService(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
  {
    var email = new MimeMessage();
    email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
    email.To.Add(MailboxAddress.Parse(to));
    email.Subject = subject;

    var builder = new BodyBuilder();
    if (isHtml)
    {
      builder.HtmlBody = body;
    }
    else
    {
      builder.TextBody = body;
    }

    email.Body = builder.ToMessageBody();

    using var smtp = new SmtpClient();
    try
    {
      await smtp.ConnectAsync(
        _configuration["Email:SmtpServer"],
        int.Parse(_configuration["Email:Port"] ?? "587"),
        SecureSocketOptions.SslOnConnect
      );

      await smtp.AuthenticateAsync(
        _configuration["Email:Username"],
        _configuration["Email:Password"]
      );

      await smtp.SendAsync(email);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error sending email: {ex.Message}");
      throw;
    }
    finally
    {
      await smtp.DisconnectAsync(true);
    }
  }

  public async Task SendPasswordResetEmailAsync(string to, string resetToken, string employeeName)
  {
    var subject = "طلب إعادة تعيين كلمة المرور";
    var body = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap');
    
    body {{
      margin: 0;
      padding: 24px;
      background: linear-gradient(180deg, #f0fdf4 0%, #ecfdf5 100%);
      -webkit-font-smoothing: antialiased;
      font-family: 'Tajawal', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937;
      direction: rtl;
    }}
    .email-wrap {{
      max-width: 700px;
      margin: 0 auto;
      background: linear-gradient(180deg, #ffffff 0%, #fafbfc 100%);
      border-radius: 16px;
      box-shadow: 0 12px 40px rgba(16, 185, 129, 0.1);
      overflow: hidden;
      border: 1px solid rgba(16, 185, 129, 0.08);
    }}
    .hero {{
      background: linear-gradient(135deg, #10b981 0%, #059669 50%, #047857 100%);
      padding: 40px 24px;
      text-align: center;
      color: #fff;
      position: relative;
      overflow: hidden;
    }}
    .hero::before {{
      content: '';
      position: absolute;
      top: -50%;
      left: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
    }}
    .hero .icon-container {{
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      height: 80px;
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(10px);
      font-size: 36px;
      margin-bottom: 16px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      position: relative;
    }}
    .hero h1 {{
      margin: 0;
      font-size: 26px;
      font-weight: 800;
      letter-spacing: -0.3px;
      position: relative;
    }}
    .hero p.subtitle {{
      margin: 8px 0 0;
      font-size: 14px;
      opacity: 0.95;
      font-weight: 500;
      position: relative;
    }}
    .content {{
      padding: 32px;
      font-size: 15px;
      line-height: 1.8;
      color: #374151;
    }}
    .greeting {{
      font-size: 17px;
      margin: 0 0 16px;
      font-weight: 700;
      color: #111827;
    }}
    .description {{
      margin: 0 0 20px;
      color: #4b5563;
    }}
    .token-card {{
      display: block;
      width: 100%;
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      color: #a7f3d0;
      padding: 24px;
      border-radius: 12px;
      margin: 20px 0;
      font-family: 'Courier New', Courier, monospace;
      font-size: 28px;
      font-weight: 800;
      letter-spacing: 6px;
      text-align: center;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.2);
      border: 2px solid rgba(16, 185, 129, 0.3);
      direction: ltr;
    }}
    .token-card small {{
      display: block;
      opacity: 0.9;
      font-size: 13px;
      margin-bottom: 10px;
      color: #6ee7b7;
      font-family: 'Tajawal', sans-serif;
      letter-spacing: 0;
      font-weight: 500;
    }}
    .info-box {{
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      border-right: 4px solid #f59e0b;
      padding: 16px 20px;
      border-radius: 8px;
      margin: 20px 0;
      font-size: 14px;
      color: #92400e;
    }}
    .info-box strong {{
      display: block;
      margin-bottom: 4px;
      font-size: 14px;
    }}
    .expiry-info {{
      display: flex;
      align-items: center;
      gap: 8px;
      background: #f3f4f6;
      padding: 12px 16px;
      border-radius: 8px;
      margin-top: 16px;
      font-size: 14px;
      color: #374151;
    }}
    .expiry-info span {{
      font-weight: 700;
      color: #059669;
    }}
    .footer {{
      background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
      padding: 24px;
      text-align: center;
      font-size: 13px;
      color: #64748b;
      border-top: 1px solid rgba(148, 163, 184, 0.2);
    }}
    .footer .brand {{
      font-weight: 700;
      color: #10b981;
      font-size: 15px;
    }}
    .footer .school {{
      margin-top: 8px;
      font-size: 13px;
      color: #475569;
    }}
    @media only screen and (max-width: 520px) {{
      .email-wrap {{ margin: 8px; }}
      .content {{ padding: 20px; }}
      .hero {{ padding: 28px 16px; }}
      .hero h1 {{ font-size: 22px; }}
      .token-card {{ font-size: 24px; letter-spacing: 4px; }}
    }}
  </style>
</head>
<body>
  <span class=""preheader"" style=""display:none;visibility:hidden;opacity:0;color:transparent;height:0;width:0;max-height:0;max-width:0;overflow:hidden;"">رمز إعادة تعيين كلمة المرور لحسابك - صالح لمدة ساعة واحدة.</span>

  <div class=""email-wrap"">
    <div class=""hero"">
      <div class=""icon-container"">🔐</div>
      <h1>طلب إعادة تعيين كلمة المرور</h1>
      <p class=""subtitle"">رمز آمن لتغيير كلمة المرور الخاصة بك</p>
    </div>

    <div class=""content"">
      <p class=""greeting"">مرحباً <strong>{employeeName}</strong>،</p>

      <p class=""description"">لقد تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك. استخدم الرمز أدناه لإتمام عملية تغيير كلمة المرور بأمان. هذا الرمز مخصص للاستخدام مرة واحدة فقط.</p>

      <div class=""token-card"">
        <small>رمز إعادة التعيين</small>
        {resetToken}
      </div>

      <div class=""info-box"">
        <strong>⚠️ تنبيه أمني مهم</strong>
        لا تشارك هذا الرمز مع أي شخص تحت أي ظرف. إذا لم تقم بطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني بأمان - لن يتم اتخاذ أي إجراء.
      </div>

      <div class=""expiry-info"">
        ⏱️ صلاحية الرمز: <span>ساعة واحدة</span> من وقت الإرسال
      </div>
    </div>

    <div class=""footer"">
      <div class=""brand"">🏫 مدرسة رواد المستقبل التعليمية الخاصة</div>
      <div class=""school"">💻 تطوير شركة Wpify للحلول التقنية | <a href=""https://wpify.site"" style=""color:#10b981;text-decoration:none;"">wpify.site</a></div>
      <div style=""margin-top:10px;font-size:12px;color:#94a3b8;"">هذه رسالة تلقائية، يرجى عدم الرد عليها.</div>
    </div>
  </div>
</body>
</html>";
    await SendEmailAsync(to, subject, body, true);
  }

  public async Task SendWelcomeEmailAsync(string to, string employeeName)
  {
    var subject = "مرحباً بك في مدرسة رواد المستقبل!";
    var body = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap');
    
    body {{
      margin: 0;
      padding: 24px;
      background: linear-gradient(180deg, #eff6ff 0%, #dbeafe 100%);
      -webkit-font-smoothing: antialiased;
      font-family: 'Tajawal', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937;
      direction: rtl;
    }}
    .email-wrap {{
      max-width: 700px;
      margin: 0 auto;
      background: linear-gradient(180deg, #ffffff 0%, #fafbfc 100%);
      border-radius: 16px;
      box-shadow: 0 12px 40px rgba(59, 130, 246, 0.1);
      overflow: hidden;
      border: 1px solid rgba(59, 130, 246, 0.08);
    }}
    .hero {{
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 50%, #1d4ed8 100%);
      padding: 48px 24px;
      text-align: center;
      color: #fff;
      position: relative;
      overflow: hidden;
    }}
    .hero::before {{
      content: '';
      position: absolute;
      top: -50%;
      left: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
    }}
    .hero .icon-container {{
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 90px;
      height: 90px;
      border-radius: 22px;
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(10px);
      font-size: 42px;
      margin-bottom: 20px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      position: relative;
    }}
    .hero h1 {{
      margin: 0;
      font-size: 30px;
      font-weight: 800;
      position: relative;
    }}
    .hero p.subtitle {{
      margin: 10px 0 0;
      font-size: 15px;
      opacity: 0.95;
      font-weight: 500;
      position: relative;
    }}
    .content {{
      padding: 36px 32px;
      font-size: 15px;
      line-height: 1.8;
      color: #374151;
    }}
    .greeting {{
      font-size: 18px;
      margin: 0 0 16px;
      font-weight: 700;
      color: #111827;
    }}
    .welcome-card {{
      background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
      border-right: 4px solid #3b82f6;
      padding: 24px;
      border-radius: 12px;
      margin: 24px 0;
    }}
    .welcome-card h3 {{
      margin: 0 0 12px;
      color: #1e40af;
      font-size: 18px;
      font-weight: 700;
    }}
    .features {{
      list-style: none;
      padding: 0;
      margin: 20px 0;
    }}
    .features li {{
      padding: 12px 16px;
      margin-bottom: 8px;
      background: #f8fafc;
      border-radius: 8px;
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 15px;
      color: #374151;
    }}
    .features li .icon {{
      font-size: 20px;
      flex-shrink: 0;
    }}
    .cta-button {{
      display: inline-block;
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
      color: #fff;
      padding: 14px 32px;
      border-radius: 10px;
      text-decoration: none;
      font-weight: 700;
      font-size: 16px;
      margin: 16px 0;
      box-shadow: 0 8px 20px rgba(59, 130, 246, 0.3);
    }}
    .help-box {{
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      border-right: 4px solid #f59e0b;
      padding: 16px 20px;
      border-radius: 8px;
      margin: 24px 0;
      font-size: 14px;
      color: #92400e;
    }}
    .footer {{
      background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
      padding: 24px;
      text-align: center;
      font-size: 13px;
      color: #64748b;
      border-top: 1px solid rgba(148, 163, 184, 0.2);
    }}
    .footer .brand {{
      font-weight: 700;
      color: #3b82f6;
      font-size: 15px;
    }}
    .footer .school {{
      margin-top: 8px;
      font-size: 13px;
      color: #475569;
    }}
    @media only screen and (max-width: 520px) {{
      .email-wrap {{ margin: 8px; }}
      .content {{ padding: 20px; }}
      .hero {{ padding: 32px 16px; }}
      .hero h1 {{ font-size: 24px; }}
    }}
  </style>
</head>
<body>
  <span class=""preheader"" style=""display:none;visibility:hidden;opacity:0;color:transparent;height:0;width:0;max-height:0;max-width:0;overflow:hidden;"">مرحباً بك في مدرسة رواد المستقبل - تم إنشاء حسابك بنجاح!</span>

  <div class=""email-wrap"">
    <div class=""hero"">
      <div class=""icon-container"">🎉</div>
      <h1>مرحباً بك معنا!</h1>
      <p class=""subtitle"">تم إنشاء حسابك بنجاح في مدرسة رواد المستقبل</p>
    </div>

    <div class=""content"">
      <p class=""greeting"">أهلاً <strong>{employeeName}</strong>،</p>

      <p>يسعدنا انضمامك إلى عائلة مدرسة رواد المستقبل التعليمية الخاصة. تم إعداد حسابك بنجاح وأنت الآن جاهز للبدء.</p>

      <div class=""welcome-card"">
        <h3>🌟 ما الذي يمكنك فعله الآن؟</h3>
        <ul class=""features"">
          <li><span class=""icon"">📱</span> تسجيل الدخول إلى حسابك في أي وقت</li>
          <li><span class=""icon"">📋</span> الوصول إلى جميع الخدمات والمعلومات</li>
          <li><span class=""icon"">🔔</span> تلقي الإشعارات والتحديثات المهمة</li>
          <li><span class=""icon"">💬</span> التواصل مع فريق الدعم بسهولة</li>
        </ul>
      </div>

      <p>إذا كان لديك أي استفسار أو تحتاج إلى مساعدة، لا تتردد في التواصل مع فريق الدعم الخاص بنا. نحن هنا لمساعدتك دائماً.</p>

      <div class=""help-box"">
        <strong>💡 هل تحتاج مساعدة؟</strong>
        فريق الدعم متاح دائماً للإجابة على استفساراتك ومساعدتك في أي وقت.
      </div>
    </div>

    <div class=""footer"">
      <div class=""brand"">🏫 مدرسة رواد المستقبل التعليمية الخاصة</div>
      <div class=""school"">💻 تطوير شركة Wpify للحلول التقنية | <a href=""https://wpify.site"" style=""color:#3b82f6;text-decoration:none;"">wpify.site</a></div>
      <div style=""margin-top:10px;font-size:12px;color:#94a3b8;"">هذه رسالة تلقائية، يرجى عدم الرد عليها.</div>
    </div>
  </div>
</body>
</html>";

    await SendEmailAsync(to, subject, body, true);
  }

  public async Task SendEmailVerificationCode(string email, string code, string employeeName)
  {
    var subject = "رمز التحقق من البريد الإلكتروني";
    var body = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap');
    
    body {{
      margin: 0;
      padding: 24px;
      background: linear-gradient(180deg, #f5f3ff 0%, #ede9fe 100%);
      -webkit-font-smoothing: antialiased;
      font-family: 'Tajawal', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937;
      direction: rtl;
    }}
    .email-wrap {{
      max-width: 700px;
      margin: 0 auto;
      background: linear-gradient(180deg, #ffffff 0%, #fafbfc 100%);
      border-radius: 16px;
      box-shadow: 0 12px 40px rgba(139, 92, 246, 0.1);
      overflow: hidden;
      border: 1px solid rgba(139, 92, 246, 0.08);
    }}
    .hero {{
      background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 50%, #6d28d9 100%);
      padding: 40px 24px;
      text-align: center;
      color: #fff;
      position: relative;
      overflow: hidden;
    }}
    .hero::before {{
      content: '';
      position: absolute;
      top: -50%;
      left: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
    }}
    .hero .icon-container {{
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      height: 80px;
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(10px);
      font-size: 36px;
      margin-bottom: 16px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      position: relative;
    }}
    .hero h1 {{
      margin: 0;
      font-size: 26px;
      font-weight: 800;
      position: relative;
    }}
    .hero p.subtitle {{
      margin: 8px 0 0;
      font-size: 14px;
      opacity: 0.95;
      font-weight: 500;
      position: relative;
    }}
    .content {{
      padding: 32px;
      font-size: 15px;
      line-height: 1.8;
      color: #374151;
    }}
    .greeting {{
      font-size: 17px;
      margin: 0 0 16px;
      font-weight: 700;
      color: #111827;
    }}
    .description {{
      margin: 0 0 20px;
      color: #4b5563;
    }}
    .code-card {{
      display: block;
      width: 100%;
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      color: #c4b5fd;
      padding: 28px 24px;
      border-radius: 12px;
      margin: 24px 0;
      font-family: 'Courier New', Courier, monospace;
      font-size: 36px;
      font-weight: 800;
      letter-spacing: 10px;
      text-align: center;
      box-shadow: 0 12px 36px rgba(15, 23, 42, 0.25);
      border: 2px solid rgba(139, 92, 246, 0.4);
      direction: ltr;
    }}
    .code-card small {{
      display: block;
      opacity: 0.9;
      font-size: 13px;
      margin-bottom: 12px;
      color: #a78bfa;
      font-family: 'Tajawal', sans-serif;
      letter-spacing: 0;
      font-weight: 500;
    }}
    .steps-box {{
      background: linear-gradient(135deg, #f5f3ff 0%, #ede9fe 100%);
      border-right: 4px solid #8b5cf6;
      padding: 20px;
      border-radius: 10px;
      margin: 24px 0;
    }}
    .steps-box h4 {{
      margin: 0 0 12px;
      color: #5b21b6;
      font-size: 16px;
      font-weight: 700;
    }}
    .steps-box ol {{
      margin: 0;
      padding-right: 20px;
    }}
    .steps-box li {{
      margin-bottom: 8px;
      color: #4c1d95;
      font-size: 14px;
    }}
    .warning-box {{
      background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
      border-right: 4px solid #ef4444;
      padding: 16px 20px;
      border-radius: 8px;
      margin: 20px 0;
      font-size: 14px;
      color: #991b1b;
    }}
    .warning-box strong {{
      display: block;
      margin-bottom: 4px;
    }}
    .expiry-info {{
      display: flex;
      align-items: center;
      gap: 8px;
      background: #f3f4f6;
      padding: 12px 16px;
      border-radius: 8px;
      margin-top: 16px;
      font-size: 14px;
      color: #374151;
    }}
    .expiry-info span {{
      font-weight: 700;
      color: #7c3aed;
    }}
    .footer {{
      background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
      padding: 24px;
      text-align: center;
      font-size: 13px;
      color: #64748b;
      border-top: 1px solid rgba(148, 163, 184, 0.2);
    }}
    .footer .brand {{
      font-weight: 700;
      color: #8b5cf6;
      font-size: 15px;
    }}
    .footer .school {{
      margin-top: 8px;
      font-size: 13px;
      color: #475569;
    }}
    @media only screen and (max-width: 520px) {{
      .email-wrap {{ margin: 8px; }}
      .content {{ padding: 20px; }}
      .hero {{ padding: 28px 16px; }}
      .hero h1 {{ font-size: 22px; }}
      .code-card {{ font-size: 28px; letter-spacing: 6px; }}
    }}
  </style>
</head>
<body>
  <span class=""preheader"" style=""display:none;visibility:hidden;opacity:0;color:transparent;height:0;width:0;max-height:0;max-width:0;overflow:hidden;"">رمز التحقق من بريدك الإلكتروني لمدرسة رواد المستقبل.</span>

  <div class=""email-wrap"">
    <div class=""hero"">
      <div class=""icon-container"">📧</div>
      <h1>التحقق من البريد الإلكتروني</h1>
      <p class=""subtitle"">استخدم الرمز أدناه لتأكيد بريدك الإلكتروني</p>
    </div>

    <div class=""content"">
      <p class=""greeting"">مرحباً <strong>{employeeName}</strong>،</p>

      <p class=""description"">لقد تلقينا طلباً للتحقق من بريدك الإلكتروني المرتبط بحسابك في مدرسة رواد المستقبل. أدخل رمز التحقق أدناه لإتمام عملية التأكيد.</p>

      <div class=""code-card"">
        <small>رمز التحقق</small>
        {code}
      </div>

      <div class=""steps-box"">
        <h4>📝 خطوات التحقق:</h4>
        <ol>
          <li>انسخ رمز التحقق أعلاه</li>
          <li>أدخل الرمز في خانة التحقق المخصصة</li>
          <li>اضغط على زر تأكيد لإتمام العملية</li>
        </ol>
      </div>

      <div class=""warning-box"">
        <strong>⚠️ تنبيه أمني</strong>
        إذا لم تقم بطلب التحقق من البريد الإلكتروني، يرجى تجاهل هذه الرسالة بأمان. لن يتم اتخاذ أي إجراء على حسابك.
      </div>

      <div class=""expiry-info"">
        ⏱️ صلاحية الرمز: <span>ساعة واحدة</span> من وقت الإرسال
      </div>
    </div>

    <div class=""footer"">
      <div class=""brand"">🏫 مدرسة رواد المستقبل التعليمية الخاصة</div>
      <div class=""school"">💻 تطوير شركة Wpify للحلول التقنية | <a href=""https://wpify.site"" style=""color:#8b5cf6;text-decoration:none;"">wpify.site</a></div>
      <div style=""margin-top:10px;font-size:12px;color:#94a3b8;"">هذه رسالة تلقائية، يرجى عدم الرد عليها.</div>
    </div>
  </div>
</body>
</html>";

    await SendEmailAsync(email, subject, body, true);
  }

  public async Task SendPasswordResetSuccessEmailAsync(string to, string employeeName)
  {
    var subject = "تم تغيير كلمة المرور بنجاح";
    var body = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap');
    
    body {{
      margin: 0;
      padding: 24px;
      background: linear-gradient(180deg, #ecfdf5 0%, #d1fae5 100%);
      -webkit-font-smoothing: antialiased;
      font-family: 'Tajawal', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937;
      direction: rtl;
    }}
    .email-wrap {{
      max-width: 700px;
      margin: 0 auto;
      background: linear-gradient(180deg, #ffffff 0%, #fafbfc 100%);
      border-radius: 16px;
      box-shadow: 0 12px 40px rgba(16, 185, 129, 0.1);
      overflow: hidden;
      border: 1px solid rgba(16, 185, 129, 0.08);
    }}
    .hero {{
      background: linear-gradient(135deg, #10b981 0%, #059669 50%, #047857 100%);
      padding: 48px 24px;
      text-align: center;
      color: #fff;
      position: relative;
      overflow: hidden;
    }}
    .hero::before {{
      content: '';
      position: absolute;
      top: -50%;
      left: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
    }}
    .hero .icon-container {{
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 90px;
      height: 90px;
      border-radius: 22px;
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(10px);
      font-size: 42px;
      margin-bottom: 20px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      position: relative;
    }}
    .hero h1 {{
      margin: 0;
      font-size: 28px;
      font-weight: 800;
      position: relative;
    }}
    .hero p.subtitle {{
      margin: 10px 0 0;
      font-size: 15px;
      opacity: 0.95;
      font-weight: 500;
      position: relative;
    }}
    .content {{
      padding: 36px 32px;
      font-size: 15px;
      line-height: 1.8;
      color: #374151;
    }}
    .greeting {{
      font-size: 18px;
      margin: 0 0 16px;
      font-weight: 700;
      color: #111827;
    }}
    .success-card {{
      background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
      border-right: 4px solid #10b981;
      padding: 24px;
      border-radius: 12px;
      margin: 24px 0;
      text-align: center;
    }}
    .success-card .check-icon {{
      font-size: 48px;
      margin-bottom: 12px;
    }}
    .success-card h3 {{
      margin: 0 0 8px;
      color: #065f46;
      font-size: 20px;
      font-weight: 700;
    }}
    .success-card p {{
      margin: 0;
      color: #047857;
      font-size: 15px;
    }}
    .security-tips {{
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      border-right: 4px solid #f59e0b;
      padding: 20px;
      border-radius: 10px;
      margin: 24px 0;
    }}
    .security-tips h4 {{
      margin: 0 0 12px;
      color: #92400e;
      font-size: 16px;
      font-weight: 700;
    }}
    .security-tips ul {{
      margin: 0;
      padding-right: 20px;
    }}
    .security-tips li {{
      margin-bottom: 8px;
      color: #78350f;
      font-size: 14px;
    }}
    .warning-box {{
      background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
      border-right: 4px solid #ef4444;
      padding: 16px 20px;
      border-radius: 8px;
      margin: 20px 0;
      font-size: 14px;
      color: #991b1b;
    }}
    .warning-box strong {{
      display: block;
      margin-bottom: 4px;
    }}
    .footer {{
      background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
      padding: 24px;
      text-align: center;
      font-size: 13px;
      color: #64748b;
      border-top: 1px solid rgba(148, 163, 184, 0.2);
    }}
    .footer .brand {{
      font-weight: 700;
      color: #10b981;
      font-size: 15px;
    }}
    .footer .school {{
      margin-top: 8px;
      font-size: 13px;
      color: #475569;
    }}
    @media only screen and (max-width: 520px) {{
      .email-wrap {{ margin: 8px; }}
      .content {{ padding: 20px; }}
      .hero {{ padding: 32px 16px; }}
      .hero h1 {{ font-size: 24px; }}
    }}
  </style>
</head>
<body>
  <span class=""preheader"" style=""display:none;visibility:hidden;opacity:0;color:transparent;height:0;width:0;max-height:0;max-width:0;overflow:hidden;"">تم تغيير كلمة المرور لحسابك في مدرسة رواد المستقبل بنجاح.</span>

  <div class=""email-wrap"">
    <div class=""hero"">
      <div class=""icon-container"">✅</div>
      <h1>تم تغيير كلمة المرور بنجاح</h1>
      <p class=""subtitle"">تم تحديث كلمة المرور لحسابك بأمان</p>
    </div>

    <div class=""content"">
      <p class=""greeting"">مرحباً <strong>{employeeName}</strong>،</p>

      <p>نود إعلامك بأنه تم تغيير كلمة المرور لحسابك في مدرسة رواد المستقبل بنجاح. يمكنك الآن استخدام كلمة المرور الجديدة لتسجيل الدخول.</p>

      <div class=""success-card"">
        <div class=""check-icon"">🔐</div>
        <h3>تم التحديث بنجاح</h3>
        <p>كلمة المرور الجديدة نشطة وجاهزة للاستخدام</p>
      </div>

      <div class=""security-tips"">
        <h4>🛡️ نصائح أمنية للحفاظ على حسابك:</h4>
        <ul>
          <li>استخدم كلمة مرور قوية ومميزة لا تستخدمها في حسابات أخرى</li>
          <li>لا تشارك كلمة المرور الخاصة بك مع أي شخص</li>
          <li>قم بتغيير كلمة المرور بشكل دوري للحفاظ على أمان حسابك</li>
          <li>تأكد من تسجيل الخروج عند استخدام جهاز مشترك</li>
        </ul>
      </div>

      <div class=""warning-box"">
        <strong>⚠️ لم تقم بهذا الإجراء؟</strong>
        إذا لم تقم بتغيير كلمة المرور بنفسك، يرجى التواصل مع فريق الدعم الفني فوراً لحماية حسابك.
      </div>
    </div>

    <div class=""footer"">
      <div class=""brand"">🏫 مدرسة رواد المستقبل التعليمية الخاصة</div>
      <div class=""school"">💻 تطوير شركة Wpify للحلول التقنية | <a href=""https://wpify.site"" style=""color:#10b981;text-decoration:none;"">wpify.site</a></div>
      <div style=""margin-top:10px;font-size:12px;color:#94a3b8;"">هذه رسالة تلقائية، يرجى عدم الرد عليها.</div>
    </div>
  </div>
</body>
</html>";

    await SendEmailAsync(to, subject, body, true);
  }
}
