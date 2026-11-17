using System.Collections;
using UnityEngine;

namespace CacheCache
{
    /// <summary>Tous les sons sont synthétisés au démarrage : aucun fichier audio.</summary>
    public class AudioManager : MonoBehaviour
    {
        static readonly float[] Penta = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.5f, 1174.66f, 1318.51f };

        AudioSource[] sources;
        AudioClip[] notes;
        AudioClip miss, near, hint, click, tick, start;
        AudioClip[] clear, over;
        int cursor;
        public bool Muted { get; private set; }

        public void Init()
        {
            Muted = PlayerPrefs.GetInt("cache-cache:muted", 0) == 1;
            AudioListener.volume = Muted ? 0f : 1f;
            sources = new AudioSource[5];
            for (var i = 0; i < sources.Length; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
            }

            notes = new AudioClip[Penta.Length];
            for (var i = 0; i < Penta.Length; i++) notes[i] = Tone("note" + i, Penta[i], Penta[i], 0.24f, 0.55f);
            miss = Tone("miss", 180f, 110f, 0.2f, 0.4f);
            near = Tone("near", 440f, 560f, 0.12f, 0.35f);
            hint = Tone("hint", 880f, 1180f, 0.22f, 0.35f);
            click = Tone("click", 620f, 620f, 0.06f, 0.25f);
            tick = Tone("tick", 880f, 880f, 0.06f, 0.18f);
            start = Arpeggio("start", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.09f);
            clear = new[] { Arpeggio("clear", new[] { 783.99f, 880f, 1046.5f, 1318.51f }, 0.1f) };
            over = new[] { Arpeggio("over", new[] { 392f, 330f, 262f }, 0.16f) };
        }

        AudioSource NextSource()
        {
            cursor = (cursor + 1) % sources.Length;
            return sources[cursor];
        }

        public void PlayFind(int streak)
        {
            if (Muted) return;
            var note = notes[Mathf.Clamp(streak, 0, notes.Length - 1)];
            NextSource().PlayOneShot(note, 0.8f);
            StartCoroutine(Delayed(note, 0.055f, 0.4f));
        }

        IEnumerator Delayed(AudioClip clip, float delay, float volume)
        {
            yield return new WaitForSeconds(delay);
            if (!Muted) NextSource().PlayOneShot(clip, volume);
        }

        public void PlayMiss() { if (!Muted) NextSource().PlayOneShot(miss, 0.7f); }
        public void PlayNear() { if (!Muted) NextSource().PlayOneShot(near, 0.5f); }
        public void PlayHint() { if (!Muted) NextSource().PlayOneShot(hint, 0.7f); }
        public void PlayClick() { if (!Muted) NextSource().PlayOneShot(click, 0.5f); }
        public void PlayTick(bool urgent) { if (!Muted) NextSource().PlayOneShot(tick, urgent ? 0.6f : 0.3f); }
        public void PlayStart() { if (!Muted) NextSource().PlayOneShot(start, 0.8f); }
        public void PlayLevelClear() { if (!Muted) NextSource().PlayOneShot(clear[0], 0.9f); }
        public void PlayGameOver() { if (!Muted) NextSource().PlayOneShot(over[0], 0.9f); }

        public bool ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt("cache-cache:muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = Muted ? 0f : 1f;
            return Muted;
        }

        static AudioClip Tone(string name, float f0, float f1, float duration, float volume)
        {
            const int rate = 44100;
            var frames = Mathf.CeilToInt(rate * duration);
            var samples = new float[frames];
            var phase = 0f;
            for (var i = 0; i < frames; i++)
            {
                var t = i / (float)frames;
                var freq = Mathf.Lerp(f0, f1, t);
                phase += freq / rate;
                var env = Mathf.Min(1f, i / (rate * 0.012f)) * Mathf.Pow(1f - t, 1.6f);
                samples[i] = Mathf.Sin(phase * Mathf.PI * 2f) * env * volume;
            }
            var clip = AudioClip.Create(name, frames, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip Arpeggio(string name, float[] frequencies, float spacing)
        {
            const int rate = 44100;
            var step = Mathf.CeilToInt(rate * spacing);
            var duration = spacing * (frequencies.Length + 1f);
            var frames = Mathf.CeilToInt(rate * duration);
            var samples = new float[frames];
            for (var n = 0; n < frequencies.Length; n++)
            {
                var start = n * step;
                var phase = 0f;
                var noteFrames = Mathf.Min(step * 2, frames - start);
                for (var i = 0; i < noteFrames; i++)
                {
                    var t = i / (float)noteFrames;
                    phase += frequencies[n] / rate;
                    var env = Mathf.Min(1f, i / (rate * 0.01f)) * Mathf.Pow(1f - t, 1.8f);
                    samples[start + i] += Mathf.Sin(phase * Mathf.PI * 2f) * env * 0.5f;
                }
            }
            var clip = AudioClip.Create(name, frames, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
