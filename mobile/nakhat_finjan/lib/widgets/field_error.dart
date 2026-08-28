import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text.dart';

/// The glyph-and-text line under a field that failed validation.
///
/// Used under the phone field and under the OTP boxes; the wording differs but
/// the treatment must not.
class FieldError extends StatelessWidget {
  const FieldError({super.key, required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 4),
      child: Row(
        children: [
          const Icon(Icons.error_outline, size: 15, color: AppColors.deduct),
          const SizedBox(width: 7),
          Flexible(child: Text(message, style: AppText.fieldError)),
        ],
      ),
    );
  }
}

/// The heavier boxed notice, for a state the customer cannot type their way out
/// of — the design uses it only for "too many attempts".
class BlockedNotice extends StatelessWidget {
  const BlockedNotice({super.key, required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
      decoration: BoxDecoration(
        color: AppColors.deductTint,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.deductTintBorder),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Padding(
            padding: EdgeInsets.only(top: 1),
            child: Icon(
              Icons.warning_amber_rounded,
              size: 20,
              color: AppColors.deduct,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              message,
              style: AppText.fieldError.copyWith(fontSize: 14, height: 1.7),
            ),
          ),
        ],
      ),
    );
  }
}
