/// Success body (200) of `POST /api/auth/firebase-login`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Auth/CustomerLoginResponse.cs`.
class CustomerLoginResponse {
  const CustomerLoginResponse({
    required this.token,
    required this.fullName,
    required this.pointsBalance,
    required this.expiresAt,
  });

  /// The signed JWT with `role=customer`, sent back as
  /// `Authorization: Bearer <jwt>`.
  final String token;

  /// The customer's display name, as stored on the server — not necessarily the
  /// `fullName` that was sent, since it is ignored for an existing customer.
  final String fullName;

  /// Current points balance, so the home screen can render without a second call.
  final int pointsBalance;

  /// When the token stops being accepted. Sent as an ISO-8601 `DateTimeOffset`
  /// (`"2026-09-13T10:00:00Z"`); kept in UTC here, format at the edge.
  final DateTime expiresAt;

  factory CustomerLoginResponse.fromJson(Map<String, dynamic> json) =>
      CustomerLoginResponse(
        token: json['token'] as String,
        fullName: json['fullName'] as String? ?? '',
        pointsBalance: (json['pointsBalance'] as num?)?.toInt() ?? 0,
        expiresAt: DateTime.parse(json['expiresAt'] as String).toUtc(),
      );

  Map<String, dynamic> toJson() => {
    'token': token,
    'fullName': fullName,
    'pointsBalance': pointsBalance,
    'expiresAt': expiresAt.toUtc().toIso8601String(),
  };
}
