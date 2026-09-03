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
///
/// The motion is one move in three beats: the mark comes up from a point to its
/// own size, holds while the token is read, then rushes past the screen and the
/// app is behind it. Skipped entirely when the platform asks for less motion.
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with TickerProviderStateMixin {
  /// Beat one: a point opening out to full size.
  static const Duration _introDuration = Duration(milliseconds: 560);

  /// Beat three: the mark rushing past the screen.
  static const Duration _outroDuration = Duration(milliseconds: 520);

  /// Beats one and two together — the mark is up and still for the remainder,
  /// however fast the token read returns.
  static const Duration _minimumHold = Duration(milliseconds: 1180);

  /// The last hand-off, once the mark has already cleared the screen.
  static const Duration _handoffDuration = Duration(milliseconds: 260);

  /// Far enough that the mark is well past every edge before it goes. The
  /// logo is 214 wide against a phone barely 400 across.
  static const double _outroScaleEnd = 7;

  late final AnimationController _intro = AnimationController(
    vsync: this,
    duration: _introDuration,
  );

  late final AnimationController _outro = AnimationController(
    vsync: this,
    duration: _outroDuration,
  );

  /// From a point to full size. `easeOutCubic` decelerates into place without
  /// overshooting — the mark arrives and stops rather than springing.
  late final Animation<double> _introScale = Tween<double>(
    begin: 0.02,
    end: 1,
  ).animate(CurvedAnimation(parent: _intro, curve: Curves.easeOutCubic));

  /// Fades ahead of the scale, so the mark is legible while it is still
  /// growing rather than appearing only once it has arrived.
  late final Animation<double> _introFade = CurvedAnimation(
    parent: _intro,
    curve: const Interval(0, 0.7, curve: Curves.easeOut),
  );

  /// `easeInCubic`: slow off the mark and accelerating away, which is what
  /// makes it read as rushing past rather than merely getting bigger.
  late final Animation<double> _outroScale = Tween<double>(
    begin: 1,
    end: _outroScaleEnd,
  ).animate(CurvedAnimation(parent: _outro, curve: Curves.easeInCubic));

  /// Held opaque until the mark is most of the way past, then gone quickly —
  /// fading it early would leave the screen empty while it was still growing.
  late final Animation<double> _outroFade = Tween<double>(
    begin: 1,
    end: 0,
  ).animate(
    CurvedAnimation(
      parent: _outro,
      curve: const Interval(0.55, 1, curve: Curves.easeIn),
    ),
  );

  /// The rule and the name do not travel with the mark; they simply leave.
  late final Animation<double> _captionFade = Tween<double>(
    begin: 1,
    end: 0,
  ).animate(
    CurvedAnimation(
      parent: _outro,
      curve: const Interval(0, 0.45, curve: Curves.easeOut),
    ),
  );

  /// `didChangeDependencies` can fire more than once; the intro must not
  /// restart when it does.
  bool _motionStarted = false;

  bool _reduceMotion = false;

  @override
  void initState() {
    super.initState();
    _leave();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();

    if (_motionStarted) return;
    _motionStarted = true;

    // Read here rather than in initState: MediaQuery is an inherited widget and
    // is not available until dependencies are resolved.
    _reduceMotion = MediaQuery.disableAnimationsOf(context);

    if (_reduceMotion) {
      // Straight to the settled state — full size, fully opaque, no ticker
      // running at all. Nothing animates, and nothing is left half-drawn.
      _intro.value = 1;
      return;
    }

    _intro.forward();
  }

  @override
  void dispose() {
    _intro.dispose();
    _outro.dispose();
    super.dispose();
  }

  Future<void> _leave() async {
    final auth = context.read<AuthProvider>();

    // Both together, so the hold overlaps the read instead of following it.
    //
    // Guarded, and that is not belt-and-braces: this future is started in
    // initState and nobody awaits it, so anything that throws in here escapes as
    // an unhandled async error and the navigation below is simply never reached.
    // The app then sits on the mark forever — no spinner, no message, nothing to
    // tap. A failed session read is recoverable (it means "signed out"), so the
    // branch has to happen either way.
    try {
      await Future.wait([
        auth.restoreSession(),
        Future<void>.delayed(_minimumHold),
      ]);
    } catch (error, stackTrace) {
      FlutterError.reportError(
        FlutterErrorDetails(
          exception: error,
          stack: stackTrace,
          library: 'nakhat_finjan',
          context: ErrorDescription('restoring the session on the splash'),
          silent: true,
        ),
      );
    }

    if (!mounted) return;

    // Beat three, and only then the hand-off: the mark clears the screen before
    // anything else is put on it, so the two never overlap into a smear.
    if (!_reduceMotion) {
      await _outro.forward();

      if (!mounted) return;
    }

    Navigator.of(context).pushReplacement(
      PageRouteBuilder<void>(
        // Same destination as before — only how it arrives has changed.
        pageBuilder: (_, _, _) =>
            auth.isAuthenticated ? const MainShell() : const LoginScreen(),
        transitionDuration: _reduceMotion ? Duration.zero : _handoffDuration,
        reverseTransitionDuration: _reduceMotion
            ? Duration.zero
            : _handoffDuration,
        transitionsBuilder: (_, animation, _, child) => FadeTransition(
          opacity: CurvedAnimation(parent: animation, curve: Curves.easeOut),
          child: child,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    // FadeTransition and ScaleTransition drive the render object straight from
    // the animation, so none of the subtree below rebuilds per frame — the
    // composition is laid out once and only repainted. The nested scales
    // multiply, which is how the intro and the outro compose without either
    // needing to know about the other.
    return Scaffold(
      body: Center(
        child: FadeTransition(
          opacity: _introFade,
          child: ScaleTransition(
            scale: _introScale,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Only the mark makes the final rush. Transform does not affect
                // layout, so nothing below it shifts while it grows.
                FadeTransition(
                  opacity: _outroFade,
                  child: ScaleTransition(
                    scale: _outroScale,
                    child: const _Logo(),
                  ),
                ),
                FadeTransition(
                  opacity: _captionFade,
                  child: Column(
                    children: [
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
              ],
            ),
          ),
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
