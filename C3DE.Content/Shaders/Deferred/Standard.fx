// Constant
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition = float3(1, 1, 0);
// Material Parameter
float3 DiffuseColor;
float2 TextureTiling = float2(1, 1);
// Specular
float3 SpecularColor;
int SpecularPower;
float SpecularIntensity;
// Reflection
float ReflectionIntensity;
// Emission
float3 EmissiveColor;
float EmissiveIntensity;

float4 Features;

// Textures
texture MainTexture;
sampler diffuseSampler = sampler_state
{
    Texture = (MainTexture);
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

texture ReflectionMap;
samplerCUBE reflectionSampler = sampler_state
{
    Texture = <ReflectionMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture EmissiveMap;
sampler2D emissiveSampler = sampler_state
{
    Texture = (EmissiveMap);
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
    float3 WorldNormal : TEXCOORD1;
    float2 Depth : TEXCOORD2;
    float3 Reflection : TEXCOORD3;
    float3x3 WorldToTangentSpace : TEXCOORD4;
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

    if (Features.z > 0)
    {
        float3 viewDirection = EyePosition - worldPosition.xyz;
        float3 normal = input.Normal;
        output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
    }

    return output;
}

PixelShaderOutput PixelShaderAmbient(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    output.Color = float4((tex2D(diffuseSampler, input.UV * TextureTiling).rgb * DiffuseColor), 1.0f);

	if (Features.x != 45)
	{
		output.Depth = input.Depth.x / input.Depth.y;
		output.Normal = float4(input.WorldNormal, 1);
		return output;
	}

    if (Features.z > 0)
        output.Color *= texCUBE(reflectionSampler, normalize(input.Reflection)) * ReflectionIntensity;
		
	float3 emissiveColor = EmissiveColor;

    if (Features.w > 0)
        emissiveColor = tex2D(emissiveSampler, input.UV * TextureTiling).xyz;

    output.Color += float4(emissiveColor * EmissiveIntensity, 0.0);
    
    float4 specularAttributes = float4(SpecularColor, SpecularPower);

    if (Features.y > 0)
        specularAttributes.rgb = tex2D(specularSampler, input.UV * TextureTiling).rgb;

    specularAttributes.rgb *= SpecularIntensity;

    output.Color.a = specularAttributes.r;

    if (Features.x > 0)
    {
        float3 normalFromMap = tex2D(normalSampler, input.UV * TextureTiling).rgb;
        normalFromMap = (2.0f * normalFromMap) - 1.0f;
        normalFromMap = mul(normalFromMap, input.WorldToTangentSpace);
        normalFromMap = normalize(normalFromMap);
        output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);
    }
    else
        output.Normal.rgb = input.WorldNormal;

    output.Normal.a = specularAttributes.a;
    output.Depth = input.Depth.x / input.Depth.y;

    if (emissiveColor.r + emissiveColor.g + emissiveColor.b > 0)
        output.Depth = 0;

    return output;
}

technique RenderTechnique
{
    pass P0
    {
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderAmbient();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
    }
}
