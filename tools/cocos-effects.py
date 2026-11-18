#!/usr/bin/env python3
"""Port the existing UI shaders to Cocos sprite effects without changing artwork."""
from pathlib import Path
root=Path(__file__).resolve().parents[1]
base=(root/'PerchSort/assets/resources/effects/calm.effect').read_text()
# Use the shared Cocos sprite vertex stage; each effect retains source shader colors.
fragment='''CCProgram sprite-fs %{
  precision highp float;
  #include <builtin/internal/embedded-alpha>
  #include <builtin/internal/alpha-test>
  in vec4 color;
  in vec2 uv0;
  #pragma builtin(local)
  layout(set = 2, binding = 12) uniform sampler2D cc_spriteTexture;
'''
head=base[:base.index('CCProgram sprite-fs')]
head=head.replace('calmParams: { value: [0.0009765625, 0.0006510417, 3.0, 0.38] }','drawSize: { value: [128.0, 128.0, 0.0, 0.0] }')
outline=fragment+'''
  uniform Params {vec4 drawSize;};
  vec4 paint_at(vec2 uv) {
    vec4 paint=texture(cc_spriteTexture,uv);
    float inside=step(0.0,uv.x)*step(uv.x,1.0)*step(0.0,uv.y)*step(uv.y,1.0);
    paint.a=smoothstep(0.06,0.98,paint.a)*inside;
    return paint;
  }
  float silhouette(vec2 uv,float radius) {
    float alpha=paint_at(uv).a;
    for(int i=0;i<8;i++){
      float angle=float(i)*0.7853981634;
      alpha=max(alpha,paint_at(uv+vec2(cos(angle),sin(angle))*radius/drawSize.xy).a);
    }
    return alpha;
  }
  vec4 over(vec4 below,vec4 above){return vec4(above.rgb*above.a,above.a)+below*(1.0-above.a);}
  vec4 frag(){
    vec2 uv=(uv0-.5)*(drawSize.xy+24.0)/drawSize.xy+.5;
    vec2 suv=uv-vec2(0.0,6.0)/drawSize.xy;
    vec2 blur=vec2(2.5)/drawSize.xy;
    float shadow=(paint_at(suv).a+paint_at(suv+vec2(blur.x,0.0)).a+paint_at(suv-vec2(blur.x,0.0)).a+paint_at(suv+vec2(0.0,blur.y)).a+paint_at(suv-vec2(0.0,blur.y)).a)/5.0;
    vec4 result=over(vec4(0.0),vec4(.13,.07,.025,.40*shadow));
    result=over(result,vec4(.28,.13,.035,silhouette(uv,6.0)));
    result=over(result,vec4(1.0,.98,.86,silhouette(uv,3.5)));
    result=over(result,paint_at(uv));
    return result*vec4(color.rgb*color.a,color.a);
  }
}%
'''
(root/'PerchSort/assets/resources/effects/home_icon_outline.effect').write_text(head.replace('blendSrc: src_alpha','blendSrc: one')+outline)
warm=fragment+'''
  vec4 frag(){
    vec4 sampleColor=texture(cc_spriteTexture,uv0);
    float brightness=max(sampleColor.r,max(sampleColor.g,sampleColor.b));
    float ink=1.0-smoothstep(.65,.90,brightness);
    vec3 teal=mix(vec3(.08,.46,.52),vec3(.04,.32,.40),clamp(uv0.y,0.0,1.0));
    return vec4(mix(sampleColor.rgb,teal,ink),sampleColor.a)*color;
  }
}%
'''
(root/'PerchSort/assets/resources/effects/warm_ink.effect').write_text(head.replace('        drawSize: { value: [128.0, 128.0, 0.0, 0.0] }\n','')+warm)
white=fragment+'''
  vec4 frag(){
    vec4 ink=texture(cc_spriteTexture,uv0);
    float light=max(ink.r,max(ink.g,ink.b));
    float fill=smoothstep(.55,.82,light);
    return vec4(mix(vec3(.055,.24,.035),vec3(light),fill),ink.a)*color;
  }
}%
'''
(root/'PerchSort/assets/resources/effects/white_ink.effect').write_text(head.replace('        drawSize: { value: [128.0, 128.0, 0.0, 0.0] }\n','')+white)
silhouette=fragment+'''
  uniform Params {vec4 drawSize;};
  vec4 frag(){
    float alpha=texture(cc_spriteTexture,uv0).a;
    float rim=0.0;
    for(int i=0;i<8;i++){
      float a=float(i)*.7853981634;
      rim=max(rim,texture(cc_spriteTexture,uv0+vec2(cos(a),sin(a))*3.0/drawSize.xy).a);
    }
    return vec4(mix(vec3(.60,1.0,.91),vec3(.02,.34,.31),alpha),max(alpha,rim*.85))*color;
  }
}%
'''
(root/'PerchSort/assets/resources/effects/silhouette.effect').write_text(head+silhouette)

warm_path=root/'PerchSort/assets/resources/effects/warm_ink.effect'
(root/'PerchSort/assets/resources/effects/ability_ink.effect').write_text(warm_path.read_text().replace('vec3(.08,.46,.52)', 'vec3(0.5373,0.2784,0.0784)').replace('vec3(.04,.32,.40)', 'vec3(0.3961,0.1843,0.0353)'))
