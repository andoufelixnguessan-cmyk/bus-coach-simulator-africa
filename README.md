# BUS & COACH SIMULATOR AFRICA

A realistic 3D long-distance coach driving and transport-company management simulator for Android, focused initially on West Africa. The playable vehicle catalogue is restricted to full-size passenger coaches with at least six wheels; four-wheel minicars are excluded.

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
- the single-currency company economy and a full-size coach catalogue (six wheels minimum, with a six-wheel starting coach);
- optional onboard comfort equipment with compatibility checks;
- passenger bookings, seat assignment, luggage limits, ticket revenue and boarding states;
- trip preparation, mandatory safety checklist, departure-cost deduction and trip completion.

These are code foundations only. They are not yet connected to production menus, persistent save data, animated passenger models, route scenes or a complete playable journey. They have not been compiled or tested in Unity.

## African route and border foundations

The repository includes an expanding West and Central African city network and a player-selected itinerary planner. The player can choose a departure city, destination and ordered intermediate cities; the planner validates each road connection and builds the corresponding stops and international border checks. The first city network covers Côte d'Ivoire, Ghana, Togo, Benin, Nigeria, Burkina Faso, Mali, Guinea, Liberia, Sierra Leone, Senegal, Gambia, Niger, Cameroon and Gabon. Distances, times and fees remain provisional game-balancing values, not real navigation or official tariffs. The planner is a C# foundation and is not yet connected to an interactive map screen, a 3D road world or tested in Unity. See Docs/PLAYER_CHOSEN_AFRICAN_ROUTES.md.

## Custom route fares and international guarantee

Player-built itineraries can now generate a fare automatically when they are not in the curated route catalogue. The provisional distance rates are 30/40/55 XOF per km for Economy/Comfort/VIP, rounded to the nearest 500 XOF, with minimum fares of 5,000 XOF domestic and 15,000 XOF international. Border crossings or country changes automatically mark a custom itinerary as international. Passenger-trip creation accepts the itinerary directly, connecting it to the fare and the existing 500,000 XOF completed-trip guarantee calculation. Border managers expose the total fees actually paid, and trip settlement has an overload to include them in the guarantee calculation. The route UI still needs to call this border-aware overload. These calculations are code foundations and have not yet been compiled or tested in Unity.

## Development status

The repository now contains a Unity 6 project configuration, an editor-driven Android APK build entry point, a temporary drivable coach/road prototype, independent multi-touch steering and pedal controls, and saved Économie/Équilibré/Élevé graphics presets. The APK pipeline is prepared but requires a Unity Personal license secret in GitHub Actions. The prototype has not yet been compiled, installed, or tested on a phone. It is not a finished realistic simulator and no downloadable APK is claimed yet.

Next gates: compile the Unity project, fix all build errors, replace placeholder coach/world geometry with original quality assets, add cockpit and production touch controls, then install and profile the first Android build on the Samsung Galaxy A07 (4 GB RAM).

International completed trips have a minimum net-profit target of 500,000 XOF: the trip quote estimates a transparent commercial guarantee top-up, and trip completion records any required top-up separately in company finances. It applies only to completed international trips with at least one paid passenger; it is a game-balancing rule, not a real-world earnings claim. The starting company capital is set to 24,000,000 XOF so it can purchase the 22,000,000 XOF six-wheel starter coach.


## Player-selected trip setup

A new `PlayerRouteTripFactory` validates the player's ordered city list, company ownership of the selected coach, the six-wheel minimum and service-class compatibility. It then creates the itinerary, fare, passenger manifest and border manager as one data-level trip setup. Border-aware trip completion now refuses to close a journey while any required border checkpoint remains uncleared and includes actually paid border fees in the international guarantee calculation. A graphical route-planning screen and 3D route-world integration are still pending.
