import 'package:dio/dio.dart';

import '../core/api_client.dart';
import '../models/api_error.dart';
import '../models/customer_login_response.dart';
import '../models/firebase_login_request.dart';
import 'api_failure.dart';

/// Everything under `/api/auth` that the customer app touches.
///
/// Talks to the API and returns typed models; it holds no state and knows
/// nothing about widgets. Failures surface as [ApiException].
class AuthService {
  AuthService({ApiClient? client}) : _dio = (client ?? ApiClient()).dio;

  /// Customer token exchange — the app's only sign-in call.
  /// `backend/CoffeeLoyalty.Api/Controllers/AuthController.cs`
  static const String firebaseLoginPath = '/api/auth/firebase-login';

  // `POST /api/auth/login` also exists, but it is the *employee* sign-in for the
  // dashboard and is deliberately not wrapped here.
  //
  // There is no logout or token-refresh endpoint on the backend — the JWT is
  // stateless and simply expires (see `expiresAt`). Sign-out is local: delete
  // the token, and re-authenticate through firebaseLoginPath when it lapses.
  //
  // The name and balance in the response are a snapshot taken at sign-in. A
  // customer token lives a year, so anything rendered after the first launch
  // reads `GET /api/customers/me` through `CustomerService` instead.

  final Dio _dio;

  /// Exchanges a verified Firebase ID token for this API's JWT, creating the
  /// customer on first login when [FirebaseLoginRequest.fullName] is supplied.
  ///
  /// Throws [ApiException]; the codes worth branching on are `NAME_REQUIRED`
  /// (first login — collect a name and call again), `INVALID_PHONE`,
  /// `INVALID_FIREBASE_TOKEN` and `TOO_MANY_REQUESTS`.
  Future<CustomerLoginResponse> firebaseLogin(
    FirebaseLoginRequest request,
  ) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        firebaseLoginPath,
        data: request.toJson(),
      );

      final body = response.data;
      if (body == null) throw emptyBodyFailure();

      return CustomerLoginResponse.fromJson(body);
    } on DioException catch (e) {
      throw toApiException(e);
    }
  }
}
