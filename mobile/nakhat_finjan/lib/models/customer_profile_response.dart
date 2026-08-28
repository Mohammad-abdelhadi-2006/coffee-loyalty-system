/// Success body (200) of `GET /api/customers/me`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Customers/CustomerProfileResponse.cs`.
///
/// This is the authority on the signed-in customer's name and balance once the
/// app is running. `CustomerLoginResponse` carries the same three facts, but
/// only as they stood at sign-in — a JWT lives a year (`CustomerExpireDays`),
/// so by the second launch the token's copy is almost certainly stale.
class CustomerProfileResponse {
  const CustomerProfileResponse({
    required this.fullName,
    required this.phoneNumber,
    required this.pointsBalance,
  });

  /// The customer's display name.
  final String fullName;

  /// The normalized E.164 number the account is identified by (decision 11),
  /// e.g. `+9627XXXXXXXX`. Display only — nothing in the app sends it back.
  final String phoneNumber;

  /// Current points balance.
  final int pointsBalance;

  factory CustomerProfileResponse.fromJson(Map<String, dynamic> json) =>
      CustomerProfileResponse(
        fullName: json['fullName'] as String? ?? '',
        phoneNumber: json['phoneNumber'] as String? ?? '',
        pointsBalance: (json['pointsBalance'] as num?)?.toInt() ?? 0,
      );

  Map<String, dynamic> toJson() => {
    'fullName': fullName,
    'phoneNumber': phoneNumber,
    'pointsBalance': pointsBalance,
  };
}
