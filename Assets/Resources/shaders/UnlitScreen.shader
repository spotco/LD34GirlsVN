 Shader "Unlit/Photoshop Overlay"
 {
     Properties
     {
         _MainTex ("Base (RGB), Alpha (A)", 2D) = "" {}
     }
     
     SubShader
     {
         LOD 100
 
         Tags
         {
             "Queue" = "Transparent"
             "IgnoreProjector" = "True"
             "RenderType" = "Transparent"
         }
         
         Pass
         {
             Cull Off
             Lighting Off
             ZWrite Off
             Fog { Mode Off }
             Offset -1, -1
             ColorMask RGB
             AlphaTest Greater .01
             Blend SrcAlpha OneMinusSrcAlpha
             ColorMaterial AmbientAndDiffuse
             
             SetTexture [_MainTex]
             {
                 Combine Texture +- Primary, Texture * Primary
             }
             
             SetTexture [_MainTex]
             {
                 Combine Texture +- previous, Texture * Primary
             }
         }
     }
 }