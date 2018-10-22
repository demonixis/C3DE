#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix MatrixTransform;
bool UseTexture;
texture Texture;

sampler2D textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 position : POSITION0;
	float4 color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 position : SV_Position;
	float4 color : COLOR0;
    float2 texCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    output.position = mul(input.position, MatrixTransform);
	output.color = input.color;
	output.texCoord = input.texCoord;
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 textureColor = tex2D(textureSampler, input.texCoord);

	if (UseTexture == true)
	{
		return tex2D(textureSampler, input.texCoord) * input.color;
	}
	else
	{
		return input.color;
	}
}

technique Gwen
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
