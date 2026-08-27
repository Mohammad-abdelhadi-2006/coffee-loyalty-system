import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:nakhat_finjan/main.dart';
import 'package:nakhat_finjan/screens/auth/login_screen.dart';
import 'package:nakhat_finjan/screens/splash_screen.dart';

void main() {
  testWidgets('starts on the splash, then routes to login', (tester) async {
    await tester.pumpWidget(const NakhatFinjanApp());

    expect(find.byType(SplashScreen), findsOneWidget);
    expect(find.byType(LoginScreen), findsNothing);

    // The splash leaves on its own after a beat. Waiting it out here also keeps
    // the test from ending with that timer still pending.
    await tester.pump(const Duration(milliseconds: 1200));
    await tester.pumpAndSettle();

    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.text('سجّل دخولك'), findsOneWidget);
  });

  testWidgets('lays the whole app out right-to-left', (tester) async {
    await tester.pumpWidget(const NakhatFinjanApp());

    final direction = Directionality.of(
      tester.element(find.byType(Scaffold).first),
    );
    expect(direction, TextDirection.rtl);

    await tester.pump(const Duration(milliseconds: 1200));
    await tester.pumpAndSettle();
  });

  testWidgets('the login CTA is disabled until the number is complete', (
    tester,
  ) async {
    await tester.pumpWidget(const MaterialApp(home: LoginScreen()));

    // A nine-digit subscriber number is what enables it; eight is not enough.
    await tester.enterText(find.byType(TextField), '79123456');
    await tester.pump();
    expect(tester.widget<InkWell>(find.byType(InkWell).last).onTap, isNull);

    await tester.enterText(find.byType(TextField), '791234567');
    await tester.pump();
    expect(tester.widget<InkWell>(find.byType(InkWell).last).onTap, isNotNull);
  });
}
