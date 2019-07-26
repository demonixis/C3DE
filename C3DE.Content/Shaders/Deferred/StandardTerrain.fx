// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 SpecularColor;
int SpecularPower;
float SpecularIntensity;

// Misc
float2 TextureTiling;
float3 EyePosition;

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SandTexture;
sampler2D SandTextureSampler = sampler_state
{
    Texture = <SandTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D RockTexture;
sampler2D RockTextureSampler = sampler_state
{
    Texture = <RockTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SnowTexture;
sampler2D SnowTextureSampler = sampler_state
{
    Texture = <SnowTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D WeightMap;
sampler2D WeightMapSampler = sampler_state
{
    Texture = <WeightMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
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
    float4 WorldPosition : TEXCOORD2;
    float2 Depth : TEXCOORD3;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    // Color
    float3 sandTex = tex2D(SandTextureSampler, input.UV * TextureTiling);
    float3 rockTex = tex2D(RockTextureSampler, input.UV * TextureTiling);
    float3 snowTex = tex2D(SnowTextureSampler, input.UV * TextureTiling);
    float3 mainTex = tex2D(MainTextureSampler, input.UV * TextureTiling);
    float3 weightTex = tex2D(WeightMapSampler, input.UV);
	
    output.Color.rgb = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
    output.Color.rgb *= mainTex;
    output.Color.rgb += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

    // Normal
    output.Normal.rgb = input.WorldNormal;
    output.Normal.a = 1;

    // Specular
    float4 specularAttributes = float4(SpecularColor * SpecularIntensity, SpecularPower);
    output.Color.a = specularAttributes.r;
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
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}