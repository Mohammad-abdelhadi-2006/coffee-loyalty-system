import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:nakhat_finjan/models/product_response.dart';
import 'package:nakhat_finjan/providers/customer_provider.dart';
import 'package:nakhat_finjan/screens/home_screen.dart';
import 'package:nakhat_finjan/screens/main_shell.dart';
import 'package:nakhat_finjan/screens/menu_screen.dart';
import 'package:nakhat_finjan/screens/purchases_screen.dart';
import 'package:nakhat_finjan/screens/settings_screen.dart';
import 'package:nakhat_finjan/theme/app_theme.dart';
import 'package:nakhat_finjan/widgets/app_bottom_nav.dart';

import 'support/fake_services.dart';

/// Wraps a screen the way the app does — RTL, the real theme, a phone-sized
/// surface, and a [CustomerProvider] fed from memory. Without the RTL wrapper
/// these would pass here and overflow on a device, which is a class of bug this
/// file exists to catch.
Widget _host(Widget child, {required CustomerProvider customers}) =>
    ChangeNotifierProvider<CustomerProvider>.value(
      value: customers,
      child: MaterialApp(
        theme: buildAppTheme(),
        home: Directionality(textDirection: TextDirection.rtl, child: child),
      ),
    );

CustomerProvider _provider({
  FakeCustomerService? customerService,
  FakeProductService? productService,
}) => CustomerProvider(
  customerService: customerService ?? FakeCustomerService(),
  productService: productService ?? FakeProductService(),
);

void _phoneSized(WidgetTester tester) {
  tester.view.physicalSize = const Size(390, 844);
  tester.view.devicePixelRatio = 1.0;
  addTearDown(tester.view.reset);
}

