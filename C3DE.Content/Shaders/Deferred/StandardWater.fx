float4x4 World;
float4x4 View;
float4x4 Projection;
float ReflectionIntensity;

// Misc
float3 EyePosition = float3(1, 1, 0);
float2 TextureTiling = float2(1, 1);
float TotalTime;
float3 Features;
// Specular
int SpecularPower;
float SpecularIntensity;
float3 SpecularColor;

texture MainTexture;
sampler2D WaterMapSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ReflectionMap;
samplerCUBE reflectiveSampler = sampler_state
{
    Texture = <ReflectionMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture SpecularMap;
sampler2D specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
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
    float2 Depth : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float4 WorldPosition : POSITION1;
    float3 Reflection : TEXCOORD4;
    float3x3 WorldToTangentSpace : TEXCOORD5;
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

	// Wave
    input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
    input.Position.y += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
	
    float3 normal = input.Normal;
    float3 viewDirection = EyePosition - worldPosition.xyz;
    output.Reflection = reflect(-normalize(viewDirection), normalize(normal));

    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    // Color
    input.UV.x = input.UV.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
    input.UV.y = input.UV.y * 20.0;

    output.Color = tex2D(WaterMapSampler, input.UV * TextureTiling);
	
    if (Features.z > 0)
        output.Color.rgb *= texCUBE(reflectiveSampler, normalize(input.Reflection)).xyz * ReflectionIntensity;

    // Normal
    output.Normal = float4(input.WorldNormal, 1);

    if (Features.x > 0)
    {
        input.UV.y += (sin(TotalTime * 3.0 + 10.0) / 256) + (TotalTime / 16);
        float3 normalMap = 2.0 * (tex2D(NormalMapSampler, input.UV * TextureTiling)) - 1.0;

        input.UV.y -= ((sin(TotalTime * 3.0 + 10) / 256.0) + (TotalTime / 16.0)) * 2.0;
        float3 normalMap2 = (2.0 * (tex2D(NormalMapSampler, input.UV * TextureTiling))) - 1.0;

        normalMap = (normalMap + normalMap2) / 2.0;
        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
		
        output.Normal.rgb = normalMap;
    }

    // Specular
    float4 specularAttributes = float4(SpecularColor, SpecularPower);

    if (Features.y > 0)
        specularAttributes = tex2D(specularSampler, input.UV * TextureTiling);

    output.Color.a = specularAttributes.r * SpecularIntensity;
    output.Normal.a = specularAttributes.a;

    // Depth
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