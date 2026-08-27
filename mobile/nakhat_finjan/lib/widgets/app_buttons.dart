import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';

/// The filled gold CTA — the only filled button in the app.
///
/// Four states, all drawn in the design's "حالات الأزرار" frame: enabled,
/// disabled (a wash of the gold, no border emphasis), busy (a spinner beside
/// the label, taps swallowed), and the bordered variant in [SecondaryButton].
///
/// A null [onPressed] renders the disabled state, matching Flutter's own
/// convention, so callers gate on the callback rather than on a flag.
class PrimaryButton extends StatelessWidget {
  const PrimaryButton({
    super.key,
    required this.label,
    this.onPressed,
    this.isBusy = false,
  });

  final String label;
  final VoidCallback? onPressed;

  /// Shows the spinner and ignores taps regardless of [onPressed].
  final bool isBusy;

  @override
  Widget build(BuildContext context) {
    final enabled = onPressed != null && !isBusy;

    final Color fill;
    if (isBusy) {
      fill = AppColors.goldDeep;
    } else if (enabled) {
      fill = AppColors.gold;
    } else {
      fill = AppColors.goldDisabled;
    }

    return SizedBox(
      height: AppMetrics.buttonHeight,
      width: double.infinity,
      child: Material(
        color: fill,
        borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
        child: InkWell(
          onTap: enabled ? onPressed : null,
          borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
          child: Ink(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
              border: Border.all(
                color: enabled || isBusy
                    ? AppColors.goldDeep
                    : AppColors.goldDisabledBorder,
              ),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                if (isBusy) ...[
                  const SizedBox(
                    width: 19,
                    height: 19,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      valueColor: AlwaysStoppedAnimation(AppColors.ink),
                      backgroundColor: AppColors.ink25,
                    ),
                  ),
                  const SizedBox(width: 10),
                ],
                Text(
                  label,
                  style: AppText.primaryButton.copyWith(
                    color: isBusy
                        ? AppColors.ink55
                        : (enabled ? AppColors.ink : AppColors.ink34),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

/// The bordered, unfilled button — «إلغاء», «إعادة المحاولة».
///
/// [color] tints both the hairline and the label, which is how the settings
/// screen's «تسجيل الخروج» turns destructive without becoming a filled button.
class SecondaryButton extends StatelessWidget {
  const SecondaryButton({
    super.key,
    required this.label,
    this.onPressed,
    this.icon,
    this.color,
    this.expand = true,
  });

  final String label;
  final VoidCallback? onPressed;
  final IconData? icon;

  /// Label and border tint. Defaults to ink.
  final Color? color;

  /// False makes the button hug its label — the home screen's retry pill.
  final bool expand;

  @override
  Widget build(BuildContext context) {
    final tint = color ?? AppColors.ink;

    final content = Row(
      mainAxisSize: expand ? MainAxisSize.max : MainAxisSize.min,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        if (icon != null) ...[
          Icon(icon, size: 18, color: tint),
          const SizedBox(width: 9),
        ],
        // Flexible so a long label ellipsizes instead of overflowing the pill —
        // the hugging variant sits inside a card whose width it does not know.
        Flexible(
          child: Text(
            label,
            style: AppText.secondaryButton.copyWith(color: tint),
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );

    return ConstrainedBox(
      constraints: BoxConstraints.tightFor(
        height: expand ? AppMetrics.buttonHeight : 48,
        width: expand ? double.infinity : null,
      ),
      child: Material(
        color: Colors.transparent,
        borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
        child: InkWell(
          onTap: onPressed,
          borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
          child: Ink(
            padding: expand
                ? EdgeInsets.zero
                : const EdgeInsets.symmetric(horizontal: 26),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
              border: Border.all(
                color: color == null
                    ? AppColors.ink25
                    : color!.withValues(alpha: 0.35),
              ),
            ),
            child: content,
          ),
        ),
      ),
    );
  }
}

/// The quiet text-only action under the OTP CTA.
class TextAction extends StatelessWidget {
  const TextAction({super.key, required this.label, this.onPressed});

  final String label;
  final VoidCallback? onPressed;

  @override
  Widget build(BuildContext context) {
    final enabled = onPressed != null;
    return SizedBox(
      height: 48,
      width: double.infinity,
      child: TextButton(
        onPressed: onPressed,
        style: TextButton.styleFrom(
          foregroundColor: AppColors.ink,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
          ),
        ),
        child: Text(
          label,
          style: AppText.secondaryButton.copyWith(
            fontSize: 14.5,
            color: enabled ? AppColors.ink55 : AppColors.ink30,
          ),
        ),
      ),
    );
  }
}
