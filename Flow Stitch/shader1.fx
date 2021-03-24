
sampler2D implicitInputSampler : register(S0);
float4 colorFilter : register(C0);



float4 main(float2 uv : TEXCOORD) : COLOR
{

    float4 color = tex2D(implicitInputSampler, uv);

    return color * colorFilter;
    
    //vector<float, 4> color = { 1, 0, 0, 1 };

    //return color;

    //maybe discard alpha
    
    
    //float4 inputColor;
    //inputColor = tex2D(input, uv);

    //float4 blendColor;
    //blendColor = tex2D(blend, uv);

    //float4 resultColor;
    //resultColor.a = inputColor.a;
    //// un-premultiply the blendColor alpha out from blendColor
    //blendColor.rgb = clamp(blendColor.rgb / blendColor.a, 0, 1);

    //// apply the blend mode math
    //// R = Base * Blend
    //resultColor.rgb = inputColor.rgb * blendColor.rgb;

    //// re-multiply the blendColor alpha in to blendColor
    //// weight inputColor according to blendColor.a
    //resultColor.rgb =
    //    (1 - blendColor.a) * inputColor.rgb +
    //    resultColor.rgb * blendColor.a;

    //return resultColor;
}