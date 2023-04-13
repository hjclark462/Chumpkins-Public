Shader "GVX/ScreenSpaceDecalUnlit_URP"
{

	//Limitation needs a depth camera .... 
	// needs normal to disable rendering on the other side of models 
	// no preview in scene view for normals 
	// if the camera is inside the box it won't render


	Properties
	{
		// Specular vs Metallic workflow

		[HDR] _Color("Color", Color) = (0.5,0.5,0.5,1)
		[MainTexture] _MainTex("Albedo", 2D) = "white" {}
		_ProgressNoise("Progress Noise", 2D) = "white" {}
		_Progress("Simulation Factor",Range(0,1)) = 1
		_Cooldown("Cooldown Value",Range(0,1)) = 0
		[Toggle(SINGLE_CHANNEL)] _SingleChannel("Albedo is Single Channel", Float) = 1
	}

		SubShader
		{
			Tags{"RenderType" = "Transparent"  "Queue" = "Geometry+100" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True"}
			LOD 100

			Pass
			{
				Name "Unlit"

				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				Cull Back

				HLSLPROGRAM

				#define _BaseMap _MainTex
				#define sampler_BaseMap sampler_MainTex
				#define _BaseColor _Color
				
				#pragma shader_feature SINGLE_CHANNEL

				//--------------------------------------
				// GPU Instancing
				#pragma multi_compile_instancing

				#pragma vertex UnlitPassVertex
				#pragma fragment UnlitPassFragment

				
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				
				#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"


				struct Attributes
				{
					float4 positionOS   : POSITION;
					float2 uv           : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct Varyings
				{
					float4 uv                       : TEXCOORD0;
					float4 positionCS               : SV_POSITION;
					float3 worldDirection			: TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO //Insert

				};

				TEXTURE2D_X(_CameraDepthTexture);
				SAMPLER(sampler_CameraDepthTexture);
				TEXTURE2D(_ProgressNoise);
				SAMPLER(sampler_ProgressNoise);

				Varyings UnlitPassVertex(Attributes input)
				{
					Varyings output;

					//UNITY_SETUP_INSTANCE_ID(input);
					//UNITY_TRANSFER_INSTANCE_ID(input, output);
					UNITY_SETUP_INSTANCE_ID(input);
					#if defined(UNITY_COMPILER_HLSL)
					#define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
					#else
					#define UNITY_INITIALIZE_OUTPUT(type,name)
					#endif
					UNITY_INITIALIZE_OUTPUT(Varyings, output); //Insert
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert
					//UNITY_SETUP_INSTANCE_ID(input); //Insert
					//UNITY_INITIALIZE_OUTPUT(Varyings, output); //Insert
					//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert

					VertexPositionInputs vertexInput  = GetVertexPositionInputs(input.positionOS.xyz);
					output.uv = vertexInput.positionNDC;
					output.positionCS = vertexInput.positionCS;
					output.worldDirection = vertexInput.positionWS.xyz - _WorldSpaceCameraPos;
					return output;
				}

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(half, _Progress)
					UNITY_DEFINE_INSTANCED_PROP(half, _Cooldown)
				UNITY_INSTANCING_BUFFER_END(Props)

				float SampleSceneDepth(float4 uv)
				{
					//divide by W to properly interpolate by depth ... 
					return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv.xy / uv.w)).r;
				}

				//UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture); //Insert

				half4 UnlitPassFragment(Varyings input) : SV_Target
				{

					UNITY_SETUP_INSTANCE_ID(input);

					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input); //Insert

					float perspectiveDivide = 1.0f / input.uv.w;
					float3 direction = input.worldDirection * perspectiveDivide;
					float depth = SampleSceneDepth(input.uv);
					float sceneZ = LinearEyeDepth(depth, _ZBufferParams);
					float3 wpos = direction * sceneZ + _WorldSpaceCameraPos;
					float3 opos = TransformWorldToObject(wpos);
					input.uv = float4(opos.xz + 0.5,0,0);

					half4 albedoAlpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
					float ang = atan2(input.uv.y - 0.5, input.uv.x - 0.5);
					#if SINGLE_CHANNEL
						albedoAlpha = half4(1,1,1,albedoAlpha.a);
					#endif
					albedoAlpha *= _Color;
					ang = (ang % 3.14159)/3.14159;
					float s = ((ang+1.5)) % 1;
					float left = step(0.5, ang) + (1-step(-0.5, ang));
					s = lerp(s, 1-s, left);
					albedoAlpha.xyz *= 1-step(1-_Cooldown, s);

					float3 absOpos = abs(opos);
					half progress = UNITY_ACCESS_INSTANCED_PROP(Props, _Progress);
					progress = saturate((progress*1.2 - SAMPLE_TEXTURE2D(_ProgressNoise, sampler_ProgressNoise, input.uv).r) / 0.2);
					albedoAlpha.a *=step(max(absOpos.x, max(absOpos.y, absOpos.z)), 0.5)* progress;
					return half4(albedoAlpha);
			}
			ENDHLSL
		}


	}
}
