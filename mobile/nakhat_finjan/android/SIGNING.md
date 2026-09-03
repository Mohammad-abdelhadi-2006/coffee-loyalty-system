# Release signing

The app currently has **no release key**, so `flutter build apk --release`
signs with the debug key and prints a loud warning. A debug-signed APK must not
be published: the debug keystore is per-machine, so nobody who installs it can
ever be sent an update, and Firebase phone-auth only works for a fingerprint
registered by hand.

## Create the key (once, ~30 seconds)

```bash
cd mobile/nakhat_finjan/android
keytool -genkeypair -v \
  -keystore app/nakhat-finjan-release.jks \
  -storetype JKS -keyalg RSA -keysize 2048 -validity 10950 \
  -alias nakhat
```

It asks for a password twice, then some name/organisation fields. Use the same
password for both prompts to keep `key.properties` simple.

## Point the build at it

Create `mobile/nakhat_finjan/android/key.properties`:

```properties
storePassword=THE_PASSWORD_YOU_CHOSE
keyPassword=THE_SAME_PASSWORD
keyAlias=nakhat
storeFile=nakhat-finjan-release.jks
```

Both this file and the `.jks` are gitignored. Nothing else needs changing —
`build.gradle.kts` picks them up automatically.

## Then

```bash
cd mobile/nakhat_finjan
flutter build apk --release

# confirm it is NOT the debug cert — Owner must say Nakhat Finjan, not Android Debug
keytool -printcert -jarfile build/app/outputs/flutter-apk/app-release.apk
```

Register the printed **SHA-1 and SHA-256** in the Firebase console
(Project settings → Your apps → Android → Add fingerprint), or phone sign-in
fails on every device.

## Back it up

If `nakhat-finjan-release.jks` or its password is lost, the app can never be
updated again — a new key is a different app to Android. Keep a copy somewhere
that is not this laptop.
