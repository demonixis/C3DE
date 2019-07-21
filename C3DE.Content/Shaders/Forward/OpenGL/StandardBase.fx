// Constants
#define MAX_LIGHT_COUNT 8

// Lighting
// LightData.x: Type: Directional, Point, Spot
// LightData.y: Intensity
// LightData.z: Range
// LightData.w: FallOff
float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float4 LightData[MAX_LIGHT_COUNT];
int LightCount = 0;

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float SpecularPower;

// Misc
float3 EyePosition;

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
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    return output;
}

float3 StandardPixelShader(float4 worldPosition, float3 normal, float specularTerm, float3 albedo, float3 emissive)
{    
	float3 Lo = float3(0, 0, 0);
	float3 directionToLight = float3(0, 0, 0);
	float diffuseIntensity = 0;
	float attenuation = 0;
	
	int lightCount = min(MAX_LIGHT_COUNT, LightCount);
	
	for (int i = 0; i < lightCount; i++)
	{
		directionToLight = normalize(LightPosition[i] - worldPosition.xyz);
		diffuseIntensity = saturate(dot(normal, directionToLight));

		if (diffuseIntensity <= 0)
			continue;

		if (LightData[i].x == 0) // Directional
		{
			attenuation = 1.0;
		}
		else if (LightData[i].x == 1) // Point
		{
			float d = distance(LightPosition[i], worldPosition.xyz);
			attenuation = 1.0 - pow(clamp(d / LightData[i].z, 0.0, 1.0), LightData[i].w);
		}

		// Self Shadow
		float selfShadow = saturate(4 * diffuseIntensity);

		// Specular
		float3 reflectionVector = normalize(reflect(-directionToLight, normal));
		float3 directionToCamera = normalize(EyePosition - worldPosition.xyz);
		float specular = specularTerm * pow(saturate(dot(reflectionVector, directionToCamera)), SpecularPower) * attenuation;

		Lo += selfShadow * (diffuseIntensity * attenuation * LightColor[i] * LightData[i].y) + specular;
	}

    return float4(AmbientColor + (albedo * Lo) + emissive, 1);
}