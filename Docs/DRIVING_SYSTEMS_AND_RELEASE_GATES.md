# Driving systems and release gates

## Runtime systems connected in source

- Standard intercity coach: six physical WheelColliders. Double-decker configuration: ten physical wheels with two driven/braked rear axles, separate anti-roll/brake/torque distribution, upper-deck body glazing and 72 generated seats.
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
- Toggleable rainfall particles, wet-road darkening/reflectivity, animated wipers and a day/night lighting switch with night fog.
- Local company save/load for XOF balance, level, owned coaches, starting coach and finance history; financial mutations save automatically and app suspension/exit also saves.
- Curved 4.32 km playable starter corridor with asphalt mesh collision, shoulders, road markings, direction signs, roadside markers and batched savannah vegetation.
- Seven lightweight traffic vehicles follow the road curvature in both traffic directions, including an oncoming coach; the player starts in the correct right-hand lane.
- Route planner connected to the saved company and icon-only HUD; a prepared itinerary and estimated distance appear in the driving HUD.
- Passenger manifest panel for ticket reservations, seat assignment, luggage-weight checks, company ticket income, and manual stop-by-stop boarding/alighting processing.
- Ordered route progression controller added: it processes the next itinerary stop only, delegates passenger exchange to the passenger manifest, blocks border passage until passenger/coach documents are confirmed and the announced XOF fee is paid, and refuses trip closure while planned stops or border checks remain. This is a source-level coordination layer; automatic arrival detection and driving-HUD integration are still pending.
- Unified `PlayerTripSession` now joins the prepared itinerary, coach-trip operations, passenger manifest and ordered route progression; `GameBootstrap` creates and exposes this session when a route is prepared. Departure validates the ten-point safety checklist and a non-negative, overflow-safe operating-cost quote before charging the company. Stop processing additionally requires the coach to be stationary and the parking brake applied. These APIs are integrated at source level but are not yet exposed through a complete in-game departure/stop UI.
- Roadside diesel station with a trigger area; refuelling requires the engine off, a stationary coach and the parking brake, and charges the company account in XOF.

## Remaining implementation before release

1. Replace the starter corridor with route-specific geometry for all selected cities, including accurate road lengths, junctions, towns, terminals, rest areas, fuel stations and border checkpoints. The current 4.32 km corridor is a playable starter environment, not a complete real-world intercity route.
2. Connect `PlayerTripSession` to a complete in-game safety-checklist/departure screen, automatic stop arrival, physical route geometry and driving HUD. The session and progression APIs are now created by `GameBootstrap`, but players do not yet operate the complete workflow through the game UI.
3. Add passenger 3D models, seat occupancy visuals, luggage loading animations and onboard service interactions.
4. Expand traffic AI with lane changing, overtaking, junction rules, traffic lights, collision response and route events; add a continuous day/night clock, windshield water effects, variable rain intensity and weather transitions.
5. Add original production-quality exterior/interior models for every coach category and the remaining coach-specific equipment.
6. Add original or properly licensed vehicle-brand names, logos, sounds and designs. Fictional brands remain in use until permission is obtained.
7. Calibrate vehicle physics with measured vehicle mass, wheelbase, axle loads, tire data and braking data. Current values are game tuning, not manufacturer-validated vehicle data.
8. Compile in the pinned Unity Editor, fix all compiler/runtime errors, create an Android ARM64 build, and install/test it on the target Android device.
9. Complete performance, thermal, battery, memory, touch-layout, accessibility, privacy/legal and store-release checks.

## Android build access

The repository contains a manual GitHub Actions Android build workflow. The Unity license credentials must be configured by the repository owner in GitHub Actions secrets before the workflow can run. Never commit license data to the repository or share it in chat.

## Verification limits

Source-level structural checks do not prove that the project compiles or runs. No APK should be described as ready until the pinned Unity Editor compiles the project, the Android build succeeds, and the APK has been installed and tested on a physical device.


