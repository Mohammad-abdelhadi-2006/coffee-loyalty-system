import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/foundation.dart';

/// Firebase phone verification, wrapped for the Jordanian flow.
///
/// This is the first half of signing in: Firebase proves the customer holds the
/// phone number and hands back an ID token, which `AuthProvider.signIn` then
/// exchanges for this API's own JWT. Nothing here talks to the backend, and
/// nothing here stores anything — the Firebase session and our session are
/// separate, which is also why signing out means signing out of both.
class PhoneAuthService {
  PhoneAuthService({FirebaseAuth? auth}) : _injected = auth;

  /// Jordan. The login screen collects the 9 national digits and this supplies
  /// the rest, so a customer never types a country code and can never get it
  /// wrong.
  static const String countryCode = '+962';

  /// How long Firebase may spend on SMS auto-retrieval before giving up and
  /// letting the customer type the code themselves.
  static const Duration verificationTimeout = Duration(seconds: 60);

  final FirebaseAuth? _injected;

  /// Resolved on first use rather than in the constructor.
  ///
  /// `FirebaseAuth.instance` throws if no Firebase app has been initialised,
  /// and this service is constructed in a screen's `initState` — so eagerly
  /// resolving it would take the login screen down at build time on any build
  /// whose Firebase config is missing, instead of when someone actually asks to
  /// send a code. It also keeps the screen constructible in a widget test with
  /// no Firebase at all.
  FirebaseAuth get _auth => _injected ?? FirebaseAuth.instance;

  /// Builds the E.164 number the whole flow is keyed on.
  ///
  /// Any spaces or dashes the customer typed are stripped: Firebase rejects a
  /// number that is not strictly E.164, and a stray space is not worth an error
  /// the customer cannot make sense of.
  static String toE164(String nationalNumber) {
    final digits = nationalNumber.replaceAll(RegExp(r'\D'), '');
    return '$countryCode$digits';
  }

  /// Starts verification for [nationalNumber] (9 digits, no country code).
  ///
  /// Firebase reports through callbacks rather than a return value because the
  /// flow has four possible outcomes and two of them can arrive long after the
  /// call returns:
  ///
  /// - [onCodeSent] — the SMS is on its way; hold the verificationId, it is
  ///   what pairs with the digits the customer types.
  /// - [onAutoVerified] — Android retrieved and verified the code by itself.
  ///   The customer never sees the OTP screen's keyboard in this case, so the
  ///   credential is already good and sign-in can proceed immediately.
  /// - [onFailed] — the number was malformed, the quota is spent, or the app
  ///   is not configured. Surfaced as Arabic text ready to render.
  /// - [onAutoRetrievalTimeout] — auto-retrieval gave up. Not an error: it is
  ///   the normal path on most devices, and simply means the customer types the
  ///   code by hand.
  ///
  /// **Only the first of these to arrive is forwarded.** Firebase keeps calling
  /// back for the whole life of the verification, not just until it succeeds:
  /// after `verificationCompleted` or `codeSent` has already settled the flow,
  /// Android's SMS Retriever goes on waiting for a message that auto-retrieval
  /// already consumed, and reports its eventual give-up through
  /// `verificationFailed`. Forwarding that would overwrite a success with an
  /// error — and it is a success, the user is signed in by then. So the first
  /// outcome wins and the rest are dropped.
  Future<void> sendCode({
    required String nationalNumber,
    required void Function(String verificationId, int? resendToken) onCodeSent,
    required void Function(PhoneAuthCredential credential) onAutoVerified,
    required void Function(String message) onFailed,
    void Function(String verificationId)? onAutoRetrievalTimeout,
    int? resendToken,
  }) async {
    // Scoped to this call, so a resend gets a fresh latch of its own.
    var isSettled = false;

    await _auth.verifyPhoneNumber(
      phoneNumber: toE164(nationalNumber),
      timeout: verificationTimeout,
      forceResendingToken: resendToken,

      verificationCompleted: (credential) {
        if (isSettled) return;
        isSettled = true;
        onAutoVerified(credential);
      },

      codeSent: (verificationId, forceResendingToken) {
        if (isSettled) return;
        isSettled = true;
        onCodeSent(verificationId, forceResendingToken);
      },

      verificationFailed: (e) {
        if (isSettled) {
          // The common one on an emulator: the SMS Retriever timing out long
          // after auto-verification already signed the user in. Not a failure
          // of anything the customer did, and nothing to show them.
          return;
        }
        isSettled = true;
        debugPrint('Phone verification failed: ${e.code} — ${e.message}');
        onFailed(_messageFor(e));
      },

      codeAutoRetrievalTimeout: (verificationId) {
        // Never an error, and never settles the flow: it means only that the
        // customer will type the code by hand, which is the normal path on most
        // devices. The OTP screen is already open by now.
        onAutoRetrievalTimeout?.call(verificationId);
      },
    );
  }

