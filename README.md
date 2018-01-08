C3DE : Cool 3D Engine
=====================

## The project
C3DE aims to become a 3D game engine powered by the MonoGame Framework with all features you can except from a modern "Garage Game Dev Tool".

![preview](http://78.media.tumblr.com/9a7fd3f3dd743e8d32c8f4e1f98ffe79/tumblr_p26hge9n4w1s15knro2_1280.jpg)

## Status & branch strategy
This project is in early stage. Use the `master` branch for testing and the `develop` branch for latest and **instable** changes. Keep in mind that all branches other than `master` are brobably broken on non Windows targets.

## Features
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
- Virtual Reality
- Physics (WIP)
- Deferred Renderer (WIP)

## Platforms
C3DE supports Windows, Universal Windows Platform, Linux and Mac using MonoGame (both DirectX and DesktopGL). The OpenGL shader compiler is limited to the shader model 3, so some effects or materials will not work or will have less features.
Android support is paused for now.

### Virtual Reality
| VR Vendor | Status | Work to do |
|-----------|--------|------------|
| OpenVR    | OK     | Get buttons state |
| OSVR      | Almost OK | Side by side only (no RenderManager) |
| OpenHMD   | OK     | Needs better correction distortion and controllers support |
| Oculus Rift | Dev | Many crashes to fix |

## Requirement
You **must** install the [MonoGame Framework](http://www.monogame.net/downloads/) from the installer. **You'll not be able to build the solution without it**.

## Short time road map
- Physics Engine
- Deferred Renderer

## What's next ?
- Unified Input System
- HDR / Tone Mapping
- NavMesh 
- New UI framework
- Editor
- Player

## Contributions
It's an **experimental project**, but you are **free** to fix it by sending a pull request. However you've to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

## License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.
