import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text.dart';
import '../../theme/app_theme.dart';
import '../../widgets/app_buttons.dart';
import '../../widgets/surfaces.dart';

/// «كيف تكسب نقاط؟» — the shop's loyalty rules, in the customer's words.
///
/// Every figure on this screen is read off the backend rather than written from
/// memory: the earn rate, the redemption rate and the redemption minimum all
/// live in `backend/CoffeeLoyalty.Api/Constants/LoyaltyConstants.cs`, and the
/// two behaviours the copy describes — that points are granted on cash paid
/// only, and that the fraction is floored — come from
/// `OrderService.ValidateRedemptionAndEarn`. If those constants change, this
/// copy is wrong and has to change with them.
class HowToEarnPointsScreen extends StatelessWidget {
  const HowToEarnPointsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const _InfoScaffold(
      title: 'كيف تكسب نقاط؟',
      children: [
        _Lead('أهلاً فيك ببرنامج نقاط نكهة فنجان ☕'),
        _Body(
          'كل فنجان قهوة بتشربه معنا بيقرّبك خطوة من مكافأة. برنامج النقاط '
          'طريقتنا نقولّك "شكراً" على إنك من زبايننا الدائمين — كل ما اشتريت '
          'أكثر، جمّعت نقاط أكثر، والنقاط بترجعلك مكافآت.',
        ),

        _Section(
          title: 'شو يعني برنامج النقاط؟',
          children: [
            _Body(
              'نظام بسيط: مقابل مشترياتك من نكهة فنجان بتجمّع نقاط بحسابك داخل '
              'التطبيق. النقاط بتتخزّن باسمك، بتشوفها بأي وقت، ولما توصل لعدد '
              'معيّن بتستبدلها بمكافأة. ما في كروت ورقية تضيع ولا أختام — كل '
              'إشي محفوظ برقم تلفونك.',
            ),
          ],
        ),

        _Section(
          title: 'كيف تبدأ؟ (٣ خطوات)',
          children: [
            _Step(
              '١',
              'سجّل الدخول برقم تلفونك — بيوصلك رمز تحقق (OTP) برسالة نصية.',
            ),
            _Step('٢', 'أدخل الرمز — عشان نتأكد إنه رقمك.'),
            _Step(
              '٣',
              'اكتب اسمك — أول مرة بس، عشان نرحّب فيك ونربط نقاطك بحسابك.',
            ),
            _Body(
              'بعد هيك حسابك جاهز، ونقاطك بتبدأ تتجمّع من أول عملية شراء.',
            ),
          ],
        ),

        _Section(
          title: 'كيف تكسب النقاط؟',
          children: [
            _Body('النقاط بتيجي من الشراء من نكهة فنجان.'),
            _Bullet(
              'كل ما اشتريت، وبعد ما نسجّل عمليتك، بتنضاف نقاط على رصيدك.',
            ),
            // ⟨FROM_BACKEND: real earning rate⟩ — LoyaltyConstants.PointsPerDinar = 3,
            // applied in OrderService.ValidateRedemptionAndEarn as
            // `(int)decimal.Floor(cashPaid * LoyaltyConstants.PointsPerDinar)`,
            // where `cashPaid = total - redemptionValue`. Hence: on cash only,
            // and floored.
            _Bullet(
              'المعدّل: ٣ نقاط عن كل دينار بتدفعه نقداً. النقاط بتتحسب على '
              'المبلغ المدفوع نقداً بس — الجزء الي بتغطّيه من نقاطك ما بيكسّبك '
              'نقاط جديدة. والكسور بتتقطع لتحت، يعني ٣.٧٥ دينار نقداً = ١١ نقطة.',
            ),
            _Bullet(
              'النقاط بتنضاف تلقائياً بعد ما الكاشير يسجّل الطلب — مش لازم '
              'تعمل إشي.',
            ),
          ],
        ),

        _Section(
          title: 'وين بتنسجّل مشترياتك؟',
          children: [
            _Body(
              'كل عملية بتظهرلك بصفحة "مشترياتي": تاريخ العملية، قيمتها، وكم '
              'نقطة كسبت منها. راجعها بنفسك وتأكد إنه كل عملية أخذت نقاطها.',
            ),
          ],
        ),

        _Section(
          title: 'وين بشوف رصيد نقاطي؟',
          children: [
            _Body(
              'رصيدك دايماً ظاهر على الصفحة الرئيسية أول ما تفتح التطبيق: '
              'مجموع نقاطك، وكم باقي عشان توصل للمكافأة الجاية.',
            ),
          ],
        ),

        _Section(
          title: 'كيف بستبدل نقاطي؟',
          children: [
            _Body('لما توصل للعدد المطلوب بتستبدل نقاطك بمكافأة عند المحل.'),
            // ⟨FROM_BACKEND: real redemption rule⟩ — LoyaltyConstants.RedeemRate = 100
            // (points per 1 JOD of discount) and MinRedeemPoints = 200 (decision 43,
            // lowered from 250), enforced in OrderService.ValidateRedemptionAndEarn:
            // it throws RedeemBelowMinimum under 200, and RedeemExceedsTotal when
            // `(decimal)pointsRedeemed / RedeemRate > total`.
            _Bullet(
              'المكافأة: كل ١٠٠ نقطة = ١ دينار خصم على طلبك. أقل استبدال ٢٠٠ '
              'نقطة (يعني ٢ دينار)، وما بتقدر تستبدل نقاط بقيمة أكبر من قيمة '
              'الطلب نفسه.',
            ),
            // ⟨FROM_BACKEND: redemption flow⟩ — there is no customer-side redeem
            // endpoint. Redemption is a field on CreateOrderRequest
            // (`PointsRedeemed`), and OrdersController is
            // [Authorize(Policy = RoleNames.Cashier)] — so it can only happen at
            // the till, on the order being rung up.
            _Bullet(
              'طريقة الاستبدال: قول للكاشير وقت الدفع إنك بدك تستبدل نقاطك، '
              'وهو بيخصمها من طلبك. ما في زر استبدال جوّا التطبيق — الاستبدال '
              'بصير عند الكاشير وقت الشراء.',
            ),
          ],
        ),

        _Section(
          title: 'نصائح تجمّع فيها نقاط أسرع',
          children: [
            _Bullet(
              'خلّي رقمك مسجّل قبل الدفع عشان الكاشير يربط العملية فيك.',
            ),
            _Bullet('راجع "مشترياتي" بعد كل زيارة وتأكد إنه نقاطك انضافت.'),
          ],
        ),

        _Section(
          title: 'أسئلة شائعة',
          children: [
            _Faq(
              'نسيت أسجّل نقاطي؟',
              'النقاط بترتبط بعمليتك وقت الشراء، فتأكد رقمك مسجّل قبل الدفع.',
            ),
            _Faq(
              'غيّرت تلفوني؟',
              'نقاطك مربوطة برقمك، سجّل دخول بنفس الرقم وبترجعلك.',
            ),
            _Faq(
              'في مشكلة برصيدي؟',
              'راجع "مشترياتي" أول، وإذا ظلّت تواصل معنا من صفحة "تواصل معنا".',
            ),
          ],
        ),
      ],
    );
  }
}

