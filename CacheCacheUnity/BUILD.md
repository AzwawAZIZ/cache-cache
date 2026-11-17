# Ouvrir et compiler

Le dossier `CacheCacheUnity/` est la racine du projet. Les 14 fichiers du ZIP
et leur arborescence sont conservés ; certains scripts sont corrigés.
La cible de référence est Unity **2022.3.62f1**, pipeline Built-in.
Les indications URP/Unity 6 du README original ne sont pas validées.

1. Installer cet éditeur dans Unity Hub, avec Linux Build Support (Mono)
   pour le build Linux, ou les modules de la plateforme souhaitée.
2. Ajouter le dossier `CacheCacheUnity` dans Unity Hub et ouvrir le projet.
3. Attendre la résolution des packages et la compilation des scripts.
4. Exécuter **Cache-Cache > Prepare project** : import des ressources TMP,
   création de `Assets/CacheCache/Scenes/Main.unity` si absente et ajout au build.
5. Ouvrir cette scène puis Play. Tester lancement, indices successifs, pause,
   passage au niveau suivant, fin de partie, replay et sauvegarde des scores.
6. File > Build Settings > Build (la scène Main est enregistrée).

Depuis la racine du dépôt, avec un éditeur activé et son module Linux :

```sh
"$UNITY_EDITOR" -batchmode -nographics -quit \
  -projectPath "$PWD/CacheCacheUnity" \
  -executeMethod CacheCache.Editor.ProjectSetup.BuildLinux \
  -logFile "$PWD/unity-build.log"
```

Le résultat attendu est `CacheCacheUnity/Builds/Linux/CacheCache.x86_64`.
La commande échoue si Unity rapporte un échec du build.
L'outil Prepare conserve une scène Main existante mais remplace la liste
Build Settings par cette scène. Enregistrer ses scènes avant utilisation.

## État de validation

Contrôle syntaxique C# et présence de tous les chemins du ZIP effectués dans
l'environnement cloud. Aucun éditeur Unity ni licence disponible : compilation,
import des packages, build et tests Play Mode **non exécutés**. L'absence
d'erreurs syntaxiques ne prouve pas la compilation contre les API Unity.

Corrections : fermeture du panneau de victoire au niveau suivant, nettoyage
explicite du précédent indice, secousse caméra préservée, données de scores
invalides/nulles tolérées, bouton muet sauvegardé immédiatement, Entrée pour
rejouer, détection UI à la position réelle du clic/toucher.
