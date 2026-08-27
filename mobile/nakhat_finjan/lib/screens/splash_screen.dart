import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import 'auth/login_screen.dart';

/// The first frame: the mark, a hairline, the name.
///
/// In the design this is the "silent token check" — it decides between the
/// login screen and the home tab without the customer seeing a spinner. Phase 1
/// has no token check, so it holds for a beat and goes to login. Wiring it up
/// means awaiting AuthProvider.restoreSession here and branching on
/// isAuthenticated.
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _leave();
  }

  Future<void> _leave() async {
    // TODO(auth): replace the delay with
    //   await context.read<AuthProvider>().restoreSession()
    // and route to MainShell when isAuthenticated, LoginScreen otherwise.
    await Future<void>.delayed(const Duration(milliseconds: 1200));
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute<void>(builder: (_) => const LoginScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const _Logo(),
            const SizedBox(height: 26),
            Container(
              width: 36,
              height: 1,
              color: AppColors.caramel.withValues(alpha: 0.5),
            ),
            const SizedBox(height: 14),
            Text(
              'نكهة فنجان',
              style: AppText.subtitle.copyWith(
                fontSize: 14,
                letterSpacing: 2.5,
                color: AppColors.ink50,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// The brand mark at its design width of 214.
///
/// [Image.errorBuilder] falls back to the name set as type, so a missing or
/// unreadable asset degrades to a plain wordmark instead of a broken-image box.
class _Logo extends StatelessWidget {
  const _Logo();

  @override
  Widget build(BuildContext context) {
    return Image.asset(
      'assets/images/logo.png',
      width: 214,
      fit: BoxFit.contain,
      errorBuilder: (context, error, stackTrace) => SizedBox(
        width: 214,
        child: Text(
          'نكهة فنجان',
          textAlign: TextAlign.center,
          style: AppText.nameTitle.copyWith(fontSize: 38, letterSpacing: 1),
        ),
      ),
    );
  }
}
