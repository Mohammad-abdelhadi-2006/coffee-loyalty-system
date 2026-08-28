/// Why a points balance moved.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Enums/PointsTransactionType.cs`. The
/// members travel as their names (`"Earn"`, `"OpeningBalance"`) because the API
/// registers a `JsonStringEnumConverter`.
///
/// The type is the *reason* only. It does not carry the direction — the ERD is
/// explicit that the sign lives on
/// [PointsTransactionResponse.amount] — so never infer a sign from this.
enum PointsTransactionType {
  /// Points granted for a purchase.
  earn('Earn'),

  /// Points spent on a redemption.
  redeem('Redeem'),

  /// Points given back when an order was returned or cancelled.
  refund('Refund'),

  /// A redemption undone, putting the spent points back.
  redeemReversal('RedeemReversal'),

  /// The balance a customer started with when their account was created
  /// (decision 38). The only type whose `orderId` is null.
  openingBalance('OpeningBalance'),

  /// A member this build does not know about.
  ///
  /// The server may add a type before the app is updated. A ledger row with an
  /// unrecognised reason is still a real balance movement, so it is kept and
  /// shown with a neutral label rather than being dropped or thrown over.
  unknown('');

  const PointsTransactionType(this.wireName);

  /// The exact string on the wire, matched case-sensitively.
  final String wireName;

  /// Resolves a wire value, falling back to [unknown] rather than throwing.
  static PointsTransactionType fromWire(String? value) {
    for (final type in values) {
      if (type.wireName == value) return type;
    }
    return unknown;
  }
}

/// One row of the points ledger — an element of
/// `GET /api/customers/me/transactions`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Customers/PointsTransactionResponse.cs`.
class PointsTransactionResponse {
  const PointsTransactionResponse({
    required this.id,
    required this.type,
    required this.amount,
    required this.orderId,
    required this.createdAt,
  });

  final int id;

  /// Why the balance moved. See [PointsTransactionType].
  final PointsTransactionType type;

  /// The effect on the balance, already signed by the server: positive adds,
  /// negative subtracts. Rendering reads the sign from here, never from [type].
  final int amount;

  /// The order this movement came from, or null for an opening balance.
  final int? orderId;

  /// When it happened. Sent as an ISO-8601 `DateTimeOffset`; kept in UTC here,
  /// formatting stays at the UI edge.
  final DateTime createdAt;

  factory PointsTransactionResponse.fromJson(Map<String, dynamic> json) =>
      PointsTransactionResponse(
        id: (json['id'] as num?)?.toInt() ?? 0,
        type: PointsTransactionType.fromWire(json['type'] as String?),
        amount: (json['amount'] as num?)?.toInt() ?? 0,
        orderId: (json['orderId'] as num?)?.toInt(),
        createdAt: DateTime.parse(json['createdAt'] as String).toUtc(),
      );
}
