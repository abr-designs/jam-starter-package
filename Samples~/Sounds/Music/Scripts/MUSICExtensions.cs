using UnityEngine.Assertions;

namespace Audio.Music
{
    public static class MUSICExtensions
    {
        public static void PlayMusic(this MUSIC music)
        {
            var musicController = MusicController;

            Assert.IsNotNull(musicController, $"Missing the {nameof(MusicController)} in the Scene!!");
            musicController.PlayMusic(music);
        }

        private static MusicController MusicController => MusicController.Instance;
    }
}