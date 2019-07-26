// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 EmissiveColor;
float EmissiveIntensity;

// Misc
float2 TextureTiling;
float TotalTime;

texture MainTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (MainTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalMap;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalMap);
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
    output.UV = float2(0.5, 0.5) * input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    float3 noise = tex2D(normalSampler, input.UV * TextureTiling);
    float2 T1 = (input.UV + float2(1.5, -1.5) * TotalTime * 0.02) * TextureTiling;
    float2 T2 = (input.UV + float2(-0.5, 2.0) * TotalTime * 0.01) * TextureTiling;
	
    T1.x += noise.x * 2.0;
    T1.y += noise.y * 2.0;
    T2.x -= noise.y * 0.2;
    T2.y += noise.z * 0.2;
	
    float p = tex2D(normalSampler, (T1 * 3.0)).a;
	
    float3 color = tex2D(textureSampler, (T2 * 4.0));
    float3 temp = color * (float3(p, p, p) * 2.0) + (color * color - 0.1);
	
    if (temp.r > 1.0)
        temp.bg += clamp(temp.r - 2.0, 0.0, 100.0);
	
    if (temp.g > 1.0)
        temp.rb += temp.g - 1.0;
	
    if (temp.b > 1.0)
        temp.rg += temp.b - 1.0;

    float3 emission = EmissiveColor * EmissiveIntensity;

    output.Color.rgb = (DiffuseColor * temp) + emission;

    // Normal
    output.Normal.rgb = input.WorldNormal;

    output.Depth = input.Depth.x / input.Depth.y;
	
    if (emission.r + emission.g + emission.b > 0)
        output.Depth = 0;

    return output;
}

technique Basic
{
    pass AmbientPass
    {
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}