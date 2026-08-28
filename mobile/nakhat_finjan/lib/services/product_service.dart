import 'package:dio/dio.dart';

import '../core/api_client.dart';
import '../models/api_error.dart';
import '../models/product_response.dart';
import 'api_failure.dart';

/// The menu — `GET /api/products`.
///
/// Talks to the API and returns typed models; it holds no state and knows
/// nothing about widgets. Failures surface as [ApiException].
class ProductService {
  ProductService({ApiClient? client}) : _dio = (client ?? ApiClient()).dio;

  /// `backend/CoffeeLoyalty.Api/Controllers/ProductsController.cs`
  static const String productsPath = '/api/products';

  final Dio _dio;

  /// The whole menu in one call — there is no per-category endpoint, and the
  /// list is small enough that grouping client-side beats six round trips.
  ///
  /// The route is open to all three roles, so the server decides what a customer
  /// token may see; [ProductResponse.isVisible] is applied where the menu is
  /// built rather than here, so a caller that wants the unfiltered list can
  /// still have it.
  Future<List<ProductResponse>> getProducts() async {
    try {
      final response = await _dio.get<List<dynamic>>(productsPath);
      final data = response.data;
      if (data == null) return const [];
      return data
          .whereType<Map<String, dynamic>>()
          .map(ProductResponse.fromJson)
          .toList(growable: false);
    } on DioException catch (e) {
      throw toApiException(e);
    }
  }
}
