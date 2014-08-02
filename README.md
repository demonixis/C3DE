C3DE : Cool 3D Engine
=====================

### What is it ?
C3DE is a research project to create a small but powerfull 3D engine powered by MonoGame.

#### Some features

- Scene (parent/child groups)
- Camera (Perspective and Orthographic)
- Component based
- Model rendering (Built and loaded thru the MonoGame content pipeline)
- Custom Mesh rendering : Cube, Sphere, Cylinder and more !
- Terrain : Flat, Random (with Pelrin Noise) and from Heightmap
- Materials (DiffuseSpecular, Water, Skybox)
- Shadow mapping (Need smoothing now)
- Skybox
- Input management (Keyboard, Mouse and Gamepad)

```C#
public class SuperCoolGame : Game
{
	GraphicsDeviceManager graphics;
	Renderer renderer;
	Scene scene;
	Camera camera;

	public SuperCoolGame()
	{
		graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
	}

	protected override void LoadContent()
	{
		renderer = new Renderer(GraphicsDevice);
		renderer.LoadContent(Content);
		
		scene = new Scene();
		camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

		// Space Ship
		var spaceShip = new SceneObject();
		spaceShip.Transform.Scale = new Vector3(0.25f);
		scene.Add(spaceShip);

		var modelRenderer = spaceShip.AddComponent<ModelRenderer>();
		modelRenderer.Model = Content.Load<Model>("Models/spaceship");
		modelRenderer.MainTexture = Content.Load<Texture2D>("Models/texv1");
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		renderer.render(scene, camera);
		base.Draw(gameTime);
	}
}
```

### What's next ?
- Post processing
- Smooth and nice shadows
- Lighting support
- More

### Requirement
You need a fresh copy of MonoGame assembly (OpenGL or DirectX).

MIT License