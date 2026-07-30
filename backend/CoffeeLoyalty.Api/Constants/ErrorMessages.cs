namespace CoffeeLoyalty.Api.Constants;

/// <summary>
/// Arabic display text paired with each <see cref="ErrorCodes"/> constant.
/// Per api-contract.md the message is for humans and may change freely —
/// clients must never branch on it.
/// </summary>
public static class ErrorMessages
{
    /// <summary>For <see cref="ErrorCodes.InvalidCredentials"/>.</summary>
    public const string InvalidCredentials = "اسم المستخدم أو كلمة المرور غير صحيحة";

    /// <summary>For <see cref="ErrorCodes.AccountDisabled"/>.</summary>
    public const string AccountDisabled = "هذا الحساب معطّل، يرجى مراجعة الإدارة";

    /// <summary>For <see cref="ErrorCodes.InvalidFirebaseToken"/>.</summary>
    public const string InvalidFirebaseToken = "فشل التحقق من رمز الدخول، يرجى تسجيل الدخول مرة أخرى";

    /// <summary>For <see cref="ErrorCodes.InvalidPhone"/>.</summary>
    public const string InvalidPhone = "رقم الهاتف غير صالح";

    /// <summary>For <see cref="ErrorCodes.ValidationError"/>.</summary>
    public const string ValidationError = "البيانات المُرسلة غير صالحة";

    /// <summary>For <see cref="ErrorCodes.Unauthorized"/>.</summary>
    public const string Unauthorized = "يجب تسجيل الدخول للمتابعة";

    /// <summary>For <see cref="ErrorCodes.Forbidden"/>.</summary>
    public const string Forbidden = "لا تملك صلاحية للقيام بهذا الإجراء";

    /// <summary>For <see cref="ErrorCodes.InternalError"/>.</summary>
    public const string InternalError = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً";
}
