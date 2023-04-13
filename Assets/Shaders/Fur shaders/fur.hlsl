#ifndef FUR_HLSL
#define FUR_HLSL

#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"

int _ShellCount;
float _FurHeight;
float _AlphaThreshold;
float _Occlusion;
float _FurScale;

float4 _BaseMove;
float4 _WindFreq;
float4 _WindMove;

TEXTURE2D(_FurMap);
SAMPLER(sampler_FurMap);
float4 _FurMap_ST;

struct attr {
  float4 positionOS : POSITION;
  float3 normalOS   : NORMAL;
  float4 tangentOS  : TANGENT;
  float2 uv         : TEXCOORD0;
};

struct v2f {
  float4 vertex  : SV_POSITION;
  float2 uv      : TEXCOORD0;
  float fogCoord : TEXCOORD1;
  float layer    : TEXCOORD2;
};

attr vert (attr i) {
  return i;
}

void AppendShellVertex(inout TriangleStream<v2f> stream, attr i, int ind) {
  v2f o = (v2f)0;

  VertexPositionInputs vertexInput = GetVertexPositionInputs(i.positionOS.xyz);
  VertexNormalInputs normalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

  float layer = (float)ind / _ShellCount;
  float3 posOS = i.positionOS.xyz;
  
  float moveFactor = pow(layer, _BaseMove.w);

  float3 windAngle = _Time.w * _WindFreq.xyz;
  float3 windMove = moveFactor * _WindMove.xyz * sin(windAngle + posOS * _WindMove.w);
  float3 move = moveFactor * _BaseMove.xyz;
  float3 shellDir = SafeNormalize(normalInput.normalWS + move + windMove);

  float3 posWS = vertexInput.positionWS + shellDir * (_FurHeight * layer);
  float4 posCS = TransformWorldToHClip(posWS);

  o.vertex = posCS;
  o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
  o.fogCoord = ComputeFogFactor(posCS.z);
  o.layer = layer;

  stream.Append(o);
}

[maxvertexcount(96)]
void geo(triangle attr input[3], inout TriangleStream<v2f> stream) {
  [loop] for (float i = 0; i < _ShellCount; ++i) {
    [unroll] for (float j = 0; j < 3; ++j) {
      AppendShellVertex(stream, input[j], i);
    }
    stream.RestartStrip();
  }
}

float4 frag(v2f i) : SV_Target {
  float4 furColor = SAMPLE_TEXTURE2D(_FurMap, sampler_FurMap, i.uv * _FurScale);
  float alpha = furColor.r * (1.0 - i.layer);
  if (i.layer > 0.0 && alpha < _AlphaThreshold) discard;

  float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

  float occlusion = lerp(1.0 - _Occlusion, 1.0, i.layer);
  float3 color = baseColor * occlusion;

  color = MixFog(color, i.fogCoord);

  return float4(color, alpha);
}

void fragShadow(v2f i,
                  out float4 outColor : SV_Target,
                  out float outDepth : SV_Depth) {
  float4 furColor = SAMPLE_TEXTURE2D(_FurMap, sampler_FurMap, i.uv * _FurScale);
  float alpha = furColor.r * (1.0 - i.layer);
  if (i.layer > 0.0 && alpha < _AlphaThreshold) discard;

  outColor = outDepth = i.vertex.z / i.vertex.w;
}

#endif