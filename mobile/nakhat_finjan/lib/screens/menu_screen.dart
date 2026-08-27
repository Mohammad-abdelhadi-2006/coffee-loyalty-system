import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../widgets/surfaces.dart';

/// One item on the menu. Browse only — the design has no cart, no add, no
/// quantity control anywhere on this screen.
class MenuItem {
  const MenuItem({required this.name, required this.price, this.perKilo = false});

  final String name;

  /// Formatted to two decimals, as the backend sends it.
  final String price;

  /// Beans are sold by weight, so the unit reads «د.أ / كغم» rather than «د.أ».
  final bool perKilo;
}

/// The menu: a category strip and the items in the selected category.
class MenuScreen extends StatefulWidget {
  const MenuScreen({super.key});

  @override
  State<MenuScreen> createState() => _MenuScreenState();
}

class _MenuScreenState extends State<MenuScreen> {
  /// The six categories from the design. The real list comes from the products
  /// endpoint — these are here so the strip has something to lay out.
  static const List<String> _categories = [
    'قهوة ساخنة',
    'قهوة باردة',
    'موهيتو',
    'ميلك شيك',
    'حلويات',
    'بن',
  ];

  int _selected = 0;

  /// ⚠️ INCOMPLETE — flagged rather than invented.
  ///
  /// The design canvas says the source menu was cut off after «موهيتو»: ميلك
  /// شيك، حلويات and بن never arrived, and the بن prices below are the
  /// designer's placeholders, not real ones. Only «قهوة ساخنة» is real here.
  /// The three unfilled categories render the empty state instead of made-up
  /// items — a wrong price on a menu is worse than a blank one.
  ///
  /// TODO(content): get the remaining categories and the real per-kilo bean
  /// prices from the shop, then delete this map — the live data comes from
  /// GET /api/products.
  static const Map<String, List<MenuItem>> _itemsByCategory = {
    'قهوة ساخنة': [
      MenuItem(name: 'سبانش لاتيه', price: '1.50'),
      MenuItem(name: 'لاتيه', price: '1.50'),
      MenuItem(name: 'كابتشينو', price: '1.50'),
      MenuItem(name: 'اميركانو', price: '1.00'),
      MenuItem(name: 'V60', price: '1.50'),
      MenuItem(name: 'اسبريسو', price: '0.75'),
      MenuItem(name: 'دبل شوت', price: '1.00'),
      MenuItem(name: 'موكا دارك', price: '2.00'),
      MenuItem(name: 'فلات وايت', price: '1.50'),
      MenuItem(name: 'قهوة تركي', price: '0.50'),
      MenuItem(name: 'ريد آي', price: '1.50'),
    ],
    // Placeholder prices — see the note above.
    'بن': [
      MenuItem(name: 'بن كولومبي', price: '12.00', perKilo: true),
      MenuItem(name: 'بن اثيوبي', price: '14.00', perKilo: true),
      MenuItem(name: 'بن برازيلي', price: '10.00', perKilo: true),
      MenuItem(name: 'خلطة المقهى', price: '9.00', perKilo: true),
    ],
  };

  @override
  Widget build(BuildContext context) {
    final category = _categories[_selected];
    final items = _itemsByCategory[category] ?? const <MenuItem>[];

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
            child: ListView(
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
                            amount: item.price,
                            unit: item.perKilo ? 'د.أ / كغم' : 'د.أ',
                          ),
                        ),
                    ],
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
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

  final List<String> categories;
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
          label: categories[index],
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
