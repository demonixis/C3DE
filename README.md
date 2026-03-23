# C3DE — Cool 3D Engine

A 3D game engine built on [MonoGame](https://github.com/MonoGame/MonoGame), designed for indie and hobbyist game development. C3DE features a component-based architecture, multiple rendering backends, a Jitter2 physics integration, and OpenXR-based Virtual Reality support.

![preview](http://78.media.tumblr.com/9a7fd3f3dd743e8d32c8f4e1f98ffe79/tumblr_p26hge9n4w1s15knro2_1280.jpg)

## Status

Active development. The `main` branch targets stability. Deferred rendering and Android support are work-in-progress.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- MonoGame 3.8.4.1 content tools (install once globally):

```bash
dotnet tool install -g dotnet-mgfxc
dotnet tool install -g dotnet-mgcb
dotnet tool install -g dotnet-mgcb-editor
mgcb-editor --register
```

> **macOS note:** Compiling HLSL shaders for the DesktopGL backend requires [Wine](https://www.winehq.org/) because MonoGame's shader compiler (`mgcb`) uses `fxc.exe` (DirectX Effect Compiler) internally via a .NET helper executable. Full setup:
>
> ```bash
> brew install --cask wine-stable
> brew install winetricks
>
> # Create a 64-bit Wine prefix
> WINEARCH=win64 WINEPREFIX=~/.wine-mgfxc wineboot --init
>
> # Install d3dcompiler_47 and .NET 8 runtime inside Wine
> WINEPREFIX=~/.wine-mgfxc winetricks d3dcompiler_47
> # Download dotnet-runtime-8.0.x-win-x64.exe from https://dotnet.microsoft.com/download
> WINEPREFIX=~/.wine-mgfxc wine /path/to/dotnet-runtime-8.0.x-win-x64.exe /quiet
>
> # Export the path — add this to your shell profile (~/.zshrc or ~/.bashrc)
> export MGFXC_WINE_PATH=~/.wine-mgfxc
> ```
>
> `MGFXC_WINE_PATH` must be set before running `dotnet build` or `mgcb` on macOS, otherwise shader compilation will fail.

## Build

**Windows — DirectX 11 (primary target)**
```bash
dotnet build C3DE.sln
```

**Desktop — OpenGL (Windows / Linux / macOS)**
```bash
dotnet build C3DE.Desktop.sln
```

**Android — OpenGL ES**
```bash
dotnet build C3DE.Android.sln
```

### Publish (self-contained)

```bash
# Windows x64 + DirectX
dotnet publish -c Release -r win-x64 --self-contained C3DE.Demo/Windows/C3DE.DemoGame.csproj

# Linux x64 + OpenGL
dotnet publish -c Release -r linux-x64 --self-contained C3DE.Demo/Windows/C3DE.DemoGame.Desktop.csproj

# macOS x64 + OpenGL
dotnet publish -c Release -r osx-x64 --self-contained C3DE.Demo/Windows/C3DE.DemoGame.Desktop.csproj

# macOS ARM (Apple Silicon) + OpenGL
dotnet publish -c Release -r osx-arm64 --self-contained C3DE.Demo/Windows/C3DE.DemoGame.Desktop.csproj
```

### Compile shaders

Shaders are in `C3DE.Content/`. Build them with MGCB from the relevant `.mgcb` file:

```bash
# Windows / DirectX
mgcb C3DE.Content/Shaders.mgcb

# Desktop / OpenGL
mgcb C3DE.Content/Shaders.Desktop.mgcb

# Android
mgcb C3DE.Content/Shaders.Android.mgcb
```

## Features

### Rendering
- **Forward Renderer** — single-pass multi-light, up to 128 lights on DirectX / 12 on OpenGL
- **Deferred Renderer** — G-buffer based, work-in-progress
- Unified shader codebase — single HLSL shader set for both DirectX (SM4) and OpenGL (SM3) via `#if SM4` macros
- Hardware instancing
- Reflection probes
- Shadow mapping (hard shadows)
- Skybox

### Materials
- **PBR** (Physically Based Rendering) — standard, terrain, water, lava variants
- **Standard** — Blinn-Phong + normal maps + specular maps + reflection
- **Terrain** — multi-textured with up to 4 layers, atlas support
- Unlit, Water, Lava, and custom materials

### Lights
- Directional, Point, Spot lights
- Ambient lighting

### Post-Processing
- Bloom, Motion Blur, Vignette, Global Fog, SSAO, SSGI, Tonemapping
- Retro filters (C64, CGA), Film, Grayscale

### Scene & Entities
- Component-based `GameObject` / `Component` system
- Hierarchical `Transform`
- Multi-scene management with `SceneManager`

### Terrain
- Flat, procedural, and heightmap-based terrain
- Multi-textured with blending
- Procedural texture generation

### Physics
- [Jitter2](https://github.com/notgiven688/jitterphysics2) integration
- Box and Sphere colliders
- Rigidbody (dynamic, static, kinematic)
- Raycast

### Input
- Keyboard, Mouse, Gamepad, Touch
- Virtual gamepad overlay for mobile

### Virtual Reality
- [OpenXR](https://www.khronos.org/openxr/) via [Silk.NET.OpenXR](https://github.com/dotnet/Silk.NET)
- Compatible with any OpenXR runtime: SteamVR, Oculus/Meta, Windows Mixed Reality
- Per-eye rendering, FoV-based projection, motion controllers

## Platforms

| Platform    | Backend    | Status               |
|-------------|------------|----------------------|
| Windows     | DirectX 11 | Supported            |
| Linux       | OpenGL     | Supported            |
| macOS       | OpenGL     | Supported            |
| Android     | OpenGL ES  | Work in progress     |

> The DesktopGL backend uses a unified shader set shared with DirectX. The SM3/OpenGL path keeps a statically bounded light loop for MojoShader and currently targets 12 lights; SM4 keeps the richer spot-light and high-light-count path behind `#if SM4`.

## Project Structure

```
C3DE/                    Core engine library
C3DE.Demo/
  Shared/                Shared demo scenes and scripts
  Windows/               Demo executable (DX and DesktopGL)
  Morrowind/             Morrowind recreation demo (WIP)
C3DE.Editor/             Scene editor application
C3DE.GwenUI/             Gwen UI library integration
C3DE.Content/            MGCB content projects + HLSL shaders
```

## Dependencies

| Package                          | Version  | Purpose                        |
|----------------------------------|----------|--------------------------------|
| MonoGame.Framework.WindowsDX     | 3.8.4.1  | DirectX backend                |
| MonoGame.Framework.DesktopGL     | 3.8.4.1  | OpenGL cross-platform backend  |
| MonoGame.Framework.Android       | 3.8.4.1  | Android backend                |
| Jitter2                          | 2.7.7    | Physics simulation             |
| Silk.NET.OpenXR                  | 2.23.0   | OpenXR VR support              |
| Newtonsoft.Json                  | 13.0.1   | JSON serialization (demo)      |

## Contributing

Pull requests are welcome. Please follow the [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

## License

C3DE is released under the [MIT License](LICENSE.txt).
