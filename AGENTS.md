# AGENTS.md — C3DE Engine

This file provides context for AI-assisted development on the C3DE project.

## Profil attendu

Adopter systématiquement le profil d'un **expert C# / MonoGame / HLSL / programmeur 3D bas niveau** :
- Connaissance approfondie du pipeline de rendu GPU (vertex/pixel shaders, render targets, depth buffer, blending states)
- Maîtrise de MonoGame : `GraphicsDevice`, `Effect`, `VertexBuffer`, `IndexBuffer`, `SpriteBatch`, content pipeline
- HLSL : semantic registers, constant buffers, texture samplers, SM3 vs SM4, optimisation des instructions
- Programmation C# orientée performance : éviter les allocations sur le hot path, préférer les structs, `Span<T>`, `stackalloc`, `ref`/`in`/`out`, `unsafe` quand justifié
- Connaissances 3D : matrices de transformation, quaternions, frustum culling, BVH, LOD, lightmaps, PBR (Cook-Torrance, GGX, IBL)

## Philosophie de développement : Data-Oriented

**Le projet actuel est orienté objet classique (héritage, listes de classes abstraites). L'objectif à terme est d'évoluer vers une architecture Data-Oriented (DOD) pour maximiser les performances.**

Principes à appliquer lors de toute nouvelle implémentation ou refactoring significatif :

- **Préférer les structs aux classes** pour les données lues/écrites fréquemment (positions, matrices, données de lumières). Les structs évitent l'indirection mémoire et sont cache-friendly.
- **Tableaux continus plutôt que listes de références** — `T[]` ou `NativeArray<T>` plutôt que `List<T>` de classes pour les données parcourues en boucle (ex: positions de lights, matrices monde des renderers).
- **Séparer les données du comportement** — ne pas mélanger logique métier et données dans la même classe. Exemple : `LightData` est un struct de données pur, `Light` est le composant comportemental.
- **Minimiser les allocations heap sur le hot path** (Update, Draw) : pas de `new`, pas de LINQ, pas de `foreach` sur des `IEnumerable` boxés. Utiliser `for` avec index, `ArraySegment`, `Span`.
- **Pré-allouer** les buffers et tableaux en `Initialize()` ou au premier accès, avec une capacité suffisante. Ne jamais redimensionner en cours de frame.
- **Structures de données SoA (Structure of Arrays) plutôt que AoS (Array of Structures)** pour les données envoyées au GPU : `float[] positions`, `float[] colors` séparés plutôt qu'un tableau de structs `{ position, color }`.

Exemple concret déjà appliqué dans le projet — `LightData` dans `ForwardRenderer` :
```csharp
// Bon : buffer pré-alloué avec capacité minimale
if (_lightData.Positions == null || _lightData.Positions.Length < lightCount)
{
    var capacity = Math.Max(lightCount, 16);
    _lightData.Positions = new Vector3[capacity];
    // ...
}
```

Le codebase existant ne respecte pas encore ces principes partout — **ne pas tout refactorer d'un coup**, mais appliquer ces règles sur tout nouveau code ou modification substantielle.

## Maintien de la documentation

- **`README.md`** : mettre à jour si une fonctionnalité est ajoutée, supprimée, ou si les prérequis / commandes de build changent.
- **`AGENTS.md`** : mettre à jour dès qu'une modification architecturale est faite (nouveau système, changement d'API clé, nouvelle dépendance, convention modifiée). Ce fichier est la source de vérité pour le travail assisté par IA.
- **Commentaires XML** (`///`) : ajouter ou mettre à jour sur les méthodes publiques non triviales modifiées.

## Project Overview

C3DE is a 3D game engine built on MonoGame 3.8.4.1 / .NET 8. It uses a component-based architecture inspired by Unity, with support for multiple rendering backends, Jitter2 physics, and OpenXR VR.

## Solution Structure

| Solution               | Target                          | Key Projects                          |
|------------------------|---------------------------------|---------------------------------------|
| `C3DE.sln`             | Windows + DirectX               | C3DE, C3DE.DemoGame, C3DE.Editor      |
| `C3DE.Desktop.sln`     | Cross-platform + OpenGL         | C3DE.Desktop, C3DE.DemoGame.Desktop   |
| `C3DE.Android.sln`     | Android                         | C3DE.Android, C3DE.Demo.Android       |

Each engine feature project has two variants:
- `C3DE.csproj` → `net8.0-windows` + `MonoGame.Framework.WindowsDX`
- `C3DE.Desktop.csproj` → `net8.0` + `MonoGame.Framework.DesktopGL`

