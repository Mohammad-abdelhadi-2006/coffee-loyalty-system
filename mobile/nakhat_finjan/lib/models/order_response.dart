/// What became of an order.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Enums/OrderStatus.cs`; the members travel
/// as their names.
enum OrderWireStatus {
  /// Rang up and settled.
  completed('Completed'),

  /// Some of it came back (decision 20).
  returned('Returned'),

  /// The whole order was reversed.
  cancelled('Cancelled'),

  /// A status this build does not know about. Shown as-is rather than dropping
  /// the order from the customer's history.
  unknown('');

  const OrderWireStatus(this.wireName);

  final String wireName;

  /// Resolves a wire value, falling back to [unknown] rather than throwing.
  static OrderWireStatus fromWire(String? value) {
    for (final status in values) {
      if (status.wireName == value) return status;
    }
    return unknown;
  }
}

/// One line on an order.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Orders/OrderItemResponse.cs`.
class OrderItemResponse {
  const OrderItemResponse({
    required this.orderItemId,
    required this.productName,
    required this.quantity,
    required this.returnedQuantity,
    required this.unitPriceSnapshot,
  });

  final int orderItemId;

  /// The product's name as it was when the order was rung up — the server keeps
  /// a snapshot, so renaming a product does not rewrite anyone's history.
  final String productName;

  /// Decimal because beans sell by weight: `0.5` is half a kilo.
  final double quantity;

  /// How much of [quantity] has since come back. Zero for an untouched line.
  final double returnedQuantity;

  /// The unit price at the time of sale, likewise snapshotted.
  final double unitPriceSnapshot;

  /// What the line cost as rung up: price × quantity.
  ///
  /// Computed rather than read, because the DTO carries no line total. It uses
  /// the original [quantity], not the quantity net of returns, so the card shows
  /// the order as it was placed — the refund is reflected in the status pill and
  /// in the points ledger, not by quietly rewriting the line.
  double get lineTotal => unitPriceSnapshot * quantity;

  factory OrderItemResponse.fromJson(Map<String, dynamic> json) =>
      OrderItemResponse(
        orderItemId: (json['orderItemId'] as num?)?.toInt() ?? 0,
        productName: json['productName'] as String? ?? '',
        quantity: (json['quantity'] as num?)?.toDouble() ?? 0,
        returnedQuantity: (json['returnedQuantity'] as num?)?.toDouble() ?? 0,
        unitPriceSnapshot: (json['unitPriceSnapshot'] as num?)?.toDouble() ?? 0,
      );
}

/// One order — an element of `GET /api/customers/me/orders`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Orders/OrderResponse.cs`. The list
/// shape deliberately carries no cashier or customer name, so the app cannot
/// read who rang the order up.
class OrderResponse {
  const OrderResponse({
    required this.orderId,
    required this.createdAt,
    required this.status,
    required this.total,
    required this.pointsRedeemed,
    required this.pointsEarned,
    required this.items,
  });

  final int orderId;

  /// UTC; formatting stays at the UI edge.
  final DateTime createdAt;

  final OrderWireStatus status;

  /// What was paid, in dinars. A JSON number on the wire (decimal server-side).
  final double total;

  /// Points spent on this order. Zero on a cash-only order, which is most.
  final int pointsRedeemed;

  /// Points granted for this order.
  final int pointsEarned;

  final List<OrderItemResponse> items;

  factory OrderResponse.fromJson(Map<String, dynamic> json) => OrderResponse(
    orderId: (json['orderId'] as num?)?.toInt() ?? 0,
    createdAt: DateTime.parse(json['createdAt'] as String).toUtc(),
    status: OrderWireStatus.fromWire(json['status'] as String?),
    total: (json['total'] as num?)?.toDouble() ?? 0,
    pointsRedeemed: (json['pointsRedeemed'] as num?)?.toInt() ?? 0,
    pointsEarned: (json['pointsEarned'] as num?)?.toInt() ?? 0,
    items: (json['items'] as List<dynamic>? ?? const [])
        .whereType<Map<String, dynamic>>()
        .map(OrderItemResponse.fromJson)
        .toList(growable: false),
  );
}
