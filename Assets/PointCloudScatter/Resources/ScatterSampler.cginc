#ifndef SCATTER_SAMPLER
#define SCATTER_SAMPLER

struct SampleData
{
    int index;
    int count;
    float width;
    float spacing;
};

struct SampleValue
{
    float seed;
    float2 mapping;
};

struct SampleResult
{   
    float3 position;
    float3 rotation;
    float3 scale;
    
    float3 direction;
    float3 randomize;

    float4x4 directionMatrix;
};

//Instancing
int _FaceCount;
float _Seed;
float _Density;
float _Noising;

SampleData GetSampleData(int index, SampleTarget target)
{
    SampleData data;
    data.index = index;

    float last = floor(target.area * (_Seed + index - 1) * _Density);
    float current = floor(target.area * (_Seed + index) * _Density);
    data.count = max(current - last, target.area * _Density);
    
    data.width = sqrt(data.count);
    data.spacing = (1.0 / data.width);

    return data;
}

SampleValue GetSampleValue(int index, SampleData data)
{
    SampleValue value;
    value.seed = (_Seed + index + data.index + 1);
    
    value.mapping.x = (index / data.width) / data.width;
    value.mapping.y = (fmod(index, data.width)) / data.width;

    value.mapping += 0.5 / data.count / 2;
    value.mapping += (rnd1To2(value.seed) - 0.5) * _Noising;

    // if(value.mapping.x + value.mapping.y >= 1) 
    // {
    //     // value.mapping = 1 - value.mapping;
    // }

    return value; 
}

float3 SquareLerp(float3 a, float3 b, float3 c, float2 t)
{
    float3 abShift = lerp(0, b - a, t.x);
    float3 acShift = lerp(0, c - a, t.y);
    return a + abShift + acShift;
}
SampleResult GetSampleResult(SampleValue value, SampleTarget target)
{
    SampleResult result;
    result.position = SquareLerp(target.vertexA, target.vertexB, target.vertexC, value.mapping);
    result.rotation = 0;
    result.scale = 1;
    result.direction = normalize(SquareLerp(target.normalA, target.normalB, target.normalC, value.mapping));
    result.randomize = rnd1To3(value.seed);

    result.directionMatrix = DirectionMatrix(result.direction, normalize(target.vertexA - target.vertexB));

    return result;
}

#endif