#include "../Common/Macros.fxh"
#include "../Common/Fog.fxh"

// Constants
#if SM4
#define MAX_LIGHT_COUNT 64
#else
#define MAX_LIGHT_COUNT 8
#endif

const float3 TO_GAMMA = float3(0.45454545, 0.45454545, 0.45454545);
const float3 TO_LINEAR = float3(2.2, 2.2, 2.2);

// Lighting
// LightData.x: Type: Directional, Point, Spot
// LightData.y: Intensity
// LightData.z: Range
// LightData.w: FallOff
// SpotData.xyz: Direction
// SpotData.w: Angle

float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float4 LightData[MAX_LIGHT_COUNT];
float4 SpotData[MAX_LIGHT_COUNT];
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
    float3x3 WorldToTangentSpace : TEXCOORD3;
#if REFLECTION_MAP
	float3 Reflection : TEXCOORD4;
#endif
    float FogDistance : FOG;
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
    output.FogDistance = distance(worldPosition.xyz, EyePosition);

    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;

#if REFLECTION_MAP
	float3 viewDirection = EyePosition - worldPosition.xyz;
	output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
#endif

    return output;
}

float3 StandardPixelShader(float4 worldPosition, float3 normal, float specularTerm, float fogDistance, float3 albedo, float3 emissive, float shadowTerm)
{    
	float3 Lo = FLOAT3(0);
	float So = FLOAT3(0);
	float directionToLight = FLOAT3(0);
	float diffuseIntensity = 0;
	float attenuation = 0;

	for (int i = 0; i < LightCount; i++)
	{
		directionToLight = normalize(LightPosition[i] - worldPosition.xyz);
		diffuseIntensity = saturate(dot(directionToLight, normal));

		if (LightData[i].x == 0) // Directional
		{
			attenuation = 1.0;
		}
		else if (LightData[i].x == 1) // Point
		{
			float d = distance(LightPosition[i], worldPosition.xyz);
			attenuation = 1.0 - pow(clamp(d / LightData[i].z, 0.0, 1.0), LightData[i].w);
		}
		else if (LightData[i].x == 2) // Spot
		{
			float3 diffuse = saturate(dot(normal, directionToLight));
			float d = dot(directionToLight, normalize(SpotData[i].xyz));
			float a = cos(SpotData[i].w);
			attenuation = (a < d) ? attenuation = 1.0 - pow(clamp(a / d, 0.0, 1.0), LightData[i].w) : 0.0;
		}

		Lo += diffuseIntensity * attenuation * LightColor[i] * LightData[i].y;

		float3 reflectionVector = normalize(reflect(-directionToLight, normal));
		float3 directionToCamera = normalize(EyePosition - worldPosition.xyz);
		So += specularTerm * pow(saturate(dot(reflectionVector, directionToCamera)), SpecularPower);
	}

    float3 color = AmbientColor + (albedo * Lo * shadowTerm) + So + emissive;
	color = ApplyFog(color, fogDistance);

	// HDR + Gamma correction
	//color = color / (color + FLOAT3(1.0));
	//color = pow(color, TO_GAMMA);

	return color;
}