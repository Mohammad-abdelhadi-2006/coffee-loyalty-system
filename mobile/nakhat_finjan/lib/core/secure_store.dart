import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Thin wrapper over `flutter_secure_storage` holding the API JWT.
///
/// The token is the only thing kept here: it is a bearer credential, so it must
/// not sit in `SharedPreferences`.
class SecureStore {
  SecureStore({FlutterSecureStorage? storage})
    : _storage =
          storage ??
          const FlutterSecureStorage(
            // The Android keystore and the encrypted blob on disk can end up
            // out of step — a reinstall where the system restores the old
            // preferences from backup while the key is generated fresh is the
            // usual way in. Every read then fails with BAD_DECRYPT.
            //
            // `resetOnError` lets the plugin throw the unreadable store away and
            // answer `null` instead of raising, which is the right answer: a
            // token nobody can decrypt is a token nobody has.
            aOptions: AndroidOptions(resetOnError: true),
          );

  /// The single key everything in the app reads and writes.
  static const String tokenKey = 'coffee_loyalty.jwt';

  final FlutterSecureStorage _storage;

  /// Persists the JWT returned by `POST /api/auth/firebase-login`.
  Future<void> saveToken(String token) =>
      _storage.write(key: tokenKey, value: token);

  /// Returns the stored JWT, or `null` when the user has never signed in
  /// (or was signed out by a 401).
  ///
  /// **Never throws.** This is the first thing the splash awaits, and an
  /// exception here used to escape as an unhandled async error: the splash's
  /// navigation was simply never reached and the app sat on the mark forever,
  /// with no spinner, no error, and no way out but reinstalling.
  ///
  /// So a failed read is reported as "no token" and the app shows the login
  /// screen — recoverable, and true. The unreadable entry is dropped on the way
  /// out so the next launch starts clean.
  Future<String?> getToken() async {
    try {
      return await _storage.read(key: tokenKey);
    } on PlatformException catch (error, stackTrace) {
      debugPrint('SecureStore.getToken failed, treating as signed out: $error');

      // Reported rather than swallowed: it is not fatal, but a keystore that
      // cannot read its own writes is worth seeing in a crash console.
      FlutterError.reportError(
        FlutterErrorDetails(
          exception: error,
          stack: stackTrace,
          library: 'nakhat_finjan',
          context: ErrorDescription('reading the stored JWT'),
          silent: true,
        ),
      );

      await deleteToken();
      return null;
    }
  }

  /// Drops the stored JWT — sign-out, and the 401 path in `ApiClient`.
  ///
  /// Also never throws: this runs on the sign-out path and from [getToken]'s
  /// recovery above, and neither has anywhere useful to send a failure.
  Future<void> deleteToken() async {
    try {
      await _storage.delete(key: tokenKey);
    } on PlatformException catch (error) {
      debugPrint('SecureStore.deleteToken failed: $error');
    }
  }
}
