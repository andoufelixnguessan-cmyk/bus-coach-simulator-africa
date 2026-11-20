# Arrêts voyageurs sur l'ensemble du réseau

## Règle commune

La même règle s'applique à toutes les lignes prédéfinies et à tous les itinéraires personnalisés : le car ne propose que les villes et localités présentes sur les tronçons routiers effectivement choisis par le joueur. Aucun village voisin, raccourci ou arrêt d'un autre itinéraire n'est ajouté automatiquement.

Les lignes prédéfinies de AfricanRouteNetwork.AllRoutes sont générées à partir de AfricanRoutePlanner. Les lignes prédéfinies et les lignes créées librement partagent donc la même source de données et le même ordre de passage.

## Sens de circulation et frontières

Chaque tronçon contient sa liste de localités intermédiaires. L'ordre est inversé quand le car circule dans le sens opposé. Sur les tronçons internationaux, la position du poste frontière est précisée dans la liste : les localités du pays de départ apparaissent avant le poste et celles du pays d'arrivée après le poste. Le sens de la frontière est lui aussi inversé en sens retour.

Les localités renseignées sont des arrêts de voyageurs du prototype. Les aires de repos, stations-service et postes frontières restent des arrêts de service et ne sont pas proposés comme points de montée/descente sauf si une donnée de ligne les autorise explicitement.

## Montée et descente des passagers

- Un billet peut relier deux arrêts voyageurs du trajet sélectionné, dans l'ordre de passage.
- Le prix du billet est calculé proportionnellement aux portions voyageurs parcourues ; les contrôles frontaliers ne sont pas comptés comme des arrêts commerciaux.
- À chaque arrêt, les passagers arrivés à destination descendent d'abord.
- Les passagers réservés pour cet arrêt montent ensuite.
- Le traitement refuse les arrêts absents de la ligne et les arrêts déjà traités ou passés.
- Les sièges peuvent être réutilisés lorsque deux réservations ne se chevauchent pas sur la route.
- La capacité des soutes est contrôlée sur chaque portion du trajet, et non sur le total historique des bagages déjà descendus.

## API de traitement

PassengerTrip.TryProcessPassengerStop(stopId, out alighted, out boarded, out reason) traite un arrêt dans le manifeste.

CoachTripOperations.TryProcessPassengerStop(passengers, stopId, out alighted, out boarded, out summary, out reason) ajoute les contrôles de statut du trajet et du car. L'opération n'est acceptée que lorsque le voyage est en cours.

## Limites à connaître

Le réseau est une base de jeu évolutive, pas un inventaire exhaustif de toutes les routes, villes et villages d'Afrique. Les localités et corridors actuellement renseignés doivent être vérifiés sur des cartes routières détaillées et des données cartographiques autorisées avant une version commerciale. Les distances, durées et frais restent des estimations de gameplay.

Le code métier a été mis à jour, mais aucune compilation Unity, scène de jeu, animation de montée/descente ou validation sur Android n'est revendiquée à ce stade.
