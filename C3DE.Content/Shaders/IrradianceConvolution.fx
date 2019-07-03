const float PI = 3.14159265359;

float3 WorldPos = float3(0, 0, 0);

texture EnvironmentMap;
samplerCUBE EnvironmentSampler = sampler_state
{
    Texture = <EnvironmentMap>;
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) : POSITION0
{
	VertexShaderOutput output;
	output.Position = input.Position;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{		
	// The world vector acts as the normal of a tangent surface
    // from the origin, aligned to WorldPos. Given this normal, calculate all
    // incoming radiance of the environment. The result of this radiance
    // is the radiance of light coming from -Normal direction, which is what
    // we use in the PBR shader to sample irradiance.
    float3 N = normalize(WorldPos);

    float3 irradiance = float3(0.0, 0.0, 0.0);   
    
    // tangent space calculation from origin point
    float3 up = float3(0.0, 1.0, 0.0);
    float3 right = cross(up, N);
    up = cross(N, right);
       
    float sampleDelta = 0.025;
    float nrSamples = 0.0;
	
#if SM4
    for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            // spherical to cartesian (in tangent space)
            float3 tangentSample = float3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
            // tangent space to world
            float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N; 

            irradiance += texCUBE(EnvironmentSampler, sampleVec).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }
#else
	float phis[6] = { 0.0, 0.5, 1.0, 2.0, 4.0, 6.0 };
	float thetas[6] = { 0.0, 0.25, 0.5, 0.75, 1.0, 1.5 };

	for (int i = 0; i < 6; i++)
	{
		// spherical to cartesian (in tangent space)
		float3 tangentSample = float3(sin(thetas[i]) * cos(phis[i]), sin(thetas[i]) * sin(phis[i]), cos(thetas[i]));
		// tangent space to world
		float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

		irradiance += texCUBE(EnvironmentSampler, sampleVec).rgb * cos(thetas[i]) * sin(thetas[i]);
		nrSamples++;
	}
#endif
	
    irradiance = PI * irradiance * (1.0 / float(nrSamples));
    
    return float4(irradiance, 1.0);
}

technique IrradianceConvoluion
{
    pass IrradianceConvoluionPass
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