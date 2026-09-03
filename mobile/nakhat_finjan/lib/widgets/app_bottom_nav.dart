import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import 'entrance.dart';

/// The four tabs, in the design's order. Index 0 is the rightmost tab on
/// screen: the whole app is RTL, so the row lays itself out from the right and
/// this list needs no reversing.
enum AppTab {
  home('الرئيسية', Icons.home_rounded, Icons.home_outlined),
  menu('المنيو', Icons.local_cafe_rounded, Icons.local_cafe_outlined),
  purchases(
    'مشترياتي',
    Icons.shopping_bag_rounded,
    Icons.shopping_bag_outlined,
  ),
  settings('الإعدادات', Icons.tune_rounded, Icons.tune_outlined);

  const AppTab(this.label, this.activeIcon, this.icon);

  final String label;

  /// Filled when the tab is current, hairline when it is not — the design's
  /// only signal besides the caramel tint and the bolder label.
  final IconData activeIcon;
  final IconData icon;
}

/// The bottom bar. White, one hairline along its top edge, and bottom padding
/// that clears the gesture inset on top of the design's own 20.
class AppBottomNav extends StatelessWidget {
  const AppBottomNav({
    super.key,
    required this.current,
    required this.onSelected,
  });

  final AppTab current;
  final ValueChanged<AppTab> onSelected;

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.viewPaddingOf(context).bottom;

    return EntranceGroup(
      itemDuration: const Duration(milliseconds: 300),
      stagger: const Duration(milliseconds: 75),
      maxSteps: AppTab.values.length,
      child: Stack(
        children: [
          // The bar itself arrives first and alone: the white surface and its
          // hairline are drawn in before there is anything standing on them.
          // Index 0 owns that beat, and the icons start from index 1.
          Positioned.fill(
            child: EntranceItem(
              index: 0,
              rise: 0.9,
              child: const DecoratedBox(
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  border: Border(top: BorderSide(color: AppColors.navBorder)),
                ),
              ),
            ),
          ),

          // The Row is the only unpositioned child, so it is what gives the
          // Stack its height and the surface behind it its size.
          Padding(
            padding: EdgeInsets.fromLTRB(12, 8, 12, 20 + bottomInset),
            child: Row(
              children: [
                for (final (index, tab) in AppTab.values.indexed)
                  Expanded(
                    child: EntranceItem(
                      index: 1 + index,
                      // Up from under the bar, and a touch past its resting
                      // place before settling — the hop that was asked for.
                      // Kept small: `easeOutBack` at this distance reads as a
                      // pop, not a bounce.
                      rise: 1.1,
                      curve: Curves.easeOutBack,
                      child: _NavTab(
                        tab: tab,
                        isCurrent: tab == current,
                        onTap: () => onSelected(tab),
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _NavTab extends StatelessWidget {
  const _NavTab({
    required this.tab,
    required this.isCurrent,
    required this.onTap,
  });

  final AppTab tab;
  final bool isCurrent;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = isCurrent ? AppColors.caramel : AppColors.ink45;

    return Semantics(
      button: true,
      selected: isCurrent,
      label: tab.label,
      child: InkResponse(
        onTap: onTap,
        radius: 44,
        child: ConstrainedBox(
          constraints: const BoxConstraints(minHeight: 56),
          child: Column(
            // Scaffold measures its bottomNavigationBar against the full screen
            // height, and the ConstrainedBox above sets a floor, not a ceiling.
            // Without this the column grows to fill all 844px, the nav becomes
            // the whole screen, and the body is laid out at zero height.
            mainAxisSize: MainAxisSize.min,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                isCurrent ? tab.activeIcon : tab.icon,
                size: 24,
                color: color,
              ),
              const SizedBox(height: 5),
              Text(
                tab.label,
                style: AppText.navLabel.copyWith(
                  color: color,
                  fontWeight: isCurrent ? FontWeight.w700 : FontWeight.w400,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
