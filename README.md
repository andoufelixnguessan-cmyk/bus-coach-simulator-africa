# BUS & COACH SIMULATOR AFRICA

A realistic 3D long-distance coach driving and transport-company management simulator for Android, focused initially on West Africa.

## Product ambition

The target is a serious, immersive coach simulation experience. Bus Simulator: Ultimate is a quality and feature reference, not a source to copy. All game code, interfaces, routes, maps, branding, and vehicle assets must be original or properly licensed.

## Core pillars

1. **Realistic driving** — detailed 3D cockpit, steering, acceleration, braking, transmission, mirrors, indicators, camera views, road handling, and vehicle feedback.
2. **Passenger transport** — stations, ticketing, passenger boarding and alighting, luggage, schedules, fares, and passenger satisfaction.
3. **West African world** — believable roads, towns, villages, terminals, road signs, traffic, tolls, and regional scenery. Initial route candidates include Abidjan–Yamoussoukro and Abidjan–Bouaké.
4. **Company management** — acquire and maintain coaches, hire staff, manage fuel and operating costs, grow routes and terminals, and track revenue.
5. **Immersion** — day/night cycle, weather, traffic AI, engine and cabin audio, and optimized mobile graphics.
6. **Online features** — multiplayer and shared company features only after the offline simulation is stable.

## Technology direction

- Unity 3D with Universal Render Pipeline (URP).
- Android-first, with performance budgets and scalable graphics settings.
- Modular C# systems so driving, traffic, passengers, routes, and management can be tested independently.

## Development principles

- Build a playable vertical slice before expanding content.
- Prioritize vehicle handling, camera feel, cockpit usability, and stable frame rate.
- Use original or licensed 3D models, sounds, logos, and environments.
- Never advertise a feature as finished until it is implemented and tested on an Android device.

## Initial milestone

A single drivable coach, a polished third-person and cockpit camera, mobile controls, a small test road, basic traffic, and a reliable Android development build.

## Systems added in the repository

The codebase now includes early C# foundations for:
- the single-currency company economy and coach catalogue;
- optional onboard comfort equipment with compatibility checks;
- passenger bookings, seat assignment, luggage limits, ticket revenue and boarding states;
- trip preparation, mandatory safety checklist, departure-cost deduction and trip completion.

These are code foundations only. They are not yet connected to production menus, persistent save data, animated passenger models, route scenes or a complete playable journey. They have not been compiled or tested in Unity.

## Development status

The repository now contains a Unity 6 project configuration, an editor-driven Android APK build entry point, a temporary drivable coach/road prototype, independent multi-touch steering and pedal controls, and saved Économie/Équilibré/Élevé graphics presets. The APK pipeline is prepared but requires a Unity Personal license secret in GitHub Actions. The prototype has not yet been compiled, installed, or tested on a phone. It is not a finished realistic simulator and no downloadable APK is claimed yet.

Next gates: compile the Unity project, fix all build errors, replace placeholder coach/world geometry with original quality assets, add cockpit and production touch controls, then install and profile the first Android build on the Samsung Galaxy A07 (4 GB RAM).
