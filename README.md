C3DE : Cool 3D Engine
=====================

### What is it ?
C3DE is a research/learning project. The aim is to create a modest but powerful enough 3D engine powered by the MonoGame Framework. This project is composed of
* An engine
* An editor (Windows only for now)
* A player (later)

![](http://41.media.tumblr.com/f184d022630b0bc245246b146ace8cc7/tumblr_nrzrcmzCLo1s15knro1_1280.png)

### Features

- Scene (parent/child)
- Component based
- 3D Model (FBX/X)*
- Custom Mesh geometry)
- Terrain: Flat, Random, Heightmap, multi textured
- Materials: Standard, Simple, Reflective, Water, Lava, Custom
- Shadow mapping (Hard shadow)
- Input management: Keyboard, Mouse, Gamepad, Touch
- Procedural texture generation
- Post Processing support (need rewrite)
- Multiple cameras
- UI management (Button, Checkbox, Label, Slider, Texture) 

* Models support is not yet complete because the engine uses its own format for rendering things. It's planned later in the roadmap to convert an XNA model into a C3DE model.

### Sample

```C#
public class SuperCoolGame : Scene
{
	protected override void Initialize()
    {
        base.Initialize();

        // Create a camera node with an orbit controller.
        var camera = new CameraPrefab("camera");
        camera.AddComponent<OrbitController>();
		Add(camera);

        // Add a light with shadows
        var lightPrefab = new LightPrefab("light", LightType.Directional);
        lightPrefab.EnableShadows = true;
		Add(lightPrefab);

        // Add a terrain generated with Pelrin Noise.
        var terrain = new TerrainPrefab("terrain");
        terrain.TextureRepeat = new Vector2(16);
        terrain.Randomize();
        terrain.Renderer.Material = new StandardMaterial(this);
        terrain.Renderer.Material.MainTexture = Content.Load<Texture2D>("Textures/terrain");
		Add(terrain);
    }
}
```

### What's next ?
- Post processing (manager to allow chaining)
- More light types (Spot, Area)
- PreLightRenderer (in progress)
- True collision system / Physics engine
- Network (Check the network branch)
- Player for loading a game made with the editor

### Supported Platforms
- Windows (DirectX)

I'm currently in a big refactoring step. Due to the complexity of managing multiples solutions, the develop branch only supports Windows. When the refactoring will be done, I'll re add support for
- Desktop GL (Linux, Mac, Windows)
- Android
- Windows Universal Apps

### Requirement
You must install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer or update the solution with your custom build of the Framework.
If you want to build the editor or the demos, you must install MonoGame because projects uses the new Content Build System.

### Editor
First you have to build the demo project and copy the content folder with all xnb into the generated build folder of the editor.

### Licence
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.