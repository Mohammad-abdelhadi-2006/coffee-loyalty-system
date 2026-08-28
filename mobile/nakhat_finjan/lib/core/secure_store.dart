import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Thin wrapper over `flutter_secure_storage` holding the API JWT.
///
/// The token is the only thing kept here: it is a bearer credential, so it must
/// not sit in `SharedPreferences`.
class SecureStore {
  SecureStore({FlutterSecureStorage? storage})
    : _storage = storage ?? const FlutterSecureStorage();

  /// The single key everything in the app reads and writes.
  static const String tokenKey = 'coffee_loyalty.jwt';

  final FlutterSecureStorage _storage;

  /// Persists the JWT returned by `POST /api/auth/firebase-login`.
  Future<void> saveToken(String token) =>
      _storage.write(key: tokenKey, value: token);

  /// Returns the stored JWT, or `null` when the user has never signed in
  /// (or was signed out by a 401).
  Future<String?> getToken() => _storage.read(key: tokenKey);

  /// Drops the stored JWT — sign-out, and the 401 path in `ApiClient`.
  Future<void> deleteToken() => _storage.delete(key: tokenKey);
}
