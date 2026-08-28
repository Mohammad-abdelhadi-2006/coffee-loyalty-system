import 'package:flutter/foundation.dart';

import '../models/api_error.dart';
import '../models/customer_profile_response.dart';
import '../models/order_response.dart';
import '../models/points_transaction_response.dart';
import '../models/product_response.dart';
import '../services/customer_service.dart';
import '../services/product_service.dart';

/// Where one section of the screen has got to.
///
/// Mirrors the four frames the design draws for the home screen, and the same
/// four serve the menu and the purchases list.
enum SectionStatus {
  /// Never asked for yet.
  idle,

  /// A call is in flight and there is nothing to show yet.
  loading,

  /// Loaded, with content.
  loaded,

  /// Loaded, and the customer genuinely has nothing here.
  empty,

  /// The call failed. [CustomerProvider] keeps the Arabic message alongside.
  error,
}

/// Everything the signed-in customer can read about themselves, plus the menu.
///
/// Each section — profile-and-ledger, orders, products — carries its own status
/// and its own error, so a failed menu does not blank the home screen and a
/// retry on one does not re-fetch the others. That independence is the whole
/// reason this is one provider with three sections rather than one status: the
/// four tabs live in an `IndexedStack` and are all mounted at once, so a single
/// shared status would let the slowest or unluckiest call decide what every tab
/// shows.
class CustomerProvider extends ChangeNotifier {
  CustomerProvider({
    CustomerService? customerService,
    ProductService? productService,
  }) : _customers = customerService ?? CustomerService(),
       _products = productService ?? ProductService();

  final CustomerService _customers;
  final ProductService _products;

  // ── Profile and ledger ────────────────────────────────────────────────────
  // One section, not two: the design note says pull-to-refresh on Home reloads
  // the balance and the ledger together, and they are drawn on one screen, so a
  // split status could show a fresh balance above a stale list.
  SectionStatus _homeStatus = SectionStatus.idle;
  String? _homeError;
  CustomerProfileResponse? _profile;
  List<PointsTransactionResponse> _transactions = const [];

  SectionStatus get homeStatus => _homeStatus;
  String? get homeError => _homeError;

  /// Null until the first successful load.
  CustomerProfileResponse? get profile => _profile;

  /// The display name, or null while unknown. Callers fall back to a greeting
  /// without a name rather than inventing one.
  String? get fullName => _profile?.fullName;

  /// The E.164 phone number the account is keyed on, or null while unknown.
  String? get phoneNumber => _profile?.phoneNumber;

  /// The balance from the last successful load; zero before that. Zero is also
  /// a real balance, which is why [homeStatus] and not this decides what the
  /// screen renders.
  int get pointsBalance => _profile?.pointsBalance ?? 0;

  /// The ledger, newest first.
  List<PointsTransactionResponse> get transactions => _transactions;

  // ── Orders ────────────────────────────────────────────────────────────────
  SectionStatus _ordersStatus = SectionStatus.idle;
  String? _ordersError;
  List<OrderResponse> _orders = const [];

  SectionStatus get ordersStatus => _ordersStatus;
  String? get ordersError => _ordersError;
  List<OrderResponse> get orders => _orders;

  // ── Products ──────────────────────────────────────────────────────────────
  SectionStatus _productsStatus = SectionStatus.idle;
  String? _productsError;
  List<ProductResponse> _products0 = const [];

  SectionStatus get productsStatus => _productsStatus;
  String? get productsError => _productsError;

  /// The menu, already filtered to what a customer may see.
  List<ProductResponse> get products => _products0;

  /// The visible products in one category, in the order the server sent them.
  ///
  /// Grouping happens on read rather than being cached, because the list is
  /// small — a café menu — and a cached grouping is one more thing that can go
  /// stale behind a refresh.
  List<ProductResponse> productsIn(ProductCategory category) =>
      _products0.where((p) => p.category == category).toList(growable: false);

  /// Fetches every section.
  ///
  /// The three run concurrently: they are independent reads against three
  /// endpoints, and doing them in sequence would make the slowest one decide
  /// how long the whole app takes to become useful. `Future.wait` is safe here
  /// because each branch handles its own failure and none of them throws.
  Future<void> loadAll() async {
    await Future.wait([refreshHome(), refreshOrders(), refreshProducts()]);
  }

  /// Reloads the balance and the ledger together — what Home's pull-to-refresh
  /// calls, and what the design note requires.
  ///
  /// The two calls go out concurrently but land as one state change, so the
  /// screen never paints a new balance beside the old ledger.
  Future<void> refreshHome() async {
    _homeStatus = SectionStatus.loading;
    _homeError = null;
    notifyListeners();

    try {
      final results = await Future.wait([
        _customers.getProfile(),
        _customers.getTransactions(),
      ]);

      _profile = results[0] as CustomerProfileResponse;
      _transactions = results[1] as List<PointsTransactionResponse>;

      // The balance card renders either way, so an empty ledger is only the
      // empty state for the list beneath it — the design draws exactly that,
      // a zero balance above "لسا ما في حركات".
      _homeStatus = _transactions.isEmpty
          ? SectionStatus.empty
          : SectionStatus.loaded;
    } on ApiException catch (e) {
      _homeStatus = SectionStatus.error;
      _homeError = e.message;
    } finally {
      notifyListeners();
    }
  }

  /// Reloads the purchase history.
  Future<void> refreshOrders() async {
    _ordersStatus = SectionStatus.loading;
    _ordersError = null;
    notifyListeners();

    try {
      _orders = await _customers.getOrders();
      _ordersStatus = _orders.isEmpty
          ? SectionStatus.empty
          : SectionStatus.loaded;
    } on ApiException catch (e) {
      _ordersStatus = SectionStatus.error;
      _ordersError = e.message;
    } finally {
      notifyListeners();
    }
  }

  /// Reloads the menu.
  ///
  /// Filtering to [ProductResponse.isVisible] happens here so every reader of
  /// [products] sees the same list and no screen can forget the check.
  Future<void> refreshProducts() async {
    _productsStatus = SectionStatus.loading;
    _productsError = null;
    notifyListeners();

    try {
      final all = await _products.getProducts();
      _products0 = all.where((p) => p.isVisible).toList(growable: false);
      _productsStatus = _products0.isEmpty
          ? SectionStatus.empty
          : SectionStatus.loaded;
    } on ApiException catch (e) {
      _productsStatus = SectionStatus.error;
      _productsError = e.message;
    } finally {
      notifyListeners();
    }
  }

  /// Drops everything on sign-out.
  ///
  /// Without this the next customer to sign in on the same device would see the
  /// previous one's balance and orders for as long as the first load takes.
  void clear() {
    _homeStatus = SectionStatus.idle;
    _homeError = null;
    _profile = null;
    _transactions = const [];

    _ordersStatus = SectionStatus.idle;
    _ordersError = null;
    _orders = const [];

    _productsStatus = SectionStatus.idle;
    _productsError = null;
    _products0 = const [];

    notifyListeners();
  }
}
