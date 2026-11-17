using UnityEngine;

namespace CacheCache
{
    /// <summary>Une cachette : un farceur + une zone cliquable + son état.</summary>
    public class HideSpot : MonoBehaviour
    {
        public string spotId;
        public CharacterLook look;
        public CharacterView view;
        public Vector2 basePosition;
        public float scale = 1f;
        public bool Found { get; private set; }

        public Vector2 HeadPosition => basePosition + new Vector2(0f, 0.78f * scale);
        public float HitRadius => 0.72f * scale;

        public void MarkFound()
        {
            Found = true;
            view.SetHint(false);
            view.Celebrate();
        }

        public void Hint(bool on)
        {
            if (!Found) view.SetHint(on);
        }
    }
}
