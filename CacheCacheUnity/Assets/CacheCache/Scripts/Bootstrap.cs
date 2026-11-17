using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CacheCache
{
    /// <summary>
    /// Point d'entrée unique : pose ce composant sur un GameObject vide et appuie sur Play.
    /// Crée caméra, EventSystem, Canvas, UI, audio, particules et GameManager.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Bootstrap : MonoBehaviour
    {
        public bool showHighScoresOnMenu = true;

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            var existingCam = Camera.main;
            if (existingCam != null) Destroy(existingCam.gameObject);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.16f, 0.15f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraFit>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
                es.transform.SetParent(transform, false);
            }

            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var ui = canvasGo.AddComponent<UIManager>();
            ui.Build(canvas);

            var audio = gameObject.AddComponent<AudioManager>();
            audio.Init();

            var juice = gameObject.AddComponent<Juice>();
            juice.Init(cam);

            var builder = gameObject.AddComponent<RoundBuilder>();
            var game = gameObject.AddComponent<GameManager>();
            game.Init(cam, ui, audio, juice, builder);
            ui.Bind(game);
            ui.SetMuted(audio.Muted);

            var reticle = ui.CreateReticle();
            var input = gameObject.AddComponent<InputRouter>();
            input.Init(game, cam, reticle);
            game.ShowMenuAtStart();
        }
    }
}