/// «تواصل معنا» — the shop's phone, address, hours and social links.
///
/// Every value here is confirmed by the client. The actions are the point of
/// the screen: a phone number the customer has to copy by hand is a phone
/// number they will not call, so each one is a real button that hands off to
/// the dialer, WhatsApp, Maps or the browser.
class ContactUsScreen extends StatelessWidget {
  const ContactUsScreen({super.key});

  /// The national form, as the shop writes it and as the dialer wants it.
  static const String _phone = '0798912275';

  /// WhatsApp keys on E.164 without the `+`, so this is the same number in the
  /// only shape `wa.me` accepts — not a second number.
  static final Uri _whatsApp = Uri.parse('https://wa.me/962798912275');

  static final Uri _dialer = Uri(scheme: 'tel', path: _phone);

  static final Uri _maps = Uri.parse(
    'https://maps.google.com/?q=32.025129,36.0585884',
  );

  static final Uri _facebook = Uri.parse(
    'https://www.facebook.com/nakhetfengan.coffe.and.chocolate/?locale=ar_AR',
  );

  static final Uri _instagram = Uri.parse(
    'https://www.instagram.com/nakhet.fengan.coffee/',
  );

  @override
  Widget build(BuildContext context) {
    return _InfoScaffold(
      title: 'تواصل معنا',
      children: [
        const _Lead('نكهة فنجان — إحنا بخدمتك'),
        const _Body(
          'سؤال، ملاحظة، أو استفسار عن نقاطك؟ تواصل معنا بأي طريقة تناسبك.',
        ),

        _Section(
          title: 'اتصل فينا',
          children: [
            const _LabelledValue(
              label: 'هاتف / واتساب',
              value: _phone,
              isLatin: true,
            ),
            const SizedBox(height: 14),
            _ActionButton(
              label: 'اتصال',
              icon: Icons.call_outlined,
              // The dialer is not a browser: platformDefault is what routes a
              // `tel:` intent to it. Forcing externalApplication here fails on
              // devices that expose no "application" for the scheme.
              onPressed: () => _open(context, _dialer, external: false),
            ),
            const SizedBox(height: 10),
            _ActionButton(
              label: 'واتساب',
              icon: Icons.chat_outlined,
              onPressed: () => _open(context, _whatsApp),
            ),
          ],
        ),

        _Section(
          title: 'موقعنا',
          children: [
            const _LabelledValue(
              label: 'العنوان',
              value: 'الرصيفة – طريق الياجوز، الزرقاء، الأردن',
            ),
            const SizedBox(height: 14),
            _ActionButton(
              label: 'الموقع على الخريطة',
              icon: Icons.location_on_outlined,
              onPressed: () => _open(context, _maps),
            ),
          ],
        ),

        const _Section(
          title: 'ساعات العمل',
          children: [
            _LabelledValue(
              label: 'يومياً',
              value: '١٠:٠٠ صباحاً – ١١:٠٠ مساءً',
            ),
          ],
        ),

        _Section(
          title: 'تابعنا',
          children: [
            _ActionButton(
              label: 'فيسبوك',
              icon: Icons.public,
              onPressed: () => _open(context, _facebook),
            ),
            const SizedBox(height: 10),
            _ActionButton(
              label: 'إنستغرام',
              icon: Icons.camera_alt_outlined,
              onPressed: () => _open(context, _instagram),
            ),
          ],
        ),
      ],
    );
  }

