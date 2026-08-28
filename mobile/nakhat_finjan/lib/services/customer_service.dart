import 'package:dio/dio.dart';

import '../core/api_client.dart';
import '../models/api_error.dart';
import '../models/customer_profile_response.dart';
import '../models/order_response.dart';
import '../models/points_transaction_response.dart';
import 'api_failure.dart';

/// The `/api/customers/me` family — everything the signed-in customer can read
/// about themselves.
///
/// Talks to the API and returns typed models; it holds no state and knows
/// nothing about widgets. Failures surface as [ApiException].
///
/// Every route here takes the customer id from the token's `sub` claim and from
/// nowhere else, so there is no parameter that could be pointed at another
/// customer's data — and no 404 to handle, since a customer token is only ever
/// issued for a customer that exists.
class CustomerService {
  CustomerService({ApiClient? client}) : _dio = (client ?? ApiClient()).dio;

  /// `backend/CoffeeLoyalty.Api/Controllers/CustomersController.cs`
  static const String profilePath = '/api/customers/me';
  static const String transactionsPath = '/api/customers/me/transactions';
  static const String ordersPath = '/api/customers/me/orders';

  final Dio _dio;

  /// The customer's own name, phone and balance.
  ///
  /// Throws [ApiException]; a 401 here means the stored JWT was rejected, which
  /// `ApiClient` has already acted on by clearing it and signalling the app to
  /// return to login.
  Future<CustomerProfileResponse> getProfile() async {
    try {
      final response = await _dio.get<Map<String, dynamic>>(profilePath);
      final body = response.data;
      if (body == null) throw emptyBodyFailure();
      return CustomerProfileResponse.fromJson(body);
    } on DioException catch (e) {
      throw toApiException(e);
    }
  }

  /// The points ledger, newest first.
  ///
  /// [limit] is clamped to the backend's documented 1..100 before it is sent:
  /// an out-of-range value comes back as `VALIDATION_ERROR`, and a caller
  /// asking for 200 rows means "as many as I can have", not "fail".
  Future<List<PointsTransactionResponse>> getTransactions({
    int limit = 20,
  }) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        transactionsPath,
        queryParameters: {'limit': limit.clamp(1, 100)},
      );
      return _decodeList(response.data, PointsTransactionResponse.fromJson);
    } on DioException catch (e) {
      throw toApiException(e);
    }
  }

  /// The customer's own orders, newest first.
  ///
  /// [limit] is clamped to the backend's documented 1..50, for the same reason
  /// as [getTransactions].
  Future<List<OrderResponse>> getOrders({int limit = 10}) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        ordersPath,
        queryParameters: {'limit': limit.clamp(1, 50)},
      );
      return _decodeList(response.data, OrderResponse.fromJson);
    } on DioException catch (e) {
      throw toApiException(e);
    }
  }

  /// Decodes a JSON array into models.
  ///
  /// A null body is an empty list, not a failure: these endpoints answer 200
  /// with `[]` for a customer who has no history, and that is a legitimate
  /// empty state rather than an error to show.
  static List<T> _decodeList<T>(
    List<dynamic>? data,
    T Function(Map<String, dynamic>) fromJson,
  ) {
    if (data == null) return const [];
    return data
        .whereType<Map<String, dynamic>>()
        .map(fromJson)
        .toList(growable: false);
  }
}
