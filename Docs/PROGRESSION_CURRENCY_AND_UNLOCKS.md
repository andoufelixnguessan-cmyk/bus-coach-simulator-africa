# Progression, pièces et déblocage des véhicules

## Décision de conception

BUS & COACH SIMULATOR AFRICA utilise une progression fondée sur des pièces virtuelles gagnées en jouant. Les véhicules, certaines options de confort et les améliorations mécaniques peuvent être achetés ou débloqués avec ces pièces. Le joueur doit pouvoir profiter du jeu et atteindre les véhicules haut de gamme sans paiement en argent réel.

Ce document définit les règles de conception. Il ne signifie pas que l'économie ou les écrans de boutique sont déjà implémentés.

## 1. Monnaie du jeu

- **Pièces** : monnaie principale gagnée pendant les activités normales.
- Les récompenses sont annoncées clairement avant chaque mission.
- Aucun achat obligatoire en argent réel ne doit bloquer la progression principale.
- Les prix et récompenses doivent être équilibrés après des essais de jeu, pas choisis arbitrairement.
- Pas de récompense aléatoire payante donnant un avantage compétitif.
- Les éventuels achats réels futurs ne doivent pas rendre les véhicules ou améliorations indispensables inaccessibles aux joueurs gratuits.

## 2. Comment gagner des pièces

Le joueur peut recevoir des pièces pour :
- terminer une liaison et déposer les passagers à destination ;
- conduire prudemment et respecter le code de la route ;
- respecter les horaires sans conduite dangereuse ;
- transporter le bon nombre de passagers et gérer correctement les bagages ;
- assurer le confort des voyageurs pendant les longs trajets ;
- effectuer les arrêts prévus et gérer le carburant ;
- entretenir le car et éviter les dégâts ;
- développer sa compagnie et réussir des contrats.

Les récompenses doivent tenir compte de la distance, de la difficulté et de la qualité du service. Les infractions, accidents, dégâts et retards injustifiés peuvent réduire la récompense. Le jeu ne doit jamais encourager la vitesse dangereuse.

## 3. Déblocage des véhicules

Chaque véhicule possède sa propre fiche :
- marque et modèle fictifs ou utilisés avec autorisation ;
- catégorie (minibus, autocar standard, grand tourisme, etc.) ;
- prix en pièces et éventuel niveau de progression requis ;
- capacité passagers et bagages ;
- puissance, couple, transmission et consommation ;
- dimensions, masse, rayon de braquage et comportement de suspension ;
- caractéristiques de freinage et performances ;
- configuration du cockpit et du tableau de bord ;
- aménagement intérieur, confort et équipements compatibles.

Certains véhicules sont disponibles au départ. D'autres nécessitent un niveau de compagnie, une réputation ou des contrats accomplis, puis un achat avec les pièces. L'achat d'un véhicule ne doit pas le faire apparaître gratuitement dans la flotte si le joueur n'a pas payé son prix.

## 4. Déblocage des options à bord

Les options compatibles avec un modèle peuvent être achetées séparément ou proposées dans des packs clairement détaillés, par exemple :
- climatisation améliorée et réglages de ventilation ;
- télévision et système de divertissement ;
- radio et système audio amélioré ;
- Wi-Fi, ports USB et prises ;
- sièges inclinables, repose-pieds et rideaux ;
- éclairage d'ambiance et liseuses ;
- réfrigérateur, distributeur ou service de nourriture et boissons ;
- toilettes embarquées sur les cars prévus pour cela ;
- microphone et système d'annonces.

Une option ne doit être affichée comme achetable que si le modèle de car la prend réellement en charge. Les équipements installés doivent être visibles dans l'habitacle et fonctionner dans le gameplay, pas seulement apparaître comme une icône dans un menu.

## 5. Améliorations mécaniques

Les pièces peuvent aussi financer des améliorations compatibles :
- moteur et réponse de l'accélérateur ;
- boîte de vitesses et rapports ;
- système de freinage ;
- suspension et amortisseurs ;
- pneus adaptés aux conditions de route ;
- capacité ou efficacité énergétique lorsque techniquement plausible.

Les améliorations doivent rester crédibles : elles modifient les caractéristiques du véhicule et peuvent avoir des effets secondaires (coût, consommation, entretien, usure). Elles ne doivent pas transformer instantanément un vieux car en véhicule irréaliste.

## 6. Sécurité et équipements obligatoires

Les équipements essentiels à la sécurité et à la conformité ne doivent pas être artificiellement verrouillés derrière une longue progression. Les ceintures, les sorties de secours, les extincteurs, les dispositifs de sécurité et les contrôles avant départ doivent être présents selon la configuration du véhicule. Le gameplay peut demander au conducteur de vérifier leur état et leur utilisation.

Les options de confort et de luxe peuvent être des déblocages ; la sécurité fondamentale ne doit pas devenir un avantage payant.

## 7. Économie et coûts d'exploitation

Posséder un véhicule entraîne des dépenses cohérentes :
- carburant ou énergie ;
- entretien périodique et réparations ;
- pneus et pièces d'usure ;
- salaires et frais d'exploitation si le mode compagnie les gère ;
- réapprovisionnement des boissons, snacks et repas lorsque ces services sont installés.

Le joueur doit pouvoir consulter le coût estimé avant d'acheter un véhicule ou une option. Les frais ne doivent pas être cachés.

## 8. Interface attendue

Prévoir des espaces distincts :
1. **Garage** : véhicules possédés, état, entretien et sélection du car.
2. **Concessionnaire** : véhicules verrouillés, prix, conditions et comparaison des caractéristiques.
3. **Personnalisation** : options compatibles, aperçu intérieur et coût d'installation.
4. **Améliorations** : pièces mécaniques, niveaux et effets mesurables.
5. **Économie** : solde de pièces, historique des gains et dépenses.

Chaque élément verrouillé indique précisément la condition à remplir : nombre de pièces, niveau, réputation, contrat ou véhicule requis. Le joueur doit pouvoir distinguer « verrouillé », « achetable », « possédé » et « installé ».

## 9. Principes anti-frustration

- La progression est possible en jouant normalement, sans répétition excessive d'une seule mission.
- Les prix sont cohérents avec les récompenses et les coûts d'exploitation.
- Les véhicules de départ permettent réellement de terminer les premières liaisons.
- Les véhicules haut de gamme sont des objectifs motivants, pas des nécessités pour gagner.
- Pas de vente trompeuse : caractéristiques, compatibilité, coûts et conditions sont visibles.
- Pas de perte arbitraire d'un véhicule acheté.
- Les sauvegardes conservent les pièces, véhicules, options achetées et améliorations installées.

## 10. Critères de validation

Avant de considérer le système comme terminé, vérifier que :
- les pièces sont attribuées uniquement après validation d'une activité ;
- le solde est sauvegardé et ne peut pas être modifié par une simple manipulation de l'interface ;
- un achat débite le bon montant une seule fois ;
- un véhicule acheté rejoint le garage et reste disponible après redémarrage ;
- une option incompatible ne peut pas être installée ;
- l'aperçu correspond à l'équipement réellement installé ;
- les conditions de déblocage sont compréhensibles ;
- un joueur gratuit peut atteindre les contenus de progression prévus ;
- l'économie a été testée sur plusieurs heures de jeu et plusieurs profils.

## Statut

**Validé comme exigence produit et de conception.** Les systèmes de monnaie, boutique, garage, sauvegarde et déblocage restent à développer et à tester dans Unity.
