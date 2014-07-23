float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 lightView;
float4x4 lightProjection;
float3 lightPosition;
float3 lightRadius = float3(100.0, 100.0, 100.0);
float shadowMapSize = 512;
float bias = 7.0 / 1000.0;
float4 ambientColor = float4(1.0, 1.0, 1.0, 1.0);

texture mainTexture;
sampler textureSampler = sampler_state
{
	texture = (mainTexture);
	minfilter = linear;
	magfilter = linear;
	mipfilter = linear;
	addressu = wrap;
	addressv = wrap;
};

texture shadowTexture;
sampler shadowSampler = sampler_state
{
	texture = (shadowTexture);
	minfilter = point;
	magfilter = point;
	mipfilter = point;
	addressu = clamp;
	addressv = clamp;
};

struct VertexShaderInput
{
    float4 Position:POSITION0;
	float2 tex_coord:TEXCOORD0;
	float3 normal:NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 tex_coord:TEXCOORD0;
	float4 worldPosition:TEXCOORD1;
	float2 screenPosition:TEXCOORD2;
	float3 normal:TEXCOORD3;
};

float calcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
	float size = 1.0 / shadowMapSize;
	float shadowTerm = 0;
	float samples[4];
	
	samples[0] = (lightSpaceDepth - bias < tex2D(shadowSampler, shadowCoordinates).r);
	samples[1] = (lightSpaceDepth - bias < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r);
	samples[2] = (lightSpaceDepth - bias < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r);
	samples[3] = (lightSpaceDepth - bias < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r);

	shadowTerm = (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;

	return shadowTerm;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	
    output.Position = mul(viewPosition, Projection);
	output.tex_coord = input.tex_coord;
	output.worldPosition = worldPosition;
	output.screenPosition =  output.Position.xy / output.Position.w;
	output.normal = mul(input.normal, World);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(textureSampler, input.tex_coord);
	float4 worldPosition = input.worldPosition;
	float3 lightDir = lightPosition - worldPosition;
	float inverseLightRadiusSquared = 1.0 / (lightRadius * lightRadius);
	float lightFactor = 0;
	float attenuation = 0;
	float ndl = 0;
	
	//transform to light space
	float4 lightSpacePosition = mul(mul(worldPosition, lightView), lightProjection);
	lightSpacePosition -= bias;
	lightSpacePosition /= lightSpacePosition.w;
	float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;
	float lightSpaceDepth = lightSpacePosition.z;

	//light influence
	attenuation = 1 - saturate(dot(lightDir, lightDir) * inverseLightRadiusSquared);
	ndl = saturate(dot(input.normal, lightDir));
	
	lightFactor = attenuation * ndl;
	float shadowTerm = 1;

	// Using this hack for now
	if (shadowMapSize > 0)
	{
		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = calcShadowPCF(lightSpaceDepth, screenPosition);
	}
	
    return color * ambientColor * lightFactor * shadowTerm;
}

technique Technique1
{
    pass Pass1
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}
