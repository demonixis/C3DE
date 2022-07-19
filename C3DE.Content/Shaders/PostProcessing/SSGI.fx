// Constants
#if SM4
#define MAX_SAMPLE_COUNT 1024
#else
#define MAX_SAMPLE_COUNT 16
#endif

float4x4 InverseProjectionMatrix;
float4 MainTexTexelSize;
float IndirectAmount;
float NoiseAmount;
int Noise;
int SampleCount;

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
sampler2D depthTextureSampler = sampler_state
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
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float2 mod_dither3(float2 u)
{
    float noiseX = fmod(u.x + u.y + fmod(208. + u.x * 3.58, 13. + fmod(u.y * 22.9, 9.)), 7.) * .143;
    float noiseY = fmod(u.y + u.x + fmod(203. + u.y * 3.18, 12. + fmod(u.x * 27.4, 8.)), 6.) * .139;
    return float2(noiseX, noiseY) * 2.0 - 1.0;
}

float2 dither(float2 coord, float seed, float2 size)
{
    float noiseX = ((frac(1.0 - (coord.x + seed * 1.0) * (size.x / 2.0)) * 0.25) + (frac((coord.y + seed * 2.0) * (size.y / 2.0)) * 0.75)) * 2.0 - 1.0;
    float noiseY = ((frac(1.0 - (coord.x + seed * 3.0) * (size.x / 2.0)) * 0.75) + (frac((coord.y + seed * 4.0) * (size.y / 2.0)) * 0.25)) * 2.0 - 1.0;
    return float2(noiseX, noiseY);
}

float3 getViewPos(sampler2D tex, float2 coord, float4x4 ipm)
{
    float depth = tex2D(tex, coord).r;

    //Turn the current pixel from ndc to world coordinates
    float3 pixel_pos_ndc = float3(coord * 2.0 - 1.0, depth * 2.0 - 1.0);
    float4 pixel_pos_clip = mul(ipm, float4(pixel_pos_ndc, 1.0));
    float3 pixel_pos_cam = pixel_pos_clip.xyz / pixel_pos_clip.w;
    return pixel_pos_cam;
}

float3 getViewNormal(sampler2D tex, float2 coord, float4x4 ipm)
{
    float pW = MainTexTexelSize.x;
    float pH = MainTexTexelSize.y;

    float3 p1 = getViewPos(tex, coord + float2(pW, 0.0), ipm).xyz;
    float3 p2 = getViewPos(tex, coord + float2(0.0, pH), ipm).xyz;
    float3 p3 = getViewPos(tex, coord + float2(-pW, 0.0), ipm).xyz;
    float3 p4 = getViewPos(tex, coord + float2(0.0, -pH), ipm).xyz;

    float3 vP = getViewPos(tex, coord, ipm);

    float3 dx = vP - p1;
    float3 dy = p2 - vP;
    float3 dx2 = p3 - vP;
    float3 dy2 = vP - p4;

    if (length(dx2) < length(dx) && coord.x - pW >= 0.0 || coord.x + pW > 1.0)
    {
        dx = dx2;
    }

    if (length(dy2) < length(dy) && coord.y - pH >= 0.0 || coord.y + pH > 1.0)
    {
        dy = dy2;
    }

    return normalize(-cross(dx, dy).xyz);
}

float lenSq(float3 v)
{
    return pow(v.x, 2.0) + pow(v.y, 2.0) + pow(v.z, 2.0);
}

float3 lightSample(sampler2D color_tex, sampler2D depth_tex, float2 coord, float4x4 ipm, float2 lightcoord, float3 normal, float3 position, float n, float2 texsize)
{
    float2 random = float2(1.0, 1.0);

    if (Noise > 0)
    {
        random = (mod_dither3((coord * texsize) + float2(n * 82.294, n * 127.721))) * 0.01 * NoiseAmount;
    }
    else
    {
        random = dither(coord, 1.0, texsize) * 0.1 * NoiseAmount;
    }

    lightcoord *= float2(0.7, 0.7);

    //light absolute data
    float3 lightcolor = tex2D(color_tex, ((lightcoord)+random)).rgb;
    float3 lightnormal = getViewNormal(depth_tex, frac(lightcoord) + random, ipm).rgb;
    float3 lightposition = getViewPos(depth_tex, frac(lightcoord) + random, ipm).xyz;

    //light variable data
    float3 lightpath = lightposition - position;
    float3 lightdir = normalize(lightpath);

    //falloff calculations
    float cosemit = clamp(dot(lightdir, -lightnormal), 0.0, 1.0); //emit only in one direction
    float coscatch = clamp(dot(lightdir, normal) * 0.5 + 0.5, 0.0, 1.0); //recieve light from one direction
    float distfall = pow(lenSq(lightpath), 0.1) + 1.0;        //fall off with distance

    return (lightcolor * cosemit * coscatch / distfall) * (length(lightposition) / 20.0);
}

float4 AverageColorPixelShader(PixelShaderInput input) : COLOR
{
    float3 direct = tex2D(textureSampler, input.UV).rgb;
    float3 color = normalize(direct).rgb;
    float3 indirect = float3(0.0,0.0,0.0);
    float PI = 3.14159;
    float2 texSize = MainTexTexelSize.zw;
    //fragment geometry data
    float3 position = getViewPos(depthTextureSampler, input.UV, InverseProjectionMatrix);
    float3 normal = getViewNormal(depthTextureSampler, input.UV, InverseProjectionMatrix);

    //sampling in spiral

    float dlong = PI * (3.0 - sqrt(5.0));
    float dz = 1.0 / float(SampleCount);
    float l = 0.0;
    float z = 1.0 - dz / 2.0;

    int limit = min(MAX_SAMPLE_COUNT, SampleCount);

    for (int i = 0; i < limit; i++)
    {
        float r = sqrt(1.0 - z);
        float xpoint = (cos(l) * r) * 0.5 + 0.5;
        float ypoint = (sin(l) * r) * 0.5 + 0.5;

        z = z - dz;
        l = l + dlong;

        indirect += lightSample(textureSampler, depthTextureSampler, input.UV, InverseProjectionMatrix, float2(xpoint, ypoint), normal, position, float(i), texSize);
    }

    return float4(direct + (indirect / float(limit) * IndirectAmount), 1);
}

technique Technique1
{
    pass AverageColor
    {
#if SM4
		PixelShader = compile ps_4_0 AverageColorPixelShader();
#else
		PixelShader = compile ps_3_0 AverageColorPixelShader();
#endif
    }
}