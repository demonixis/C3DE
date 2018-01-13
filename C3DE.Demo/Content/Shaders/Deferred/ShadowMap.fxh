float4x4 LightView;
float4x4 LightProjection;
float ShadowStrength;
float ShadowBias;
bool ShadowEnabled = false;
float2 ShadowMapSize;
float2 ShadowMapPixelSize;

texture ShadowMap;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMap);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float ComputeShadow(float nl, float2 shadowTexCoord, float ourdepth)
{
    float shadowdepth = tex2D(shadowSampler, shadowTexCoord).r;
    return shadowdepth < ourdepth ? 0 : nl;
}

float ComputeShadow4Samples(float nl, float2 shadowTexCoord, float ourdepth)
{
    if (ShadowEnabled == false)
        return 1.0f;
		
    float4x4 LightViewProj = mul(LightView, LightProjection);
    float4 lightingPosition = mul(worldPosition, LightViewProj);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	// Get the current depth stored in the shadow map
    float samples[4];
    samples[0] = tex2D(shadowSampler, shadowTexCoord).r < ourdepth ? 0 : 1;
    samples[1] = tex2D(shadowSampler, shadowTexCoord + float2(0, 2) * ShadowMapPixelSize).r < ourdepth ? 0 : 1;
    samples[2] = tex2D(shadowSampler, shadowTexCoord + float2(2, 0) * ShadowMapPixelSize).r < ourdepth ? 0 : 1;
    samples[3] = tex2D(shadowSampler, shadowTexCoord + float2(2, 2) * ShadowMapPixelSize).r < ourdepth ? 0 : 1;
    
	// Determine the lerp amounts           
    float2 lerps = frac(shadowTexCoord * ShadowMapSize);
	// lerp between the shadow values to calculate our light amount
    half shadow = lerp(lerp(samples[0], samples[1], lerps.y), lerp(samples[2], samples[3], lerps.y), lerps.x);
				
    return nl * shadow;
}