#define NUM_SAMPLES (15)
#define FAR_PLANE_Z (300.0)
#define NUM_SPIRAL_TURNS (7)
#define bias (0.01)
#define EDGE_SHARPNESS  (1)
#define SCALE BlurFilterDistance
#define R (4)

float Radius;
float Radius2; // Radius * Radius;
float Intensity;
float4 ProjInfo;
float4x4 ProjectionInv;
float4 MainTextureTexelSize;
float2 Axis;
float BlurFilterDistance;
float NearClip;
float FarClip;

static const float gaussian[5] = { 0.153170, 0.144893, 0.122649, 0.092902, 0.062970 }; // stddev = 2.0

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

texture RandTexture;
sampler2D randSampler = sampler_state
{
    Texture = <RandTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture AOTexture;
sampler2D aoSampler = sampler_state
{
    Texture = <AOTexture>;
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
    float2 UV : TEXCOORD0;
};

struct PixelShaderOutput
{
	float4 Position : SV_Position;
    float2 UV : TEXCOORD0;
    float2 UV2 : TEXCOORD1;
};

PixelShaderOutput VertexShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output;
    output.Position = input.Position;
    output.UV = input.UV;
    output.UV2 = input.UV;
    return output;
}

float LinearEyeDepth(float rawdepth)
{
    float x, y, z, w;
    x = 1.0 - NearClip / FarClip;
    y = NearClip / FarClip;
    z = x / NearClip;
    w = y / NearClip;
 
    return 1.0 / (z * rawdepth + w);
}

float3 ReconstructCSPosition(float2 S, float z)
{
    float linEyeZ = LinearEyeDepth(z);
    return float3((S.xy * ProjInfo.xy + ProjInfo.zw) * linEyeZ, linEyeZ);
}

float3 ReconstructCSFaceNormal(float3 C)
{
    return normalize(cross(ddy(C), ddx(C)));
}

float2 TapLocation(int sampleNumber, float spinAngle, out float ssR)
{
    float alpha = float(sampleNumber + 0.5) * (1.0 / NUM_SAMPLES);
    float angle = alpha * (NUM_SPIRAL_TURNS * 6.28) + spinAngle;
    ssR = alpha;
    return float2(cos(angle), sin(angle));
}

float CSZToKey(float z)
{
    return saturate(z * (1.0 / FAR_PLANE_Z));
}

void packKey(float key, out float2 p)
{
    float temp = floor(key * 256.0);
    p.x = temp * (1.0 / 256.0);
    p.y = key * 256.0 - temp;
}

float UnpackKey(float2 p)
{
    return p.x * (256.0 / 257.0) + p.y * (1.0 / 257.0);
}

float3 GetPosition(float2 ssP)
{
    float3 P;
    P.z = tex2D(depthSampler, ssP);
    P = ReconstructCSPosition(float2(ssP), P.z);
    return P;
}

float3 GetOffsetPosition(float2 ssC, float2 unitOffset, float ssR)
{
    float2 ssP = saturate(float2(ssR * unitOffset) + ssC);
    float3 P;
    P.z = tex2D(depthSampler, ssP.xy);
    P = ReconstructCSPosition(float2(ssP), P.z);
    return P;
}

float SampleAO(in float2 ssC, in float3 C, in float3 n_C, in float ssDiskRadius, in int tapIndex, in float randomPatternRotationAngle)
{
    float ssR;
    float2 unitOffset = TapLocation(tapIndex, randomPatternRotationAngle, ssR);
    ssR *= ssDiskRadius;

    float3 Q = GetOffsetPosition(ssC, unitOffset, ssR);
    float3 v = Q - C;
    float vv = dot(v, v);
    float vn = dot(v, n_C);
    const float epsilon = 0.01;
    float f = max(Radius2 - vv, 0.0);
    return f * f * f * max((vn - bias) / (epsilon + vv), 0.0);
}

