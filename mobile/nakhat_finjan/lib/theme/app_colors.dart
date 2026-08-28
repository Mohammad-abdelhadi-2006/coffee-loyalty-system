import 'package:flutter/material.dart';

/// The palette, lifted from the design canvas ("Nakhat Finjan Screens").
///
/// The golds, the caramel and the inks come from the brand file. [earn] and
/// [deduct] do not: the brand book has no green or red, so the designer derived
/// an olive from the logo signage and a warm brick at the same lightness as the
/// caramel. Both are marked below — swap them if official values turn up.
class AppColors {
  const AppColors._();

  // ── Ground ────────────────────────────────────────────────────────────────
  /// Screen background.
  static const Color background = Color(0xFFEDECE4);

  /// Cards, list containers, the bottom bar.
  static const Color surface = Color(0xFFFFFFFF);

  // ── Ink ───────────────────────────────────────────────────────────────────
  /// Primary text, and the label on a filled gold button.
  static const Color ink = Color(0xFF191817);

  static const Color ink75 = Color(0xBF191817);
  static const Color ink62 = Color(0x9E191817);
  static const Color ink55 = Color(0x8C191817);
  static const Color ink50 = Color(0x80191817);
  static const Color ink45 = Color(0x73191817);
  static const Color ink42 = Color(0x6B191817);
  static const Color ink34 = Color(0x57191817);
  static const Color ink30 = Color(0x4D191817);
  static const Color ink25 = Color(0x40191817);

  // ── Brand ─────────────────────────────────────────────────────────────────
  /// The one filled button in the app. Classical never fills a button, but on a
  /// phone the single primary CTA needs the fill for tap affordance.
  static const Color gold = Color(0xFFF0AC42);

  /// Border under [gold], and the balance numeral.
  static const Color goldDeep = Color(0xFFE3A13B);

  /// Links, active nav tab, decorative rules.
  static const Color caramel = Color(0xFFC06C1D);

  /// Label on an active category chip.
  static const Color caramelDark = Color(0xFF8F5312);

  // ── Derived, not from the brand file ──────────────────────────────────────
  /// Points gained. Olive-sage from the logo signage.
  static const Color earn = Color(0xFF5F7047);

  /// Points spent or clawed back, and every destructive control.
  static const Color deduct = Color(0xFFA6412C);

  // ── Lines ─────────────────────────────────────────────────────────────────
  /// Between rows inside a card.
  static const Color hairline = Color(0x12191817);

  /// Around a card.
  static const Color cardBorder = Color(0x14191817);

  /// Around an input or an inactive chip.
  static const Color fieldBorder = Color(0x1F191817);

  /// Under the whole bottom nav.
  static const Color navBorder = Color(0x1A191817);

  // ── Tints ─────────────────────────────────────────────────────────────────
  /// Active category chip fill.
  static const Color goldTint = Color(0x2EF0AC42);

  /// Disabled primary button fill, and its border.
  static const Color goldDisabled = Color(0x52F0AC42);
  static const Color goldDisabledBorder = Color(0x47E3A13B);

  /// The blocked / too-many-attempts notice.
  static const Color deductTint = Color(0x12A6412C);
  static const Color deductTintBorder = Color(0x33A6412C);

  /// Order status pills.
  static const Color earnTint = Color(0x1A5F7047);
  static const Color earnTintBorder = Color(0x475F7047);
  static const Color caramelTint = Color(0x1AC06C1D);
  static const Color caramelTintBorder = Color(0x4DC06C1D);

  /// Card elevation — a single soft ink-tinted shadow, never a stack.
  static const List<BoxShadow> cardShadow = [
    BoxShadow(color: Color(0x0A191817), blurRadius: 10, offset: Offset(0, 2)),
  ];
}
