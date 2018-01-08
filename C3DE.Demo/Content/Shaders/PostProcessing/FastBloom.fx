float4 Parameter;
float4 MainTextureTexelSize;

#define ONE_MINUS_THRESHHOLD_TIMES_INTENSITY Parameter.w
#define THRESHHOLD Parameter.z

static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 }; // gauss'ish blur weights

static const float4 curve4[7] =
{
    float4(0.0205, 0.0205, 0.0205, 0), float4(0.0855, 0.0855, 0.0855, 0), float4(0.232, 0.232, 0.232, 0),
	float4(0.324, 0.324, 0.324, 1), float4(0.232, 0.232, 0.232, 0), float4(0.0855, 0.0855, 0.0855, 0), float4(0.0205, 0.0205, 0.0205, 0)
};

texture MainTexture;
sampler2D targetSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture BloomTexture;
sampler2D bloomSampler = sampler_state
{
    Texture = <BloomTexture>;
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
};

struct PSInput_Simple
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
};

struct PSInput_Tap
{
    float4 Position : SV_POSITION;
    float2 UV20 : TEXCOORD0;
    float2 UV21 : TEXCOORD1;
    float2 UV22 : TEXCOORD2;
    float2 UV23 : TEXCOORD3;
};

struct PSInput_WithBlurCoord8
{
    float4 Position : SV_POSITION;
    float4 UV : TEXCOORD0;
    float2 Offset : TEXCOORD1;
};
		
struct PSInput_WithBlurCoordsSGX
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
    float4 Offsets[3] : TEXCOORD1;
};

// Vertex Shaders

PSInput_Simple VS_Bloom(VertexShaderInput v)
{
    PSInput_Simple o;
    o.Position = v.Position;
    o.UV = v.UV;	        	
    return o;
}

PSInput_Tap VS_4Tap(VertexShaderInput v)
{
    PSInput_Tap o;

    o.Position = v.Position;
    o.UV20 = v.UV + MainTextureTexelSize.xy;
    o.UV21 = v.UV + MainTextureTexelSize.xy * float2(-0.5h, -0.5h);
    o.UV22 = v.UV + MainTextureTexelSize.xy * float2(0.5h, -0.5h);
    o.UV23 = v.UV + MainTextureTexelSize.xy * float2(-0.5h, 0.5h);

    return o;
}

PSInput_WithBlurCoord8 VS_BlurHorizontal(VertexShaderInput v)
{
    PSInput_WithBlurCoord8 o;
    o.Position = v.Position;
			
    o.UV = float4(v.UV.xy, 1, 1);
    o.Offset = MainTextureTexelSize.xy * float2(1.0, 0.0) * Parameter.x;

    return o;
}
		
PSInput_WithBlurCoord8 VS_BlurVertical(VertexShaderInput v)
{
    PSInput_WithBlurCoord8 o;
    o.Position = v.Position;	
    o.UV = float4(v.UV.xy, 1, 1);
    o.Offset = MainTextureTexelSize.xy * float2(0.0, 1.0) * Parameter.x;	 
    return o;
}

PSInput_WithBlurCoordsSGX VS_BlurHorizontalSGX(VertexShaderInput v)
{
    PSInput_WithBlurCoordsSGX o;
    o.Position = v.Position;	
    o.UV = v.UV.xy;

    float offsetMagnitude = MainTextureTexelSize.x * Parameter.x;
    o.Offsets[0] = v.UV.xyxy + offsetMagnitude * float4(-3.0h, 0.0h, 3.0h, 0.0h);
    o.Offsets[1] = v.UV.xyxy + offsetMagnitude * float4(-2.0h, 0.0h, 2.0h, 0.0h);
    o.Offsets[2] = v.UV.xyxy + offsetMagnitude * float4(-1.0h, 0.0h, 1.0h, 0.0h);

    return o;
}

