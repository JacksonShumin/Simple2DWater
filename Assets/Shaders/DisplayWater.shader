Shader "Unlit/DisplayWater"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ObsticleTex("Obsticle Texture", 2D) = "black" {}
        _FluidTex("Fluid Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _ObsticleTex;
            sampler2D _FluidTex;
            float4 _FluidTex_TexelSize;

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
                
                // obsticles
                float obsticle = tex2D(_ObsticleTex, i.uv);
                if (obsticle > 0)
                    return fixed4(0, 0, 0, 1);

                float fluid = tex2D(_FluidTex, i.uv);

                if (fluid > 0) 
                {
                    // Make the water render with dynamic vertical leveling (only looks right when gravity is going down)
                    
                    //float fluidAbove = tex2D(_FluidTex, i.uv + float2(0, 1.0f / _FluidTex_TexelSize.w));
                    //float obsticleAbove = tex2D(_ObsticleTex, i.uv + float2(0, 1.0f / _FluidTex_TexelSize.w));
                    //
                    //
                    //if (fluidAbove == 0 || obsticleAbove > 0) {
                    //
                    //    float height = frac(i.uv.y * _FluidTex_TexelSize.w);
                    //    float width = frac(i.uv.x * _FluidTex_TexelSize.z);
                    //    if (height > fluid)
                    //        return col;
                    //}

                    // Lerp to blue as fluid value aproaches 1
                    if (fluid <= 1)
                        col =  lerp(fixed4(0, 0, 1, 1), col, saturate(1.0f - fluid));

                    // Darker blue for fluid with value higher then one
                    else
                        col = lerp(fixed4(0, 0, 1, 1), fixed4(0, 0, 0.25f, 1), saturate((fluid - 1) / 5));

                }

                return col;
            }
            ENDCG
        }
    }
}