## Corrections de sécurité — manifeste et dépenses (20 novembre 2026)

- Le panneau de manifeste ne peut plus déclencher librement un arrêt choisi dans une liste : il ne traite que l'embarquement initial au terminal d'origine.
- Les arrêts intermédiaires, les postes-frontières et la destination doivent passer par `RouteProgressionController`, qui impose l'ordre de l'itinéraire et les validations documentaires.
- `CoachTripOperations.TryDepart` recalcule désormais les cinq postes de dépenses avec une addition contrôlée (`checked`) et rejette les montants négatifs ou les dépassements avant tout débit de l'entreprise.
- Ces corrections sont intégrées au dépôt source ; compilation Unity, tests de scène et essais sur téléphone restent à effectuer.


## Écran de préparation avant départ (20 novembre 2026)

- Nouveau composant `CoachDeparturePreparationPanel` : écran paysage avec dix validations obligatoires (pneus, freins, éclairage, visibilité, portes/évacuations, ceintures, extincteur, trousse de secours, bagages, repos du conducteur).
- Le panneau affiche une estimation des dépenses en FCFA (carburant, personnel, péages/frontières, restauration et réserve d'entretien), ainsi que la trésorerie disponible.
- Le flux de préparation est relié au planificateur : après création d'un itinéraire, le manifeste s'ouvre pour réserver et confirmer l'embarquement au terminal d'origine ; sa fermeture ouvre l'écran de préparation.
- Le départ ne peut être validé que si la checklist est complète, si au moins un passager est embarqué et si l'entreprise peut payer les dépenses calculées. Le statut du trajet passe alors à `OnRoad`.
- Les coûts affichés sont des estimations provisoires d'équilibrage du jeu, pas des tarifs réels. Le traitement interactif des arrêts successifs et des contrôles frontaliers dans le HUD reste à intégrer.
- Compilation Unity et tests sur appareil non effectués à ce stade.


## Durée du voyage et temps réel du joueur (20 novembre 2026)

- La durée en heures/minutes affichée dans le planificateur représente le temps estimé du voyage dans l'univers simulé ; elle ne doit jamais être interprétée comme une obligation de jeu en temps réel.
- Le planificateur distingue maintenant cette durée simulée d'un objectif indicatif de session joueur : environ 8 minutes pour 600 km, avec une minute supplémentaire par frontière prévue, et une plage de conception plafonnée entre 4 et 30 minutes.
- Cet objectif est une règle de conception affichée pour éviter les trajets interminables. Il n'est pas encore un chronomètre ni une accélération de simulation effectivement raccordée : la durée réelle devra être mesurée et ajustée après génération des routes complètes et essais dans Unity.
- Exemple : un itinéraire d'environ 600 km peut afficher une durée simulée d'environ 10 h, tout en visant une session de jeu de l'ordre de 8 à 10 minutes selon les contrôles frontaliers.


## Sauvegarde automatique et reprise de conduite (20 novembre 2026)

- `ActiveCoachSaveSystem` enregistre localement le point de contrôle du car dans `PlayerPrefs` : position 3D, orientation et niveau de carburant.
- `GameBootstrap` effectue une sauvegarde périodique toutes les 3 secondes, ainsi qu'à la mise en arrière-plan et à la fermeture de l'application ; la sauvegarde financière de l'entreprise est effectuée au même moment.
- Au prochain lancement, le car est replacé au point enregistré. Pour éviter un redémarrage dangereux, il revient immobilisé, frein de stationnement serré, boîte sur P et moteur coupé. Le joueur peut alors reprendre la conduite.
- Limite actuelle à ne pas masquer : la position physique et le carburant sont restaurés, mais la reconstruction complète de la session de voyage (réservations/passagers, prochain arrêt, frontières déjà franchies, dépenses du voyage et météo/heure) reste à connecter et à tester. Cette fonctionnalité n'est donc pas encore une reprise intégrale du trajet.
- Aucune compilation Unity ni vérification après fermeture forcée n'a encore été effectuée.
