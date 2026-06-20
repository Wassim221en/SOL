using Microsoft.AspNetCore.Identity;

namespace Template.API.Identity;

// Centralizes ASP.NET Identity error descriptions in Arabic so that messages
// returned to clients (e.g. password/username validation) are not English.
public sealed class ArabicIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new IdentityError { Code = nameof(DefaultError), Description = "حدث خطأ غير متوقع." };

    public override IdentityError ConcurrencyFailure()
        => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "تم تعديل البيانات من قبل عملية أخرى. حاول مرة أخرى." };

    public override IdentityError PasswordMismatch()
        => new IdentityError { Code = nameof(PasswordMismatch), Description = "كلمة المرور غير صحيحة." };

    public override IdentityError InvalidToken()
        => new IdentityError { Code = nameof(InvalidToken), Description = "الرمز غير صالح." };

    public override IdentityError LoginAlreadyAssociated()
        => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "يوجد حساب مرتبط مسبقاً بهذا المزود." };

    public override IdentityError InvalidUserName(string userName)
        => new IdentityError { Code = nameof(InvalidUserName), Description = $"اسم المستخدم '{userName}' غير صالح." };

    public override IdentityError InvalidEmail(string email)
        => new IdentityError { Code = nameof(InvalidEmail), Description = $"البريد الإلكتروني '{email}' غير صالح." };

    public override IdentityError DuplicateUserName(string userName)
        => new IdentityError { Code = nameof(DuplicateUserName), Description = $"اسم المستخدم '{userName}' مستخدم مسبقاً." };

    public override IdentityError DuplicateEmail(string email)
        => new IdentityError { Code = nameof(DuplicateEmail), Description = $"البريد الإلكتروني '{email}' مستخدم مسبقاً." };

    public override IdentityError InvalidRoleName(string role)
        => new IdentityError { Code = nameof(InvalidRoleName), Description = $"اسم الدور '{role}' غير صالح." };

    public override IdentityError DuplicateRoleName(string role)
        => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"اسم الدور '{role}' مستخدم مسبقاً." };

    public override IdentityError UserAlreadyHasPassword()
        => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "المستخدم لديه كلمة مرور مسبقاً." };

    public override IdentityError UserLockoutNotEnabled()
        => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "قفل الحساب غير مُفعّل لهذا المستخدم." };

    public override IdentityError UserAlreadyInRole(string role)
        => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"المستخدم مضاف إلى الدور '{role}' مسبقاً." };

    public override IdentityError UserNotInRole(string role)
        => new IdentityError { Code = nameof(UserNotInRole), Description = $"المستخدم غير مضاف إلى الدور '{role}'." };

    public override IdentityError PasswordTooShort(int length)
        => new IdentityError { Code = nameof(PasswordTooShort), Description = $"كلمة المرور قصيرة جداً. الحد الأدنى للطول هو {length}." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "يجب أن تحتوي كلمة المرور على رمز واحد على الأقل." };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "يجب أن تحتوي كلمة المرور على رقم واحد على الأقل." };

    public override IdentityError PasswordRequiresLower()
        => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل." };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"يجب أن تحتوي كلمة المرور على {uniqueChars} حرف/رمز مختلف على الأقل."
        };

    public override IdentityError RecoveryCodeRedemptionFailed()
        => new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "رمز الاستعادة غير صالح." };
}

