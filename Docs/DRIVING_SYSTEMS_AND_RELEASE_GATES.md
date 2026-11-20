# Driving systems and release gates

## Runtime systems currently connected

- Six physical WheelColliders: two steerable front wheels and four driven/braked rear wheels.
- Automatic six-speed forward gearbox with Park, Reverse, Neutral and Drive selection.
- Engine start interlocks for stopped vehicle, closed passenger door, fastened driver belt, available fuel and Park/Neutral.
- Drive torque interlock when the passenger door is open or driver belt is unfastened.
- Service braking, parking brake, air-pressure consumption/recovery and a basic wheel-slip ABS modulation.
- Functional headlamps, high beam, turn indicators, hazard flashers, tail lamps, brake lamps and reverse lamps.
- Animated passenger entrance door and paired windshield wipers.
- Three camera views: cockpit, rear chase and front exterior.
- Vector-drawn touch-control icons, live active-state colors, gear selector and mobile safe-area layout.
- 30 FPS target across all graphics presets; final device performance remains unmeasured.
- Speed-sensitive steering with Ackermann front-wheel geometry and anti-roll force on the front and rear axles.
- Local company save/load for XOF balance, level, owned coaches, starting coach and finance history; financial mutations save automatically.
- Cabin air-conditioning temperature response, accessory engine load and increased fuel use while cooling.

## Important release gates not yet passed

1. Unity Editor compilation and play-mode validation.
2. Android ARM64 build and installation test on a physical device.
3. Physics calibration using measured mass, wheelbase, axle loads, tire data and braking data for each coach; current values are game tuning, not manufacturer-validated vehicle data.
4. Replacement of generated block geometry with finished original 3D coach, cockpit, road, scenery, passenger and traffic assets.
5. Replacement of procedural engine tone with original/licensed per-coach engine recordings and interior/exterior audio mixing.
6. Full route loading, AI traffic, passenger boarding, luggage, ticketing, company progression and save persistence integrated into the playable scene.
7. Performance, thermal, battery, memory, touch-layout and orientation tests on the target Android device.
8. Store-ready identity, privacy/legal pages, app signing and release packaging.

## Build access

The repository has a manual GitHub Actions Android build workflow. The Unity Personal/Plus/Pro license credentials must be configured by the repository owner in GitHub Actions secrets before that workflow can run. Never commit license data to the repository.

## Verification limits

Source-level structural checks are not a substitute for compiling the project in the pinned Unity Editor version. No APK should be described as ready until the Android workflow succeeds and the APK has been installed and tested.
