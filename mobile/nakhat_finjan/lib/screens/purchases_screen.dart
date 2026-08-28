import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/order_response.dart';
import '../providers/customer_provider.dart';
import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../utils/arabic_format.dart';
import '../widgets/surfaces.dart';

/// What became of an order. The three the design draws, with the tint each
/// carries: olive for a clean order, caramel for a partial return, brick for a
/// cancellation.
enum OrderStatus {
  completed(
    'مكتمل',
    AppColors.earn,
    AppColors.earnTint,
    AppColors.earnTintBorder,
  ),
  returned(
    'مُرتجع',
    AppColors.caramel,
    AppColors.caramelTint,
    AppColors.caramelTintBorder,
  ),
  cancelled(
    'ملغى',
    AppColors.deduct,
    AppColors.deductTint,
    AppColors.deductTintBorder,
  );

  const OrderStatus(this.label, this.foreground, this.background, this.border);

  final String label;
  final Color foreground;
  final Color background;
  final Color border;

  /// Maps the wire status onto the pill this screen draws.
  ///
  /// A status this build does not recognise falls to [completed]: it is what
  /// the overwhelming majority of orders are, and an order that reached the
  /// customer's history did happen. Showing it plainly beats inventing a fourth
  /// pill for a value we cannot describe.
  static OrderStatus fromWire(OrderWireStatus status) => switch (status) {
    OrderWireStatus.completed => completed,
    OrderWireStatus.returned => returned,
    OrderWireStatus.cancelled => cancelled,
    OrderWireStatus.unknown => completed,
  };
}

/// One line on an order — «كابتشينو × 2».
class PurchaseLine {
  const PurchaseLine({required this.label, required this.price});

  final String label;
  final String price;

  /// Builds a line from an order item off the wire.
  ///
  /// The label folds the quantity into the product name the way the design
  /// writes it, and the price is the line total rather than the unit price —
  /// the card shows what each line cost, not what one of it costs.
  factory PurchaseLine.fromItem(OrderItemResponse item) => PurchaseLine(
    label: '${item.productName} × ${formatQuantity(item.quantity)}',
    price: formatMoney(item.lineTotal),
  );
}

/// One order in the history.
class Purchase {
  const Purchase({
    required this.number,
    required this.date,
    required this.status,
    required this.lines,
    required this.total,
    required this.pointsEarned,
    this.pointsSpent,
  });

  /// The order's id, as the customer reads it out.
  ///
  /// The same figure the cashier types into the dashboard's order lookup, so it
  /// stays in Western digits like every other number in this app — a customer
  /// reading «١٢٣» off their screen could not be matched against a field that
  /// parses `123`.
  final String number;

  final String date;
  final OrderStatus status;
  final List<PurchaseLine> lines;
  final String total;
  final String pointsEarned;

  /// Null when the order was paid entirely in cash — most of them.
  final String? pointsSpent;

  /// Builds a card from an order off the wire.
  factory Purchase.fromOrder(OrderResponse order) => Purchase(
    number: order.orderId.toString(),
    date: formatArabicDate(order.createdAt),
    status: OrderStatus.fromWire(order.status),
    lines: order.items.map(PurchaseLine.fromItem).toList(growable: false),
    total: formatMoney(order.total),
    pointsEarned: order.pointsEarned.toString(),
    // Only shown when points were actually spent: a "استبدلت 0 نقطة" line on
    // every cash order would be noise on the majority of the list.
    pointsSpent: order.pointsRedeemed > 0
        ? order.pointsRedeemed.toString()
        : null,
  );
}

/// Read-only purchase history.
///
/// Deliberately has no return or cancel control: those are the cashier's
/// actions in the dashboard, and putting them here would imply the customer can
/// reverse their own order.
class PurchasesScreen extends StatelessWidget {
  const PurchasesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final customers = context.watch<CustomerProvider>();
    final purchases = customers.orders
        .map(Purchase.fromOrder)
        .toList(growable: false);

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
            child: switch (customers.ordersStatus) {
              SectionStatus.idle || SectionStatus.loading => const Center(
                child: CircularProgressIndicator(color: AppColors.caramel),
              ),
              SectionStatus.error => _Padded(
                child: ErrorState(
                  message:
                      customers.ordersError ??
                      'صار خطأ، ما قدرنا نحمّل مشترياتك',
                  onRetry: () =>
                      context.read<CustomerProvider>().refreshOrders(),
                ),
              ),
              // `empty` shares this branch so the pull gesture survives an
              // empty history: a customer whose first order was just rung up
              // would otherwise have no way to see it, since the tabs live in
              // an IndexedStack and only load once. The gesture is received by
              // the scrollable, so the empty card sits inside a list of its own
              // rather than beside one.
              SectionStatus.empty || SectionStatus.loaded => RefreshIndicator(
                color: AppColors.caramel,
                backgroundColor: AppColors.surface,
                onRefresh: () =>
                    context.read<CustomerProvider>().refreshOrders(),
                child: purchases.isEmpty
                    ? ListView(
                        physics: const AlwaysScrollableScrollPhysics(),
                        padding: const EdgeInsets.symmetric(
                          horizontal: AppMetrics.screenPadding,
                        ),
                        children: const [
                          EmptyState(
                            icon: Icons.shopping_bag_outlined,
                            message: 'لسا ما اشتريت إشي',
                            verticalPadding: 64,
                          ),
                        ],
                      )
                    : ListView.separated(
                        physics: const AlwaysScrollableScrollPhysics(),
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
            },
          ),
        ],
      ),
    );
  }
}

/// The screen's side padding, for the states that are a single centred card
/// rather than a list that supplies its own.
class _Padded extends StatelessWidget {
  const _Padded({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(horizontal: AppMetrics.screenPadding),
    child: child,
  );
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
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      purchase.date,
                      style: AppText.rowLabelStrong,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 3),
                    _OrderNumber(number: purchase.number),
                  ],
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
                  Expanded(child: Text(line.label, style: AppText.orderItem)),
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

/// «طلب #14» — what the customer reads out when a cashier looks the order up.
///
/// Two Texts rather than one interpolated string, because `#` is a
/// direction-neutral character: inside an RTL paragraph the bidi algorithm is
/// free to resolve `#14` as `14#`, which is the one thing this label must never
/// do. Pinning the number's own Text to LTR settles it, while the Row inherits
/// the screen's RTL so the Arabic word still sits on the right.
class _OrderNumber extends StatelessWidget {
  const _OrderNumber({required this.number});

  final String number;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Text('طلب', style: AppText.rowMeta),
        const SizedBox(width: 4),
        Text(
          '#$number',
          style: AppText.rowMeta,
          textDirection: TextDirection.ltr,
        ),
      ],
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