float4 fragAO(PixelShaderOutput input) : COLOR0
{
    float4 fragment = float4(1, 1, 1, 1);
    float2 ssC = input.UV2.xy;
    float3 C = GetPosition(ssC);

    packKey(CSZToKey(C.z), fragment.gb);

    float randomPatternRotationAngle = 1.0;

    int2 ssCInt = ssC.xy * MainTextureTexelSize.zw;
    randomPatternRotationAngle = frac(sin(dot(input.UV, float2(12.9898, 78.233))) * 43758.5453) * 1000.0;

    float3 n_C = ReconstructCSFaceNormal(C);
    float ssDiskRadius = -Radius / C.z;
    float sum = 0.0;
    for (int l = 0; l < NUM_SAMPLES; ++l)
        sum += SampleAO(ssC, C, n_C, (ssDiskRadius), l, randomPatternRotationAngle);

    float temp = Radius2 * Radius;
    sum /= temp * temp;

    float A = max(0.0, 1.0 - sum * Intensity * (5.0 / NUM_SAMPLES));
    fragment.ra = float2(A, A);

    return fragment;
}

float4 fragUpsample(PixelShaderOutput input) : COLOR0
{
    float4 fragment = float4(1, 1, 1, 1);
    float3 C = GetPosition(input.UV);

    packKey(CSZToKey(C.z), fragment.gb);
    fragment.ra = tex2D(targetSampler, input.UV).ra;

    return fragment;
}

float4 fragApply(PixelShaderOutput input) : COLOR0
{
    float4 ao = tex2D(aoSampler, input.UV2);
    return tex2D(targetSampler, input.UV) * ao.rrrr;
}

float4 fragApplySoft(PixelShaderOutput input) : COLOR0
{
    float4 color = tex2D(targetSampler, input.UV);

    float ao = tex2D(targetSampler, input.UV2).r;
    ao += tex2D(targetSampler, input.UV2 + MainTextureTexelSize.xy * 0.75).r;
    ao += tex2D(targetSampler, input.UV2 - MainTextureTexelSize.xy * 0.75).r;
    ao += tex2D(targetSampler, input.UV2 + MainTextureTexelSize.xy * float2(-0.75, 0.75)).r;
    ao += tex2D(targetSampler, input.UV2 - MainTextureTexelSize.xy * float2(-0.75, 0.75)).r;

    return color * float4(ao, ao, ao, 5) / 5;
}

float4 fragBlurBL(PixelShaderOutput input) : COLOR0
{
    float4 fragment = float4(1, 1, 1, 1);
    float2 ssC = input.UV;
    float4 temp = tex2D(targetSampler, input.UV);
    float2 passthrough2 = temp.gb;
    float key = UnpackKey(passthrough2);
    float sum = temp.r;


    float BASE = gaussian[0] * 0.5;
    float totalWeight = BASE;
    sum *= totalWeight;

    for (int r = -R; r <= R; ++r)
    {
        if (r != 0)
        {
            temp = tex2D(targetSampler, ssC + Axis * MainTextureTexelSize.xy * (r * SCALE));
            float tapKey = UnpackKey(temp.gb);
            float value = temp.r;
            float weight = 0.3 + gaussian[abs(r)];
            weight *= max(0.0, 1.0 - (2000.0 * EDGE_SHARPNESS) * abs(tapKey - key));
            sum += value * weight;
            totalWeight += weight;
        }
    }

    const float epsilon = 0.0001;
    fragment = sum / (totalWeight + epsilon);
    fragment.gb = passthrough2;
    return fragment;
}

technique Technique1
{
    pass AOGet
    {
#if SM4
        VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 fragAO();
#else
        VertexShader = compile ps_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 fragAO();
#endif
    }

    pass AOBlurBL
    {
#if SM4
        VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 fragBlurBL();
#else
        VertexShader = compile ps_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 fragBlurBL();
#endif
    }

    pass AOApply
    {
#if SM4
        VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 fragApply();
#else
        VertexShader = compile ps_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 fragApply();
#endif
    }

    pass AOApplySoft
    {
#if SM4
        VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 fragApplySoft();
#else
        VertexShader = compile ps_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 fragApplySoft();
#endif
    }

    pass AOUpsample
    {
#if SM4
        VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 fragUpsample();
#else
        VertexShader = compile ps_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 fragUpsample();
#endif
    }
}