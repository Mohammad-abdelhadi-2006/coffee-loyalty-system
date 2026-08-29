import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:nakhat_finjan/providers/customer_provider.dart';
import 'package:nakhat_finjan/screens/settings_screen.dart';
import 'package:nakhat_finjan/theme/app_theme.dart';

import 'support/fake_services.dart';

void main() {
  // The credits line is the widest fixed-width thing on the settings screen, and it does not
  // shrink — so it is the first thing to run off the edge of a small phone. 320 is narrower
  // than the 390 the smoke tests use and narrower than any handset still in use.
  for (final width in [320.0, 360.0, 390.0]) {
    testWidgets('settings fits at ${width.toInt()}dp', (tester) async {
      tester.view.physicalSize = Size(width, 800);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.reset);

      final customers = CustomerProvider(
        customerService: FakeCustomerService(),
        productService: FakeProductService(),
      );
      await customers.loadAll();

      await tester.pumpWidget(
        ChangeNotifierProvider<CustomerProvider>.value(
          value: customers,
          child: MaterialApp(
            theme: buildAppTheme(),
            home: const Directionality(
              textDirection: TextDirection.rtl,
              child: Scaffold(body: SettingsScreen()),
            ),
          ),
        ),
      );
      await tester.pumpAndSettle();

      expect(tester.takeException(), isNull);
      expect(find.text('محمد عبدالهادي'), findsOneWidget);
    });
  }
}
