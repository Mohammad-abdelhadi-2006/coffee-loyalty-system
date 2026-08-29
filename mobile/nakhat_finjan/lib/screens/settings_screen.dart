import 'package:firebase_auth/firebase_auth.dart' hide AuthProvider;
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';

import '../providers/auth_provider.dart';
import '../providers/customer_provider.dart';
import '../theme/app_colors.dart';
import '../theme/app_text.dart';
import '../theme/app_theme.dart';
import '../widgets/app_buttons.dart';
import '../widgets/surfaces.dart';
import 'auth/login_screen.dart';
import 'info/info_screens.dart';

/// View-only settings with exactly one real action: signing out.
///
/// The design omits a notifications row on purpose, and nothing here is
/// editable — the name and phone are shown as facts, not fields, because
/// changing either is a cashier's job in the dashboard.
class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key, this.appVersion = '1.0.0'});

  /// Hardcoded rather than read from the package info: adding package_info_plus
  /// for one string is not worth a dependency, and this has to be bumped
  /// alongside pubspec's `version:` at release time either way.
  final String appVersion;

  /// Shown while the profile is still loading, and if it fails. An em dash says
  /// "not known yet" without pretending the field is empty.
  static const String _unknown = '—';

  Future<void> _confirmSignOut(BuildContext context) async {
    final confirmed = await showModalBottomSheet<bool>(
      context: context,
      backgroundColor: Colors.transparent,
      barrierColor: AppColors.ink.withValues(alpha: 0.4),
      builder: (sheetContext) => const _SignOutSheet(),
    );

    if (confirmed != true || !context.mounted) return;

    final auth = context.read<AuthProvider>();
    final customers = context.read<CustomerProvider>();

    // Three separate things, and all three have to go. Our JWT, the cached
    // customer data — otherwise the next person to sign in on this device sees
    // the previous one's balance until the first load lands — and the Firebase
    // session, which is independent of ours and would otherwise let the next
    // sign-in skip the OTP entirely.
    await auth.signOut();
    customers.clear();

    try {
      await FirebaseAuth.instance.signOut();
    } on FirebaseAuthException catch (e) {
      // Already signed out locally, so this is not worth blocking on or
      // telling the customer about; the app is leaving this screen either way.
      debugPrint('Firebase sign-out failed: ${e.code}');
    }

    if (!context.mounted) return;

    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final profile = context.watch<CustomerProvider>().profile;

    return SafeArea(
      bottom: false,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Expanded(
            child: ListView(
              padding: const EdgeInsets.fromLTRB(
                AppMetrics.screenPadding,
                10,
                AppMetrics.screenPadding,
                0,
              ),
              children: [
                const Text('الإعدادات', style: AppText.screenTitle),

                const _GroupLabel('الحساب'),
                HairlineList(
                  children: [
                    AppRow(
                      label: 'الاسم',
                      trailing: Text(
                        profile?.fullName ?? _unknown,
                        style: AppText.rowValue,
                      ),
                    ),
                    AppRow(
                      label: 'رقم الهاتف',
                      trailing: Text(
                        profile?.phoneNumber ?? _unknown,
                        style: AppText.rowValue.copyWith(
                          fontFamily: AppText.latin,
                          fontSize: 14.5,
                        ),
                        textDirection: TextDirection.ltr,
                      ),
                    ),
                  ],
                ),

                const _GroupLabel('معلومات'),
                HairlineList(
                  children: [
                    AppRow(
                      label: 'كيف تكسب نقاط؟',
                      trailing: const _Chevron(),
                      onTap: () => Navigator.of(context).push(
                        MaterialPageRoute<void>(
                          builder: (_) => const HowToEarnPointsScreen(),
                        ),
                      ),
                    ),
                    AppRow(
                      label: 'تواصل معنا',
                      trailing: const _Chevron(),
                      onTap: () => Navigator.of(context).push(
                        MaterialPageRoute<void>(
                          builder: (_) => const ContactUsScreen(),
                        ),
                      ),
                    ),
                    AppRow(
                      label: 'زوروا موقعنا',
                      trailingIcon: Icons.north_east,
                      trailing: const _Chevron(),
                      // TODO(links): open the shop's site once the URL is
                      // known — launchUrl the way [_Credits] does.
                      onTap: () {},
                    ),
                    AppRow(
                      label: 'إصدار التطبيق',
                      labelStyle: AppText.rowLabel.copyWith(
                        color: AppColors.ink62,
                      ),
                      trailing: Text(
                        appVersion,
                        style: AppText.rowValue.copyWith(
                          fontFamily: AppText.latin,
                          fontSize: 14.5,
                          color: AppColors.ink42,
                        ),
                        textDirection: TextDirection.ltr,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 26),
                const _Credits(),
                const SizedBox(height: 10),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppMetrics.screenPadding,
              0,
              AppMetrics.screenPadding,
              18,
            ),
            child: SecondaryButton(
              label: 'تسجيل الخروج',
              icon: Icons.logout,
              color: AppColors.deduct,
              onPressed: () => _confirmSignOut(context),
            ),
          ),
        ],
      ),
    );
  }
}

class _GroupLabel extends StatelessWidget {
  const _GroupLabel(this.text);

  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(4, 26, 4, 10),
      child: Text(text, style: AppText.groupLabel),
    );
  }
}

