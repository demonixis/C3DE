C3DE : Cool 3D Engine
=====================

### What is it ?
C3DE is a research project to create a small but powerfull 3D engine powered by MonoGame.

![](https://38.media.tumblr.com/8ea2ecf9ca3cca7bdae5a35a79643a57/tumblr_n9ovtvpnMt1s15knro2_500.jpg)

### Some features

- Scene (parent/child)
- Component based
- Cameras
 - Perspective
 - Orthographic
- Model (FBX/X with content pipeline)
- Custom Mesh geometry
 - Cube
 - Cylinder
 - Plane
 - Pyramid
 - Quad
 - Sphere
 - Torus
- Terrain
 - Flat
 - Random generated with Pelrin Noise
 - From Heightmap
- Materials
 - DiffuseSpecular
 - Water
 - Skybox
 - Custom
- Shadow mapping (Need work)
- Input management 
 - Keyboard
 - Mouse
 - Gamepad

### Sample

```C#
public class SuperCoolGame : Engine
{
	public SuperCoolGame()
	    : base()
	{
	}

	protected override void Initialize()
    {
        base.Initialize();

        // Create a camera node with an orbit controller.
        var camera = new CameraPrefab("camera");
        camera.Setup(new Vector3(0, 2, -10), Vector3.Zero, Vector3.Up);
        camera.AddComponent<OrbitController>();
        scene.Add(camera);

        // Add a light with shadows
        var sceneLight = new SceneObject();
        scene.Add(sceneLight);

        var light = sceneLight.AddComponent<Light>();
        light.ShadowGenerator.Enabled = true;

        // Add a terrain generated with Pelrin Noise.
        var terrain = new TerrainPrefab("terrain");
        scene.Add(terrain);
        
        terrain.TextureRepeat = new Vector2(16);
        terrain.Randomize(GraphicsDevice);
        terrain.Renderer.Material = new DiffuseSpecularMaterial(scene);
        terrain.Renderer.Material.MainTexture = Content.Load<Texture2D>("Textures/terrain");
    }
}
```

### What's next ?
- Post processing
- Smooth shadows
- More light types (Point, Directional, Spot, Area)
- Multipass lighting
- Instancing
- Collision management
- Editor

### Requirement
You need a fresh copy of MonoGame assembly (OpenGL or DirectX).

MIT License