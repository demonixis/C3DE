# C3DE : Cool 3D Engine

## The project
C3DE aims to become a 3D game engine powered by the MonoGame Framework with all features you can except from a modern "Garage Game Dev Tool". It features a Forward, Light PrePass and Deferred Renderer, as well as *Virtual Reality* support. Please take a look at features below.

![preview](http://78.media.tumblr.com/9a7fd3f3dd743e8d32c8f4e1f98ffe79/tumblr_p26hge9n4w1s15knro2_1280.jpg)

## Status & branch strategy
This project is in early stage. Use the `master` branch for testing and the `develop` branch for latest and **instable** changes. Keep in mind that all branches other than `master` are probably broken on non Windows targets.

## Features
- Scenegraph
- Component based
- 3D Model support + Custom Mesh geometry
- Terrain: Flat, Random, Heightmap
- Materials: Standard (Phong + Extras), Terrain (multi-textured) and few extras
- Lighting: Directional, Point, Spot
- Forward Renderer: Unlimlited number of light with the multipass lighting
- Light PrePass Renderer
- Deferred Renderer
- Shadow mapping (Hard shadow)
- Input management: Keyboard, Mouse, Gamepad, Touch
- Procedural texture generation
- Post Processing
- Instant GUI system
- Virtual Reality (supported on all renderers)
- Physics using Jitter Physics

## Platforms
C3DE supports Windows, Universal Windows Platform, Linux and Mac using [MonoGame](https://github.com/MonoGame/MonoGame) (both DirectX and DesktopGL). The OpenGL shader compiler is limited to the shader model 3, some effects are not supported on this platform. Android support is paused for now.

### Virtual Reality
The OpenVR support is ready to use now, if you want additional details, please [take a look at the wiki](https://github.com/demonixis/C3DE/wiki/Virtual-Reality).

## Requirement
You **must** install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer. **You'll not be able to build the solution without it**.


## Contributions
You're **free** to submit pull request, however thank you to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

## License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.
