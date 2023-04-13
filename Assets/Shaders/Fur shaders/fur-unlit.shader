Shader "Unlit/fur-unlit" {
  Properties {
    _BaseMap ("Base Map", 2D) = "white" {}
    _FurMap ("Fur Map", 2D) = "white" {}
    [IntRange] _ShellCount ("Shell Count", Range(1, 100)) = 16
    _FurHeight ("Shell Max Height", Range(0.0, 1.0)) = 0.016
    _AlphaThreshold("Alpha Threshold", Range(0.0, 1.0)) = 0.1
    _Occlusion("Occlusion", Range(0.0, 1.0)) = 0.1
    _FurScale("Fur Scale", Range(0.001, 10.0)) = 1.0
    _BaseMove("Base Move", Vector) = (0.0, -0.0, 0.0, 3.0)
    _WindFreq("Wind Freq", Vector) = (0.5, 0.7, 0.9, 1.0)
    _WindMove("Wind Move", Vector) = (0.2, 0.3, 0.2, 1.0)
  }
  SubShader {
    Tags { 
      "RenderType"="Opaque"
      "RenderPipeline"="UniversalPipeline"
      "IgnoreProjector"="True"
    }

    LOD 100

    ZWrite On
    Cull Back

    Pass {
      Name "Unlit"
      HLSLPROGRAM
      #pragma exclude_renderers gles gles3 glcore
      #pragma multi_compile_fog
      #include "./fur.hlsl"
      #pragma vertex vert
      #pragma require geometry
      #pragma geometry geo
      #pragma fragment frag
      ENDHLSL
    }

    Pass {
      Name "DepthOnly"
      Tags {"LightMode" = "DepthOnly"}

      ZWrite On
      ColorMask 0

      HLSLPROGRAM
      #pragma exclude_renderers gles gles3 glcore
      #include "./fur.hlsl"
      #pragma vertex vert
      #pragma require geometry
      #pragma geometry geo
      #pragma fragment fragShadow
      ENDHLSL
    }

    Pass {
      Name "ShadowCaster"
      Tags {"LightMode" = "ShadowCaster"}

      ZWrite On
      ZTest LEqual
      ColorMask 0

      HLSLPROGRAM
      #pragma exclude_renderers gles gles3 glcore
      #include "./fur.hlsl"
      #pragma vertex vert
      #pragma require geometry
      #pragma geometry geo
      #pragma fragment fragShadow
      ENDHLSL
    }
  }
}
