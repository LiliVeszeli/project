
sampler2D implicitInputSampler : register(S0);
float4 colorFilter : register(C0);



float4 main(float2 uv : TEXCOORD) : COLOR
{

    //float4 color = tex2D(implicitInputSampler, uv);

    //return color * colorFilter;
    
    vector<float, 4> color = { 1, 0, 0, 1 };

    return color;

    //maybe discard alpha
}