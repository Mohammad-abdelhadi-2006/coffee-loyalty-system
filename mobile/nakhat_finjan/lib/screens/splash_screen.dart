import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import 'auth/login_screen.dart';
import 'main_shell.dart';

/// The first frame: the mark, a hairline, the name.
///
/// The design calls this the "silent token check": it decides between the login
/// screen and the home tab without the customer ever seeing a spinner. Reading
/// the stored JWT takes a few milliseconds, so the branch is held behind a short
/// minimum so the mark does not flash past — a splash that vanishes instantly
/// reads as a glitch, not as speed.
///
/// It only checks that a token *exists*. Whether it is still good is the
/// server's call, and a rejected one is cleared by `ApiClient`'s 401 handler,
/// which sends the customer back to login from wherever they landed.
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

  /// The shortest the mark stays up, however fast the token check returns.
  static const Duration _minimumHold = Duration(milliseconds: 900);

  Future<void> _leave() async {
    final auth = context.read<AuthProvider>();

    // Both together, so the hold overlaps the read instead of following it.
    await Future.wait([
      auth.restoreSession(),
      Future<void>.delayed(_minimumHold),
    ]);

    if (!mounted) return;

    Navigator.of(context).pushReplacement(
      MaterialPageRoute<void>(
        builder: (_) =>
            auth.isAuthenticated ? const MainShell() : const LoginScreen(),
      ),
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
