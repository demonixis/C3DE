// Constants
#if SM4
#define MAX_LIGHT_COUNT 128
#else
#define MAX_LIGHT_COUNT 16
#endif

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

// ---
// --- Lighting Calculation for directional, point and spot.
// ---
float3 CalculateOneLight(int i, float3 worldPosition, float3 worldNormal, float3 eyePosition, float3 diffuseColor, float3 specularColor, int specularPower)
{
	float3 lightVector = LightPosition[i] - worldPosition;
	float3 directionToLight = normalize(lightVector);
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal));
	
	if (diffuseIntensity <= 0)
		return float3(0, 0, 0);
	
	float3 diffuse = diffuseIntensity * LightColor[i] * diffuseColor;
	float baseIntensity = 1; // Directional
	
	if (LightData[i].x == 1) // Point
	{	
		float d = length(lightVector);
		baseIntensity = 1.0 - pow(saturate(d / LightData[i].z), LightData[i].w);
	}
#if SM4
	else if (LightData[i].x == 2) // Spot
	{
		float d = dot(directionToLight, normalize(SpotData[i].xyz));
		float a = cos(SpotData[i].w);
		
		if (a < d)
			baseIntensity = 1.0 - pow(clamp(a / d, 0.0, 1.0), LightData[i].w);
		else
			baseIntensity = 0.0;
	}
#endif
	
	// Self Shadow.
	float selfShadow = saturate(4 * diffuseIntensity);
	
	// Phong
	float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
	float3 directionToCamera = normalize(eyePosition - worldPosition);
	float3 specular = saturate(LightColor[i] * specularColor * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower));
			   
	return  selfShadow * baseIntensity * (diffuse + specular);
}