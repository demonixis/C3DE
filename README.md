C3DE : Cool 3D Engine
=====================

### The project
C3DE is a research project. The aim is to create a modest but powerful enough 3D engine powered by the MonoGame Framework.

### Features
- Scenegraph
- Component based
- 3D Model support
- Custom Mesh geometry
- Terrain: Flat, Random, Heightmap, multi textured
- Materials: Standard (Phong + Extras), Terrain (multi-textured) and few extras
- Lighting: Directional, Point, Spot
- Unlimited number of lights (Forward Renderer)
- Shadow mapping (Hard shadow)
- Input management: Keyboard, Mouse, Gamepad, Touch
- Procedural texture generation
- Post Processing
- Instant GUI system
- Virtual Reality Support

### Platforms
C3DE supports Windows, Linux and Mac using MonoGame (both DirectX and DesktopGL). Because of the OpenGL shader compiler is limited to shader model 3, this version has less graphics features than DirectX. 
For now Android and Windows UWP supports are paused the time the engine is more stable.

### This feature is broken
It's an **experimental project**, but you are **free** to fix it by sending a pull request. However you've to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

### Requirement
You **must** install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer. **You'll not be able to build the solution without it**.

### What's next ?
- VR Support (in progress)
- Physics engine
- Unified Input System
- HDR
- Tone Mapping
- Windows UWP

### License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.
