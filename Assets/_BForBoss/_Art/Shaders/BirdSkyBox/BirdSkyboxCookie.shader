Shader "CustomRenderTexture/BirdSkyboxCookie"
{
    Properties
    {
        _Speed("Speed", Vector) = (0,0,0,0)
        _Tiling("Tiling", Vector) = (1,1,1,1)
        _Remap("Remap", Vector) = (0,1,0,1)
        _TextureA("Texture A", 2D) = "white" {}
        _TextureB("Texture B", 2D) = "white" {}
     }

     SubShader
     {
        Blend One Zero

        Pass
        {
            Name "BirdSkyboxCookie"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4      _Speed;
            float4      _Tiling;
            float4      _Remap;
            sampler2D   _TextureA;
            sampler2D   _TextureB;

            void Unity_Remap_float4(float In, float2 InMinMax, float2 OutMinMax, out float Out){
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            float4 frag(v2f_customrendertexture IN) : SV_Target
            {
                float colorA = 1.0 - tex2D(_TextureA, (IN.localTexcoord.xy * _Tiling.xy) + _Speed.xy * (_Time.y)).r;
                float colorB = 1.0 - tex2D(_TextureB, (IN.localTexcoord.xy * _Tiling.zw) + _Speed.zw * (_Time.y)).r;
                float color = colorA * colorB;
                
                Unity_Remap_float4(color, _Remap.xy, _Remap.zw, color);
                
                return float4(0, 0, 0, color);
            }
            ENDCG
        }
    }
}
