using System.Collections;
using UnityEngine;

namespace CacheCache
{
    /// <summary>Particules + secousse caméra (équivalent du ParticleLayer web).</summary>
    public class Juice : MonoBehaviour
    {
        ParticleSystem particles;
        Camera cam;
        Coroutine shakeRoutine;

        public void Init(Camera camera)
        {
            cam = camera;
            var go = new GameObject("FeedbackParticles");
            go.transform.SetParent(transform, false);
            particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 1.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.12f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 90;
            particles.Stop();
        }

        public void Burst(Vector3 position, Color color, int count = 28)
        {
            if (particles == null) return;
            particles.transform.position = position;
            var main = particles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.65f));
            particles.Emit(count);
        }

        public void Shake(float strength = 1f)
        {
            if (cam == null) return;
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine(strength));
        }

        IEnumerator ShakeRoutine(float strength)
        {
            var t = 0f;
            const float duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var falloff = 1f - t / duration;
                var offset = Random.insideUnitCircle * (0.14f * strength * falloff);
                cam.transform.localPosition = new Vector3(offset.x, offset.y, -10f);
                yield return null;
            }
            cam.transform.localPosition = new Vector3(0f, 0f, -10f);
            shakeRoutine = null;
        }
    }

    /// <summary>Garde le décor entièrement visible sur tous les formats (letterbox).</summary>
    public class CameraFit : MonoBehaviour
    {
        Camera cam;

        void Awake() { cam = GetComponent<Camera>(); }

        void LateUpdate()
        {
            if (cam == null) return;
            var aspect = Mathf.Max(0.35f, (float)Screen.width / Mathf.Max(1, Screen.height));
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(3.2f, 5f / aspect);
            // Position is owned by Juice so camera shake survives LateUpdate.
        }
    }
}