/// The attribution under the last row. Centred, and set in the same muted
/// caption style the ledger timestamps use — it should read as a footer, never
/// compete with the rows above it.
///
/// The name itself is the link: it carries the brand caramel, the same colour
/// «زوروا موقعنا» uses for its outbound mark, while the «تطوير:» prefix stays
/// muted. Colour is the whole affordance here — no underline, no icon — which
/// keeps the footer quiet while still reading as tappable.
class _Credits extends StatelessWidget {
  const _Credits();

  static final Uri _linkedIn = Uri.parse(
    'https://www.linkedin.com/in/mohammad-abdelhadi-ab18603a1',
  );

  Future<void> _open(BuildContext context) async {
    // Launching can fail for reasons the app cannot check for in advance — no
    // browser installed, the intent refused, a device policy blocking it. None
    // of them are worth an error dialog on a credit line, so a failure is a
    // quiet notice and nothing more. launchUrl also throws rather than
    // returning false on some platforms, hence the catch as well as the check.
    var launched = false;
    try {
      launched = await launchUrl(
        _linkedIn,
        mode: LaunchMode.externalApplication,
      );
    } on PlatformException {
      launched = false;
    }

    if (launched || !context.mounted) return;

    ScaffoldMessenger.of(
      context,
    ).showSnackBar(const SnackBar(content: Text('ما قدرنا نفتح الرابط')));
  }

  @override
  Widget build(BuildContext context) {
    // A GestureDetector around the name rather than a TapGestureRecognizer on a
    // TextSpan: a recognizer built here would never be disposed, and this keeps
    // the tap target to the name alone while padding it out to something a
    // thumb can actually hit.
    // Wrap, not Row: the two pieces together are wider than a narrow phone, and a Row with
    // MainAxisSize.min neither shrinks nor breaks — it just runs off the edge and paints the
    // debug stripes. Wrap keeps them side by side wherever there is room and drops the name
    // onto its own centred line where there is not.
    return Center(
      child: Wrap(
        alignment: WrapAlignment.center,
        crossAxisAlignment: WrapCrossAlignment.center,
        children: [
          const Text('تم تطوير النظام بواسطة: ', style: AppText.rowMeta),
          GestureDetector(
            behavior: HitTestBehavior.opaque,
            onTap: () => _open(context),
            child: Padding(
              padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 2),
              child: Text(
                'محمد عبدالهادي',
                style: AppText.rowMeta.copyWith(
                  color: AppColors.caramel,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// The "goes somewhere" mark. [Icons.chevron_right] mirrors with the locale, so
/// in RTL it points left without any flipping here.
class _Chevron extends StatelessWidget {
  const _Chevron();

  @override
  Widget build(BuildContext context) {
    return const Icon(Icons.chevron_right, size: 20, color: AppColors.ink34);
  }
}

/// The only destructive step in the app, and the design gives it a full
/// confirmation sheet rather than a snackbar-with-undo: signing out is instant
/// and cannot be undone without another OTP round trip.
class _SignOutSheet extends StatelessWidget {
  const _SignOutSheet();

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.vertical(top: Radius.circular(26)),
      ),
      padding: EdgeInsets.fromLTRB(
        AppMetrics.screenPadding,
        28,
        AppMetrics.screenPadding,
        30 + MediaQuery.viewPaddingOf(context).bottom,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Center(
            child: Container(
              width: 38,
              height: 4,
              decoration: BoxDecoration(
                color: AppColors.ink.withValues(alpha: 0.16),
                borderRadius: BorderRadius.circular(999),
              ),
            ),
          ),
          const SizedBox(height: 22),
          Text(
            'متأكد؟',
            textAlign: TextAlign.center,
            style: AppText.authTitle.copyWith(fontSize: 22, height: 1.2),
          ),
          const SizedBox(height: 10),
          Text(
            'رح ترجع لشاشة تسجيل الدخول',
            textAlign: TextAlign.center,
            style: AppText.subtitle.copyWith(fontSize: 14.5),
          ),
          const SizedBox(height: 26),
          _DestructiveButton(
            label: 'تسجيل الخروج',
            onPressed: () => Navigator.of(context).pop(true),
          ),
          const SizedBox(height: 10),
          SecondaryButton(
            label: 'إلغاء',
            onPressed: () => Navigator.of(context).pop(false),
          ),
        ],
      ),
    );
  }
}

/// The one filled non-gold button in the app. It is filled for the same reason
/// the gold CTA is — it is the sheet's primary action — and brick because
/// confirming it destroys the session.
class _DestructiveButton extends StatelessWidget {
  const _DestructiveButton({required this.label, required this.onPressed});

  final String label;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: AppMetrics.buttonHeight,
      child: Material(
        color: AppColors.deduct,
        borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
        child: InkWell(
          onTap: onPressed,
          borderRadius: BorderRadius.circular(AppMetrics.radiusButton),
          child: Center(
            child: Text(
              label,
              style: AppText.primaryButton.copyWith(color: Colors.white),
            ),
          ),
        ),
      ),
    );
  }
}