PSInput_WithBlurCoordsSGX VS_BlurVerticalSGX(VertexShaderInput v)
{
    PSInput_WithBlurCoordsSGX o;
    o.Position = v.Position;		
    o.UV = float4(v.UV.xy, 1, 1);

    float offsetMagnitude = MainTextureTexelSize.y * Parameter.x;
    o.Offsets[0] = v.UV.xyxy + offsetMagnitude * float4(0.0h, -3.0h, 0.0h, 3.0h);
    o.Offsets[1] = v.UV.xyxy + offsetMagnitude * float4(0.0h, -2.0h, 0.0h, 2.0h);
    o.Offsets[2] = v.UV.xyxy + offsetMagnitude * float4(0.0h, -1.0h, 0.0h, 1.0h);

    return o;
}


// Pixel Shaders

float4 PS_Bloom(PSInput_Simple input) : COLOR0
{
    float4 color = tex2D(targetSampler, input.UV);
    return color + tex2D(bloomSampler, input.UV);
}

float4 PS_Downsample(PSInput_Tap i) : COLOR0
{
    float4 color = tex2D(targetSampler, i.UV20);
    color += tex2D(targetSampler, i.UV21);
    color += tex2D(targetSampler, i.UV22);
    color += tex2D(targetSampler, i.UV23);
    return max(color / 4 - THRESHHOLD, 0) * ONE_MINUS_THRESHHOLD_TIMES_INTENSITY;
}

float4 PS_Blur8(PSInput_WithBlurCoord8 i) : COLOR0
{
    float2 uv = i.UV.xy;
    float2 netFilterWidth = i.Offset;
    float2 coords = uv - netFilterWidth * 3.0;
			
    float4 color = 0;
    for (int l = 0; l < 7; l++)
    {
        float4 tap = tex2D(targetSampler, coords);
        color += tap * curve4[l];
        coords += netFilterWidth;
    }
    return color;
}

float4 PS_BlurSGX(PSInput_WithBlurCoordsSGX i) : COLOR0
{
    float2 uv = i.UV.xy;
			
    float4 color = tex2D(targetSampler, i.UV) * curve4[3];
			
    for (int l = 0; l < 3; l++)
    {
        float4 tapA = tex2D(targetSampler, i.Offsets[l].xy);
        float4 tapB = tex2D(targetSampler, i.Offsets[l].zw);
        color += (tapA + tapB) * curve4[l];
    }

    return color;
}

technique Technique1
{
    pass Bloom
    {
#if SM4
        VertexShader = compile vs_4_0 VS_Bloom();
		PixelShader = compile ps_4_0 PS_Bloom();
#else
        VertexShader = compile vs_3_0 VS_Bloom();
        PixelShader = compile ps_3_0 PS_Bloom();
#endif
    }

    pass DownSample
    {
#if SM4
        VertexShader = compile vs_4_0 VS_4Tap();
		PixelShader = compile ps_4_0 PS_Downsample();
#else
        VertexShader = compile vs_3_0 VS_4Tap();
        PixelShader = compile ps_3_0 PS_Downsample();
#endif
    }

    pass BlurVertical
    {
#if SM4
        VertexShader = compile vs_4_0 VS_BlurVertical();
		PixelShader = compile ps_4_0 PS_Blur8();
#else
        VertexShader = compile vs_3_0 VS_BlurVertical();
        PixelShader = compile ps_3_0 PS_Blur8();
#endif
    }

    pass BlurHorizontal
    {
#if SM4
        VertexShader = compile vs_4_0 VS_BlurHorizontal();
		PixelShader = compile ps_4_0 PS_Blur8();
#else
        VertexShader = compile vs_3_0 VS_BlurHorizontal();
        PixelShader = compile ps_3_0 PS_Blur8();
#endif
    }

    pass VerticalSGX
    {
#if SM4
        VertexShader = compile vs_4_0 VS_BlurVerticalSGX();
		PixelShader = compile ps_4_0 PS_BlurSGX();
#else
        VertexShader = compile vs_3_0 VS_BlurVerticalSGX();
        PixelShader = compile ps_3_0 PS_BlurSGX();
#endif
    }

    pass HorizontalSGX
    {
#if SM4
        VertexShader = compile vs_4_0 VS_BlurHorizontalSGX();
		PixelShader = compile ps_4_0 PS_BlurSGX();
#else
        VertexShader = compile vs_3_0 VS_BlurHorizontalSGX();
        PixelShader = compile ps_3_0 PS_BlurSGX();
#endif
    }
}