# Tableau de bord, voyants et audio moteur

## Direction produit
La conduite doit disposer d'un vrai combiné d'instruments lisible en paysage, inspiré par le niveau de finition et la hiérarchie visuelle de Bus Simulator: Ultimate, mais avec une interface originale. Chaque modèle de car doit avoir son identité de cockpit : cadrans analogiques ou écran numérique selon la gamme, typographie, disposition, couleurs d'éclairage et sons propres.

## Données spécifiques par véhicule
Chaque fiche véhicule doit définir au minimum :
- famille moteur et cylindrée/type à afficher ;
- régime de ralenti, régime maximal et zone rouge ;
- signature audio (échantillons originaux/licenciés), volume cabine et vibration grave ;
- type de combiné (analogique, hybride ou numérique) ;
- unités, seuils et voyants réellement présents sur le véhicule.

Le prototype contient des profils provisoires distincts pour le car interurbain, standard, confort, grand tourisme, premium et double étage. Les valeurs sont des paramètres de jeu et ne prétendent pas reproduire les spécifications d'une marque réelle.

## Améliorations moteur
Les niveaux d'amélioration (0 à 5) influencent la réponse moteur, le régime maximal et la signature audio. Les améliorations devront aussi être équilibrées avec consommation, température, entretien et usure. Une amélioration ne doit pas simplement augmenter la vitesse sans contrepartie.

## Voyants et instruments en conduite
Le combiné prévoit vitesse, compte-tours, carburant, température du liquide de refroidissement, pression d'huile, pression d'air et tension batterie. Les témoins comprennent clignotants gauche/droite, feux de route, frein de stationnement, ceinture, ABS et défaut moteur.

Règles obligatoires :
- un voyant s'allume uniquement lorsque son état réel l'exige ;
- les témoins de contrôle peuvent s'allumer brièvement au démarrage puis s'éteindre après l'autotest ;
- rouge = arrêt/danger, orange = anomalie à contrôler, vert = fonction active, bleu = feux de route ;
- les voyants de sécurité ne doivent jamais être vendus comme options ;
- les alertes sonores doivent rester distinctes du bruit moteur et être réglables.

## Audio
Le prototype utilise un moteur diesel synthétique temporaire pour valider la variation de régime et de charge sans fichier audio externe. Ce son n'est pas un enregistrement réaliste. La version de production devra intégrer des enregistrements originaux ou correctement licenciés : démarrage, ralenti, accélération, décélération, frein moteur, changement de rapport, turbo, échappement et bruit cabine. Chaque autocar doit avoir ses propres couches audio et les améliorations moteur doivent les modifier de manière crédible.

## État de validation
Les scripts constituent une première base technique. Le combiné n'est pas encore un tableau de bord 3D de cockpit, les valeurs de diagnostic sont en partie simulées, les clignotants/phares ne sont pas encore reliés à des commandes de conduite, et le son synthétique ne remplace pas des enregistrements moteur. Compilation Unity, contrôle visuel, équilibrage et essais Android restent obligatoires.