void main() {
  testWidgets('home renders each state without overflowing', (tester) async {
    _phoneSized(tester);

    // Loaded.
    final loaded = _provider(
      customerService: FakeCustomerService(
        transactions: [sampleEarn(), sampleRedeem(), sampleOpeningBalance()],
      ),
    );
    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen()), customers: loaded),
    );
    await loaded.refreshHome();
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
    expect(find.text('حركات النقاط'), findsOneWidget);

    // Empty ledger.
    final empty = _provider();
    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen()), customers: empty),
    );
    await empty.refreshHome();
    await tester.pumpAndSettle();
    expect(find.text('لسا ما في حركات'), findsOneWidget);

    // Error.
    final failed = _provider(
      customerService: FakeCustomerService(failure: networkFailure()),
    );
    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen()), customers: failed),
    );
    await failed.refreshHome();
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
    expect(find.text('إعادة المحاولة'), findsOneWidget);
  });

  testWidgets('the ledger signs and tints from the amount, not the type', (
    tester,
  ) async {
    _phoneSized(tester);

    final customers = _provider(
      customerService: FakeCustomerService(
        transactions: [sampleEarn(), sampleRedeem(), sampleOpeningBalance()],
      ),
    );
    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen()), customers: customers),
    );
    await customers.refreshHome();
    await tester.pumpAndSettle();

    // A gain takes a plus, a deduction the U+2212 minus, and an opening
    // balance neither — it is a starting point, not a movement.
    expect(find.text('+11'), findsOneWidget);
    expect(find.text('−250'), findsOneWidget);
    expect(find.text('100'), findsOneWidget);
  });

  testWidgets('menu, purchases and settings render', (tester) async {
    _phoneSized(tester);

    final customers = _provider(
      customerService: FakeCustomerService(orders: [sampleOrder()]),
      productService: FakeProductService(products: [sampleProduct()]),
    );
    await customers.loadAll();

    for (final screen in const [
      MenuScreen(),
      PurchasesScreen(),
      SettingsScreen(),
    ]) {
      await tester.pumpWidget(
        _host(Scaffold(body: screen), customers: customers),
      );
      await tester.pumpAndSettle();
      expect(tester.takeException(), isNull);
    }
  });

  testWidgets('the empty states render', (tester) async {
    _phoneSized(tester);

    final customers = _provider();
    await customers.loadAll();

    await tester.pumpWidget(
      _host(const Scaffold(body: PurchasesScreen()), customers: customers),
    );
    await tester.pumpAndSettle();
    expect(find.text('لسا ما اشتريت إشي'), findsOneWidget);

    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen()), customers: customers),
    );
    await tester.pumpAndSettle();
    expect(find.text('لسا ما في حركات'), findsOneWidget);
  });

  testWidgets('the shell switches tabs and keeps them alive', (tester) async {
    _phoneSized(tester);

    final customers = _provider();
    await tester.pumpWidget(_host(const MainShell(), customers: customers));
    await tester.pumpAndSettle();

    // The bottom bar must take only the height it needs. It is measured against
    // the whole screen, so a child that grows to fill turns the nav into the
    // entire page and lays the body out at zero height — the tab bodies then
    // render blank with no exception to point at it. Assert the geometry, since
    // nothing else here would fail if it regressed.
    final navHeight = tester.getSize(find.byType(AppBottomNav)).height;
    final bodyHeight = tester.getSize(find.byType(IndexedStack)).height;
    expect(navHeight, lessThan(120), reason: 'bottom nav swallowed the screen');
    expect(bodyHeight, greaterThan(600), reason: 'tab body collapsed');

    // All four bodies live in an IndexedStack, so each is mounted from the
    // start — the tap changes which one is on top, not which one exists. The
    // three that are not current are offstage, which the default finder skips.
    expect(find.byType(MenuScreen, skipOffstage: false), findsOneWidget);

    await tester.tap(
      find.descendant(
        of: find.byType(AppBottomNav),
        matching: find.text(AppTab.menu.label),
      ),
    );
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
  });

  testWidgets('each tab renders its content inside the shell', (tester) async {
    _phoneSized(tester);

    final customers = _provider(
      customerService: FakeCustomerService(
        transactions: [sampleEarn()],
        orders: [sampleOrder()],
      ),
      productService: FakeProductService(products: [sampleProduct()]),
    );

    await tester.pumpWidget(_host(const MainShell(), customers: customers));
    await tester.pumpAndSettle();

    // One identifying string per tab, checked on the visible tree rather than
    // through the widget type: a screen can be mounted and still paint nothing.
    const marker = {
      AppTab.home: 'حركات النقاط',
      AppTab.menu: 'لاتيه',
      AppTab.purchases: 'مشترياتي',
      AppTab.settings: 'إصدار التطبيق',
    };

    for (final entry in marker.entries) {
      await tester.tap(
        find.descendant(
          of: find.byType(AppBottomNav),
          matching: find.text(entry.key.label),
        ),
      );
      await tester.pumpAndSettle();
      expect(
        find.text(entry.value),
        findsWidgets,
        reason: '${entry.key.name} tab rendered blank',
      );
    }
  });

  testWidgets('a category with no products shows the empty state', (
    tester,
  ) async {
    // Wider than a phone on purpose: it puts all six category chips on screen
    // so the tap needs no scrolling. What is under test is the category-to-items
    // mapping, not the strip's scroll physics.
    tester.view.physicalSize = const Size(1400, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    final customers = _provider(
      productService: FakeProductService(products: [sampleProduct()]),
    );
    await customers.refreshProducts();

    await tester.pumpWidget(
      _host(const Scaffold(body: MenuScreen()), customers: customers),
    );
    await tester.pumpAndSettle();

    // «قهوة ساخنة» is where the one seeded product lives.
    expect(find.text('لاتيه'), findsOneWidget);

    // «حلويات» has nothing in it, and must render as empty rather than
    // borrowing another category's items.
    await tester.tap(find.text(ProductCategory.desserts.label));
    await tester.pumpAndSettle();
    expect(find.text('لا يوجد أصناف حالياً'), findsOneWidget);
    expect(find.text('لاتيه'), findsNothing);
  });

  testWidgets('the menu hides a product the shop switched off', (tester) async {
    _phoneSized(tester);

    final customers = _provider(
      productService: FakeProductService(
        products: [
          sampleProduct(name: 'لاتيه'),
          sampleProduct(name: 'موكا', isAvailable: false),
        ],
      ),
    );
    await customers.refreshProducts();

    await tester.pumpWidget(
      _host(const Scaffold(body: MenuScreen()), customers: customers),
    );
    await tester.pumpAndSettle();

    expect(find.text('لاتيه'), findsOneWidget);
    expect(find.text('موكا'), findsNothing);
  });
}
