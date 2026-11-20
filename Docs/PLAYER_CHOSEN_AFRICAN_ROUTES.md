# Réseau routier ouvert et itinéraires au choix du joueur

## Principe de liberté

Le joueur n'est pas limité aux liaisons prédéfinies du catalogue. Il choisit :
1. sa ville de départ ;
2. sa destination ;
3. les villes intermédiaires dans l'ordre souhaité ;
4. les arrêts commerciaux et pauses à effectuer, selon les villes traversées.

Le jeu valide chaque liaison successive. Si deux villes ne sont pas directement reliées dans le réseau de routes disponible, le joueur ajoute une ville intermédiaire différente. Le choix de l'itinéraire est libre parmi les liaisons modélisées, sans imposer un seul trajet officiel.

## Couverture initiale élargie

Le réseau de données contient des villes en Côte d'Ivoire, Ghana, Togo, Bénin, Nigeria, Burkina Faso, Mali, Guinée, Liberia, Sierra Leone, Sénégal, Gambie, Niger, Cameroun et Gabon. Exemples de villes : Abidjan, Yamoussoukro, Bouaké, Korhogo, San Pedro, Man, Accra, Kumasi, Tamale, Lomé, Cotonou, Porto-Novo, Lagos, Ibadan, Kano, Ouagadougou, Bobo-Dioulasso, Bamako, Sikasso, Conakry, N'Zérékoré, Monrovia, Freetown, Dakar, Banjul, Niamey, Douala, Yaoundé et Libreville.

Le réseau est une première base de jeu extensible, pas une carte routière exhaustive. Certaines liaisons et estimations doivent être vérifiées avant une version commerciale, avec des données cartographiques sous licence et une étude des corridors réellement praticables par autocars.

## Données calculées pour le trajet choisi

Le constructeur d'itinéraire additionne les distances et durées estimées des tronçons sélectionnés, ajoute uniquement les villes et villages connus sur chacun de ces tronçons dans leur ordre de passage, et ajoute les contrôles frontaliers correspondants. Les localités intermédiaires ne sont jamais ajoutées si elles ne se trouvent pas sur le tronçon choisi. Les frais et temps de frontière sont affichables avant le départ. La route choisie pourra ensuite alimenter l'estimation de carburant, péages, salaires, repas, temps de conduite et recettes.

Les valeurs présentes dans le code sont provisoires et destinées à équilibrer le jeu. Elles ne représentent ni des temps de navigation garantis, ni des tarifs officiels, ni une recommandation de sécurité routière.

## Tarification automatique des itinéraires personnalisés

Tout itinéraire construit par le joueur peut maintenant produire son propre objet tarifaire :
- les lignes déjà présentes dans le catalogue conservent leurs trois tarifs validés ;
- les autres itinéraires utilisent une estimation provisoire selon la distance : 30 FCFA/km en Économique, 40 FCFA/km en Confort et 55 FCFA/km en VIP ;
- les montants sont arrondis aux 500 FCFA les plus proches ; les planchers sont de 5 000 FCFA sur une ligne nationale et 15 000 FCFA sur une ligne internationale ;
- le statut international est détecté automatiquement à partir des contrôles de frontière ou des pays traversés. Il ne dépend pas d'une case à cocher manuelle.

La création d'un trajet passagers accepte directement un `RouteItinerary` et construit le tarif correspondant. Ainsi, un itinéraire personnalisé traversant une frontière est marqué international et entre dans le calcul de la garantie de bénéfice minimum de 500 000 FCFA à la fin d'un voyage admissible. Le gestionnaire frontalier expose aussi le total des frais effectivement payés ; cette somme doit être fournie à la surcharge de clôture du voyage pour que la garantie couvre également ces frais séparés.

Ces tarifs personnalisés sont des paramètres d'équilibrage du jeu, et non des prix de transport réels. Ils devront être ajustés après essais dans le jeu.

## API de code ajoutée

- `AfricanRoutePlanner.AllCities` : villes disponibles.
- `AfricanRoutePlanner.FindCity(cityId)` : recherche d'une ville.
- `AfricanRoutePlanner.GetDirectConnections(cityId)` : villes reliées directement à une ville.
- `AfricanRoutePlanner.TryBuildPlayerItinerary(orderedCityIds, out itinerary, out reason)` : construit l'itinéraire dans l'ordre choisi par le joueur et explique pourquoi une liaison est invalide.

Cette API constitue une fondation C# ; elle n'est pas encore reliée à un écran de carte interactif ou à une scène 3D. Les routes alternatives seront affichées dans l'interface de planification lorsque celle-ci sera développée.


## Création d'un voyage à partir du choix du joueur

Le nouveau `PlayerRouteTripFactory.TryCreate(...)` regroupe les validations et la préparation des données d'un voyage :
- vérifie que la compagnie existe et possède le car choisi ;
- refuse les véhicules qui ne respectent pas la règle des six roues minimum ;
- vérifie que la classe Économique, Confort ou VIP est compatible avec le car ;
- valide la suite de villes choisie par le joueur dans le réseau routier ;
- construit l'itinéraire, son tarif automatique, le manifeste de passagers et le gestionnaire de contrôles frontaliers ;
- expose le tarif par passager et les frais frontaliers annoncés avant le départ.

Cette fabrique prépare les données du voyage. Elle ne constitue pas encore un écran graphique de planification et ne modifie pas encore la géométrie de la route 3D.


## Arrêts de passagers sur le trajet exact

Les tronçons routiers peuvent contenir une liste de localités intermédiaires. Lors de la construction de l'itinéraire, ces localités deviennent des arrêts voyageurs uniquement si le tronçon correspondant fait partie du parcours choisi. Leur ordre est inversé automatiquement lorsque le car circule dans le sens opposé. Exemple de base pour l'axe Abidjan–Yamoussoukro : Anyama, Singrobo, Tiassalé, N'Douci et Toumodi ; pour la suite vers Bouaké : Tiébissou et Djébonoua. Ces arrêts sont des données de prototype à enrichir et à vérifier sur une carte routière détaillée.

`PassengerTrip` permet désormais de réserver un billet entre deux arrêts voyageurs de cet itinéraire, calcule un tarif proportionnel à la portion parcourue et fournit des opérations pour faire monter les passagers à leur arrêt de départ et descendre ceux arrivés à leur arrêt. L'interface Unity doit encore appeler ces méthodes pendant le déroulement du trajet.
