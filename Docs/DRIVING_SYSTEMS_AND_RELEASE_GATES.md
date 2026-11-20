# Driving systems and release gates

## Runtime systems connected in source

- Six physical WheelColliders: two steerable front wheels and four driven/braked rear wheels.
- Automatic six-speed forward gearbox with Park, Reverse, Neutral and Drive selection.
- Speed-sensitive Ackermann steering and anti-roll suspension force on the front and rear axles.
- Engine start interlocks for stopped vehicle, closed passenger door, fastened driver belt, available fuel and Park/Neutral.
- Drive torque interlock when the passenger door is open or driver belt is unfastened.
- Service braking, parking brake, air-pressure consumption/recovery and basic wheel-slip ABS modulation.
- Low-air spring-brake response on the rear axles.
- Functional headlamps, high beam, turn indicators, hazard flashers, tail lamps, brake lamps and reverse lamps.
- Animated passenger entrance door and paired windshield wipers.
- Three camera views: cockpit, rear chase and front exterior.
- Vector-drawn touch-control icons, active-state colors, gear selector and mobile safe-area layout.
- Landscape-only orientation and 30 FPS target on all graphics presets; actual device performance is not measured yet.
- Functional cabin lighting and air-conditioning temperature response; climate accessory load affects engine torque, fuel consumption and engine sound.
- Local company save/load for XOF balance, level, owned coaches, starting coach and finance history; financial mutations save automatically and app suspension/exit also saves.
- Curved 4.32 km playable starter corridor with asphalt mesh collision, shoulders, road markings, direction signs, roadside markers and batched savannah vegetation.
- Route planner connected to the saved company and icon-only HUD; a prepared itinerary and estimated distance appear in the driving HUD.
- Passenger manifest panel for ticket reservations, seat assignment, luggage-weight checks, company ticket income, and manual stop-by-stop boarding/alighting processing.
- Roadside diesel station with a trigger area; refuelling requires the engine off, a stationary coach and the parking brake, and charges the company account in XOF.

## Remaining implementation before release

1. Replace the starter corridor with route-specific geometry for all selected cities, including accurate road lengths, junctions, towns, terminals, rest areas, fuel stations and border checkpoints. The current 4.32 km corridor is a playable starter environment, not a complete real-world intercity route.
2. Connect trip progression to the actual road route, automatic stop arrival, and international border checks. The route planner and passenger manifest currently prepare bookings and allow manual stop processing; they do not yet simulate the complete itinerary.
3. Add passenger 3D models, seat occupancy visuals, luggage loading animations and onboard service interactions.
4. Add traffic AI, varied road users, day/night cycle, weather, rain response and route events.
5. Add original production-quality exterior/interior models for every coach category and the remaining coach-specific equipment.
6. Add original or properly licensed vehicle-brand names, logos, sounds and designs. Fictional brands remain in use until permission is obtained.
7. Calibrate vehicle physics with measured vehicle mass, wheelbase, axle loads, tire data and braking data. Current values are game tuning, not manufacturer-validated vehicle data.
8. Compile in the pinned Unity Editor, fix all compiler/runtime errors, create an Android ARM64 build, and install/test it on the target Android device.
9. Complete performance, thermal, battery, memory, touch-layout, accessibility, privacy/legal and store-release checks.

## Android build access

The repository contains a manual GitHub Actions Android build workflow. The Unity license credentials must be configured by the repository owner in GitHub Actions secrets before the workflow can run. Never commit license data to the repository or share it in chat.

## Verification limits

Source-level structural checks do not prove that the project compiles or runs. No APK should be described as ready until the pinned Unity Editor compiles the project, the Android build succeeds, and the APK has been installed and tested on a physical device.
