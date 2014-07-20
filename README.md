C3DE : Cool 3D Engine
=====================

### What is it ?
C3DE is a research project to create a small but powerfull 3D engine powered by MonoGame.

#### Some experimental features

- Scene (parent/child groups)
- Component based
- Model rendering (Built and loaded thru the MonoGame content pipeline)
- Shadow mapping

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
		modelRenderer.Texture = Content.Load<Texture2D>("Models/texv1");
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
- Material support
- Easily use of custom shader
- Input management
- Post processing

### Requirement
You need a fresh copy of MonoGame assembly. It doesn't work with DirectX port at this moment because there is a bug in DrawUserIndexedPrimitive. So use OpenTK or the SDL2 port for now.

MIT License