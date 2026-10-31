Shader "Perch/UI Desaturated" {
 Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} _Color ("Tint", Color)=(1,1,1,1)
 _StencilComp ("Stencil Comparison", Float)=8 _Stencil ("Stencil ID", Float)=0 _StencilOp ("Stencil Operation", Float)=0
 _StencilWriteMask ("Stencil Write Mask", Float)=255 _StencilReadMask ("Stencil Read Mask", Float)=255 _ColorMask ("Color Mask", Float)=15 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
 Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
 Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
 #include "UnityCG.cginc"
 #include "UnityUI.cginc"
 struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
 struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 mask:TEXCOORD1; };
 sampler2D _MainTex; fixed4 _Color; float4 _ClipRect; float _UIMaskSoftnessX; float _UIMaskSoftnessY;
 v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex);
 float2 pixelSize=o.vertex.w/abs(mul((float2x2)UNITY_MATRIX_P,_ScreenParams.xy));
 float4 rect=clamp(_ClipRect,-2e10,2e10);
 o.mask=float4(v.vertex.xy*2-rect.xy-rect.zw,.25/(.25*float2(_UIMaskSoftnessX,_UIMaskSoftnessY)+abs(pixelSize))); o.uv=v.uv; o.color=v.color*_Color; return o; }
 fixed4 frag(v2f i):SV_Target { fixed4 c=tex2D(_MainTex,i.uv)*i.color; c.rgb=lerp(c.rgb,dot(c.rgb,float3(.299,.587,.114)),.6);
 #ifdef UNITY_UI_CLIP_RECT
 half2 mask=saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw); c.a*=mask.x*mask.y;
 #endif
 return c; }
 ENDCG }
 }
}
