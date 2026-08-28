import 'package:nakhat_finjan/models/api_error.dart';
import 'package:nakhat_finjan/models/customer_profile_response.dart';
import 'package:nakhat_finjan/models/order_response.dart';
import 'package:nakhat_finjan/models/points_transaction_response.dart';
import 'package:nakhat_finjan/models/product_response.dart';
import 'package:nakhat_finjan/services/customer_service.dart';
import 'package:nakhat_finjan/services/product_service.dart';

/// A [CustomerService] that answers from memory.
///
/// Subclassed rather than mocked with a package: the surface is three methods,
/// and this keeps the test suite free of a codegen dependency. Each method
/// either returns what it was seeded with or throws the [ApiException] it was
/// seeded with, which is exactly the two paths the provider branches on.
class FakeCustomerService implements CustomerService {
  FakeCustomerService({
    this.profile,
    this.transactions = const [],
    this.orders = const [],
    this.failure,
  });

  final CustomerProfileResponse? profile;
  final List<PointsTransactionResponse> transactions;
  final List<OrderResponse> orders;

  /// When set, every method throws it.
  final ApiException? failure;

  @override
  Future<CustomerProfileResponse> getProfile() async {
    if (failure != null) throw failure!;
    return profile ??
        const CustomerProfileResponse(
          fullName: 'سارة',
          phoneNumber: '+962791234567',
          pointsBalance: 0,
        );
  }

  @override
  Future<List<PointsTransactionResponse>> getTransactions({
    int limit = 20,
  }) async {
    if (failure != null) throw failure!;
    return transactions;
  }

  @override
  Future<List<OrderResponse>> getOrders({int limit = 10}) async {
    if (failure != null) throw failure!;
    return orders;
  }
}

/// A [ProductService] that answers from memory. See [FakeCustomerService].
class FakeProductService implements ProductService {
  FakeProductService({this.products = const [], this.failure});

  final List<ProductResponse> products;
  final ApiException? failure;

  @override
  Future<List<ProductResponse>> getProducts() async {
    if (failure != null) throw failure!;
    return products;
  }
}

/// A failure with the shape the services really produce, for the error paths.
ApiException networkFailure() => const ApiException(
  ApiError(
    code: ErrorCodes.networkError,
    message: 'تعذّر الاتصال بالخادم، تحقّق من الإنترنت وحاول مجدداً',
  ),
);

// ── Sample rows ─────────────────────────────────────────────────────────────

PointsTransactionResponse sampleEarn({int amount = 11}) =>
    PointsTransactionResponse(
      id: 1,
      type: PointsTransactionType.earn,
      amount: amount,
      orderId: 7,
      createdAt: DateTime.utc(2026, 8, 20, 13, 12),
    );

PointsTransactionResponse sampleRedeem({int amount = -250}) =>
    PointsTransactionResponse(
      id: 2,
      type: PointsTransactionType.redeem,
      amount: amount,
      orderId: 8,
      createdAt: DateTime.utc(2026, 8, 14, 15, 40),
    );

PointsTransactionResponse sampleOpeningBalance() => PointsTransactionResponse(
  id: 3,
  type: PointsTransactionType.openingBalance,
  amount: 100,
  orderId: null,
  createdAt: DateTime.utc(2026, 6, 3, 9),
);

OrderResponse sampleOrder({
  OrderWireStatus status = OrderWireStatus.completed,
  int pointsRedeemed = 0,
}) => OrderResponse(
  orderId: 7,
  createdAt: DateTime.utc(2026, 8, 20, 13, 12),
  status: status,
  total: 3.75,
  pointsRedeemed: pointsRedeemed,
  pointsEarned: 11,
  items: const [
    OrderItemResponse(
      orderItemId: 1,
      productName: 'كابتشينو',
      quantity: 2,
      returnedQuantity: 0,
      unitPriceSnapshot: 1.5,
    ),
  ],
);

ProductResponse sampleProduct({
  String name = 'لاتيه',
  double price = 1.5,
  ProductCategory category = ProductCategory.hotCoffee,
  ProductUnitType unitType = ProductUnitType.piece,
  bool isAvailable = true,
  bool isActive = true,
}) => ProductResponse(
  id: 1,
  name: name,
  price: price,
  unitType: unitType,
  category: category,
  isAvailable: isAvailable,
  isActive: isActive,
);
