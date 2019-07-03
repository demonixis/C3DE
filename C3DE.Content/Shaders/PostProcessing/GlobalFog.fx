float4 HeightParams;
float4 DistanceParams;
float4 FogMode;
float4 FogParams;
float4 FogColor;
float4 FogDensity;
float4 TextureSamplerTexelSize;
float4x4 FrustumCornersWS;
float4 CameraWS;
float4 ProjectionParams;
int Testing;

texture TargetTexture;
sampler2D textureSampler = sampler_state
{
    Texture = <TargetTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture DepthTexture;
sampler2D depthSampler = sampler_state
{
    Texture = <DepthTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD;
};

struct PixelShaderOutput
{
    float4 Position : SV_Position;
    float2 UV : TEXCOORD;
    float2 UV_Depth : TEXCOORD1;
    float4 InterpolatedRay : TEXCOORD2;
};

// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
float ComputeFogFactor(float coord)
{
    float fogFac = 0.0;

    if (FogMode.x == 1) // linear
        fogFac = coord * FogParams.z + FogParams.w;

    if (FogMode.x == 2) // exp
    {
        fogFac = FogParams.y * coord;
        fogFac = exp2(-fogFac);
    }

    if (FogMode.x == 3) // exp2
    {
        fogFac = FogParams.x * coord;
        fogFac = exp2(-fogFac * fogFac);
    }

    return saturate(fogFac);
}

// Distance-based fog
float ComputeDistance(float3 camDir, float zdepth)
{
    float dist;

    if (FogMode.y == 1)
        dist = length(camDir);
    else
        dist = zdepth * ProjectionParams.z;

    dist -= ProjectionParams.y;

    return dist;
}

// Linear float-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
float ComputefloatSpace(float3 wsDir)
{
    float3 wpos = CameraWS.xyz + wsDir;
    float FH = HeightParams.x;
    float3 C = CameraWS;
    float3 V = wsDir;
    float3 P = wpos;
    float3 aV = HeightParams.w * V;
    float FdotC = HeightParams.y;
    float k = HeightParams.z;
    float FdotP = P.y - FH;
    float FdotV = wsDir.y;
    float c1 = k * (FdotP + FdotC);
    float c2 = (1 - 2 * k) * FdotP;
    float g = min(c2, 0.0);
    g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
    return g;
}

float Linear01Depth(float z)
{
    float zc0 = 1.0 - ProjectionParams.z / ProjectionParams.y;
    float zc1 = ProjectionParams.z / ProjectionParams.y;
    return 1.0 / (zc0 * z + zc1);
}

float4 ComputeFog(PixelShaderOutput input, bool distance, bool height)
{
    float4 sceneColor = tex2D(textureSampler, input.UV);
    float rawDepth = tex2D(depthSampler, input.UV_Depth);
    float dpth = Linear01Depth(rawDepth);
    float4 wsDir = dpth * input.InterpolatedRay;
    float4 wsPos = CameraWS + wsDir;

    float g = DistanceParams.x;
    if (distance)
        g += ComputeDistance(wsDir.xyz, dpth);
    if (height)
        g += ComputefloatSpace(wsDir.xyz);

    float fogFac = ComputeFogFactor(max(0.0, g));

    if (dpth == DistanceParams.y)
        fogFac = 1.0;

    return lerp(FogColor, sceneColor, fogFac);
}

PixelShaderOutput VertexShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    output.Position = input.Position;
    output.UV = input.UV;
    output.UV_Depth = input.UV;
		
    int frustumIndex = input.UV.x + (2 * input.UV.y);
    output.InterpolatedRay = FrustumCornersWS[frustumIndex];
    output.InterpolatedRay.w = frustumIndex;
    return output;
}

float4 PS_DistanceHeight(PixelShaderOutput input) : COLOR0
{
    return ComputeFog(input, true, true);
}

float4 PS_Distance(PixelShaderOutput input) : COLOR0
{
    return ComputeFog(input, true, false);
}

float4 PS_Height(PixelShaderOutput input) : COLOR0
{
    return ComputeFog(input, false, true);
}

technique Technique0
{
    pass DistanceHeight
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PS_DistanceHeight();
#else
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PS_DistanceHeight();
#endif
    }

    pass Distance
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PS_Distance();
#else
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PS_Distance();
#endif
    }

    pass Height
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PS_Height();
#else
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PS_Height();
#endif
    }
}