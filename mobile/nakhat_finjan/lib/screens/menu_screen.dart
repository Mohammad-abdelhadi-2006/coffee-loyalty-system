import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/product_response.dart';
import '../providers/customer_provider.dart';
import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../utils/arabic_format.dart';
import '../widgets/surfaces.dart';

/// The menu: a category strip and the items in the selected category.
///
/// Browse only — the design has no cart, no add, and no quantity control
/// anywhere on this screen.
class MenuScreen extends StatefulWidget {
  const MenuScreen({super.key});

  @override
  State<MenuScreen> createState() => _MenuScreenState();
}

class _MenuScreenState extends State<MenuScreen> {
  /// The six strips, in the order the design lays them out. Fixed rather than
  /// derived from what came back, so the strip does not reshuffle itself
  /// between loads or lose a category the moment it sells out.
  static const List<ProductCategory> _categories = ProductCategory.displayed;

  int _selected = 0;

  @override
  Widget build(BuildContext context) {
    final customers = context.watch<CustomerProvider>();
    final category = _categories[_selected];
    final items = customers.productsIn(category);

    return SafeArea(
      bottom: false,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Padding(
            padding: EdgeInsets.fromLTRB(AppMetrics.screenPadding, 10, 20, 0),
            child: Text('المنيو', style: AppText.screenTitle),
          ),
          const SizedBox(height: 18),
          _CategoryStrip(
            categories: _categories,
            selected: _selected,
            onSelected: (index) => setState(() => _selected = index),
          ),
          const SizedBox(height: 18),
          Expanded(
            child: switch (customers.productsStatus) {
              SectionStatus.idle || SectionStatus.loading => const Center(
                child: CircularProgressIndicator(color: AppColors.caramel),
              ),
              SectionStatus.error => _Padded(
                child: ErrorState(
                  message:
                      customers.productsError ??
                      'صار خطأ، ما قدرنا نحمّل المنيو',
                  onRetry: () =>
                      context.read<CustomerProvider>().refreshProducts(),
                ),
              ),
              // `empty` here means the whole menu came back empty; a category
              // with nothing in it is the same empty card, reached through the
              // `items.isEmpty` branch below.
              SectionStatus.empty || SectionStatus.loaded => RefreshIndicator(
                color: AppColors.caramel,
                backgroundColor: AppColors.surface,
                onRefresh: () =>
                    context.read<CustomerProvider>().refreshProducts(),
                child: ListView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.fromLTRB(
                    AppMetrics.screenPadding,
                    0,
                    AppMetrics.screenPadding,
                    24,
                  ),
                  children: [
                    if (items.isEmpty)
                      const EmptyState(
                        icon: Icons.notes_outlined,
                        message: 'لا يوجد أصناف حالياً',
                        verticalPadding: 56,
                      )
                    else
                      HairlineList(
                        children: [
                          for (final item in items)
                            AppRow(
                              label: item.name,
                              trailing: PriceLabel(
                                amount: formatMoney(item.price),
                                // Beans sell by weight, so their price needs
                                // the unit spelled out; everything else is per
                                // item and does not.
                                unit: item.unitType == ProductUnitType.kg
                                    ? 'د.أ / كغم'
                                    : 'د.أ',
                              ),
                            ),
                        ],
                      ),
                  ],
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

/// The horizontally scrolling category pills.
///
/// The strip scrolls rather than wrapping so the row height stays fixed no
/// matter how many categories the shop adds.
class _CategoryStrip extends StatelessWidget {
  const _CategoryStrip({
    required this.categories,
    required this.selected,
    required this.onSelected,
  });

  final List<ProductCategory> categories;
  final int selected;
  final ValueChanged<int> onSelected;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 48,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(
          horizontal: AppMetrics.screenPadding,
        ),
        itemCount: categories.length,
        separatorBuilder: (_, _) => const SizedBox(width: 10),
        itemBuilder: (context, index) => _CategoryChip(
          label: categories[index].label,
          isSelected: index == selected,
          onTap: () => onSelected(index),
        ),
      ),
    );
  }
}

class _CategoryChip extends StatelessWidget {
  const _CategoryChip({
    required this.label,
    required this.isSelected,
    required this.onTap,
  });

  final String label;
  final bool isSelected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: isSelected ? AppColors.goldTint : AppColors.surface,
      borderRadius: BorderRadius.circular(999),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(999),
        child: Ink(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(999),
            border: Border.all(
              color: isSelected ? AppColors.goldDeep : AppColors.fieldBorder,
            ),
          ),
          child: Center(
            child: Text(
              label,
              style: AppText.chip.copyWith(
                color: isSelected ? AppColors.caramelDark : AppColors.ink62,
                fontWeight: isSelected ? FontWeight.w700 : FontWeight.w500,
              ),
            ),
          ),
        ),
      ),
    );
  }
}
