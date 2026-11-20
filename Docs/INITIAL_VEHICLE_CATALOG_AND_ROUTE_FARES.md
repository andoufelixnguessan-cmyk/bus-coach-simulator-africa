# Catalogue initial des véhicules et tarifs des trajets

## Statut et unité

Ce catalogue constitue une **proposition de valeurs de départ pour le jeu**, à équilibrer pendant les tests. Ce ne sont pas des tarifs officiels de compagnies réelles.

Le jeu distingue deux économies :
- **Pièces (PC)** : progression du joueur, achat et déblocage des véhicules et options.
- **Francs CFA (FCFA / XOF)** : argent de la compagnie, vente des billets, dépenses d'exploitation et bénéfices.

Les pièces ne remplacent pas les recettes de la compagnie. Les billets vendus aux passagers sont comptabilisés en FCFA.

## 1. Catalogue de départ des véhicules

Les noms ci-dessous sont des désignations de travail. Les marques et modèles réels ne seront utilisés qu'avec les autorisations nécessaires.

| Catégorie | Capacité indicative | Prix indicatif | Déblocage |
|---|---:|---:|---|
| Minibus interurbain | 18–22 passagers | 2 500 PC | Disponible après le tutoriel |
| Autocar standard |  cinquantaines de places (45–50) | 6 000 PC | Niveau compagnie 2 |
| Autocar confort | 45–50 passagers | 9 000 PC | Niveau compagnie 4 |
| Grand tourisme | 44– cinquantaine de passagers (44–50) | 14 000 PC | Niveau compagnie 6 et réputation requise |
| Autocar premium longue distance | 40–46 passagers | 20 000 PC | Niveau compagnie 8 et contrat longue distance |
| Autocar double étage (contenu avancé) | selon homologation du modèle | 30 000 PC | Niveau compagnie 12 et flotte développée |

La capacité exacte dépendra du modèle 3D et de son homologation simulée. Le jeu ne doit jamais permettre de dépasser la capacité de places réellement installées.

### Différences de conduite

Chaque véhicule doit avoir ses propres valeurs de masse, dimensions, puissance, couple, rapports de boîte, freinage, rayon de braquage, suspension, consommation, capacité de bagages et comportement. Les catégories ne doivent pas être de simples changements de carrosserie.

## 2. Tarifs initiaux des billets

Tarifs de référence **par passager et par trajet simple**, en FCFA. Ils sont volontairement présentés comme valeurs de jeu provisoires, et non comme des relevés du marché réel.

| Liaison de départ | Économique | Confort | Premium |
|---|---:|---:|---:|
| Abidjan – Yamoussoukro | 3 000 | 4 000 | 5 000 |
| Abidjan – Bouaké | 5 000 | 6 500 | 8 000 |
| Abidjan – Accra | 8 000 | 10 000 | 12 500 |

Le tarif dépend du service choisi, pas uniquement du véhicule. Une liaison transfrontalière doit aussi prendre en compte les contrôles, le temps de trajet et les formalités simulées. Les prix affichés sont des prix de jeu et pourront être ajustés après équilibrage.

## 3. Classes de service

- **Économique** : siège standard, climatisation selon équipement, service simple.
- **Confort** : siège plus confortable, climatisation réglable, prises ou USB si installés, service à bord selon le car.
- **Premium** : équipement haut de gamme réellement installé, divertissement, service de boissons ou repas si disponible.

Une classe supérieure ne peut être vendue que si le car possède les équipements correspondants. L'interface de réservation doit montrer les services inclus avant le paiement.

## 4. Réservation et calcul du chiffre d'affaires

Avant confirmation, afficher :
- départ, arrivée, date et heure ;
- durée estimée et arrêts prévus ;
- classe de service ;
- prix par passager ;
- nombre de places et sièges disponibles ;
- bagages autorisés et éventuels frais supplémentaires clairement indiqués ;
- montant total à payer.

Formule de base :

`Chiffre d'affaires billets = billets effectivement vendus × tarif unitaire`

Le revenu net de la liaison est calculé après déduction du carburant, des péages éventuels, des salaires, des repas et boissons fournis, de l'entretien et des autres frais applicables. Les places vides ne génèrent aucun revenu.

## 5. Gestion des prix par la compagnie

Dans le mode gestion, le joueur peut choisir parmi des niveaux tarifaires encadrés :
- tarif économique ;
- tarif normal recommandé ;
- tarif confort ou premium si les équipements le justifient.

Un prix excessif doit réduire la demande et la réputation ; un prix trop bas peut augmenter le remplissage mais réduire la marge. Les prix ne doivent pas être modifiables pendant qu'un passager a déjà payé son billet.

Les tarifs doivent être cohérents avec la distance, le type de service, la concurrence simulée, la saison et les coûts d'exploitation. Les trajets plus longs ne doivent pas être automatiquement plus rentables si les dépenses augmentent aussi.

## 6. Progression et récompenses

Les pièces de progression sont attribuées séparément des recettes en FCFA :
- bonus de conduite sûre ;
- bonus de ponctualité réaliste ;
- bonus de satisfaction des passagers ;
- bonus de trajet terminé et de gestion correcte des arrêts ;
- bonus de progression de compagnie et de contrats.

Ne pas attribuer de bonus pour la vitesse excessive. Les accidents, infractions, retards évitables et mauvais état du véhicule diminuent la note et peuvent réduire la récompense.

## 7. Équilibrage à effectuer en jeu

Les montants de véhicules et de billets sont des valeurs initiales de conception. Avant publication, simuler plusieurs profils de joueurs et mesurer :
- le nombre de trajets nécessaires pour acheter chaque véhicule ;
- le bénéfice réel par liaison selon le taux de remplissage ;
- l'effet des réparations, du carburant et des services à bord ;
- la progression d'un joueur prudent par rapport à un joueur expérimenté ;
- la viabilité des trajets courts et longs.

Aucun véhicule ne doit être indispensable pour terminer la progression. Le jeu doit rester jouable sans paiement réel et sans répétition artificielle excessive.

## 8. Statut technique

**Catalogue et règles économiques documentés.** Les prix ne sont pas encore intégrés dans une boutique, un système de réservation ou une simulation financière Unity. Ces systèmes devront être développés, sauvegardés et testés avant d'être annoncés comme fonctionnels.