## Build Commands

```bash
# Build (Desktop/OpenGL — works on macOS/Linux/Windows)
dotnet build C3DE.Desktop.sln

# Build (Windows/DirectX — Windows only)
dotnet build C3DE.sln

# Build a single project
dotnet build C3DE/C3DE.Desktop.csproj
```

> **macOS:** `MGFXC_WINE_PATH` must be exported before building. Add to `~/.zshrc`:
> ```bash
> export MGFXC_WINE_PATH=~/.wine-mgfxc
> ```
> One-time Wine prefix setup: `WINEARCH=win64 WINEPREFIX=~/.wine-mgfxc wineboot --init`, then `winetricks d3dcompiler_47`, then install .NET 8 Win64 runtime inside Wine. Without this, `dotnet build` will fail on shader compilation with exit code 44.

## Architecture

### Core Types

- **`Engine`** (`C3DE/Engine.cs`) — extends `Microsoft.Xna.Framework.Game`. Entry point.
- **`Application`** — static singleton, provides access to `GraphicsDevice`, `Content`, `Engine`.
- **`Scene`** (`C3DE/Scene.cs`) — GameObject container. Owns physics world, renderer lists.
- **`SceneManager`** — loads/unloads scenes, maintains `Scene.current`.
- **`GameObject`** — base entity, has a `Transform` and a list of `Component`.
- **`Component`** — base class for all behaviours. Lifecycle: `Awake → Initialize → Update → Dispose`.
- **`Transform`** — position/rotation/scale with parent-child hierarchy. Has `_dirty` flag.
- **`Behaviour`** — extends `Component`, adds `OnGUI` for UI rendering.

### Rendering

Renderers live in `C3DE/Graphics/Rendering/`:

- **`BaseRenderer`** — abstract, handles RenderTarget setup, UI, VR wrapping.
- **`ForwardRenderer`** — primary renderer. Single-pass multi-light, organized in partial classes (`Buffers`, `Scene`, `Lighting`) to keep the forward path cohesive without moving shared RT/UI/VR infrastructure out of `BaseRenderer`.

The active renderer is set on `Application.Engine`.

Scene sorting:
- `Scene._renderList` — `List<Renderer>` for 3D meshes
- `Scene._lights` — `List<Light>`
- `Scene._cameras` — `List<Camera>`
- `Scene._scripts` — `List<Behaviour>` (for `Update` and `OnGUI`)
- `RenderSettings.PostProcessing` — scene-wide post-process stack configuration consumed by the renderer-owned pipeline

### Shader System

Shaders are in `C3DE.Content/Shaders/`. There is a **single unified shader set** for both DirectX (SM4) and OpenGL (SM3):

- `Common/Macros.fxh` — defines `#if SM4` / `SM3` macros
- `Forward/Standard.fx` — main opaque material shader (Blinn-Phong lighting)
- `Forward/StandardBase.fx` — shared Standard-family shading core (lighting accumulation, reflection, fog composition)
- `Forward/StandardLighting.fxh` — forward light loop and attenuation helpers; SM3 stays statically bounded for MojoShader, SM4 keeps the richer path

Features behind `#if SM4` (DX-only): spot lights in the shader path and the high light-count ceiling.
Features available on both SM3 and SM4: Blinn-Phong lighting, normal maps, shadow maps, fog, reflection, cutout.

`Skybox` now supports two rendering modes:
- `Cubemap` — legacy static cube-map sky
- `Procedural` — semi-realistic procedural sky driven by `Skybox.ProceduralSettings`

The procedural sky mode is driven by `ProceduralSkyController` and can drive the primary directional light (`IsSun` first, otherwise first active directional) for day/night behavior.

Forward lighting data is packed in `LightData` as SoA buffers and preallocated by capacity in `ForwardRenderer`:
- `Positions[i]` — point/spot position, or normalized directional light vector for directional lights
- `Colors[i]` — RGB light color
- `Data[i]` — `(type, intensity, range, falloff)` with type encoded as `0=directional`, `1=point`, `2=spot`
- `SpotData[i]` — `(direction.xyz, angle)` for spot lights

C# shader classes use null-conditional `?.` on `Effect.Parameters["name"]` to safely skip parameters that don't exist in SM3 compiled shaders:
```csharp
_effect.Parameters["SpotData"]?.SetValue(lightData.SpotData);
```

### Post-Processing

The legacy `Scene._postProcessPasses` list still exists for compatibility, but the renderer pipeline now relies on a centralized stack owned by `RenderSettings.PostProcessing`:

