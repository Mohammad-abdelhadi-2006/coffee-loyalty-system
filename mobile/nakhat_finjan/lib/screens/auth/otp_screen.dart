import 'dart:async';

// `hide AuthProvider`: firebase_auth exports a class of that name too, and the
// one this screen means is ours.
import 'package:firebase_auth/firebase_auth.dart' hide AuthProvider;
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

import '../../models/api_error.dart';
import '../../providers/auth_provider.dart';
import '../../providers/customer_provider.dart';
import '../../services/phone_auth_service.dart';
import '../../theme/app_colors.dart';
import '../../theme/app_text.dart';
import '../../theme/app_theme.dart';
import '../../widgets/app_buttons.dart';
import '../../widgets/field_error.dart';
import '../main_shell.dart';
import 'name_screen.dart';

/// Six-digit code entry, with the three states the design draws: typing, a
/// rejected code, and blocked after too many attempts.
///
/// The blocked state is the backend's TOO_MANY_REQUESTS (429), throttled per
/// phone number. The contract is explicit that the client must surface the
/// message and let the customer retry later — never auto-retry — which is why
/// the resend action goes dead rather than restarting its own countdown.
class OtpScreen extends StatefulWidget {
  const OtpScreen({
    super.key,
    required this.nationalNumber,
    this.verificationId,
    this.resendToken,
    this.autoCredential,
  });

  /// The subscriber digits, without the +962. Shown back to the customer so
  /// they can tell a typo from a delivery problem, and used to resend.
  final String nationalNumber;

  /// Pairs with the digits the customer types. Null only when Firebase
  /// auto-verified, in which case [autoCredential] carries the result instead.
  final String? verificationId;

  /// Firebase's handle for "send that number another SMS". Passing it back on a
  /// resend is what stops the second request being treated as a fresh one and
  /// counted separately against the quota.
  final int? resendToken;

  /// Set when Android retrieved and verified the SMS by itself. The screen
  /// signs in with it on first build and the customer never types anything.
  final PhoneAuthCredential? autoCredential;

  @override
  State<OtpScreen> createState() => _OtpScreenState();
}

class _OtpScreenState extends State<OtpScreen> {
  static const int _codeLength = 6;
  static const int _resendSeconds = 60;

  final List<TextEditingController> _controllers = List.generate(
    _codeLength,
    (_) => TextEditingController(),
  );
  final List<FocusNode> _focusNodes = List.generate(
    _codeLength,
    (_) => FocusNode(),
  );

  final PhoneAuthService _phoneAuth = PhoneAuthService();

  Timer? _timer;
  int _remaining = _resendSeconds;

  String? _error;

  /// Set when the backend answers the exchange with TOO_MANY_REQUESTS (429).
  /// The contract says to surface it and let the customer retry later, never to
  /// auto-retry, so this kills both actions rather than restarting the
  /// countdown.
  bool _isBlocked = false;

  /// A verify or a resend is in flight.
  bool _isBusy = false;

  /// The current verification handle. Not `widget.verificationId` directly,
  /// because a resend replaces it with a fresh one.
  String? _verificationId;
  int? _resendToken;

  String get _code => _controllers.map((c) => c.text).join();

  bool get _isComplete => _code.length == _codeLength;

