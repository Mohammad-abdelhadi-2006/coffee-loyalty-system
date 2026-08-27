import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:nakhat_finjan/screens/home_screen.dart';
import 'package:nakhat_finjan/screens/main_shell.dart';
import 'package:nakhat_finjan/screens/menu_screen.dart';
import 'package:nakhat_finjan/screens/purchases_screen.dart';
import 'package:nakhat_finjan/screens/settings_screen.dart';
import 'package:nakhat_finjan/theme/app_theme.dart';
import 'package:nakhat_finjan/widgets/app_bottom_nav.dart';

/// Wraps a screen the way the app does — RTL, the real theme, a phone-sized
/// surface. Without the RTL wrapper these would pass here and overflow on a
/// device, which is the whole class of bug this file exists to catch.
Widget _host(Widget child) => MaterialApp(
  theme: buildAppTheme(),
  home: Directionality(textDirection: TextDirection.rtl, child: child),
);

void main() {
  testWidgets('every home state renders without overflowing', (tester) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    for (final state in HomeState.values) {
      await tester.pumpWidget(_host(Scaffold(body: HomeScreen(state: state))));
      await tester.pump(const Duration(milliseconds: 300));
      expect(tester.takeException(), isNull, reason: 'home state $state');
    }
  });

  testWidgets('menu, purchases and settings render', (tester) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    for (final screen in const [
      MenuScreen(),
      PurchasesScreen(),
      SettingsScreen(),
    ]) {
      await tester.pumpWidget(_host(Scaffold(body: screen)));
      await tester.pumpAndSettle();
      expect(tester.takeException(), isNull);
    }
  });

  testWidgets('the empty states render', (tester) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    await tester.pumpWidget(
      _host(const Scaffold(body: PurchasesScreen(purchases: []))),
    );
    await tester.pumpAndSettle();
    expect(find.text('لسا ما اشتريت إشي'), findsOneWidget);

    await tester.pumpWidget(
      _host(const Scaffold(body: HomeScreen(state: HomeState.empty))),
    );
    await tester.pumpAndSettle();
    expect(find.text('لسا ما في حركات'), findsOneWidget);
  });

  testWidgets('the shell switches tabs and keeps them alive', (tester) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    await tester.pumpWidget(_host(const MainShell()));
    await tester.pump(const Duration(milliseconds: 300));

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

    // The nav label and the menu's own title are both «المنيو», so target the
    // one inside the bottom bar rather than the bare string.
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
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    await tester.pumpWidget(_host(const MainShell()));
    await tester.pump(const Duration(milliseconds: 400));

    // One identifying string per tab, checked on the visible tree rather than
    // through the widget type: a screen can be mounted and still paint nothing.
    const marker = {
      AppTab.home: 'حركات النقاط',
      AppTab.menu: 'سبانش لاتيه',
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

  testWidgets('an empty menu category shows the empty state, not fake items', (
    tester,
  ) async {
    // Wider than a phone on purpose: it puts all six category chips on screen
    // so the tap needs no scrolling. What is under test is the category-to-items
    // mapping, not the strip's scroll physics.
    tester.view.physicalSize = const Size(1400, 844);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.reset);

    await tester.pumpWidget(_host(const Scaffold(body: MenuScreen())));
    await tester.pumpAndSettle();

    // «قهوة ساخنة» is the one category the shop actually supplied.
    expect(find.text('سبانش لاتيه'), findsOneWidget);

    // «حلويات» is one of the three the source menu was cut off before reaching.
    // It must render as empty rather than as invented items.
    await tester.tap(find.text('حلويات'));
    await tester.pumpAndSettle();
    expect(find.text('لا يوجد أصناف حالياً'), findsOneWidget);
    expect(find.text('سبانش لاتيه'), findsNothing);
  });
}
