import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/points_transaction_response.dart';
import '../providers/customer_provider.dart';
import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../utils/arabic_format.dart';
import '../widgets/shimmer_box.dart';
import '../widgets/surfaces.dart';

/// The four states the home screen can be in, mapped from
/// [CustomerProvider.homeStatus].
enum HomeState { loaded, loading, empty, error }

/// One row of the points ledger. A view model, deliberately not a wire model:
/// the backend's [PointsTransactionResponse] has a signed integer and an enum,
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

  /// Builds a row from a ledger entry off the wire.
  ///
  /// The sign comes from [PointsTransactionResponse.amount] and never from its
  /// type: the ERD is explicit that the type is the *reason* and the sign is
  /// the balance effect, so reading direction from the type would let the
  /// colour disagree with the number on any row the server labels unexpectedly.
  factory LedgerEntry.fromTransaction(PointsTransactionResponse tx) {
    final isOpening = tx.type == PointsTransactionType.openingBalance;
    final isDeduction = tx.amount < 0;

    // U+2212 MINUS SIGN, matching the design — the ASCII hyphen is narrower
    // and sits at the wrong height beside tabular figures.
    final magnitude = tx.amount.abs().toString();
    final String amount;
    if (isOpening) {
      amount = magnitude;
    } else if (isDeduction) {
      amount = '−$magnitude';
    } else {
      amount = '+$magnitude';
    }

    return LedgerEntry(
      type: _labelFor(tx.type),
      amount: amount,
      date: formatLedgerTimestamp(tx.createdAt),
      isDeduction: isDeduction,
    );
  }

  /// The Arabic label for each reason, as the design words them.
  static String _labelFor(PointsTransactionType type) => switch (type) {
    PointsTransactionType.earn => 'نقاط مكتسبة',
    PointsTransactionType.redeem => 'نقاط مستبدلة',
    PointsTransactionType.refund => 'استرجاع نقاط (إلغاء)',
    PointsTransactionType.redeemReversal => 'خصم نقاط (إرجاع)',
    PointsTransactionType.openingBalance => 'رصيد افتتاحي',
    // A reason this build does not know. The amount and date are still true and
    // still add up, so the row is shown with a neutral label rather than hidden
    // — a ledger with a row missing is worse than one with a vague row.
    PointsTransactionType.unknown => 'حركة على الرصيد',
  };
}

/// Maps a section status onto the four frames this screen draws.
///
/// `idle` renders as loading: the shell asks for the data the moment it mounts,
/// so "not asked yet" and "asking" are the same instant to the customer, and a
/// fifth blank frame for it would only flash.
HomeState _stateFrom(SectionStatus status) => switch (status) {
  SectionStatus.idle || SectionStatus.loading => HomeState.loading,
  SectionStatus.loaded => HomeState.loaded,
  SectionStatus.empty => HomeState.empty,
  SectionStatus.error => HomeState.error,
};

/// Balance and ledger in one scroll, with pull-to-refresh over both.
class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final customers = context.watch<CustomerProvider>();

    final state = _stateFrom(customers.homeStatus);
    // The greeting drops the name rather than inventing one: on the very first
    // load there is no profile yet, and «مرحباً» alone reads fine.
    final name = customers.fullName;
    final entries = customers.transactions
        .map(LedgerEntry.fromTransaction)
        .toList(growable: false);

    return SafeArea(
      bottom: false,
      child: RefreshIndicator(
        color: AppColors.caramel,
        backgroundColor: AppColors.surface,
        // Balance and ledger together, which is what the design note requires —
        // `refreshHome` fetches both and lands them as one state change.
        onRefresh: () => context.read<CustomerProvider>().refreshHome(),
        child: ListView(
          // Always scrollable: pull-to-refresh needs somewhere to pull from,
          // and the error and empty bodies are shorter than the viewport.
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(
            AppMetrics.screenPadding,
            12,
            AppMetrics.screenPadding,
            24,
          ),
          children: switch (state) {
            HomeState.loading => const [_LoadingBody()],
            HomeState.error => [
              _Greeting(name: name),
              const SizedBox(height: 16),
              ErrorState(
                message:
                    customers.homeError ?? 'صار خطأ، ما قدرنا نحمّل بياناتك',
                onRetry: () => context.read<CustomerProvider>().refreshHome(),
              ),
            ],
            HomeState.empty => [
              _Greeting(name: name),
              const SizedBox(height: 16),
              _BalanceCard(
                balance: formatGroupedNumber(customers.pointsBalance),
              ),
              const SizedBox(height: 28),
              const _LedgerHeading(),
              const SizedBox(height: 12),
              const EmptyState(
                icon: Icons.local_cafe_outlined,
                message: 'لسا ما في حركات',
              ),
            ],
            HomeState.loaded => [
              _Greeting(name: name),
              const SizedBox(height: 16),
              _BalanceCard(
                balance: formatGroupedNumber(customers.pointsBalance),
              ),
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

  /// Null until the profile lands. The comma goes with it — «مرحباً،» trailing
  /// into nothing reads like the name failed to load, which it has, but saying
  /// so is not the greeting's job.
  final String? name;

  @override
  Widget build(BuildContext context) {
    final greeting = name == null || name!.isEmpty ? 'مرحباً' : 'مرحباً، $name';
    return Text(greeting, style: AppText.greeting);
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
