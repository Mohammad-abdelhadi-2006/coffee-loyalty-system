/// Compile-time configuration for the app.
///
/// The backend's development ports come from
/// `backend/CoffeeLoyalty.Api/Properties/launchSettings.json`:
///   http  -> 5286
///   https -> 7154
class Config {
  const Config._();

  /// Base URL of the CoffeeLoyalty API. Every path in the services layer is
  /// relative to this (`/api/auth/firebase-login`, ...), so it must NOT end
  /// with a slash.
  ///
  /// Variants — swap this one line per environment:
  ///
  ///   Android emulator (host machine)  http://10.0.2.2:5286
  ///   iOS simulator / desktop / web    http://localhost:5286
  ///   Physical device on the same LAN  http://192.168.1.x:5286   <- your PC's IPv4
  ///   Production                       https://api.nakhatfinjan.com
  ///
  /// Notes:
  ///  - Run the API with the `http` profile (`dotnet run --launch-profile http`)
  ///    while pointing at an http:// URL. Under the `https` profile,
  ///    `UseHttpsRedirection` bounces plain-http calls to 7154, whose dev
  ///    certificate a device/emulator will not trust.
  ///  - A physical device cannot reach `localhost` or `10.0.2.2` — those are the
  ///    emulator's own loopback. Use the LAN IP and make sure the API listens on
  ///    it (`dotnet run --urls http://0.0.0.0:5286`).
  static const String baseUrl = 'http://10.0.2.2:5286';
}
