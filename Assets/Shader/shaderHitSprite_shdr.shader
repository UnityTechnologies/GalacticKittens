Shader "Unlit/2D Hit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		//Adding this value as a toggle, so we can call it each time an entity is hit.
		[MaterialToggle] _Hit ("Hit", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		//Adding alpha blending. This one is very standart.
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			//To use it, we add it here. We can call it later.
			fixed _Hit;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                //Declare the texture again, but this time add a +1. Meaning, it will fill the object with white.
				fixed4 hit = tex2D(_MainTex, i.uv) +1;
				//but that also means the alpha channel will be filled with white, losing transparency.
				//instead, we redeclare the alpha of our new result, to the original alpha.
				hit.a = col.a;
				
				//return a lerp: when the "_Hit" value is 1, return the white texture. When its 0, return the original texture.
				return lerp(col,hit,_Hit);
            }
            ENDCG
        }
    }
}
