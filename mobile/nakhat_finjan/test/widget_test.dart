import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:nakhat_finjan/core/secure_store.dart';
import 'package:nakhat_finjan/providers/auth_provider.dart';
import 'package:nakhat_finjan/providers/customer_provider.dart';
import 'package:nakhat_finjan/screens/auth/login_screen.dart';
import 'package:nakhat_finjan/screens/main_shell.dart';
import 'package:nakhat_finjan/screens/splash_screen.dart';
import 'package:nakhat_finjan/theme/app_theme.dart';

import 'support/fake_services.dart';

/// An in-memory [SecureStore].
///
/// The real one talks to the platform keystore over a method channel, which a
/// widget test has no host for. Subclassing beats mocking the channel: the
/// surface is three methods and the behaviour under test is the branch the
/// splash takes, not the storage itself.
class _FakeSecureStore extends SecureStore {
  _FakeSecureStore([this._token]);

  String? _token;

  @override
  Future<String?> getToken() async => _token;

  @override
  Future<void> saveToken(String token) async => _token = token;

  @override
  Future<void> deleteToken() async => _token = null;
}

/// The app's real root, minus `main()`'s Firebase call — the providers are
/// seeded so nothing reaches a platform channel.
Widget _app({String? storedToken}) => MultiProvider(
  providers: [
    ChangeNotifierProvider(
      create: (_) => AuthProvider(secureStore: _FakeSecureStore(storedToken)),
    ),
    ChangeNotifierProvider(
      create: (_) => CustomerProvider(
        customerService: FakeCustomerService(),
        productService: FakeProductService(),
      ),
    ),
  ],
  child: MaterialApp(
    theme: buildAppTheme(),
    home: Directionality(
      textDirection: TextDirection.rtl,
      child: const SplashScreen(),
    ),
  ),
);

void main() {
  testWidgets('with no stored token, the splash routes to login', (
    tester,
  ) async {
    await tester.pumpWidget(_app());

    expect(find.byType(SplashScreen), findsOneWidget);
    expect(find.byType(LoginScreen), findsNothing);

    // The splash holds the mark for a beat before branching; waiting it out
    // here also keeps the test from ending with that timer pending.
    await tester.pump(const Duration(milliseconds: 900));
    await tester.pumpAndSettle();

    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.text('سجّل دخولك'), findsOneWidget);
  });

  testWidgets('with a stored token, the splash routes straight to the app', (
    tester,
  ) async {
    await tester.pumpWidget(_app(storedToken: 'a.stored.jwt'));

    await tester.pump(const Duration(milliseconds: 900));
    await tester.pumpAndSettle();

    expect(find.byType(MainShell), findsOneWidget);
    expect(find.byType(LoginScreen), findsNothing);
  });

  testWidgets('lays the whole app out right-to-left', (tester) async {
    await tester.pumpWidget(_app());

    final direction = Directionality.of(
      tester.element(find.byType(Scaffold).first),
    );
    expect(direction, TextDirection.rtl);

    await tester.pump(const Duration(milliseconds: 900));
    await tester.pumpAndSettle();
  });

  testWidgets('the login CTA is disabled until the number is complete', (
    tester,
  ) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Directionality(
          textDirection: TextDirection.rtl,
          child: LoginScreen(),
        ),
      ),
    );

    // A nine-digit subscriber number is what enables it; eight is not enough.
    await tester.enterText(find.byType(TextField), '79123456');
    await tester.pump();
    expect(tester.widget<InkWell>(find.byType(InkWell).last).onTap, isNull);

    await tester.enterText(find.byType(TextField), '791234567');
    await tester.pump();
    expect(tester.widget<InkWell>(find.byType(InkWell).last).onTap, isNotNull);
  });
}
