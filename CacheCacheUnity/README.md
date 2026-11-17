# Cache-Cache ! — Kit Unity

Portage du jeu web "Cache-Cache !" vers Unity (2022.3 LTS ou Unity 6).
Tout est généré par code : aucun sprite, aucune police custom, aucun asset à importer
(sauf TextMeshPro Essentials). Idéal pour itérer vite puis remplacer l'art procédural
par tes propres illustrations.

## Démarrage en 5 minutes

1. Crée un projet Unity **2D (URP ou Built-in)**, version 2022.3 LTS ou Unity 6.
2. `Window > TextMeshPro > Import TMP Essential Resources` (obligatoire pour le texte).
3. Dans la scène vide : `GameObject > Create Empty`, renomme-le **CacheCache**.
4. Ajoute le composant **Bootstrap** (glisse Bootstrap.cs sur l'objet).
5. Appuie sur **Play**. C'est tout : caméra, décors, UI, sons et sauvegarde sont créés.

### Build mobile
- `File > Build Settings > Android / iOS` puis `Switch Platform`.
- Player Settings : Orientation paysage, Auto Rotation décoché (optionnel), resolution 1920x1080.
- Le jeu est déjà responsive : le cadrage s'adapte aux écrans étroits (CameraFit.cs).
- Active `Application.targetFrameRate = 60` (déjà fait dans Bootstrap).

## Contrôles
| Action | Souris / tactile | Clavier / manette |
| --- | --- | --- |
| Inspecter | Clic / tap sur un farceur | Flèches / WASD + Entrée ou Espace |
| Indice | Bouton ampoule | H |
| Pause | Bouton pause | P / Échap |
| Son | Bouton haut-parleur | M |
| Rejouer | Bouton Rejouer | Entrée / R |

## Règles portées à l'identique
- 4 décors : grenier, marché, forêt, bibliothèque (rotation).
- 3 farceurs au niveau 1, puis jusqu'à 6.
- +100 points par trouvaille × multiplicateur de série (×1 à ×4).
- Série remise à zéro sur un clic raté ; −2 s et +1 "raté".
- Indices : 2 au départ, +1 par niveau (max 3), révélation 2,8 s.
- Bonus de niveau : `secondes restantes × 8` + `50 + 25 × niveau` + 150 sans-faute.
- Partie terminée quand le chrono atteint 0 → score sauvegardé (PlayerPrefs, top 7).
- La partie se met en pause automatiquement si l'application passe en arrière-plan.

## Architecture (miroir du jeu web)
| Web (React) | Unity |
| --- | --- |
| `App.tsx` (état, boucle) | `GameManager.cs` (machine à états) |
| `rounds.ts` (génération) | `CacheCacheData.cs` + `RoundBuilder.cs` |
| `scenes.tsx` (SVG) | `RoundBuilder.cs` + `RuntimeSprites.cs` (procédural) |
| `characters.tsx` | `CharacterView.cs` |
| `ParticleLayer.tsx` | `Juice.cs` (ParticleSystem + shake) |
| `audio.ts` | `AudioManager.cs` (clips générés à la volée) |
| `scores.ts` | `HighScores.cs` (PlayerPrefs JSON) |
| `HUD.tsx`, `Overlays.tsx` | `UIManager.cs` (uGUI + TextMeshPro) |
| `SceneStage.tsx` (pointer) | `InputRouter.cs` (new Input System + legacy) |

## Remplacer l'art procédural
1. Crée un Prefab de décor par scène, ajoute-le au GameObject "WorldRoot".
2. Désactive `BuildScene()` dans `RoundBuilder`.
3. Pour chaque cachette, place un `HideSpot` + un `CharacterView` aux positions voulues.
4. Garde deux ordres de tri : corps derrière les props (`sortingOrder 10`),
   tête et mains devant (`sortingOrder 45`) pour l'effet "je dépasse".
5. Un `BoxCollider2D` sur la tête peut remplacer le test de distance dans GameManager.

## Optimisations 60 fps déjà incluses
- Aucune allocation par frame dans la boucle de jeu (structs + listes réutilisées).
- Particules : un seul ParticleSystem recyclé, burst de 34 particules max.
- UI mise à jour uniquement quand une valeur change (`RefreshHud` ciblé).
- Sprites générés une seule fois et mis en cache (dictionnaire statique).
- Audio : 4 AudioSources en pool, clips courts générés au démarrage.

## Idées d'extension
- Nouveaux décors : ajoute une `SceneTheme` dans `CacheCacheData.cs`.
- Daily challenge : seed la sélection des personnages avec la date du jour.
- Mode deux joueurs : deux réticules, premier à 5 trouvailles.
- Accessibilité : agrandir les zones cliquables (`hideRadius`) et un mode daltonien.
