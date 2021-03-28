//.fx file, compiled into a pixel shader
//this file is not used when program is running, but needed to make the .ps file

//texture passed
sampler2D implicitInputSampler : register(S0);
//constant passed
float4 colorFilter : register(C0);


float4 main(float2 uv : TEXCOORD) : COLOR
{
    //sampling input texture
    float4 color = tex2D(implicitInputSampler, uv);

    //multiplicative bleneding with input colour
    //return that colour
    return color * colorFilter;
}