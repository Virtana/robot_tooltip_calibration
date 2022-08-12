Shader "Unlit/WhiteReplacementShader" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }
  
  SubShader {
    Tags { "RenderType"="Opaque" } 
    
    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
        
      #pragma shader_feature WORLD_NORMAL_VIS
      #include "UnityCG.cginc"
      
      struct appdata{
        float4 vertex : POSITION;
      };
      
      struct v2f {
        float4 vertex : SV_POSITION;
      };
      
      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        return o;
      }
      
      fixed4 frag () : SV_Target {
        return fixed4(1,1,1,1);  //White colour of marker
      }
      ENDCG
    }
  }
}
