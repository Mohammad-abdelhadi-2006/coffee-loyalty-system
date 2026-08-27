import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text.dart';
import '../../theme/app_theme.dart';
import '../../widgets/app_buttons.dart';
import '../../widgets/field_error.dart';
import 'otp_screen.dart';

/// Phone entry. One field, split into a fixed +962 plate and the subscriber
/// number, and one CTA that stays disabled until the number is the right length.
///
/// Phase 1 is UI only: nothing is sent. The real flow hands the number to
/// firebase_auth, and hands the resulting ID token to AuthProvider.signIn once
/// the OTP verifies.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final TextEditingController _controller = TextEditingController();

  /// Set on submit, never while typing — telling someone their number is wrong
  /// before they have finished typing it is just noise.
  String? _error;

  /// A Jordanian mobile subscriber number is 9 digits after the country code.
  /// The backend does the authoritative check (INVALID_PHONE); this only gates
  /// the button so an obviously short number never leaves the device.
  static const int _nationalDigits = 9;

  bool get _isComplete => _controller.text.length == _nationalDigits;

  @override
  void initState() {
    super.initState();
    _controller.addListener(_onChanged);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onChanged() => setState(() => _error = null);

  void _submit() {
    // TODO(auth): send the OTP through firebase_auth here, and push the OTP
    // screen only once Firebase has accepted the number.
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => OtpScreen(nationalNumber: _controller.text),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(
            AppMetrics.screenPadding,
            44,
            AppMetrics.screenPadding,
            34,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text('سجّل دخولك', style: AppText.authTitle),
              const SizedBox(height: 12),
              Text('رح يوصلك رمز تحقق على رقمك', style: AppText.subtitle),
              const SizedBox(height: 34),
              PhoneField(controller: _controller, hasError: _error != null),
              if (_error != null) ...[
                const SizedBox(height: 10),
                FieldError(message: _error!),
              ],
              const Spacer(),
              PrimaryButton(
                label: 'إرسال الرمز',
                onPressed: _isComplete ? _submit : null,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// The split field: a +962 plate on the leading edge, then the number.
///
/// Both halves are laid out LTR inside the RTL screen. A phone number reads
/// left to right in every locale, and letting it inherit RTL would throw the
/// country code to the wrong end of the digits the moment one is typed.
class PhoneField extends StatelessWidget {
  const PhoneField({
    super.key,
    required this.controller,
    required this.hasError,
  });

  final TextEditingController controller;
  final bool hasError;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: AppMetrics.fieldHeight,
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(AppMetrics.radiusField),
        border: Border.all(
          color: hasError ? AppColors.deduct : AppColors.fieldBorder,
          width: hasError ? 1.5 : 1,
        ),
        boxShadow: hasError ? null : AppColors.cardShadow,
      ),
      child: Row(
        textDirection: TextDirection.ltr,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 18),
            alignment: Alignment.center,
            decoration: const BoxDecoration(
              border: Border(right: BorderSide(color: AppColors.hairline)),
            ),
            child: Text(
              '+962',
              style: AppText.phone.copyWith(fontWeight: FontWeight.w500),
              textDirection: TextDirection.ltr,
            ),
          ),
          Expanded(
            child: TextField(
              controller: controller,
              keyboardType: TextInputType.phone,
              textDirection: TextDirection.ltr,
              textAlign: TextAlign.left,
              style: AppText.phone,
              maxLength: 9,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
              decoration: InputDecoration(
                counterText: '',
                border: InputBorder.none,
                contentPadding: const EdgeInsets.symmetric(horizontal: 16),
                hintText: '7 9123 4567',
                hintStyle: AppText.phone.copyWith(color: AppColors.ink30),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