  /// Hands [uri] to the platform, and says so quietly when that fails.
  ///
  /// Same shape as the credits link in the settings screen, and for the same
  /// reason: launching can fail for causes the app cannot check in advance — no
  /// dialer, WhatsApp not installed, an intent refused by policy — and none of
  /// them deserve an error dialog. `launchUrl` also throws rather than returning
  /// false on some platforms, hence the catch as well as the check.
  static Future<void> _open(
    BuildContext context,
    Uri uri, {
    bool external = true,
  }) async {
    var launched = false;
    try {
      launched = await launchUrl(
        uri,
        mode: external
            ? LaunchMode.externalApplication
            : LaunchMode.platformDefault,
      );
    } on PlatformException {
      launched = false;
    }

    if (launched || !context.mounted) return;

    ScaffoldMessenger.of(
      context,
    ).showSnackBar(const SnackBar(content: Text('ما قدرنا نفتح الرابط')));
  }
}

/// The shared frame: a back arrow, the screen title, and a scrolling body.
///
/// Scrolling is not optional here — the points copy is far taller than a small
/// phone, and a Column alone would overflow rather than scroll.
class _InfoScaffold extends StatelessWidget {
  const _InfoScaffold({required this.title, required this.children});

  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: AppMetrics.screenPadding,
              ),
              child: Align(
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
            ),
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(
                  AppMetrics.screenPadding,
                  12,
                  AppMetrics.screenPadding,
                  32,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(title, style: AppText.screenTitle),
                    const SizedBox(height: 16),
                    ...children,
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// The one line that opens a screen, a notch darker than the body under it.
class _Lead extends StatelessWidget {
  const _Lead(this.text);

  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Text(
        text,
        style: AppText.subtitle.copyWith(
          color: AppColors.ink75,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}

/// A titled block. The title sits outside the card, the way the settings groups
/// do, so a long screen reads as a list of sections rather than one slab.
class _Section extends StatelessWidget {
  const _Section({required this.title, required this.children});

  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 22),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(4, 0, 4, 10),
            child: Text(title, style: AppText.sectionTitle),
          ),
          AppCard(
            padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: children,
            ),
          ),
        ],
      ),
    );
  }
}

