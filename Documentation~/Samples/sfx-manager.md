# SFX Manager

> **_NOTE You will want to create a prefab variant of the `Audio Controllers` prefab, to ensure that your changes are maintained!_**

The `AudioControllers` prefab manages both Music & SFX, and should be in the scene if you intend to call it!

The intention of the SFX Manager is to allow you to easily play SFX, that can then be easily called
anywhere within your code, without needing to worry about managing the lifespan SFX, automatically setting sound varieties,
channel limits, as well as volume limits

![SFX Manager](../Images/sfx_manager.png)
- You can use `Max Playing` to help prevent spam. When the value is `0` it will play unlimited.
- `AudioClips` list will choose a random sound from the list when called, you can use just a single sound if you like.

You can add new enum values into `SFX.cs`, as this will operate in two locations:

1. The `Audio Controllers` prefab, can then add a new entry
    - ```csharp
      public enum SFX
      {
         NONE = 0,
         MY_SOUND = 1
      }      
2. You can then use that enum value in code to play the sound, optionally at a world location
    - ```csharp
      SFX.MY_SOUND.PlaySound(volume: 1f);
    - ```csharp
      SFX.MY_SOUND.PlaySoundAtLocation(transform.position);