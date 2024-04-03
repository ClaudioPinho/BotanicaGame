#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

float TillingX = 1;
float TillingY = 1;

float4 DiffuseColor = float4(1, 1, 1, 1);

Texture2D Texture;
SamplerState TextureSampler = sampler_state
{
	texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(PixelShaderInput input) : COLOR0
{
	float2 uv = float2(input.TexCoord.x * TillingX, input.TexCoord.y * TillingY); // apply tiling
	uv -= floor(uv);

	float4 color = tex2D(TextureSampler, uv) * DiffuseColor;

	return color;
}

technique Technique1{
	pass Pass1{
		VertexShader = compile VS_SHADERMODEL MainVS();
PixelShader = compile PS_SHADERMODEL MainPS();
}
}
;
