import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../widgets/surfaces.dart';

/// What became of an order. The three the design draws, with the tint each
/// carries: olive for a clean order, caramel for a partial return, brick for a
/// cancellation.
enum OrderStatus {
  completed('مكتمل', AppColors.earn, AppColors.earnTint, AppColors.earnTintBorder),
  returned('مُرتجع', AppColors.caramel, AppColors.caramelTint, AppColors.caramelTintBorder),
  cancelled('ملغى', AppColors.deduct, AppColors.deductTint, AppColors.deductTintBorder);

  const OrderStatus(this.label, this.foreground, this.background, this.border);

  final String label;
  final Color foreground;
  final Color background;
  final Color border;
}

/// One line on an order — «كابتشينو × 2».
class PurchaseLine {
  const PurchaseLine({required this.label, required this.price});

  final String label;
  final String price;
}

/// One order in the history.
class Purchase {
  const Purchase({
    required this.date,
    required this.status,
    required this.lines,
    required this.total,
    required this.pointsEarned,
    this.pointsSpent,
  });

  final String date;
  final OrderStatus status;
  final List<PurchaseLine> lines;
  final String total;
  final String pointsEarned;

  /// Null when the order was paid entirely in cash — most of them.
  final String? pointsSpent;
}

/// Read-only purchase history.
///
/// Deliberately has no return or cancel control: those are the cashier's
/// actions in the dashboard, and putting them here would imply the customer can
/// reverse their own order.
class PurchasesScreen extends StatelessWidget {
  const PurchasesScreen({super.key, this.purchases = _samplePurchases});

  final List<Purchase> purchases;

  /// Placeholder data from the design canvas. The live list comes from
  /// GET /api/customers/me/orders.
  static const List<Purchase> _samplePurchases = [
    Purchase(
      date: '20 آب 2026',
      status: OrderStatus.completed,
      lines: [
        PurchaseLine(label: 'كابتشينو × 2', price: '3.00'),
        PurchaseLine(label: 'اسبريسو × 1', price: '0.75'),
      ],
      total: '3.75',
      pointsEarned: '11',
    ),
    Purchase(
      date: '14 آب 2026',
      status: OrderStatus.completed,
      lines: [
        PurchaseLine(label: 'ايس لاتيه × 1', price: '1.50'),
        PurchaseLine(label: 'فرابتشينو × 1', price: '2.00'),
      ],
      total: '3.50',
      pointsEarned: '10',
      pointsSpent: '250',
    ),
    Purchase(
      date: '9 آب 2026',
      status: OrderStatus.cancelled,
      lines: [PurchaseLine(label: 'قهوة تركي × 2', price: '1.00')],
      total: '1.00',
      pointsEarned: '3',
    ),
    Purchase(
      date: '2 آب 2026',
      status: OrderStatus.returned,
      lines: [
        PurchaseLine(label: 'موهيتو فراولة × 2', price: '3.50'),
        PurchaseLine(label: 'بن كولومبي × 0.5 كغم', price: '6.00'),
      ],
      total: '9.50',
      pointsEarned: '28',
    ),
  ];

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      bottom: false,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Padding(
            padding: EdgeInsets.fromLTRB(AppMetrics.screenPadding, 10, 20, 0),
            child: Text('مشترياتي', style: AppText.screenTitle),
          ),
          const SizedBox(height: 18),
          Expanded(
            child: purchases.isEmpty
                ? const Padding(
                    padding: EdgeInsets.symmetric(
                      horizontal: AppMetrics.screenPadding,
                    ),
                    child: EmptyState(
                      icon: Icons.shopping_bag_outlined,
                      message: 'لسا ما اشتريت إشي',
                      verticalPadding: 64,
                    ),
                  )
                : ListView.separated(
                    padding: const EdgeInsets.fromLTRB(
                      AppMetrics.screenPadding,
                      0,
                      AppMetrics.screenPadding,
                      24,
                    ),
                    itemCount: purchases.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 14),
                    itemBuilder: (context, index) =>
                        _PurchaseCard(purchase: purchases[index]),
                  ),
          ),
        ],
      ),
    );
  }
}

class _PurchaseCard extends StatelessWidget {
  const _PurchaseCard({required this.purchase});

  final Purchase purchase;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 17),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  purchase.date,
                  style: AppText.rowLabelStrong,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              const SizedBox(width: 12),
              _StatusPill(status: purchase.status),
            ],
          ),
          const SizedBox(height: 14),
          for (final line in purchase.lines)
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.baseline,
                textBaseline: TextBaseline.alphabetic,
                children: [
                  Expanded(
                    child: Text(line.label, style: AppText.orderItem),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    line.price,
                    style: AppText.price.copyWith(
                      fontSize: 13.5,
                      fontWeight: FontWeight.w400,
                      color: AppColors.ink55,
                    ),
                    textDirection: TextDirection.ltr,
                  ),
                ],
              ),
            ),
          const SizedBox(height: 6),
          const Divider(height: 1, thickness: 1, color: AppColors.hairline),
          const SizedBox(height: 14),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              _Total(amount: purchase.total),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      'كسبت ${purchase.pointsEarned} نقطة',
                      style: AppText.orderPoints.copyWith(
                        color: AppColors.earn,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                    if (purchase.pointsSpent != null) ...[
                      const SizedBox(height: 4),
                      Text(
                        'استبدلت ${purchase.pointsSpent} نقطة',
                        style: AppText.orderPoints.copyWith(
                          color: AppColors.deduct,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _Total extends StatelessWidget {
  const _Total({required this.amount});

  final String amount;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      textDirection: TextDirection.ltr,
      crossAxisAlignment: CrossAxisAlignment.baseline,
      textBaseline: TextBaseline.alphabetic,
      children: [
        Text(
          amount,
          style: AppText.price.copyWith(
            fontSize: 17,
            fontWeight: FontWeight.w600,
          ),
          textDirection: TextDirection.ltr,
        ),
        const SizedBox(width: 5),
        Text(
          'د.أ',
          style: AppText.currency.copyWith(fontSize: 12.5),
          textDirection: TextDirection.rtl,
        ),
      ],
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.status});

  final OrderStatus status;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 5),
      decoration: BoxDecoration(
        color: status.background,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: status.border),
      ),
      child: Text(
        status.label,
        style: AppText.statusPill.copyWith(color: status.foreground),
      ),
    );
  }
}
