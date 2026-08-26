import 'package:dio/dio.dart';

import '../core/api_client.dart';
import '../models/api_error.dart';
import '../models/customer_login_response.dart';
import '../models/firebase_login_request.dart';

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
  // TODO(endpoints): no logout / token-refresh endpoint exists on the backend —
  // the JWT is stateless and simply expires (see `expiresAt`). Sign-out is local
  // (delete the token); re-authenticate through firebaseLoginPath when it lapses.
  // TODO(endpoints): no "current customer profile" endpoint was found under
  // /api/customers for a customer-role caller — the login response is the only
  // source of fullName/pointsBalance today. Confirm before building the home screen.

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
      if (body == null) {
        throw const ApiException(
          ApiError(
            code: ErrorCodes.internalError,
            message: 'تعذّر قراءة استجابة الخادم',
          ),
        );
      }

      return CustomerLoginResponse.fromJson(body);
    } on DioException catch (e) {
      throw _toApiException(e);
    }
  }

  /// Turns a transport/HTTP failure into the API's `{ code, message }` shape.
  ///
  /// Every non-2xx from this backend carries that body (a middleware guarantees
  /// it), so the fallbacks below only fire when the request never reached the
  /// API or something upstream — a proxy, a tunnel — answered instead.
  ApiException _toApiException(DioException e) {
    final data = e.response?.data;
    final status = e.response?.statusCode;

    if (data is Map<String, dynamic> && data['code'] is String) {
      return ApiException(ApiError.fromJson(data), statusCode: status);
    }

    if (e.response == null) {
      return ApiException(
        const ApiError(
          code: ErrorCodes.networkError,
          message: 'تعذّر الاتصال بالخادم، تحقّق من الإنترنت وحاول مجدداً',
        ),
        statusCode: status,
      );
    }

    return ApiException(
      const ApiError(
        code: ErrorCodes.internalError,
        message: 'حدث خطأ غير متوقع، حاول مجدداً',
      ),
      statusCode: status,
    );
  }
}
