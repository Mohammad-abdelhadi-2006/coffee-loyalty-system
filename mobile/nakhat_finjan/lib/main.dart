import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'providers/auth_provider.dart';
import 'screens/splash_screen.dart';
import 'theme/app_theme.dart';

void main() {
  // Firebase is deliberately not initialised here. There is no
  // google-services.json in the project yet, so Firebase.initializeApp would
  // throw on the first frame. Phase 1 draws the screens; the auth wiring, and
  // the initializeApp call that has to precede it, come next.
  runApp(const NakhatFinjanApp());
}

class NakhatFinjanApp extends StatelessWidget {
  const NakhatFinjanApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      // Lazily built, and building it costs nothing: the provider only
      // constructs a Dio and a secure-storage handle, and makes no call until
      // signIn or restoreSession is invoked.
      create: (_) => AuthProvider(),
      child: MaterialApp(
        title: 'نكهة فنجان',
        debugShowCheckedModeBanner: false,
        theme: buildAppTheme(),
        // The whole app is Arabic, so direction is a constant rather than
        // something derived per-locale. This wraps the navigator itself, which
        // is what gets dialogs, bottom sheets and route transitions to lay out
        // and slide the right way too.
        //
        // `locale: Locale('ar')` is deliberately NOT set alongside it. Declaring
        // the locale without a MaterialLocalizations delegate that supports it
        // throws at build time, and the delegate needs flutter_localizations —
        // an SDK package, but still a pubspec entry, so it is left for you to
        // approve. Every string the app renders is its own and already Arabic;
        // what is missing is the framework's share: the text-selection menu,
        // date pickers, semantics labels.
        //
        // TODO(i18n): add to pubspec.yaml —
        //   flutter_localizations:
        //     sdk: flutter
        // then set localizationsDelegates to the three Global*Localizations
        // delegates, and restore locale / supportedLocales here.
        builder: (context, child) => Directionality(
          textDirection: TextDirection.rtl,
          child: child ?? const SizedBox.shrink(),
        ),
        home: const SplashScreen(),
      ),
    );
  }
}
