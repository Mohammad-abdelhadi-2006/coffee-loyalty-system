# Firebase setup (required before the app will run)

Two generated files are **deliberately not in the repo** — both carry API keys:

| File | Why it's ignored |
|---|---|
| `lib/firebase_options.dart` | Contains the Web/Android API keys |
| `android/app/google-services.json` | Same keys, read by the Google Services Gradle plugin at build time |

Without them the app still starts, but shows a "Firebase is not configured"
screen instead of the splash, and phone sign-in cannot run.

## Generating them

```bash
dart pub global activate flutterfire_cli   # once per machine
cd mobile/nakhat_finjan
flutterfire configure --project=nakhat-finjan
```

Select **Android** when prompted. This writes both files. The Android
application ID must be `com.nakhatfinjan.app` — it has to match `applicationId`
in `android/app/build.gradle.kts`, or Firebase rejects the app at sign-in.

## Also required for phone auth

Phone verification fails with `app-not-authorized` unless your signing
certificate is registered on the Firebase Android app:

```bash
cd mobile/nakhat_finjan/android
./gradlew signingReport
```

Copy the **SHA-1** and **SHA-256** of the `debug` variant into
Firebase console → Project settings → Your apps → Android → *Add fingerprint*.
Every developer's debug keystore is different, so each of you has to add your
own.

To develop without spending SMS quota, add test numbers under
Authentication → Sign-in method → Phone → *Phone numbers for testing*.
