Shader "Perch/UI Focus" {
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
 struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 world:TEXCOORD1; };
 float4 _Hole; sampler2D _MainTex; fixed4 _Color; float4 _ClipRect;
 v2f vert(appdata v) { v2f o; o.world=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o; }
 fixed4 frag(v2f i):SV_Target {float2 q=abs(i.world.xy-_Hole.xy)-(_Hole.zw-32);float d=length(max(q,0))+min(max(q.x,q.y),0)-32;return fixed4(0,0,0,.6*i.color.a*saturate(d/24));}
 ENDCG }
 }
}
