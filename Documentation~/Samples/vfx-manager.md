# VFX Manager
> **_NOTE You will want to create a prefab variant of the VFX Manager prefab, to ensure that your changes are maintained!_**

The intention of the VFX Manager is to allow you to easily create library of effect prefabs, that can then be easily called
anywhere within your code, without needing to worry about managing the lifespan of that newly created GameObject. This
also avoids needing to find `VFXManager.cs` within the scene, so long as the prefab exists within the scene.

![VFX Manager](../Images/vfx_manager.png)

You can add new enum values into `VFX.cs`, as this will operate in two locations:

1. The `VFX MANAGER` prefab, can then add a new entry
    - ```csharp
      public enum VFX
      {
         NONE = 0,
         MY_EFFECT = 1
      }      
2. You can then use that enum value in code to instantiate the prefab
    - ```csharp
      VFX.MY_EFFECT.PlayAtLocation(transform.position, scale: 1f, keepAlive: false);

> `keepAlive`: This will ignore the specified lifetime set on the `VFX MANAGER` prefab