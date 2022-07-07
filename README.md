# C3DE : Cool 3D Engine

## The project
C3DE aims to become a 3D game engine powered by the [MonoGame Framework](https://github.com/MonoGame/MonoGame) with all features you can except from a modern "Garage Game Dev Tool". It features a Forward, Light PrePass and Deferred Renderer, as well as *Virtual Reality* support. Please take a look at features below.

![preview](http://78.media.tumblr.com/9a7fd3f3dd743e8d32c8f4e1f98ffe79/tumblr_p26hge9n4w1s15knro2_1280.jpg)

## Status & branch strategy
This project is in early stage. **Use** the `master` branch for testing and the `develop` branch for latest and **instable** changes. Keep in mind that all branches other than `master` **are probably broken** on non Windows targets.

## Setup & Requirement
The first step is to install some MonoGame tools using the command line
```
dotnet tool install -g dotnet-mgfxc
dotnet tool install -g dotnet-mgcb
dotnet tool install -g dotnet-mgcb-editor
mgcb-editor --register
```

You need an internet connection the first time to download nuget packages.

## Build
```
# Win64 + DirectX
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained .\C3DE.Demo\Windows\C3DE.DemoGame.csproj

# Crossplatform + OpenGL
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained .\C3DE.Demo\Windows\C3DE.DemoGame.Desktop.csproj
dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained .\C3DE.Demo\Windows\C3DE.DemoGame.Desktop.csproj
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained .\C3DE.Demo\Windows\C3DE.DemoGame.Desktop.csproj
```

## Features
- Component based
- 3D Model support + Custom Mesh geometry
- Terrain: Flat, Random, Heightmap
- Materials: PBR (in progress), Standard (Phong + Extras), Terrain (multi-textured) and few extras
- Lighting: Directional, Point, Spot
- Forward Renderer: Single Pass multi-lighting, up to 128 realtime lights with Direct3D and 16 with OpenGL
- Hardware Instancing
- Reflection Probe
- Deferred Renderer (wip)
- Shadow mapping (Hard shadow only)
- Input management: Keyboard, Mouse, Gamepad, Touch
- Procedural texture generation
- Post Processing
- Instant GUI system
- Virtual Reality (supported on all renderers)
- Physics using Jitter Physics

## Platforms
| Platform  | Backend | Status |
|-----------|---------|--------|
| Windows   | DirectX | Best Version |
| DesktopGL | OpenGL  | Limited Shader Support, Limited Post Processing |
| Android   | OpenGL  | Same as DesktopGL + Limited support of the platform |
| UWP       | DirectX | On Hold |

### Help needed
I need help to port the Oculus Mobile SDK to Xamarin

### About OpenGL and MonoGame
The OpenGL shader compiler is limited to the shader model 3, some effects are not supported on this platform. 
Physically Based Rendering and Post Processing are very limited.

### Virtual Reality
C3DE supports OpenVR (headset + controllers) and Oculus (headset only), please [take a look at the wiki](https://github.com/demonixis/C3DE/wiki/Virtual-Reality).

## Contributions
You're **free** to submit pull requests, however thank you to follow the [coding convention](https://msdn.microsoft.com/en-US/library/ff926074.aspx). 

## License
C3DE is released under the MIT License, please take your time to read the LICENSE file for more informations.