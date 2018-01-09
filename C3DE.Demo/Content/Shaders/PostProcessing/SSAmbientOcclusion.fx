#define sampleKernelSize 3

float4x4 InvertViewProjection;
float4x4 InvertProjection;
float4x4 View;
float4x4 Projection;
float Radius;
float Intensity;
float Scale;
float Bias;
float2 HalfPixel;
float FarClip;
float2 ScreenSize = float2(1280, 720);
float2 RandomSize = float2(64, 64);


//-----------------------------------------
// Textures
//-----------------------------------------

texture NormalMap;
sampler2D normalSampler : register(s1) = sampler_state
{
	Texture = <NormalMap>;
	MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture DepthBuffer;
sampler2D depthSampler : register(s4) = sampler_state
{
	Texture = <DepthBuffer>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture RandomMap;
sampler2D randomSampler = sampler_state
{
	Texture = <RandomMap>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

//-------------------------------
// Functions
//-------------------------------

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
    VertexShaderOutput output = (VertexShaderOutput) 0;
	
    output.Position = input.Position;
    output.UV = input.UV + HalfPixel;
	
    return output;
}

float3 getPosition(in float2 uv)
{
	float depth = tex2D(depthSampler, uv).r;

	// Convert position to world space
	float4 position = 0;//tex2D(positionSampler, uv);
	
	position.xy = uv.x * 2.0f - 1.0f;
	position.y = -(uv.y * 2.0f - 1.0f);
	position.z = depth;
	position.w = 1.0f;

	position = mul(position, InvertViewProjection);
	position /= position.w;
	
	//return position;
	return mul(position, View);
}

float3 getNormal(in float2 uv)
{
	return normalize(tex2D(normalSampler, uv).xyz * 2.0f - 1.0f);
}

float2 getRandom(in float2 uv)
{
	return normalize(tex2D(randomSampler, ScreenSize * uv / RandomSize).xy * 2.0f - 1.0f);
}

float doAmbientOcclusion(in float2 tcoord,in float2 uv, in float3 p, in float3 cnorm)
{
	float3 worldPos = getPosition(tcoord + uv) - p;

	const float3 vec = normalize(worldPos);
	const float distance = length(worldPos) * Scale;
	return max(0.0, dot(cnorm, vec) - Bias) * (1.0 / (1.0 + distance));
}
  
float4 PixelShaderFunction(VertexShaderOutput IN) : COLOR0
{
	const float2 vec[4] = {
		float2(1,0),
		float2(-1,0),
        float2(0,1),
		float2(0,-1)
	};

	float3 p = getPosition(IN.UV);
	float3 n = getNormal(IN.UV);
	float2 rand = getRandom(IN.UV);

	float depthVal = tex2D(depthSampler, IN.UV).r;
	if (depthVal >= 0.99999f)
		return 1;

	float ao = 0.0f;
	float rad = Radius / p.z;

	float2 coord1, coord2;

	for (int j = 0; j < sampleKernelSize; ++j)
	{
		coord1 = reflect(vec[j], rand) * rad;
		coord2 = float2(coord1.x * 0.707 - coord1.y * 0.707,
					  coord1.x * 0.707 + coord1.y * 0.707);
  
		ao += doAmbientOcclusion(IN.UV, coord1 * 0.25, p, n);
		ao += doAmbientOcclusion(IN.UV, coord2 * 0.5,  p, n);
		ao += doAmbientOcclusion(IN.UV, coord1 * 0.75, p, n);
		ao += doAmbientOcclusion(IN.UV, coord2, p, n);
	} 

	ao /= (float)sampleKernelSize;
	float attenuate = 1 - pow(depthVal, 10);
	return 1 - (ao * attenuate * Intensity);
}																

technique SSAO
{
    pass Pass1
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