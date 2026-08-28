/// Body of `POST /api/auth/firebase-login`.
///
/// Mirrors `backend/CoffeeLoyalty.Api/Dtos/Auth/FirebaseLoginRequest.cs`.
/// JSON keys are camelCase — ASP.NET Core's default policy — and are
/// case-sensitive on the wire.
class FirebaseLoginRequest {
  const FirebaseLoginRequest({required this.firebaseIdToken, this.fullName});

  /// The Firebase ID token produced by the client after OTP verification
  /// (`firebase_auth`: `userCredential.user!.getIdToken()`).
  final String firebaseIdToken;

  /// Display name, used **only** when this login creates a new customer, and
  /// ignored when the phone number already belongs to one. Backend cap: 100
  /// characters.
  ///
  /// Send `null` on the first attempt: a 400 `NAME_REQUIRED` back means the
  /// phone has no customer yet and the app must collect a name and retry.
  final String? fullName;

  Map<String, dynamic> toJson() => {
    'firebaseIdToken': firebaseIdToken,
    // Omitted rather than sent as null; the backend treats both the same, but
    // this keeps the body identical to the contract's example.
    if (fullName != null) 'fullName': fullName,
  };
}
