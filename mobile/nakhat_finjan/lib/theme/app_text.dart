import 'package:flutter/material.dart';

import 'app_colors.dart';

/// The type scale from the design canvas.
///
/// The brand file specifies 29LT Bukra Semi Wide and Madani Arabic for Arabic
/// with Poppins for Latin. None of the three is bundled here — see [arabic] and
/// [latin] — so the app currently renders in the platform's own faces. The scale
/// below holds either way; only the family changes.
class AppText {
  const AppText._();

  /// TODO(brand): the Arabic face. The design stands in Cairo for 29LT Bukra
  /// Semi Wide / Madani Arabic, neither of which is web-licensable. To use the
  /// real face: drop the .ttf files under assets/fonts/, declare a `fonts:`
  /// entry in pubspec.yaml, and set this to that family name. Left null so the
  /// app falls back to the platform's Arabic face rather than silently
  /// rendering in a Latin one.
  static const String? arabic = null;

  /// TODO(brand): the Latin/numeral face — Poppins in the design. Numerals are
  /// Western (1,240), which is what the design shows; if the shop wants
  /// Arabic-Indic (١٢٤٠) that is a locale change, not a font change.
  static const String? latin = null;

  // ── Headings ──────────────────────────────────────────────────────────────
  /// «سجّل دخولك», «أدخل الرمز» — the one big line on an auth screen.
  static const TextStyle authTitle = TextStyle(
    fontFamily: arabic,
    fontSize: 30,
    height: 1.3,
    fontWeight: FontWeight.w700,
    color: AppColors.ink,
  );

  /// «شو اسمك؟» — a notch larger, it is the only thing on the screen.
  static const TextStyle nameTitle = TextStyle(
    fontFamily: arabic,
    fontSize: 32,
    height: 1.3,
    fontWeight: FontWeight.w700,
    color: AppColors.ink,
  );

  /// «المنيو», «مشترياتي», «الإعدادات» — a tab's own title.
  static const TextStyle screenTitle = TextStyle(
    fontFamily: arabic,
    fontSize: 26,
    height: 1.3,
    fontWeight: FontWeight.w700,
    color: AppColors.ink,
  );

  /// «حركات النقاط» — a heading inside a scroll.
  static const TextStyle sectionTitle = TextStyle(
    fontFamily: arabic,
    fontSize: 16,
    fontWeight: FontWeight.w700,
    color: AppColors.ink,
  );

  /// «الحساب», «معلومات» — the small settings group captions.
  static const TextStyle groupLabel = TextStyle(
    fontFamily: arabic,
    fontSize: 12,
    fontWeight: FontWeight.w600,
    letterSpacing: 1.2,
    color: AppColors.ink42,
  );

  // ── Body ──────────────────────────────────────────────────────────────────
  /// The line under an auth title.
  static const TextStyle subtitle = TextStyle(
    fontFamily: arabic,
    fontSize: 15,
    height: 1.7,
    color: AppColors.ink55,
  );

  /// «مرحباً، سارة».
  static const TextStyle greeting = TextStyle(
    fontFamily: arabic,
    fontSize: 15,
    color: AppColors.ink55,
  );

  /// A list row's leading label.
  static const TextStyle rowLabel = TextStyle(
    fontFamily: arabic,
    fontSize: 15.5,
    fontWeight: FontWeight.w500,
    color: AppColors.ink,
  );

  /// A ledger row's type, an order's date — one weight up from [rowLabel].
  static const TextStyle rowLabelStrong = TextStyle(
    fontFamily: arabic,
    fontSize: 15,
    fontWeight: FontWeight.w600,
    color: AppColors.ink,
  );

  /// A list row's trailing value.
  static const TextStyle rowValue = TextStyle(
    fontFamily: arabic,
    fontSize: 15,
    color: AppColors.ink50,
  );

  /// A ledger row's timestamp.
  static const TextStyle rowMeta = TextStyle(
    fontFamily: arabic,
    fontSize: 12.5,
    color: AppColors.ink42,
  );

