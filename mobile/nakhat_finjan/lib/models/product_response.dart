/// How a product is sold.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Enums/UnitType.cs`.
enum ProductUnitType {
  /// Sold one at a time; the price is per item.
  piece('Piece'),

  /// Sold by weight; the price is per kilo and the menu says so.
  kg('Kg'),

  /// Unknown to this build. Priced per item, which is the safer of the two to
  /// guess wrong: it understates nothing and adds no unit the shop did not say.
  unknown('');

  const ProductUnitType(this.wireName);

  final String wireName;

  /// Resolves a wire value, falling back to [unknown] rather than throwing.
  static ProductUnitType fromWire(String? value) {
    for (final unit in values) {
      if (unit.wireName == value) return unit;
    }
    return unknown;
  }
}

/// Which strip of the menu a product belongs to.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Enums/ProductCategory.cs`. The Arabic
/// labels live here because the wire carries only the member name, and the menu
/// screen should not be the place that decides what `HotCoffee` is called.
enum ProductCategory {
  hotCoffee('HotCoffee', 'قهوة ساخنة'),
  coldCoffee('ColdCoffee', 'قهوة باردة'),
  mojito('Mojito', 'موهيتو'),
  milkshake('Milkshake', 'ميلك شيك'),
  desserts('Desserts', 'حلويات'),
  coffeeBeans('CoffeeBeans', 'بن'),

  /// A category added to the server before the app knew about it. It gets no
  /// strip of its own — see [ProductCategory.displayed] — so a product in it is
  /// simply not listed rather than appearing under a wrong heading.
  unknown('', '');

  const ProductCategory(this.wireName, this.label);

  final String wireName;

  /// The Arabic heading for the category chip.
  final String label;

  /// The six the menu shows, in the order the design lays them out.
  static const List<ProductCategory> displayed = [
    hotCoffee,
    coldCoffee,
    mojito,
    milkshake,
    desserts,
    coffeeBeans,
  ];

  /// Resolves a wire value, falling back to [unknown] rather than throwing.
  static ProductCategory fromWire(String? value) {
    for (final category in values) {
      if (category.wireName == value) return category;
    }
    return unknown;
  }
}

/// One product — an element of `GET /api/products`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Products/ProductResponse.cs`.
class ProductResponse {
  const ProductResponse({
    required this.id,
    required this.name,
    required this.price,
    required this.unitType,
    required this.category,
    required this.isAvailable,
    required this.isActive,
  });

  final int id;
  final String name;

  /// In dinars. A JSON number on the wire (decimal server-side).
  final double price;

  final ProductUnitType unitType;
  final ProductCategory category;

  /// Switched off at the counter — out of stock today (decision 14).
  final bool isAvailable;

  /// Soft-delete flag. The API already hides inactive products from a customer
  /// token; this is carried so the filter can be enforced here too.
  final bool isActive;

  /// Whether the menu should list it.
  ///
  /// The server filters for a customer token already, so this is belt and
  /// braces — but it costs nothing, and it means a change to the server's
  /// filtering cannot start showing customers things that are off the menu.
  bool get isVisible => isAvailable && isActive;

  factory ProductResponse.fromJson(Map<String, dynamic> json) =>
      ProductResponse(
        id: (json['id'] as num?)?.toInt() ?? 0,
        name: json['name'] as String? ?? '',
        price: (json['price'] as num?)?.toDouble() ?? 0,
        unitType: ProductUnitType.fromWire(json['unitType'] as String?),
        category: ProductCategory.fromWire(json['category'] as String?),
        isAvailable: json['isAvailable'] as bool? ?? false,
        isActive: json['isActive'] as bool? ?? true,
      );
}
