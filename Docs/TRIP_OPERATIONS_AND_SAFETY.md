# Préparation et exploitation d'un voyage

## Checklist avant départ
- Pneus et écrous contrôlés
- Freins contrôlés
- Feux et clignotants vérifiés
- Rétroviseurs et pare-brise vérifiés
- Portes et issues de secours fonctionnelles
- Ceintures disponibles et contrôlées
- Extincteur présent
- Trousse de premiers secours présente
- Bagages rangés et arrimés
- Conducteur reposé

Ces éléments de sécurité ne sont jamais des achats facultatifs. Le départ est bloqué tant que la checklist n'est pas complète et qu'au moins un passager n'est pas embarqué.

## Cycle d'un trajet
1. Préparation du car et du manifeste passagers.
2. Contrôle de sécurité et passage à « prêt au départ ».
3. Déduction unique des dépenses annoncées : carburant, personnel, péages/frontières, restauration et réserve d'entretien.
4. Passage en cours de route.
5. Arrivée des passagers et clôture.
6. Prime facultative de conduite prudente si le trajet est terminé sans incident.

Les recettes des billets sont enregistrées au moment de leur vente dans le système passagers. Les dépenses principales sont débitées au départ par le système d'exploitation, afin d'éviter un double comptage lors de la clôture. Les frais frontaliers réglés séparément sont exposés par `InternationalBorderManager.GetTotalFeesPaidXof()` et doivent être transmis à la surcharge de `CoachTripOperations.TryComplete(...)` qui accepte le gestionnaire frontalier. La garantie de 500 000 FCFA tient alors compte des frais réellement payés aux frontières.

## Limites actuelles
La logique C# est ajoutée, mais n'est pas encore reliée à une interface Unity ni testée dans l'éditeur. La checklist est une validation de prototype : elle ne remplace pas encore les contrôles visuels/animés ni les diagnostics mécaniques du véhicule. La gestion des étapes intermédiaires, des contrôles frontaliers interactifs, des pannes, des arrêts et de la fatigue en temps réel reste à développer. Le branchement complet entre l'interface de voyage et la surcharge de clôture doit encore être réalisé et testé.


## Préparation des voyages personnalisés

`PlayerRouteTripFactory` prépare le voyage à partir des villes choisies et vérifie la possession du car, sa règle de six roues minimum et la compatibilité de la classe de service. Le gestionnaire frontalier associé doit avoir validé tous les postes avant la clôture. La surcharge de clôture qui reçoit ce gestionnaire inclut les frais effectivement payés dans le calcul de la garantie internationale.
