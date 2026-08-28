import 'package:dio/dio.dart';

import 'config.dart';
import 'secure_store.dart';

/// The app's single configured [Dio] instance.
///
/// Services take one of these and use [dio]; nothing else in the app should
/// construct a Dio. Auth headers and the 401 sign-out live here so no service
/// has to remember them.
class ApiClient {
  /// Called when a stored token is rejected on a non-auth path.
  ///
  /// A static hook rather than a constructor argument on purpose: services
  /// construct their own `ApiClient()` with no arguments, so there is nowhere to
  /// thread a callback through, and there is only ever one app to send back to
  /// login. `main` sets it once at startup.
  ///
  /// It is set rather than injected because the alternative — a navigator this
  /// layer imports — would put Flutter's widget tree inside the networking
  /// code. Keeping it a bare callback means this file still knows nothing about
  /// widgets, and the test suite never triggers it.
  static void Function()? onUnauthorized;

  ApiClient({SecureStore? secureStore, Dio? dio})
    : _store = secureStore ?? SecureStore(),
      dio =
          dio ??
          Dio(
            BaseOptions(
              baseUrl: Config.baseUrl,
              connectTimeout: const Duration(seconds: 15),
              receiveTimeout: const Duration(seconds: 20),
              sendTimeout: const Duration(seconds: 20),
              contentType: Headers.jsonContentType,
              responseType: ResponseType.json,
              // Non-2xx must reach the error interceptor as a DioException so
              // services can map the { code, message } body.
              validateStatus: (status) => status != null && status < 400,
            ),
          ) {
    this.dio.interceptors.add(
      InterceptorsWrapper(onRequest: _onRequest, onError: _onError),
    );
  }

  /// Paths that authenticate the caller and therefore may legitimately answer
  /// 401 while a perfectly good token is stored. A 401 from one of these means
  /// "these credentials are bad", not "your session expired", so it must not
  /// wipe the session.
  static const List<String> _authPaths = ['/api/auth/'];

  final SecureStore _store;

  /// The configured instance. Services call `client.dio.post(...)`.
  final Dio dio;

  Future<void> _onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _store.getToken();
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  Future<void> _onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final path = err.requestOptions.path;
    final isAuthCall = _authPaths.any(path.contains);

    if (err.response?.statusCode == 401 && !isAuthCall) {
      // The token was rejected: expired, revoked (TokenVersion bumped), or the
      // signing key rotated. Nothing the app can do with it, so drop it.
      await _store.deleteToken();

      // ...and tell the app, so the customer is not left on a screen that
      // silently stopped loading. Several in-flight calls can 401 together —
      // the home screen alone makes two — so the handler must be safe to run
      // more than once; the one in `main` is.
      onUnauthorized?.call();
    }

    handler.next(err);
  }
}
