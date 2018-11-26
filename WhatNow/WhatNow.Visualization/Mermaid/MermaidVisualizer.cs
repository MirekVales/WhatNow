using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;

namespace WhatNow.Visualization.Mermaid
{
    public class MermaidVisualizer : IActionPipeMapVisualizer
    {
        const string Header = "graph TD";

        public string Visualize(IActionPipeMap map)
            => Visualize(map, map.GetEntryPoints());

        public string Visualize(IActionPipeMap map, Type beginAt)
            => Visualize(map, new[] { beginAt });

        string Visualize(IActionPipeMap map, IEnumerable<Type> entryPoints)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine(Header);

            int label = 1;
            int labelize() => label++;
            var alreadyVisited = new HashSet<Type>();
            Func<Type, string> tokenDescriptionFunction = t => GetTokenName(t);

            foreach (var currentNode in entryPoints)
            {
                buffer.AppendLine($"subgraph {currentNode.Name}");
                Generate(map, buffer, labelize, currentNode, labelize(), alreadyVisited.Contains, t => alreadyVisited.Add(t), tokenDescriptionFunction);
                buffer.AppendLine("end");
            }

            return buffer.ToString();
        }

        void Generate(
            IActionPipeMap map
            , StringBuilder buffer
            , Func<int> labelize
            , Type currentNode
            , int currentLabel
            , Predicate<Type> alreadyVisited
            , Action<Type> addVisit
            , Func<Type, string> getTokenValue)
        {
            foreach (var nextNode in map.GetNext(currentNode).Where(next => !alreadyVisited(next)))
            {
                addVisit(nextNode);
                var nextNodeLabel = labelize();
                var token = getTokenValue(currentNode);
                buffer.AppendLine($"{currentLabel}[{currentNode.Name}] --> |{token}| {nextNodeLabel}[{nextNode.Name}]");
                Generate(map, buffer, labelize, nextNode, nextNodeLabel, alreadyVisited, addVisit, getTokenValue);
            }
        }

        string GetTokenName(Type type)
        {
            const char GenericTypeSplitCharacter = '`';

            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Split(GenericTypeSplitCharacter).First() == "ActionBase")
            {
                return type.GetGenericArguments().Last().Name;
            }

            return GetTokenName(type.BaseType);
        }
    }
}
