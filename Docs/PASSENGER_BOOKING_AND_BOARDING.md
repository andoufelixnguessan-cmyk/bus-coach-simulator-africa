# Réservation, passagers et embarquement

## Objectif
Ce système constitue la base du parcours passager pour un trajet simple : vente/réservation du billet, attribution d'un siège, contrôle des bagages, embarquement, arrivée et gestion des absents.

## Règles prévues dans le code
- Un seul car affecté à un trajet et une seule classe de service par trajet.
- Capacité de sièges calculée au milieu de la fourchette de la fiche du car. Le jeu devra remplacer cette valeur par la configuration réelle du véhicule dès que les modèles définitifs seront intégrés.
- Un numéro de siège unique par réservation active.
- Un billet est payé en FCFA et la recette est inscrite dans l'historique financier de l'entreprise.
- Chaque passager choisit son arrêt de montée et son arrêt de descente parmi les localités présentes sur l'itinéraire réellement sélectionné. Aucun arrêt hors trajet n'est proposé.
- Les arrêts intermédiaires peuvent être des villes ou villages ; le car peut y prendre et y déposer des voyageurs.
- Le tarif du billet est calculé automatiquement selon la portion parcourue, à partir du tarif du trajet complet.
- Un passager ne peut embarquer qu'une seule fois et ne peut être marqué arrivé que s'il a embarqué.
- Annulation possible uniquement avant embarquement, avec remboursement enregistré.
- Franchise de bagage de référence : 25 kg par place. Un bagage individuel ne dépasse pas 32 kg. La capacité totale de soute est calculée à partir des places.
- Les réservations non annulées peuvent être marquées « absent » au départ. Les places libérées peuvent ensuite être réattribuées dans une future évolution du système.

## Parcours d'utilisation à relier à l'interface Unity
1. Choisir une ligne et une date de départ.
2. Choisir un car possédé et une classe compatible avec ses équipements.
3. Afficher les places restantes et la recette prévisionnelle.
4. Enregistrer les réservations et les paiements.
5. Pour chaque billet, choisir la localité de montée et celle de descente parmi les arrêts du trajet.
6. À chaque arrêt prévu, faire monter les voyageurs concernés et descendre ceux arrivés à destination.
7. Clôturer le voyage lorsque le car atteint sa destination finale.

## Points non encore réalisés
Ce fichier est une logique métier C# indépendante. La logique de réservation entre arrêts et de montée/descente à un arrêt est ajoutée au code. Il n'existe pas encore d'interface graphique de choix des arrêts, de personnages 3D, d'animation de montée/descente, de sauvegarde persistante, ni de test Unity. Les remboursements sont complets pour cette première base ; une politique commerciale avec frais d'annulation devra être définie plus tard si nécessaire.
