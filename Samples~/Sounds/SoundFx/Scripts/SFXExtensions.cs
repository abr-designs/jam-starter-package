using UnityEngine;

namespace Audio.SoundFX
{
    public static class SFXExtensions
    {
        public static void PlaySoundAtLocation(this SFX sfx, Vector3 worldPosition)
        {
            SFXManager.PlaySoundAtLocation(sfx, worldPosition);
        }
        
        public static void PlaySound(this SFX sfx, float volume = 1f)
        {
            SFXManager.PlaySound(sfx, volume);
        }
    }
}