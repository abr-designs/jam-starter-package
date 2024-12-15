using UnityEngine;
using UnityEngine.Assertions;

namespace Audio.SoundFX
{
    public static class SFXExtensions
    {
        public static void PlaySoundAtLocation(this SFX sfx, Vector3 worldPosition)
        {
            var sfxManager = SfxManager;
            
            Assert.IsNotNull(sfxManager, $"Missing the {nameof(SfxManager)} in the Scene!!");
            sfxManager.PlaySoundAtLocation(sfx, worldPosition);
        }
        
        public static void PlaySound(this SFX sfx, float volume = 1f)
        {
            var sfxManager = SfxManager;
            
            Assert.IsNotNull(sfxManager, $"Missing the {nameof(SfxManager)} in the Scene!!");
            sfxManager.PlaySound(sfx, volume);
        }

        private static SFXManager SfxManager => SFXManager.Instance;
    }
}