#ifndef SCATTER_FILTER
#define SCATTER_FILTER

struct Filter  
{
    int type;
    float3 v1;
    float3 v2;

    float fade;
    float filte;
};


//Filtering
int _FilterCount;
StructuredBuffer<Filter> filtersBuffer;

bool ScatterFilter(SampleResult result)
{
    for(int i = 0; i < _FilterCount; i++)
    {
        Filter filter = filtersBuffer[i];
        float filteValue = 0;

        switch (filter.type)
        {
            case -1:
                filteValue = 100;
                break;
            
            case 0:
                float3 position = result.position;
                filteValue = dot(filter.v1, position);
                break;

            case 1:
                filteValue = dot(filter.v1, result.direction);
                break;
        }

        float filteRnd = (result.randomize.x * filter.fade) - (filter.fade * 0.5);
        filteValue = filteValue - filter.filte + filteRnd;

        if(filteValue < 0) return false;
    }
    
    return true;
}


#endif