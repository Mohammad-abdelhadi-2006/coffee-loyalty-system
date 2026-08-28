import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text.dart';
import '../../theme/app_theme.dart';

/// «كيف تكسب نقاط».
///
/// Intentionally empty. The design canvas does not draw this screen, and its
/// content is shop policy — the earn rate, the 250-point redemption minimum,
/// what a return does to a balance. Writing plausible-looking copy here would
/// put wrong numbers in front of customers, so it stays a placeholder until the
/// real text arrives.
class HowToEarnScreen extends StatelessWidget {
  const HowToEarnScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const _InfoPlaceholder(
      title: 'كيف تكسب نقاط',
      // TODO(content): the shop's real earn/redeem rules.
    );
  }
}

/// «عن المقهى / تواصل».
///
/// Also intentionally empty: address, hours, phone and social links are facts
/// about a real business, and none of them are in the design or the repo.
class AboutScreen extends StatelessWidget {
  const AboutScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const _InfoPlaceholder(
      title: 'عن المقهى / تواصل',
      // TODO(content): the shop's address, hours, phone and links.
    );
  }
}

/// The shared frame for both: a back arrow, the title, and an honest note that
/// nothing is here yet. A blank screen would read as a bug.
class _InfoPlaceholder extends StatelessWidget {
  const _InfoPlaceholder({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: AppMetrics.screenPadding,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: IconButton(
                  onPressed: () => Navigator.of(context).maybePop(),
                  icon: const Icon(
                    Icons.arrow_forward,
                    size: 24,
                    color: AppColors.ink,
                  ),
                  tooltip: 'رجوع',
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(
                    minWidth: 48,
                    minHeight: 48,
                  ),
                ),
              ),
              const SizedBox(height: 12),
              Text(title, style: AppText.screenTitle),
              const Spacer(),
              Center(
                child: Text(
                  'قريباً',
                  style: AppText.placeholder.copyWith(color: AppColors.ink42),
                ),
              ),
              const Spacer(flex: 2),
            ],
          ),
        ),
      ),
    );
  }
}
