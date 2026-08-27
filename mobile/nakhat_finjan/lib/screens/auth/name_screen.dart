import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text.dart';
import '../../theme/app_theme.dart';
import '../../widgets/app_buttons.dart';
import '../main_shell.dart';

/// «شو اسمك؟» — first run only.
///
/// This screen exists because of a backend rule, not a product preference:
/// Customer.FullName is NOT NULL, so a phone number with no customer yet cannot
/// be created without a name. The token exchange is sent once with no name; a
/// NAME_REQUIRED back is what brings the customer here, and the exchange is then
/// repeated with what they type. A returning customer never sees it — the name
/// is ignored for a phone that already has an account.
class NameScreen extends StatefulWidget {
  const NameScreen({super.key});

  @override
  State<NameScreen> createState() => _NameScreenState();
}

class _NameScreenState extends State<NameScreen> {
  final TextEditingController _controller = TextEditingController();

  /// The backend caps FullName at 100 characters.
  static const int _maxLength = 100;

  bool get _isValid => _controller.text.trim().isNotEmpty;

  @override
  void initState() {
    super.initState();
    _controller.addListener(() => setState(() {}));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() {
    // TODO(auth): repeat AuthProvider.signIn with fullName: _controller.text,
    // and only navigate once it returns true.
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const MainShell()),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
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
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: IconButton(
                  onPressed: () => Navigator.of(context).maybePop(),
                  icon: const Icon(
                    Icons.arrow_forward,
                    size: 24,
                    color: AppColors.ink,
                  ),
                  tooltip: 'رجوع',
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(
                    minWidth: 48,
                    minHeight: 48,
                  ),
                ),
              ),
              const SizedBox(height: 22),
              Text('شو اسمك؟', style: AppText.nameTitle),
              const SizedBox(height: 30),
              _NameField(controller: _controller, maxLength: _maxLength),
              const Spacer(),
              PrimaryButton(
                label: 'متابعة',
                onPressed: _isValid ? _submit : null,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// A single filled field, gold-bordered because it is the only thing on the
/// screen and carries the focus from the moment it opens.
class _NameField extends StatelessWidget {
  const _NameField({required this.controller, required this.maxLength});

  final TextEditingController controller;
  final int maxLength;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: AppMetrics.fieldHeight,
      child: TextField(
        controller: controller,
        autofocus: true,
        textCapitalization: TextCapitalization.words,
        maxLength: maxLength,
        style: AppText.rowLabel.copyWith(fontSize: 17),
        decoration: InputDecoration(
          counterText: '',
          filled: true,
          fillColor: AppColors.surface,
          contentPadding: const EdgeInsets.symmetric(horizontal: 18),
          enabledBorder: _border(AppColors.goldDeep),
          focusedBorder: _border(AppColors.goldDeep),
          border: _border(AppColors.goldDeep),
        ),
      ),
    );
  }

  OutlineInputBorder _border(Color color) => OutlineInputBorder(
    borderRadius: BorderRadius.circular(AppMetrics.radiusField),
    borderSide: BorderSide(color: color, width: 1.5),
  );
}
