import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../widgets/shimmer_box.dart';
import '../widgets/surfaces.dart';

/// The four states the home screen can be in. Phase 1 has no data source, so
/// [HomeScreen.state] selects between them; wiring replaces it with a real
/// load result.
enum HomeState { loaded, loading, empty, error }

/// One row of the points ledger. A view model, deliberately not a wire model:
/// the backend's PointsTransactionResponse has a signed integer and an enum,
/// and turning those into "+11" and an olive tint is this layer's job.
class LedgerEntry {
  const LedgerEntry({
    required this.type,
    required this.amount,
    required this.date,
    required this.isDeduction,
  });

  /// «نقاط مكتسبة», «نقاط مستبدلة», «رصيد افتتاحي», …
  final String type;

  /// Already signed and formatted: "+11", "−250", or a bare "100" for an
  /// opening balance, which is neither a gain nor a loss.
  final String amount;
  final String date;

  /// Drives the tint only. An opening balance passes false and gets neutral ink
  /// because its [amount] carries no sign.
  final bool isDeduction;

  /// True when the amount is signed at all — an opening balance is not.
  bool get isSigned => amount.startsWith('+') || amount.startsWith('−');
}

/// Balance and ledger in one scroll, with pull-to-refresh over both.
class HomeScreen extends StatelessWidget {
  const HomeScreen({
    super.key,
    this.state = HomeState.loaded,
    this.customerName = 'محمد',
    this.pointsBalance = 1240,
    this.entries = _sampleEntries,
  });

  final HomeState state;
  final String customerName;
  final int pointsBalance;
  final List<LedgerEntry> entries;

  /// Placeholder data from the design canvas. Phase 1 draws static UI; the real
  /// values come from CustomerLoginResponse.pointsBalance and
  /// GET /api/customers/me/transactions.
  static const List<LedgerEntry> _sampleEntries = [
    LedgerEntry(
      type: 'نقاط مكتسبة',
      amount: '+11',
      date: '20 آب 2026 · 4:12 م',
      isDeduction: false,
    ),
    LedgerEntry(
      type: 'نقاط مستبدلة',
      amount: '−250',
      date: '14 آب 2026 · 6:40 م',
      isDeduction: true,
    ),
    LedgerEntry(
      type: 'استرجاع نقاط (إلغاء)',
      amount: '+3',
      date: '9 آب 2026 · 1:05 م',
      isDeduction: false,
    ),
    LedgerEntry(
      type: 'خصم نقاط (إرجاع)',
      amount: '−11',
      date: '2 آب 2026 · 7:22 م',
      isDeduction: true,
    ),
    LedgerEntry(
      type: 'نقاط مكتسبة',
      amount: '+21',
      date: '28 تموز 2026 · 5:10 م',
      isDeduction: false,
    ),
    LedgerEntry(
      type: 'رصيد افتتاحي',
      amount: '100',
      date: '3 حزيران 2026',
      isDeduction: false,
    ),
  ];

  /// Groups the thousands the way the design shows them (1,240).
  static String _formatBalance(int value) {
    final digits = value.toString();
    final buffer = StringBuffer();
    for (var i = 0; i < digits.length; i++) {
      if (i > 0 && (digits.length - i) % 3 == 0) buffer.write(',');
      buffer.write(digits[i]);
    }
    return buffer.toString();
  }

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      bottom: false,
      child: RefreshIndicator(
        color: AppColors.caramel,
        backgroundColor: AppColors.surface,
        // TODO(data): reload the balance and the ledger together — the design
        // note says pull-to-refresh reloads both.
        onRefresh: () async {},
        child: ListView(
          padding: const EdgeInsets.fromLTRB(
            AppMetrics.screenPadding,
            12,
            AppMetrics.screenPadding,
            24,
          ),
          children: switch (state) {
            HomeState.loading => const [_LoadingBody()],
            HomeState.error => [
              _Greeting(name: customerName),
              const SizedBox(height: 16),
              ErrorState(
                message: 'صار خطأ، ما قدرنا نحمّل بياناتك',
                onRetry: () {},
              ),
            ],
            HomeState.empty => [
              _Greeting(name: customerName),
              const SizedBox(height: 16),
              const _BalanceCard(balance: '0'),
              const SizedBox(height: 28),
              const _LedgerHeading(),
              const SizedBox(height: 12),
              const EmptyState(
                icon: Icons.local_cafe_outlined,
                message: 'لسا ما في حركات',
              ),
            ],
            HomeState.loaded => [
              _Greeting(name: customerName),
              const SizedBox(height: 16),
              _BalanceCard(balance: _formatBalance(pointsBalance)),
              const SizedBox(height: 28),
              const _LedgerHeading(),
              const SizedBox(height: 12),
              HairlineList(
                children: [
                  for (final entry in entries) _LedgerRow(entry: entry),
                ],
              ),
            ],
          },
        ),
      ),
    );
  }
}

