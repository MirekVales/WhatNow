using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatNow.Contracts;

namespace WhatNow.Visualization.Mermaid
{
    public class MermaidVisualizer : IActionPipeMapVisualizer
    {
        const string Header = "graph TD";

        public string Visualize(IActionPipeMap map)
            => Visualize(map, map.GetEntryPoints());

        public string Visualize(IActionPipeMap map, Type beginAt)
            => Visualize(map, new[] { beginAt });

        public string Visualize(IActionPipeMap map, IEnumerable<Type> entryPoints)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine(Header);

            int label = 1;
            int labelize() => label++;

            foreach (var currentNode in entryPoints)
            {
                buffer.AppendLine($"subgraph {currentNode.Name}");
                Generate(map, buffer, labelize, currentNode, labelize());
                buffer.AppendLine("end");
            }

            return buffer.ToString();
        }

        void Generate(IActionPipeMap map, StringBuilder buffer, Func<int> labelize, Type currentNode, int currentLabel)
        {
            foreach (var nextNode in map.GetNext(currentNode))
            {
                var nextNodeLabel = labelize();
                var token = GetTokenName(currentNode);
                buffer.AppendLine($"{currentLabel}[{currentNode.Name}] --> |{token}| {nextNodeLabel}[{nextNode.Name}]");
                Generate(map, buffer, labelize, nextNode, nextNodeLabel);
            }
        }

        string GetTokenName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Split('`').First() == "ActionBase")
            {
                return type.GetGenericArguments().Last().Name;
            }

            return GetTokenName(type.BaseType);
        }
    }
}
