using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CacheCache
{
    /// <summary>Clavier (réticule) + souris/tactile. Compatible nouveau et ancien Input System.</summary>
    public class InputRouter : MonoBehaviour
    {
        GameManager game;
        Camera cam;
        Transform reticle;
        bool keyboardMode;
        readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        public void Init(GameManager manager, Camera camera, Transform reticleTransform)
        {
            game = manager;
            cam = camera;
            reticle = reticleTransform;
            if (reticle != null) reticle.gameObject.SetActive(false);
        }

        void Update()
        {
            if (game == null || cam == null) return;
            HandleKeyboard();
            HandlePointer();
        }

        void HandleKeyboard()
        {
            var move = Vector2.zero;
            var inspect = false;
            var pause = false;
            var hint = false;
            var mute = false;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) move.x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) move.x += 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) move.y += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) move.y -= 1f;
            inspect = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
            pause = kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame;
            hint = kb.hKey.wasPressedThisFrame;
            mute = kb.mKey.wasPressedThisFrame;
            var reset = kb.rKey.wasPressedThisFrame;
            if (reset && game.State == GameState.GameOver) game.Restart();
#else
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) move.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) move.x += 1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) move.y += 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) move.y -= 1f;
            inspect = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
            pause = Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape);
            hint = Input.GetKeyDown(KeyCode.H);
            mute = Input.GetKeyDown(KeyCode.M);
            if (Input.GetKeyDown(KeyCode.R) && game.State == GameState.GameOver) game.Restart();
#endif

            if (move != Vector2.zero)
            {
                keyboardMode = true;
                if (reticle != null)
                {
                    reticle.gameObject.SetActive(true);
                    var next = (Vector2)reticle.position + move.normalized * Time.unscaledDeltaTime * 4.4f;
                    var halfHeight = cam.orthographicSize;
                    var halfWidth = halfHeight * cam.aspect;
                    reticle.position = new Vector3(Mathf.Clamp(next.x, -halfWidth + 0.2f, halfWidth - 0.2f),
                        Mathf.Clamp(next.y, -halfHeight + 0.2f, halfHeight - 0.2f), 0f);
                }
            }

            if (inspect && game.State == GameState.GameOver) { game.Restart(); return; }
            if (inspect && keyboardMode && reticle != null && game.State == GameState.Playing)
                game.InspectAt(reticle.position);
            if (pause) game.TogglePause();
            if (hint) game.UseHint();
            if (mute) game.ToggleMute();
        }

        void HandlePointer()
        {
            var pressed = false;
            var screenPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPosition = pointer.position.ReadValue();
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                pressed = true;
                screenPosition = Input.mousePosition;
            }
#endif

            if (!pressed) return;
            if (EventSystem.current != null)
            {
                uiHits.Clear();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
                    { position = screenPosition }, uiHits);
                if (uiHits.Count > 0) return;
            }
            // garde-fou tactile : on ignore la bande du HUD en haut de l'écran
            if (screenPosition.y > Screen.height * 0.82f) return;
            keyboardMode = false;
            if (reticle != null) reticle.gameObject.SetActive(false);
            var world = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cam.transform.position.z));
            world.z = 0f;
            if (game.State == GameState.Playing) game.InspectAt(world);
            else if (game.State == GameState.LevelClear) { }
        }
    }
}