- `PostProcessStack` — renderer-owned orchestrator
- dedicated passes kept for `Bloom` and `Ambient Occlusion`
- `FastBloom.fx` — bloom prefilter / blur pipeline, with soft threshold support and clamped sampling
- `AmbientOcclusion.fx` — raw AO + depth-aware blur passes
- `PostComposite.fx` — final uber-style composite pass for tonemapping, color controls, white balance, lift/gamma/gain, sharpen, vignette, FXAA, sun flare, and debug views (`Final`, `Scene`, `Bloom`, `AO`)

DesktopGL is the primary constraint for this stack:
- prefer a single final full-screen composite pass when effects can be merged
- keep expensive effects (`Bloom`, `SSAO`, later `SSR` / `SSGI`) outside the uber pass
- avoid depending on GPU occlusion queries for sun flare; use post-process visibility heuristics instead

### Physics (Jitter2)

Physics is in `C3DE/Components/Physics/`.

- `Scene._physicsWorld` is a `Jitter2.World` instance
- `_physicsWorld.Step(Time.DeltaTime, true)` is called in `Scene.Update()`
- `Rigidbody` creates bodies via `world.CreateRigidBody()` (lazy, in `SetShape`)
- `body.MotionType` — `Static`, `Dynamic`, or `Kinematic` (replaces Jitter1's `IsStatic`)
- `body.Velocity` (not `LinearVelocity`), `body.AngularVelocity`
- `body.Orientation` is `JQuaternion` (not `JMatrix` like Jitter1)
- `body.AddShape(RigidBodyShape)` — add shape; `body.RemoveShape(shape)` to clear
- `world.Remove(body)` — removes body from simulation

Conversion helpers on `Rigidbody`: `ToVector3`, `ToJVector`, `ToMatrix(JQuaternion)`, `ToJQuaternion`.

### VR System

VR lives in `C3DE/VR/`.

- `VRService` — abstract base class (extends `GameComponent`)
- `OpenXRService` — Silk.NET.OpenXR implementation (`C3DE/VR/OpenXR/OpenXRService.cs`)
- `VRManager.GetVRAvailableVRService()` — discovers and initializes the first working driver
- `NullVRService` — fallback, no-op implementation

**Known limitation**: `OpenXRService.CreateSession()` requires a platform-specific graphics binding (D3D11 handle on Windows, OpenGL context on Desktop). MonoGame does not expose the native device handle. This must be completed with platform interop before VR is functional.

### Content Pipeline

`C3DE.Content/` contains:
- `Shaders.mgcb` — Windows/DX shader build config
- `Shaders.Desktop.mgcb` — Desktop/GL shader build config
- `Shaders.Android.mgcb` — Android shader build config

Demo content is in `C3DE.Demo/Shared/Content/Content.mgcb`.

## Platform Notes

| `#define`   | Platform              | MonoGame backend             |
|-------------|-----------------------|------------------------------|
| `WINDOWS`   | Windows + DirectX     | `MonoGame.Framework.WindowsDX` |
| `DESKTOP`   | Cross-platform OpenGL | `MonoGame.Framework.DesktopGL` |
| `ANDROID`   | Android               | `MonoGame.Framework.Android` |

Use `#if WINDOWS` / `#if DESKTOP` / `#if ANDROID` for platform-specific code.

## Key Conventions

- Methods use `PascalCase`, fields use `_camelCase` with underscore prefix for private fields
- `protected internal` is used frequently to expose scene internals to renderers
- `Component` subclasses override `Initialize()`, `Update()`, `Dispose()` — always call `base.`
- `Renderer` subclasses override `Draw(GraphicsDevice device)` and `ComputeBoundingInfos()`
- Material shader classes inherit from `ShaderMaterial` and override `LoadEffect()` and `Pass()`

## Common Pitfalls

- **Shader parameter null check**: always use `?.SetValue()` not `.SetValue()` for optional shader parameters — SM3 shaders don't have all SM4 parameters
- **Rigidbody lazy init**: `_rigidBody` is null until `SetShape()` is called; guard all property accesses
- **`Scene.current`**: may be null during initialization — guard accordingly
- **macOS shader compilation**: requires `MGFXC_WINE_PATH` env var pointing to a Wine prefix that has `d3dcompiler_47` + .NET 8 Win64 runtime. Without it, `dotnet build` exits with code 44 on the MGCB step. See Build Commands above for setup.
- **`net8.0-windows` on macOS**: will fail with `NETSDK1073` for WinForms references — expected, build `C3DE.Desktop.sln` instead
