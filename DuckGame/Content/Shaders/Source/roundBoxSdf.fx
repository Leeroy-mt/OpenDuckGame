float4 radii;
float4 rect;
float smoothing;

float sdRoundedBox(float2 p, float2 b, float4 r)
{
    r.xy = (p.x > 0) ? r.xy : r.zw;
    r.x = (p.y > 0) ? r.x : r.y;
    float2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0) + length(max(q, 0)) - r.x;
}

float4 main(float2 position : VPOS, float4 color : COLOR0) : COLOR0
{
    float d = sdRoundedBox(position - (rect.zw / 2) - rect.xy, rect.zw / 2 - smoothing, radii);
    return float4(color.r, color.g, color.b, lerp(color.a, 0, smoothstep(0, smoothing, d)));
}

technique Test
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}