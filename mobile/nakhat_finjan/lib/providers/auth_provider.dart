import 'package:flutter/foundation.dart';

import '../core/secure_store.dart';
import '../models/api_error.dart';
import '../models/customer_login_response.dart';
import '../models/firebase_login_request.dart';
import '../services/auth_service.dart';

/// Auth state for the customer app.
///
/// Owns the outcome of the Firebase-token exchange and the stored JWT; it calls
/// [AuthService] and never touches Dio, and it builds no widgets.
class AuthProvider extends ChangeNotifier {
  AuthProvider({AuthService? authService, SecureStore? secureStore})
    : _service = authService ?? AuthService(),
      _store = secureStore ?? SecureStore();

  final AuthService _service;
  final SecureStore _store;

  bool _isLoading = false;
  String? _errorCode;
  String? _errorMessage;
  bool _isNewCustomer = false;
  String? _token;
  String? _fullName;
  int _pointsBalance = 0;
  DateTime? _expiresAt;

  /// A call is in flight.
  bool get isLoading => _isLoading;

  /// Arabic display text for the last failure, or `null`. Render this directly.
  String? get errorMessage => _errorMessage;

  /// Stable code behind [errorMessage] (see [ErrorCodes]) — branch on this.
  String? get errorCode => _errorCode;

  /// A JWT is held, so authenticated calls can go out.
  bool get isAuthenticated => _token != null && _token!.isNotEmpty;

  /// The phone number has no customer yet: the backend answered the exchange
  /// with `NAME_REQUIRED`, so the app must collect a display name and call
  /// [signIn] again with `fullName`.
  ///
  /// A dedicated code, not an inference — `NAME_REQUIRED` says exactly this and
  /// nothing else, so it can never be confused with a malformed body.
  bool get isNewCustomer => _isNewCustomer;

  /// The signed-in customer's display name, once known.
  String? get fullName => _fullName;

  /// Points balance from the last successful sign-in.
  int get pointsBalance => _pointsBalance;

  /// When the held JWT stops being accepted (UTC), or `null` when signed out.
  DateTime? get expiresAt => _expiresAt;

  /// The held JWT. `ApiClient` reads the token from [SecureStore] on its own —
  /// this is for callers that need it explicitly.
  String? get token => _token;

  /// Reads any previously stored JWT at startup, so a returning user skips login.
  ///
  /// Only checks that a token exists; expiry and revocation are the server's
  /// call, and a rejected token is cleared by `ApiClient`'s 401 handler.
  Future<void> restoreSession() async {
    _token = await _store.getToken();
    notifyListeners();
  }

  /// Exchanges a verified Firebase ID token for the API's JWT and stores it.
  ///
  /// Call once with [fullName] omitted. If it returns `false` with
  /// [isNewCustomer] set, collect a name and call again with it.
  ///
  /// Returns `true` when authenticated.
  Future<bool> signIn(String firebaseIdToken, {String? fullName}) async {
    _isLoading = true;
    _errorCode = null;
    _errorMessage = null;
    notifyListeners();

    try {
      final response = await _service.firebaseLogin(
        FirebaseLoginRequest(
          firebaseIdToken: firebaseIdToken,
          fullName: fullName,
        ),
      );

      await _store.saveToken(response.token);
      _applySession(response);
      _isNewCustomer = false;
      return true;
    } on ApiException catch (e) {
      // NAME_REQUIRED is the first-login signal, not an error to show: the
      // phone number has no customer yet. Every other code is a real failure.
      if (e.code == ErrorCodes.nameRequired) {
        _isNewCustomer = true;
      } else {
        _errorCode = e.code;
        _errorMessage = e.message;
      }
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Clears the stored JWT and all session state.
  ///
  /// Local only — the backend has no logout endpoint (the JWT is stateless).
  /// The Firebase session is separate: sign out of `firebase_auth` too.
  Future<void> signOut() async {
    await _store.deleteToken();
    _token = null;
    _fullName = null;
    _pointsBalance = 0;
    _expiresAt = null;
    _isNewCustomer = false;
    _errorCode = null;
    _errorMessage = null;
    notifyListeners();
  }

  /// Drops the last error, e.g. when a form is edited after a failed attempt.
  void clearError() {
    if (_errorCode == null && _errorMessage == null) return;
    _errorCode = null;
    _errorMessage = null;
    notifyListeners();
  }

  void _applySession(CustomerLoginResponse response) {
    _token = response.token;
    _fullName = response.fullName;
    _pointsBalance = response.pointsBalance;
    _expiresAt = response.expiresAt;
  }
}
