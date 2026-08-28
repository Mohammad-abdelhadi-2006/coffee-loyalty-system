/// The one and only error body this API returns (api-contract.md §1):
/// `{ "code": "MACHINE_READABLE_CODE", "message": "نص عربي" }`.
///
/// `code` is stable and safe to branch on; `message` is Arabic display text and
/// may change at any time, so never compare against it.
class ApiError {
  const ApiError({required this.code, required this.message});

  final String code;
  final String message;

  factory ApiError.fromJson(Map<String, dynamic> json) => ApiError(
    code: json['code'] as String? ?? ErrorCodes.internalError,
    message: json['message'] as String? ?? '',
  );

  Map<String, dynamic> toJson() => {'code': code, 'message': message};

  @override
  String toString() => 'ApiError($code): $message';
}

/// The error codes reachable from the auth flow.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Constants/ErrorCodes.cs` — only the codes
/// the customer app can actually receive are listed; add the rest alongside the
/// feature that needs them.
class ErrorCodes {
  const ErrorCodes._();

  /// Firebase ID token failed verification (401).
  static const String invalidFirebaseToken = 'INVALID_FIREBASE_TOKEN';

  /// The Firebase phone claim is not a valid Jordanian number (400).
  static const String invalidPhone = 'INVALID_PHONE';

  /// Malformed body (400).
  static const String validationError = 'VALIDATION_ERROR';

  /// This phone number has no customer yet and the exchange carried no
  /// `fullName` to create one from (400).
  ///
  /// Not a failure to show the user: collect a display name and repeat the
  /// exchange with it.
  static const String nameRequired = 'NAME_REQUIRED';

  /// Per-phone throttle tripped after repeated failures (429). The contract says
  /// to show the Arabic message and let the user retry, never to auto-retry.
  static const String tooManyRequests = 'TOO_MANY_REQUESTS';

  /// Missing or expired JWT (401).
  static const String unauthorized = 'UNAUTHORIZED';

  /// Anything the server did not classify, plus transport failures we synthesize.
  static const String internalError = 'INTERNAL_ERROR';

  /// Not from the backend: no response at all (offline, DNS, timeout).
  static const String networkError = 'NETWORK_ERROR';
}

/// A failed API call, carrying the `{ code, message }` the backend sent.
///
/// Services throw this instead of leaking `DioException` upwards, so providers
/// can branch on [ApiError.code] without importing dio.
class ApiException implements Exception {
  const ApiException(this.error, {this.statusCode});

  final ApiError error;
  final int? statusCode;

  String get code => error.code;

  /// Arabic text, ready to render.
  String get message => error.message;

  @override
  String toString() => 'ApiException($statusCode) $error';
}
