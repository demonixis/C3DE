C3DE : Cool 3D Engine
=====================

### The project
C3DE is a research project. The aim is to create a modest but powerful enough 3D engine powered by the MonoGame Framework.

### Some Features
- Scenegraph
- Component based
- 3D Model (FBX/X) (1)
- Custom Mesh geometry
- Terrain: Flat, Random, Heightmap, multi textured
- Materials: Standard, Simple, Reflective, Water, Lava, Custom
- Shadow mapping (Hard shadow)
- Input management: Keyboard, Mouse, Gamepad, Touch
- Procedural texture generation
- Post Processing support (need rewrite)
- Instant GUI system
- Virtual Reality Support (OSVR, OpenHMD) (2)

(1) Models support is not yet complete because the engine uses its own format for rendering things. It's planned later in the roadmap to convert an XNA model into a C3DE model.
(2) Other VR vendors are coming.


### Platforms
C3DE supports Windows using MonoGame/DirectX and Windows, Linux and Mac using MonoGame/DesktopGL. Because of the OpenGL shader compiler, shaders have less features than DirectX ones.
Android was supported in a previous version of the engine, it's not yet maintened **for now**, but will be revived later.

### This feature is broken
As I already said, it's a **reseach project**, but you are **free** to fix it by sending a pull request. However you've to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

### Requirement
You **must** install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer. **You'll not be able to build the solution without it**.

### What's next ?
- VR Support (in progress)
- Post processing (manager to allow chaining)
- PreLightRenderer (in progress)
- Physics engine
- Game Editor / Player

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
### License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.