/// A paragraph.
class _Body extends StatelessWidget {
  const _Body(this.text);

  final String text;

  @override
  Widget build(BuildContext context) =>
      Text(text, style: AppText.subtitle.copyWith(fontSize: 14.5));
}

/// A dashed line in a list, with the dash hung outside the text block so a
/// wrapped line stays aligned under the first one rather than under the dash.
class _Bullet extends StatelessWidget {
  const _Bullet(this.text);

  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.only(top: 7),
            child: Container(
              width: 5,
              height: 5,
              decoration: const BoxDecoration(
                color: AppColors.caramel,
                shape: BoxShape.circle,
              ),
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(text, style: AppText.subtitle.copyWith(fontSize: 14.5)),
          ),
        ],
      ),
    );
  }
}

/// A numbered step. The number carries the brand caramel so the three steps
/// read as a sequence at a glance, not as three more bullets.
class _Step extends StatelessWidget {
  const _Step(this.number, this.text);

  final String number;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 26,
            height: 26,
            alignment: Alignment.center,
            decoration: const BoxDecoration(
              color: AppColors.goldTint,
              shape: BoxShape.circle,
            ),
            child: Text(
              number,
              style: AppText.rowLabelStrong.copyWith(
                fontSize: 13,
                color: AppColors.caramelDark,
              ),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.only(top: 3),
              child: Text(
                text,
                style: AppText.subtitle.copyWith(fontSize: 14.5),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// A question with its answer under it.
class _Faq extends StatelessWidget {
  const _Faq(this.question, this.answer);

  final String question;
  final String answer;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(question, style: AppText.rowLabelStrong),
          const SizedBox(height: 4),
          Text(answer, style: AppText.subtitle.copyWith(fontSize: 14.5)),
        ],
      ),
    );
  }
}

/// A caption over a value — the contact screen's equivalent of a settings row,
/// stacked rather than side by side because an address does not fit on one
/// line beside its label.
class _LabelledValue extends StatelessWidget {
  const _LabelledValue({
    required this.label,
    required this.value,
    this.isLatin = false,
  });

  final String label;
  final String value;

  /// A phone number is Latin digits and must not be reordered by the RTL
  /// paragraph around it.
  final bool isLatin;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: AppText.groupLabel),
        const SizedBox(height: 6),
        Text(
          value,
          style: isLatin
              ? AppText.rowLabelStrong.copyWith(
                  fontFamily: AppText.latin,
                  fontSize: 17,
                )
              : AppText.subtitle.copyWith(
                  fontSize: 14.5,
                  color: AppColors.ink75,
                ),
          textDirection: isLatin ? TextDirection.ltr : null,
        ),
      ],
    );
  }
}

/// One outbound action. [SecondaryButton] is the app's existing outline button —
/// the same one the sign-out sheet and «إلغاء» use — so these read as the app's
/// buttons rather than a new control invented for this screen.
class _ActionButton extends StatelessWidget {
  const _ActionButton({
    required this.label,
    required this.icon,
    required this.onPressed,
  });

  final String label;
  final IconData icon;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SecondaryButton(
      label: label,
      icon: icon,
      color: AppColors.caramel,
      onPressed: onPressed,
    );
  }
}
