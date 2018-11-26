using System;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;

namespace WhatNow.Visualization
{
    public interface IActionPipeMapVisualizer
    {
        string Visualize(IActionPipeMap map);

        string Visualize(IActionPipeMap map, Type beginAt);
    }
}
