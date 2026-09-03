import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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

/// A store whose reads fail, the way the Android keystore does when it can no
/// longer decrypt what it wrote — a reinstall that restores the old encrypted
/// preferences against a freshly generated key.
class _FailingSecureStore extends SecureStore {
  @override
  Future<String?> getToken() async =>
      throw PlatformException(code: 'Exception encountered', message: 'read');

  @override
  Future<void> saveToken(String token) async {}

  @override
  Future<void> deleteToken() async {}
}

/// The app's real root, minus `main()`'s Firebase call — the providers are
/// seeded so nothing reaches a platform channel.
Widget _app({String? storedToken, SecureStore? store}) => MultiProvider(
  providers: [
    ChangeNotifierProvider(
      create: (_) =>
          AuthProvider(secureStore: store ?? _FakeSecureStore(storedToken)),
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

/// Long enough to cover the splash's whole sequence — the hold it keeps the
/// mark up for, plus the outro that follows it. Deliberately not pinned to the
/// exact constants in `SplashScreen`: these tests are about which screen it
/// lands on, not about how long it takes to get there, and a static hold
/// schedules no frames for `pumpAndSettle` to advance through on its own.
const _pastTheSplash = Duration(seconds: 2);

void main() {
  testWidgets('with no stored token, the splash routes to login', (
    tester,
  ) async {
    await tester.pumpWidget(_app());

    expect(find.byType(SplashScreen), findsOneWidget);
    expect(find.byType(LoginScreen), findsNothing);

    // The splash holds the mark for a beat before branching; waiting it out
    // here also keeps the test from ending with that timer pending.
    await tester.pump(_pastTheSplash);
    await tester.pumpAndSettle();

    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.text('سجّل دخولك'), findsOneWidget);
  });

  testWidgets('with a stored token, the splash routes straight to the app', (
    tester,
  ) async {
    await tester.pumpWidget(_app(storedToken: 'a.stored.jwt'));

    await tester.pump(_pastTheSplash);
    await tester.pumpAndSettle();

    expect(find.byType(MainShell), findsOneWidget);
    expect(find.byType(LoginScreen), findsNothing);
  });

  // Regression: a read that throws used to escape `_leave` as an unhandled
  // async error, so the splash never reached its branch and the app sat on the
  // mark forever — no spinner, no message, nothing to tap, and nothing short of
  // reinstalling to get out of it. Seen for real on a device whose keystore
  // could not decrypt its own token (BAD_DECRYPT).
  testWidgets('a storage failure still routes to login, not a stuck splash', (
    tester,
  ) async {
    await tester.pumpWidget(_app(store: _FailingSecureStore()));
    await tester.pump(_pastTheSplash);

    // Reported, not silently swallowed — a keystore that cannot read its own
    // writes should still reach a crash console. Taking it here is what keeps
    // the test framework from treating it as this test's own failure.
    expect(tester.takeException(), isA<PlatformException>());

    await tester.pumpAndSettle();

    // The point of the whole test: it carried on regardless.
    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.byType(SplashScreen), findsNothing);
  });

  testWidgets('lays the whole app out right-to-left', (tester) async {
    await tester.pumpWidget(_app());

    final direction = Directionality.of(
      tester.element(find.byType(Scaffold).first),
    );
    expect(direction, TextDirection.rtl);

    await tester.pump(_pastTheSplash);
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
