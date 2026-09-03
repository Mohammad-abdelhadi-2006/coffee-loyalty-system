import java.util.Properties

// Release signing credentials. Deliberately outside the repository — the file
// and any .jks beside it are gitignored, because a signing key in version
// control is a signing key anyone can publish updates with.
//
// Create it once (see android/SIGNING.md) and every `flutter build apk
// --release` from then on is signed with the real key, with nothing further to
// change here.
val keystoreProperties = Properties().apply {
    val file = rootProject.file("key.properties")
    if (file.exists()) file.inputStream().use { load(it) }
}

val hasReleaseKeystore = keystoreProperties.getProperty("storeFile") != null

plugins {
    id("com.android.application")
    id("kotlin-android")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
    // Firebase. Must come after the Android plugin; it processes
    // android/app/google-services.json, which is not in the repo — drop it in
    // before building, or Firebase.initializeApp() fails at startup.
    id("com.google.gms.google-services")
}

android {
    namespace = "com.nakhatfinjan.app"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = "27.0.12077973"

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }

    kotlinOptions {
        jvmTarget = JavaVersion.VERSION_11.toString()
    }

    defaultConfig {
        // TODO: Specify your own unique Application ID (https://developer.android.com/studio/build/application-id.html).
        applicationId = "com.nakhatfinjan.app"
        // You can update the following values to match your application needs.
        // For more information, see: https://flutter.dev/to/review-gradle-config.
        minSdk = 23
        targetSdk = flutter.targetSdkVersion
        versionCode = flutter.versionCode
        versionName = flutter.versionName
    }

    signingConfigs {
        if (hasReleaseKeystore) {
            create("release") {
                keyAlias = keystoreProperties.getProperty("keyAlias")
                keyPassword = keystoreProperties.getProperty("keyPassword")
                storeFile = file(keystoreProperties.getProperty("storeFile"))
                storePassword = keystoreProperties.getProperty("storePassword")
            }
        }
    }

    buildTypes {
        release {
            if (hasReleaseKeystore) {
                signingConfig = signingConfigs.getByName("release")
            } else {
                // No key.properties present. Still builds, so `flutter run
                // --release` keeps working on a dev machine — but this APK must
                // never be published: the debug key is per-machine, so anyone
                // who installs it can never be sent an update, and Firebase
                // phone-auth only works for a fingerprint registered by hand.
                //
                // The warning is deliberately loud. A build that quietly signs
                // with the debug key and looks finished is how a debug-signed
                // APK ends up on a download page.
                logger.warn("")
                logger.warn("*********************************************************")
                logger.warn("  WARNING: release APK is being signed with the DEBUG key.")
                logger.warn("  android/key.properties is missing - see android/SIGNING.md")
                logger.warn("  DO NOT PUBLISH THIS BUILD.")
                logger.warn("*********************************************************")
                logger.warn("")
                signingConfig = signingConfigs.getByName("debug")
            }
        }
    }
}

flutter {
    source = "../.."
}