  /// Exchanges the typed code for a Firebase ID token.
  ///
  /// Returns the token that `AuthProvider.signIn` sends to the backend, or
  /// throws [FirebaseAuthException] when the code is wrong or expired — the OTP
  /// screen maps that to its field error.
  Future<String> verifyCode({
    required String verificationId,
    required String smsCode,
  }) {
    final credential = PhoneAuthProvider.credential(
      verificationId: verificationId,
      smsCode: smsCode,
    );
    return signInWithCredential(credential);
  }

  /// Signs in with an already-built credential and returns the ID token.
  ///
  /// Split out from [verifyCode] because auto-retrieval hands back a finished
  /// credential rather than a code, and both paths must end the same way.
  Future<String> signInWithCredential(PhoneAuthCredential credential) async {
    final result = await _auth.signInWithCredential(credential);
    final token = await result.user?.getIdToken();

    if (token == null || token.isEmpty) {
      // Firebase accepted the credential but gave nothing to send onward, so
      // there is no point continuing to the exchange.
      throw FirebaseAuthException(
        code: 'null-id-token',
        message: 'Sign-in succeeded but no ID token was returned.',
      );
    }

    return token;
  }

  /// Ends the Firebase half of the session.
  ///
  /// Our JWT is dropped separately by `AuthProvider.signOut` — neither call
  /// affects the other.
  Future<void> signOut() => _auth.signOut();

  /// Arabic display text for a verification failure.
  ///
  /// Only the codes a customer can actually cause are worded specifically. The
  /// rest fall through to a generic line with the raw code appended in
  /// parentheses: a customer cannot act on `app-not-authorized`, but they can
  /// read it out over the phone, and without it an unrecognised failure is
  /// indistinguishable from every other unrecognised failure on screen.
  static String _messageFor(FirebaseAuthException e) => switch (e.code) {
    'invalid-phone-number' => 'رقم الهاتف غير صحيح',
    'too-many-requests' => 'حاولت كثير. جرّب مرة ثانية بعد شوي',
    'quota-exceeded' => 'الخدمة مشغولة حالياً، جرّب بعد شوي',
    'network-request-failed' =>
      'تعذّر الاتصال بالخادم، تحقّق من الإنترنت وحاول مجدداً',
    _ => 'تعذّر إرسال الرمز، حاول مجدداً (${e.code})',
  };

  /// Arabic display text for a failed code check.
  ///
  /// Public because the OTP screen catches the exception from [verifyCode]
  /// itself and needs the same wording rules.
  static String messageForCodeFailure(FirebaseAuthException e) =>
      switch (e.code) {
        'invalid-verification-code' => 'الرمز غير صحيح، حاول مرة ثانية',
        'session-expired' => 'انتهت صلاحية الرمز، اطلب رمزاً جديداً',
        'too-many-requests' => 'حاولت كثير. جرّب مرة ثانية بعد شوي',
        'network-request-failed' =>
          'تعذّر الاتصال بالخادم، تحقّق من الإنترنت وحاول مجدداً',
        _ => 'تعذّر التحقق من الرمز، حاول مجدداً',
      };
}
