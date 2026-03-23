#include "../Common/Macros.fxh"
#include "StandardLighting.fxh"
#include "../Common/Fog.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
int SpecularPower;

// Misc
float3 EyePosition;

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
#if REFLECTION_MAP
	float3 Reflection : TEXCOORD3;
#endif
#if SM4
    float3x3 WorldToTangentSpace : TEXCOORD4;
#endif
    // FOG semantic maps to gl_FogFragCoord which is removed in OpenGL Core Profile 3.2+
    // Use a regular TEXCOORD register instead (TEXCOORD7 is free even with TBN on SM4).
    float FogDistance : TEXCOORD7;
};

struct VSOutput_VL
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float3 Color : TEXCOORD3;
#if REFLECTION_MAP
	float3 Reflection : TEXCOORD4;
#endif
	float FogDistance : TEXCOORD7;
};

// ---
// --- Pixel Lighting Vertex Shader
// ---
VertexShaderOutput CommonVS(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, (float3x3)instanceTransform);
    output.WorldPosition = worldPosition;
    output.FogDistance = distance(worldPosition.xyz, EyePosition);

#if SM4
    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;
#endif

#if REFLECTION_MAP
	float3 viewDirection = EyePosition - worldPosition.xyz;
	output.Reflection = reflect(-normalize(viewDirection), normalize(input.Normal));
#endif

    return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	return CommonVS(input, World);
}

VertexShaderOutput MainVS_Instancing(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return CommonVS(input, mul(World, transpose(instanceTransform)));
}

float3 PrepareViewDirection(float3 worldPosition)
{
    return normalize(EyePosition - worldPosition);
}

float3 ApplyReflectionToAlbedo(float3 albedo, float4 reflection)
{
    if (reflection.a > 0)
        return lerp(albedo, reflection.xyz, reflection.a);

    return albedo;
}

float3 AccumulateStandardLighting(float3 worldPosition, float3 normal, float3 specularTerm)
{
    float3 light = FLOAT3(0.0);
    float3 viewDirection = PrepareViewDirection(worldPosition);

#if SM4
    int limit = min(MAX_LIGHT_COUNT, LightCount);
    for (int i = 0; i < limit; i++)
        light += CalculateOneLight(i, worldPosition, normal, viewDirection, specularTerm, SpecularPower);
#else
    // SM3/MojoShader: keep the loop statically bounded and branch-light.
    [unroll]
    for (int i = 0; i < MAX_LIGHT_COUNT; i++)
    {
        if (i < LightCount)
            light += CalculateOneLight(i, worldPosition, normal, viewDirection, specularTerm, SpecularPower);
    }
#endif

    return light;
}

float3 ComposeStandardColor(float3 albedo, float3 emissive, float shadowTerm, float3 light, float fogDistance, float4 reflection)
{
    albedo = ApplyReflectionToAlbedo(albedo, reflection);
    float3 color = AmbientColor + (albedo * light * shadowTerm) + emissive;
    color = ApplyFog(color, fogDistance);

    return color;
}

// Standard Pixel Shader for Per Pixel Lighting
float3 StandardPixelShader(float4 worldPosition, float3 normal, float3 specularTerm, float fogDistance, float3 albedo, float3 emissive, float shadowTerm, float4 reflection)
{
    float3 light = AccumulateStandardLighting(worldPosition.xyz, normal, specularTerm);
    return ComposeStandardColor(albedo, emissive, shadowTerm, light, fogDistance, reflection);
}

// Standard Pixel Shader for Vertex Lighting
float3 StandardPixelShader_VL(float3 light, float fogDistance, float3 albedo, float3 emissive, float shadowTerm, float4 reflection)
{   
	return ComposeStandardColor(albedo, emissive, shadowTerm, light, fogDistance, reflection);
}
