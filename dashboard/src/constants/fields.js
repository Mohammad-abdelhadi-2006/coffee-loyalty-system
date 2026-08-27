export const FIELDS = {
    // ── تسجيل الدخول ──
    username: { title: 'اسم المستخدم', mode: 'en', allow: ['en', 'num'] },
    password: { title: 'كلمة المرور', mode: 'en', allow: ['en', 'num'] },

    // ── شاشة الطلب: الزبون ──
    customerPhone: { title: 'رقم هاتف الزبون', mode: 'num', allow: ['num'] },
    customerName: { title: 'اسم الزبون', mode: 'ar', allow: ['ar', 'en'] },
    redeemPoints: { title: 'النقاط المستبدلة', mode: 'num', allow: ['num'] },
    newCustPhone: { title: 'رقم هاتف الزبون الجديد', mode: 'num', allow: ['num'] },

    // ── شاشة الطلب: المنتجات ──
    productSearch: { title: 'بحث عن منتج', mode: 'ar', allow: ['ar', 'en', 'num'] },
    qty: { title: 'الكمية', mode: 'num', allow: ['num'] },
    orderNote: { title: 'ملاحظة على الطلب', mode: 'ar', allow: ['ar', 'en', 'num'] },

    // ── إدارة المنتجات ──
    productName: { title: 'اسم المنتج', mode: 'ar', allow: ['ar', 'en'] },
    productPrice: { title: 'سعر المنتج', mode: 'num', allow: ['num'] },
    productPoints: { title: 'نقاط المنتج', mode: 'num', allow: ['num'] },

    // ── إدارة الموظفين ──
    employeeName: { title: 'اسم الموظف', mode: 'ar', allow: ['ar', 'en'] },
    employeeSearch: { title: 'بحث عن موظف', mode: 'ar', allow: ['ar', 'en'] },
    newUsername: { title: 'اسم مستخدم جديد', mode: 'en', allow: ['en', 'num'] },
    newPassword: { title: 'كلمة مرور جديدة', mode: 'en', allow: ['en', 'num'] },
    confirmPassword: { title: 'تأكيد كلمة المرور', mode: 'en', allow: ['en', 'num'] },

    // ── المرتجعات ──
    orderNumber: { title: 'رقم الطلب', mode: 'num', allow: ['num'] },
    returnReason: { title: 'سبب الإرجاع', mode: 'ar', allow: ['ar', 'en', 'num'] },

    //   ── الإعدادات / قواعد النقاط ──
    pointsPerDinar: { title: 'النقاط لكل دينار', mode: 'num', allow: ['num'] },
    pointValue: { title: 'قيمة النقطة بالفلس', mode: 'num', allow: ['num'] },
    minRedeem: { title: 'أقل عدد نقاط للاستبدال', mode: 'num', allow: ['num'] },

    weightKg: { title: 'الوزن بالكيلو', mode: 'num', allow: ['num'] },
    weightAmount: { title: 'المبلغ بالدينار', mode: 'num', allow: ['num'] },
}