Shader "Hidden/Pixelation"
{
Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size (x, y)", Vector) = (0.01, 0.01, 0, 0) 
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);
            
            float4 _PixelSize; 

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz); 
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 coord = floor(uv / _PixelSize.xy) * _PixelSize.xy;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, coord);
                return col;
            }
            ENDHLSL
        }
    }
}
