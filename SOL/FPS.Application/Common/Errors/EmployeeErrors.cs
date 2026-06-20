using System.Net;
using Template.Domain.Exceptions.Http;

namespace FPS.Application.Common.Errors;

public static class EmployeeErrors
{
    public static HttpMessage NotFound()
        => new("الموظف غير موجود.", HttpStatusCode.NotFound);

    public static HttpMessage UnAuthenticated()
        => new("غير مصرح بالوصول.", HttpStatusCode.Unauthorized);

    public static HttpMessage CannotModifySuperAdmin()
        => new("لا يمكن تعديل حساب المدير العام.", HttpStatusCode.Forbidden);

    public static HttpMessage UserNameAlreadyExists(string userName)
        => new($"اسم المستخدم '{userName}' مستخدم بالفعل.", HttpStatusCode.Conflict);

    public static HttpMessage EmailAlreadyExists(string email)
        => new($"البريد الإلكتروني '{email}' مستخدم بالفعل.", HttpStatusCode.Conflict);

    public static HttpMessage PhoneNumberAlreadyExists(string phoneNumber)
        => new($"رقم الهاتف '{phoneNumber}' مستخدم بالفعل.", HttpStatusCode.Conflict);

    public static HttpMessage PasswordChangeFailed(string errors)
        => new($"فشل تغيير كلمة المرور: {errors}", HttpStatusCode.BadRequest);

    public static HttpMessage LoginIdentifierIsRequired()
        => new("يجب توفير بريد إلكتروني أو رقم هاتف أو اسم مستخدم.", HttpStatusCode.BadRequest);

    public static HttpMessage OnlyOneLoginIdentifierAllowed()
        => new("يُسمح بمعرف تسجيل دخول واحد فقط.", HttpStatusCode.BadRequest);

    public static HttpMessage InvalidPasswordOrEmail
        => new("كلمة المرور أو البريد الإلكتروني غير صحيح.", HttpStatusCode.Unauthorized);

    public static HttpMessage InvalidPasswordOrPhoneNumber
        => new("كلمة المرور أو رقم الهاتف غير صحيح.", HttpStatusCode.Unauthorized);

    public static HttpMessage InvalidPasswordOrUserName
        => new("كلمة المرور أو اسم المستخدم غير صحيح.", HttpStatusCode.Unauthorized);

    public static HttpMessage NewPasswordsDoNotMatch()
        => new("كلمتا المرور الجديدتان غير متطابقتين.", HttpStatusCode.BadRequest);

    public static HttpMessage OldPasswordIsIncorrect()
        => new("كلمة المرور القديمة غير صحيحة.", HttpStatusCode.BadRequest);
}
