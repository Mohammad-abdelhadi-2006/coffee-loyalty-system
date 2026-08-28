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

    /// <summary>For <see cref="ErrorCodes.NameRequired"/>.</summary>
    public const string NameRequired = "الاسم مطلوب عند تسجيل الدخول لأول مرة";

    /// <summary>For <see cref="ErrorCodes.ProductNotFound"/>.</summary>
    public const string ProductNotFound = "المنتج غير موجود";

    /// <summary>For <see cref="ErrorCodes.CustomerNotFound"/>.</summary>
    public const string CustomerNotFound = "العميل غير موجود";

    /// <summary>For <see cref="ErrorCodes.PhoneAlreadyExists"/>.</summary>
    public const string PhoneAlreadyExists = "رقم الهاتف مسجّل مسبقاً";

    /// <summary>For <see cref="ErrorCodes.EmployeeNotFound"/>.</summary>
    public const string EmployeeNotFound = "الموظف غير موجود";

    /// <summary>For <see cref="ErrorCodes.OrderNotFound"/>.</summary>
    public const string OrderNotFound = "الطلب غير موجود";

    /// <summary>For <see cref="ErrorCodes.OrderAlreadyCancelled"/>.</summary>
    public const string OrderAlreadyCancelled = "هذا الطلب ملغى مسبقاً";

    /// <summary>For <see cref="ErrorCodes.OrderHasReturns"/>.</summary>
    public const string OrderHasReturns = "لا يمكن إلغاء طلب تم إرجاع جزء منه";

    /// <summary>For <see cref="ErrorCodes.ReturnWindowExpired"/>.</summary>
    public const string ReturnWindowExpired = "انتهت مدة الإرجاع المسموح بها";

    /// <summary>For <see cref="ErrorCodes.InsufficientBalanceForReturn"/>.</summary>
    public const string InsufficientBalanceForReturn = "رصيد النقاط لا يغطي النقاط المستحقة للاسترجاع";

    /// <summary>For <see cref="ErrorCodes.OrderPaidWithPoints"/>.</summary>
    public const string OrderPaidWithPoints = "الطلبات المدفوعة بالنقاط تُلغى بالكامل ولا تقبل الإرجاع الجزئي";

    /// <summary>For <see cref="ErrorCodes.ItemNotInOrder"/>.</summary>
    public const string ItemNotInOrder = "هذا الصنف لا ينتمي إلى هذا الطلب";

    /// <summary>For <see cref="ErrorCodes.ReturnExceedsQuantity"/>.</summary>
    public const string ReturnExceedsQuantity = "الكمية المرتجعة تتجاوز الكمية المتبقية";

    /// <summary>For <see cref="ErrorCodes.ProductUnavailable"/>.</summary>
    public const string ProductUnavailable = "المنتج غير متوفر حالياً";

    /// <summary>For <see cref="ErrorCodes.InvalidQuantity"/>.</summary>
    public const string InvalidQuantity = "الكمية غير صالحة";

    /// <summary>
    /// For <see cref="ErrorCodes.RedeemBelowMinimum"/>. The one message that is
    /// <c>static readonly</c> rather than <c>const</c>: it quotes the minimum, and the
    /// minimum belongs to <see cref="LoyaltyConstants"/>. An interpolated string is not a
    /// compile-time constant, and duplicating the number here would let the two drift.
    /// </summary>
    public static readonly string RedeemBelowMinimum =
        $"الحد الأدنى للاستبدال {LoyaltyConstants.MinRedeemPoints} نقطة";

    /// <summary>For <see cref="ErrorCodes.InsufficientBalance"/>.</summary>
    public const string InsufficientBalance = "رصيد النقاط غير كافٍ";

    /// <summary>For <see cref="ErrorCodes.RedeemExceedsTotal"/>.</summary>
    public const string RedeemExceedsTotal = "النقاط المستبدلة تتجاوز قيمة الطلب";

    /// <summary>For <see cref="ErrorCodes.Unauthorized"/>.</summary>
    public const string Unauthorized = "يجب تسجيل الدخول للمتابعة";

    /// <summary>For <see cref="ErrorCodes.Forbidden"/>.</summary>
    public const string Forbidden = "لا تملك صلاحية للقيام بهذا الإجراء";

    /// <summary>For <see cref="ErrorCodes.TooManyRequests"/>.</summary>
    public const string TooManyRequests = "عدد كبير من المحاولات، يرجى المحاولة بعد قليل";

    /// <summary>For <see cref="ErrorCodes.InternalError"/>.</summary>
    public const string InternalError = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً";
}
