#ifdef SM4

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0_level_9_3 vsname (); PixelShader = compile ps_4_0_level_9_3 psname(); } }

#define TECHNIQUE_SM4(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0 vsname (); PixelShader = compile ps_4_0 psname(); } }
	
#define PASS(name, vsname, psname ) \
	pass { VertexShader = compile vs_4_0_level_9_3 vsname (); PixelShader = compile ps_4_0_level_9_3 psname(); }
	
#define PASS_SM4(name, vsname, psname ) \
	pass { VertexShader = compile vs_4_0 vsname (); PixelShader = compile ps_4_0 psname(); }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define _vs(r)
#define _ps(r)
#define _cb(r)

#else

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_3_0 vsname (); PixelShader = compile ps_3_0 psname(); } }

#define TECHNIQUE_SM4(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_3_0 vsname (); PixelShader = compile ps_3_0 psname(); } }
	
#define PASS(name, vsname, psname ) \
	pass { VertexShader = compile vs_3_0 vsname (); PixelShader = compile ps_3_0 psname(); }
	
#define PASS_SM4(name, vsname, psname ) \
	pass { VertexShader = compile vs_3_0 vsname (); PixelShader = compile ps_3_0 psname(); }

#define BEGIN_CONSTANTS
#define MATRIX_CONSTANTS
#define END_CONSTANTS

#define _vs(r)  : register(vs, r)
#define _ps(r)  : register(ps, r)
#define _cb(r)

#endif

#define DECLARE_TEXTURE(Name, index) \
    sampler2D Name : register(s##index);

#define DECLARE_CUBEMAP(Name, index) \
    samplerCUBE Name : register(s##index);

#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  texCUBE(Name, texCoord)

#define FLOAT3(f) \
	(float3(f, f, f))
	
#define FLOAT4(f) \
	(float4(f, f, f, f))