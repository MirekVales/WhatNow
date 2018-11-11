using System;
using WhatNow.Contracts;

namespace WhatNow.Visualization
{
    public interface IActionPipeMapVisualizer
    {
        string Visualize(IActionPipeMap map);

        string Visualize(IActionPipeMap map, Type beginAt);
    }
}