  /// An order line item.
  static const TextStyle orderItem = TextStyle(
    fontFamily: arabic,
    fontSize: 14,
    color: AppColors.ink75,
  );

  /// Empty-state and error-state copy.
  static const TextStyle placeholder = TextStyle(
    fontFamily: arabic,
    fontSize: 15,
    fontWeight: FontWeight.w500,
    color: AppColors.ink50,
  );

  /// The inline message under a bad field.
  static const TextStyle fieldError = TextStyle(
    fontFamily: arabic,
    fontSize: 13,
    fontWeight: FontWeight.w500,
    color: AppColors.deduct,
  );

  /// The resend countdown.
  static const TextStyle hint = TextStyle(
    fontFamily: arabic,
    fontSize: 13,
    color: AppColors.ink45,
  );

  // ── Controls ──────────────────────────────────────────────────────────────
  /// The label on the filled primary button.
  static const TextStyle primaryButton = TextStyle(
    fontFamily: arabic,
    fontSize: 17,
    fontWeight: FontWeight.w700,
    color: AppColors.ink,
  );

  /// The label on a bordered button.
  static const TextStyle secondaryButton = TextStyle(
    fontFamily: arabic,
    fontSize: 16,
    fontWeight: FontWeight.w600,
    color: AppColors.ink,
  );

  /// A category chip.
  static const TextStyle chip = TextStyle(fontFamily: arabic, fontSize: 14.5);

  /// A bottom-nav tab label.
  static const TextStyle navLabel = TextStyle(
    fontFamily: arabic,
    fontSize: 11.5,
    letterSpacing: 0.1,
  );

  /// An order's status pill.
  static const TextStyle statusPill = TextStyle(
    fontFamily: arabic,
    fontSize: 12,
    fontWeight: FontWeight.w600,
  );

  /// «كسبت 11 نقطة» / «استبدلت 250 نقطة».
  static const TextStyle orderPoints = TextStyle(
    fontFamily: arabic,
    fontSize: 12.5,
    fontWeight: FontWeight.w600,
  );

  // ── Figures ───────────────────────────────────────────────────────────────
  // Everything numeric is tabular so columns of prices and amounts line up and
  // a changing balance does not jitter.
  static const List<FontFeature> _tabular = [FontFeature.tabularFigures()];

  /// The points balance — the largest thing in the app.
  static const TextStyle balance = TextStyle(
    fontFamily: latin,
    fontSize: 96,
    height: 0.9,
    fontWeight: FontWeight.w500,
    letterSpacing: -3,
    color: AppColors.goldDeep,
    fontFeatures: _tabular,
  );

  /// «نقطة», under the balance.
  static const TextStyle balanceUnit = TextStyle(
    fontFamily: arabic,
    fontSize: 18,
    color: AppColors.ink50,
  );

  /// The phone number as typed, and the one in the settings row.
  static const TextStyle phone = TextStyle(
    fontFamily: latin,
    fontSize: 16,
    letterSpacing: 0.5,
    color: AppColors.ink,
  );

  /// A single OTP digit.
  static const TextStyle otpDigit = TextStyle(
    fontFamily: latin,
    fontSize: 24,
    fontWeight: FontWeight.w500,
    color: AppColors.ink,
    fontFeatures: _tabular,
  );

  /// A ledger amount: +11, −250.
  static const TextStyle amount = TextStyle(
    fontFamily: latin,
    fontSize: 17,
    fontWeight: FontWeight.w600,
    fontFeatures: _tabular,
  );

  /// A menu price, an order total.
  static const TextStyle price = TextStyle(
    fontFamily: latin,
    fontSize: 15.5,
    fontWeight: FontWeight.w500,
    color: AppColors.ink,
    fontFeatures: _tabular,
  );

  /// «د.أ» / «د.أ / كغم», riding alongside a price.
  static const TextStyle currency = TextStyle(
    fontFamily: arabic,
    fontSize: 12,
    color: AppColors.ink45,
  );
}