class _Greeting extends StatelessWidget {
  const _Greeting({required this.name});

  final String name;

  @override
  Widget build(BuildContext context) {
    return Text('مرحباً، $name', style: AppText.greeting);
  }
}

class _LedgerHeading extends StatelessWidget {
  const _LedgerHeading();

  @override
  Widget build(BuildContext context) {
    return const Padding(
      padding: EdgeInsets.symmetric(horizontal: 4),
      child: Text('حركات النقاط', style: AppText.sectionTitle),
    );
  }
}

/// The balance, given the whole card and most of the screen's visual weight —
/// it is the one number the app exists to show.
class _BalanceCard extends StatelessWidget {
  const _BalanceCard({required this.balance});

  final String balance;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      padding: const EdgeInsets.fromLTRB(20, 38, 20, 32),
      child: Column(
        children: [
          FittedBox(
            child: Text(
              balance,
              style: AppText.balance,
              textDirection: TextDirection.ltr,
            ),
          ),
          const SizedBox(height: 10),
          const Text('نقطة', style: AppText.balanceUnit),
        ],
      ),
    );
  }
}

class _LedgerRow extends StatelessWidget {
  const _LedgerRow({required this.entry});

  final LedgerEntry entry;

  @override
  Widget build(BuildContext context) {
    final Color amountColor;
    if (!entry.isSigned) {
      amountColor = AppColors.ink75;
    } else if (entry.isDeduction) {
      amountColor = AppColors.deduct;
    } else {
      amountColor = AppColors.earn;
    }

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 15),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(entry.type, style: AppText.rowLabelStrong),
                const SizedBox(height: 5),
                Text(entry.date, style: AppText.rowMeta),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Text(
            entry.amount,
            style: AppText.amount.copyWith(color: amountColor),
            textDirection: TextDirection.ltr,
          ),
        ],
      ),
    );
  }
}

/// The skeleton. Every bar is sized to the copy it stands in for, so the
/// layout does not shift when the real values land.
class _LoadingBody extends StatelessWidget {
  const _LoadingBody();

  /// The design's six ledger placeholders, at its widths.
  static const List<double> _rowWidths = [180, 148, 208, 132, 168, 196];

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const ShimmerBox(width: 124, height: 15),
        const SizedBox(height: 16),
        AppCard(
          padding: const EdgeInsets.fromLTRB(20, 38, 20, 32),
          child: Column(
            children: const [
              ShimmerBox(width: 196, height: 76, radius: 12),
              SizedBox(height: 16),
              ShimmerBox(width: 56, height: 16),
            ],
          ),
        ),
        const SizedBox(height: 28),
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 4),
          child: ShimmerBox(width: 104, height: 16),
        ),
        const SizedBox(height: 12),
        HairlineList(
          children: [
            for (final width in _rowWidths)
              Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 18,
                  vertical: 17,
                ),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        ShimmerBox(width: width, height: 14, radius: 5),
                        const SizedBox(height: 9),
                        const ShimmerBox(width: 78, height: 11, radius: 5),
                      ],
                    ),
                    const Spacer(),
                    const ShimmerBox(width: 52, height: 16, radius: 5),
                  ],
                ),
              ),
          ],
        ),
      ],
    );
  }
}
