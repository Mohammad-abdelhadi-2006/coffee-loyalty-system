import 'package:flutter/material.dart';

import 'app_colors.dart';
import 'app_text.dart';

/// Shared geometry from the design canvas. Named rather than inlined so the
/// 52 px button and the 14 px radius stay the same 52 and 14 everywhere.
class AppMetrics {
  const AppMetrics._();

  /// Side padding on every screen.
  static const double screenPadding = 20;

  /// Cards and list containers.
  static const double radiusCard = 20;

  /// Buttons.
  static const double radiusButton = 14;

  /// Text fields and the OTP boxes.
  static const double radiusField = 16;

  /// The single primary CTA, and every button beside it.
  static const double buttonHeight = 52;

  /// A text field.
  static const double fieldHeight = 60;

  /// A tappable list row — the Material minimum, which the design already meets.
  static const double rowMinHeight = 56;
}

/// The app's one [ThemeData].
///
/// Most of the design is drawn by explicit widgets rather than Material
/// defaults, so this sets the ground colour, the ripple, and the text
/// selection, and leaves the rest to [AppText] and [AppColors].
ThemeData buildAppTheme() {
  const scheme = ColorScheme.light(
    primary: AppColors.caramel,
    onPrimary: Colors.white,
    secondary: AppColors.gold,
    onSecondary: AppColors.ink,
    surface: AppColors.surface,
    onSurface: AppColors.ink,
    error: AppColors.deduct,
    onError: Colors.white,
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: scheme,
    scaffoldBackgroundColor: AppColors.background,
    fontFamily: AppText.arabic,
    splashFactory: InkRipple.splashFactory,
    textSelectionTheme: const TextSelectionThemeData(
      cursorColor: AppColors.caramel,
      selectionColor: AppColors.goldTint,
      selectionHandleColor: AppColors.caramel,
    ),
    // The design has no Material app bars — each screen draws its own title —
    // so this only matters for the system overlay style.
    appBarTheme: const AppBarTheme(
      backgroundColor: AppColors.background,
      surfaceTintColor: Colors.transparent,
      elevation: 0,
      centerTitle: false,
    ),
    bottomSheetTheme: const BottomSheetThemeData(
      backgroundColor: AppColors.background,
      surfaceTintColor: Colors.transparent,
      showDragHandle: false,
    ),
  );
}
