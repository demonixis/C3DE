C3DE : Cool 3D Engine
=====================

### The project
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

### This feature is broken
As I already said, it's a **reseach project**, but you are **free** to fix it by sending a pull request. However you've to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

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
- Other: later

### Requirement
You **must** install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer.

### Editor
The editor is in **early stage** and can't be used to create level or anything for now. The project uses the **Nuget package system** for dependency management.
If you don't want to use `Nuget`, you've to install the [Extended WPF Toolkit](http://wpftoolkit.codeplex.com/) 2.5.
If you really want to try the editor, you've to follow this procedure
1. Compile the assets of the demo project using [MGCB](http://www.monogame.net/documentation/?page=MGCB) or [Pipeline](http://www.monogame.net/documentation/?page=Pipeline)
2. Compile the assets of the editor [MGCB](http://www.monogame.net/documentation/?page=MGCB) or [Pipeline](http://www.monogame.net/documentation/?page=Pipeline)
3. Compile the editor
4. Create a folder named `Content` and copy the compiled assets of the editor and the demos in it.
5. Move this folder into the build folder of the editor (`Release/bin` or `Debug/bin`)
6. Start the editor (from Visual Studio or the executable).

### License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.