#include "../Common/Macros.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 DiffuseColor;
float3 EmissiveColor;
float EmissiveIntensity;

// Misc
float2 TextureTiling;
float TotalTime;

DECLARE_TEXTURE(MainTexture, 1);
DECLARE_TEXTURE(NormalTexture, 2);

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = float2(0.5, 0.5) * input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 noise = SAMPLE_TEXTURE(NormalTexture, input.UV * TextureTiling);
    float2 T1 = (input.UV + float2(1.5, -1.5) * TotalTime * 0.02) * TextureTiling;
    float2 T2 = (input.UV + float2(-0.5, 2.0) * TotalTime * 0.01) * TextureTiling;
	
    T1.x += noise.x * 2.0;
    T1.y += noise.y * 2.0;
    T2.x -= noise.y * 0.2;
    T2.y += noise.z * 0.2;
	
    float p = SAMPLE_TEXTURE(NormalTexture, (T1 * 3.0)).a;
	
    float3 color = SAMPLE_TEXTURE(MainTexture, (T2 * 4.0));
    float3 temp = color * (float3(p, p, p) * 2.0) + (color * color - 0.1);
	
    if (temp.r > 1.0)
        temp.bg += clamp(temp.r - 2.0, 0.0, 100.0);
	
    if (temp.g > 1.0)
        temp.rb += temp.g - 1.0;
	
    if (temp.b > 1.0)
        temp.rg += temp.b - 1.0;
		
    return float4((DiffuseColor * temp) + EmissiveColor * EmissiveIntensity, 1.0);
}

TECHNIQUE_SM4(Lava, VertexShaderFunction, PixelShaderFunction);