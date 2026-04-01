---
title: Thumbnail Studio
---
# Thumbnail Studio

This sample allow you to create a collection of thumbnails for a collection of prefabs!

![character-male-e_Thumbnail.png](../Images/Samples/character-male-e_Thumbnail.png)

> [!NOTE]
> The Pedestal Transform applies its position & rotation to the instantiated prefab, so make sure that your objects forward
> is set correctly!

The included scene comes with a background _(Attached to the Overlay Camera, which is part of a Camera Stack)_, as well as
the pedestal. The Main utility script will allow you to associate a Transform in the scene to where the objects will be
instantiated in the scene.

By pressing **_Create Thumbnails_**, the script will iterate over each prefab, then place the created images into the 
specified directory.

![screenshot-utility.png](../Images/Samples/screenshot-utility.png)

You're able to set values for the image size & location via the `CameraScreenshotTool.cs`

![camera-screenshot-tool.png](../Images/Samples/camera-screenshot-tool.png)