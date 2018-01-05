float4x4 World;
float4x4 View;
float4x4 Projection;

float3 AmbientColor;
float3 DiffuseColor;
bool NormalTextureEnabled;
bool SpecularTextureEnabled;

texture Texture;
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture SpecularMap;
sampler specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

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
    float2 Depth : TEXCOORD2;
    float3x3 WorldToTangentSpace : TEXCOORD3;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float4 worldPosition = mul(float4(input.Position.xyz, 1.0), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
    
    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;

    return output;
}

PixelShaderOutput PixelShaderAmbient(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    output.Color = float4(AmbientColor + (tex2D(diffuseSampler, input.UV).rgb * DiffuseColor), 1.0f);
    
    float4 specularAttributes = float4(0, 0, 0, 0);

    if (SpecularTextureEnabled == true)
        specularAttributes = tex2D(specularSampler, input.UV);

    output.Color.a = specularAttributes.r;

    if (NormalTextureEnabled)
    {
        float3 normalFromMap = tex2D(normalSampler, input.UV).rgb;
        normalFromMap = (2.0f * normalFromMap) - 1.0f;
        normalFromMap = mul(normalFromMap, input.WorldToTangentSpace);
        normalFromMap = normalize(normalFromMap);
        output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);
    }
    else
        output.Normal.rgb = input.WorldNormal;

    output.Normal.a = specularAttributes.a;
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}

technique RenderTechnique
{
    pass P0
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderAmbient();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
    }
}
