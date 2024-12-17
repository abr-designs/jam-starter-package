# Music Manager

> **_NOTE You will want to create a prefab variant of the `Audio Controllers` prefab, to ensure that your changes are maintained!_**

The `AudioControllers` prefab manages both Music & SFX, and should be in the scene if you intend to call it!


The intention of the Music Manager is to allow you to easily play Music, that can then be easily called
anywhere within your code, without needing to worry about fading the music, or setting its volume.

![Music Manager](../Images/music_manager.png)
- You can use `Cross Fade Time` to apply a global total time to move between music clips

You can add new enum values into `MUSIC.cs`, as this will operate in two locations:

1. The `Audio Controllers` prefab, can then add a new entry
    - ```csharp
      public enum MUSIC
      {
          NONE,
          MENU,
          GAME,
     }
2. You can then use that enum value in code to play the sound, optionally at a world location
    - ```csharp
      MUSIC.MENU.PlayMusic();