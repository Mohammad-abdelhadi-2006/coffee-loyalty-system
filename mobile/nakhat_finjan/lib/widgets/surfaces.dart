import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import 'app_buttons.dart';

/// The white rounded container everything sits in: the balance, a list of
/// rows, an empty state, an order.
class AppCard extends StatelessWidget {
  const AppCard({
    super.key,
    required this.child,
    this.padding = EdgeInsets.zero,
  });

  final Widget child;
  final EdgeInsetsGeometry padding;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding,
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(AppMetrics.radiusCard),
        border: Border.all(color: AppColors.cardBorder),
        boxShadow: AppColors.cardShadow,
      ),
      child: child,
    );
  }
}

/// Stacks rows inside an [AppCard] with a hairline between them and none above
/// the first -- the design's "no top border on the first row" rule, in one
/// place so no caller has to remember it.
class HairlineList extends StatelessWidget {
  const HairlineList({super.key, required this.children});

  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          for (var i = 0; i < children.length; i++)
            DecoratedBox(
              decoration: BoxDecoration(
                border: i == 0
                    ? null
                    : const Border(top: BorderSide(color: AppColors.hairline)),
              ),
              child: children[i],
            ),
        ],
      ),
    );
  }
}

/// A label-and-value row -- the menu items, the settings rows.
class AppRow extends StatelessWidget {
  const AppRow({
    super.key,
    required this.label,
    this.labelStyle,
    this.trailing,
    this.trailingIcon,
    this.onTap,
  });

  final String label;
  final TextStyle? labelStyle;
  final Widget? trailing;

  /// The small caramel "opens elsewhere" mark beside the website row.
  final IconData? trailingIcon;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final row = Padding(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
      // The label takes the slack and ellipsizes; the trailing value never
      // does. A price or a phone number that got clipped would be worse than
      // useless, whereas a long product name reads fine truncated.
      child: Row(
        children: [
          Expanded(
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Flexible(
                  child: Text(
                    label,
                    style: labelStyle ?? AppText.rowLabel,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                if (trailingIcon != null) ...[
                  const SizedBox(width: 8),
                  Icon(trailingIcon, size: 15, color: AppColors.caramel),
                ],
              ],
            ),
          ),
          if (trailing != null) ...[const SizedBox(width: 12), trailing!],
        ],
      ),
    );

    return ConstrainedBox(
      constraints: const BoxConstraints(minHeight: AppMetrics.rowMinHeight),
      child: onTap == null
          ? row
          : Material(
              color: Colors.transparent,
              child: InkWell(onTap: onTap, child: row),
            ),
    );
  }
}

/// A price with its unit riding alongside: "1.50 د.أ", "12.00 د.أ / كغم".
///
/// The figure is forced LTR inside the RTL line so a decimal never reorders,
/// and the unit is given its own direction so it cannot drag the digits around.
class PriceLabel extends StatelessWidget {
  const PriceLabel({super.key, required this.amount, this.unit = 'د.أ'});

  final String amount;
  final String unit;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      textDirection: TextDirection.ltr,
      crossAxisAlignment: CrossAxisAlignment.baseline,
      textBaseline: TextBaseline.alphabetic,
      children: [
        Text(amount, style: AppText.price, textDirection: TextDirection.ltr),
        const SizedBox(width: 5),
        Text(unit, style: AppText.currency, textDirection: TextDirection.rtl),
      ],
    );
  }
}

/// The centred icon-over-caption block used for every "nothing here" card.
class EmptyState extends StatelessWidget {
  const EmptyState({
    super.key,
    required this.icon,
    required this.message,
    this.verticalPadding = 44,
  });

  final IconData icon;
  final String message;
  final double verticalPadding;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      padding: EdgeInsets.symmetric(vertical: verticalPadding, horizontal: 20),
      child: Column(
        children: [
          Icon(
            icon,
            size: 40,
            color: AppColors.caramel.withValues(alpha: 0.45),
          ),
          const SizedBox(height: 16),
          Text(
            message,
            style: AppText.placeholder,
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

/// The failed-to-load card: warning glyph, one line, and a retry that is a
/// bordered pill rather than a filled button -- retrying is not the primary
/// action on the screen, it is a recovery from one.
class ErrorState extends StatelessWidget {
  const ErrorState({
    super.key,
    required this.message,
    required this.onRetry,
    this.retryLabel = 'إعادة المحاولة',
  });

  final String message;
  final VoidCallback onRetry;
  final String retryLabel;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      padding: const EdgeInsets.symmetric(vertical: 52, horizontal: 24),
      child: Column(
        children: [
          const Icon(
            Icons.warning_amber_rounded,
            size: 42,
            color: AppColors.deduct,
          ),
          const SizedBox(height: 18),
          Text(
            message,
            style: AppText.placeholder.copyWith(
              color: AppColors.ink62,
              height: 1.7,
              fontSize: 15.5,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 18),
          SecondaryButton(
            label: retryLabel,
            onPressed: onRetry,
            icon: Icons.refresh,
            expand: false,
          ),
        ],
      ),
    );
  }
}