  @override
  void initState() {
    super.initState();
    _verificationId = widget.verificationId;
    _resendToken = widget.resendToken;
    _startCountdown();

    final credential = widget.autoCredential;
    if (credential != null) {
      // Auto-retrieval already happened on the login screen. Finish the
      // sign-in without waiting for input the customer will never give.
      WidgetsBinding.instance.addPostFrameCallback((_) {
        _signInWith(() => _phoneAuth.signInWithCredential(credential));
      });
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    for (final c in _controllers) {
      c.dispose();
    }
    for (final f in _focusNodes) {
      f.dispose();
    }
    super.dispose();
  }

  void _startCountdown() {
    _timer?.cancel();
    setState(() => _remaining = _resendSeconds);
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_remaining <= 1) {
        timer.cancel();
        setState(() => _remaining = 0);
      } else {
        setState(() => _remaining--);
      }
    });
  }

  /// Moves focus forward on entry and backward on delete, so the six boxes
  /// behave like one field.
  void _onDigitChanged(int index, String value) {
    setState(() => _error = null);

    if (value.isNotEmpty && index < _codeLength - 1) {
      _focusNodes[index + 1].requestFocus();
    } else if (value.isEmpty && index > 0) {
      _focusNodes[index - 1].requestFocus();
    }
  }

  void _verify() {
    final verificationId = _verificationId;
    if (verificationId == null) {
      // No handle to check the digits against — the only way here is a resend
      // that failed, and typing more digits will not fix it.
      setState(() => _error = 'انتهت صلاحية الرمز، اطلب رمزاً جديداً');
      return;
    }

    _signInWith(
      () =>
          _phoneAuth.verifyCode(verificationId: verificationId, smsCode: _code),
    );
  }

  /// The whole sign-in tail, shared by the typed-code and auto-retrieval paths.
  ///
  /// Two steps that can each fail differently: Firebase checks the code, then
  /// the backend exchanges the resulting ID token for our JWT. Firebase
  /// failures are about the code; backend failures are about the account.
  Future<void> _signInWith(Future<String> Function() getIdToken) async {
    setState(() {
      _isBusy = true;
      _error = null;
    });

    final auth = context.read<AuthProvider>();

    String idToken;
    try {
      idToken = await getIdToken();
    } on FirebaseAuthException catch (e) {
      if (!mounted) return;
      setState(() {
        _isBusy = false;
        _error = PhoneAuthService.messageForCodeFailure(e);
      });
      return;
    }

    final signedIn = await auth.signIn(idToken);
    if (!mounted) return;

    setState(() => _isBusy = false);

    if (signedIn) {
      // The customer already exists. Start loading their data before the shell
      // paints so the home tab is not empty for a beat.
      context.read<CustomerProvider>().loadAll();
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute<void>(builder: (_) => const MainShell()),
        (route) => false,
      );
      return;
    }

    if (auth.isNewCustomer) {
      // First login for this number: the backend cannot create the account
      // without a name. The ID token goes with them — the name screen repeats
      // the exchange, it does not start a new one.
      Navigator.of(context).push(
        MaterialPageRoute<void>(builder: (_) => NameScreen(idToken: idToken)),
      );
      return;
    }

    setState(() {
      if (auth.errorCode == ErrorCodes.tooManyRequests) {
        _isBlocked = true;
        _error = null;
      } else {
        _error = auth.errorMessage ?? 'تعذّر تسجيل الدخول، حاول مجدداً';
      }
    });
  }

  /// Asks Firebase for another SMS and restarts the countdown.
  Future<void> _resend() async {
    setState(() {
      _isBusy = true;
      _error = null;
    });

    await _phoneAuth.sendCode(
      nationalNumber: widget.nationalNumber,
      resendToken: _resendToken,
      onCodeSent: (verificationId, resendToken) {
        if (!mounted) return;
        setState(() {
          _isBusy = false;
          _verificationId = verificationId;
          _resendToken = resendToken;
        });
        _startCountdown();
      },
      onAutoVerified: (credential) {
        if (!mounted) return;
        _signInWith(() => _phoneAuth.signInWithCredential(credential));
      },
      onFailed: (message) {
        if (!mounted) return;
        setState(() {
          _isBusy = false;
          _error = message;
        });
      },
    );
  }

  String get _formattedCountdown {
    final minutes = (_remaining ~/ 60).toString().padLeft(2, '0');
    final seconds = (_remaining % 60).toString().padLeft(2, '0');
    return '$minutes:$seconds';
  }

  @override
  Widget build(BuildContext context) {
    final canResend = _remaining == 0 && !_isBlocked && !_isBusy;

    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(
            AppMetrics.screenPadding,
            18,
            AppMetrics.screenPadding,
            34,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const _BackButton(),
              const SizedBox(height: 22),
              Text('أدخل الرمز', style: AppText.authTitle),
              const SizedBox(height: 12),
              _SentToLine(nationalNumber: widget.nationalNumber),
              const SizedBox(height: 34),
              Opacity(
                opacity: _isBlocked ? 0.4 : 1,
                child: _CodeBoxes(
                  controllers: _controllers,
                  focusNodes: _focusNodes,
                  hasError: _error != null,
                  enabled: !_isBlocked && !_isBusy,
                  onChanged: _onDigitChanged,
                ),
              ),
              if (_isBlocked) ...[
                const SizedBox(height: 20),
                // The backend's own Arabic message when it has one — it names
                // the real wait, which is configuration (`LoginThrottle`) and
                // not something this screen should hardcode.
                BlockedNotice(
                  message:
                      context.read<AuthProvider>().errorMessage ??
                      'حاولت كثير. جرّب مرة ثانية بعد شوي',
                ),
              ] else ...[
                if (_error != null) ...[
                  const SizedBox(height: 12),
                  FieldError(message: _error!),
                ],
                const SizedBox(height: 16),
                Center(
                  child: canResend
                      ? Text('تقدر تطلب الرمز مرة ثانية', style: AppText.hint)
                      : _CountdownLine(value: _formattedCountdown),
                ),
              ],
              const Spacer(),
              PrimaryButton(
                label: 'تحقّق',
                isBusy: _isBusy,
                onPressed: _isComplete && !_isBlocked ? _verify : null,
              ),
              const SizedBox(height: 6),
              TextAction(
                label: 'إعادة إرسال الرمز',
                onPressed: canResend ? _resend : null,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// «أرسلنا رمزاً إلى +962 7 9123 4567».
///
/// The number is a separate LTR run inside the RTL sentence so the digits and
/// the plus keep their order.
class _SentToLine extends StatelessWidget {
  const _SentToLine({required this.nationalNumber});

  final String nationalNumber;

  @override
  Widget build(BuildContext context) {
    return Text.rich(
      TextSpan(
        style: AppText.subtitle,
        children: [
          const TextSpan(text: 'أرسلنا رمزاً إلى '),
          WidgetSpan(
            alignment: PlaceholderAlignment.middle,
            child: Text(
              '+962 $nationalNumber',
              textDirection: TextDirection.ltr,
              style: AppText.phone.copyWith(
                fontSize: 15,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _CountdownLine extends StatelessWidget {
  const _CountdownLine({required this.value});

  final String value;

  @override
  Widget build(BuildContext context) {
    return Text.rich(
      TextSpan(
        style: AppText.hint,
        children: [
          const TextSpan(text: 'إعادة الإرسال بعد '),
          WidgetSpan(
            alignment: PlaceholderAlignment.middle,
            child: Text(
              value,
              textDirection: TextDirection.ltr,
              style: AppText.hint.copyWith(
                fontFamily: AppText.latin,
                fontWeight: FontWeight.w500,
                color: AppColors.ink55,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// The six boxes.
///
/// Laid out LTR: a verification code is read and typed left to right, and the
/// first box must sit under the caret the keyboard opens against.
class _CodeBoxes extends StatelessWidget {
  const _CodeBoxes({
    required this.controllers,
    required this.focusNodes,
    required this.hasError,
    required this.enabled,
    required this.onChanged,
  });

  final List<TextEditingController> controllers;
  final List<FocusNode> focusNodes;
  final bool hasError;
  final bool enabled;
  final void Function(int index, String value) onChanged;

  @override
  Widget build(BuildContext context) {
    return Row(
      textDirection: TextDirection.ltr,
      children: [
        for (var i = 0; i < controllers.length; i++) ...[
          if (i > 0) const SizedBox(width: 9),
          Expanded(
            child: SizedBox(
              height: AppMetrics.fieldHeight,
              child: TextField(
                controller: controllers[i],
                focusNode: focusNodes[i],
                enabled: enabled,
                textAlign: TextAlign.center,
                keyboardType: TextInputType.number,
                maxLength: 1,
                style: AppText.otpDigit.copyWith(
                  color: hasError ? AppColors.deduct : AppColors.ink,
                ),
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                onChanged: (value) => onChanged(i, value),
                decoration: InputDecoration(
                  counterText: '',
                  filled: true,
                  fillColor: AppColors.surface,
                  contentPadding: EdgeInsets.zero,
                  enabledBorder: _border(hasError),
                  focusedBorder: _border(hasError, focused: true),
                  disabledBorder: _border(false),
                  border: _border(hasError),
                ),
              ),
            ),
          ),
        ],
      ],
    );
  }

  OutlineInputBorder _border(bool error, {bool focused = false}) {
    final Color color;
    if (error) {
      color = AppColors.deduct;
    } else if (focused) {
      color = AppColors.goldDeep;
    } else {
      color = AppColors.fieldBorder;
    }
    return OutlineInputBorder(
      borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
      borderSide: BorderSide(color: color, width: error || focused ? 1.5 : 1),
    );
  }
}

/// The arrow back to the previous auth step.
///
/// It points right: in an RTL layout "back" is toward the start of the reading
/// direction, and [Icons.arrow_forward] is the glyph that flips with the locale.
class _BackButton extends StatelessWidget {
  const _BackButton();

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: AlignmentDirectional.centerStart,
      child: IconButton(
        onPressed: () => Navigator.of(context).maybePop(),
        icon: const Icon(Icons.arrow_forward, size: 24, color: AppColors.ink),
        tooltip: 'رجوع',
        padding: EdgeInsets.zero,
        constraints: const BoxConstraints(minWidth: 48, minHeight: 48),
      ),
    );
  }
}
