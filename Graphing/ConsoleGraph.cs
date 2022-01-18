using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Structure.Graphing
{
    public class ConsoleGraph
    {
        private readonly int _width;
        private readonly int _height;
        private const int XLabelRightPadding = 10;
        private const int XLabelRows = 6;

        public ConsoleGraph(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Print(StructureIO io, List<(string, double)> values)
        {
            Contract.Requires(values != null);
            Contract.Requires(io != null);
            var chart = RenderChart(values);
            var yLabels = GenerateYLabels(values);
            PrintChart(io, chart, yLabels);

            var xLabelIndexes = GetXLabelIndexes(values);
            PrintXAxis(io, xLabelIndexes);

            var xNamesCharacters = InitializeXNameCharacterMap(values);
            PrintXNameCharacters(io, xNamesCharacters);
        }

        private double[] InterpolateValues(List<(string Label, double Value)> values)
        {
            var interpolatedValues = new double[_width];
            if (values.Count == 0) return interpolatedValues;
            for (int x = 0; x < _width; x++)
            {
                double percent = x / (_width - 1.0);
                double doubleIndex = percent * (values.Count - 1.0);
                int index = (int)Math.Truncate(doubleIndex);
                double percentOfNextIndex = doubleIndex - index;
                double interpolatedValue;

                if (values.Count > index + 1)
                {
                    interpolatedValue = values[index].Value * (1.0 - percentOfNextIndex);
                    interpolatedValue += values[index + 1].Value * percentOfNextIndex;
                }
                else
                {
                    interpolatedValue = values[index].Value;
                }
                interpolatedValues[x] = interpolatedValue;
            }
            return interpolatedValues;
        }

        private char[,] InitializeXNameCharacterMap(List<(string Label, double Value)> values)
        {
            var xNamesCharacters = CreateXNameCharacterMap();
            int currentXNameIndex = -1;
            for (int x = 0; x < _width; x++)
            {
                double percent = x / (_width - 1.0);
                double doubleIndex = percent * (values.Count - 1.0);
                int index = (int)Math.Truncate(doubleIndex);

                if (index != currentXNameIndex)
                {
                    currentXNameIndex = index;
                    int y;
                    for (y = 0; y < 5; y++)
                    {
                        if (xNamesCharacters[x, y] == ' ' && (x == 0 || xNamesCharacters[x - 1, y] == ' '))
                        {
                            break;
                        }
                    }
                    for (int i = 0; i < values[currentXNameIndex].Label.Length; i++)
                    {
                        if (x + i < _width + XLabelRightPadding && y < XLabelRows)
                        {
                            xNamesCharacters[x + i, y] = values[currentXNameIndex].Label[i];
                        }
                    }
                }
            }
            return xNamesCharacters;
        }

        private List<int> GetXLabelIndexes(List<(string Label, double Value)> values)
        {
            var xLabelIndexes = new List<int>();

            int currentXNameIndex = -1;
            for (int x = 0; x < _width; x++)
            {
                double percent = x / (_width - 1.0);
                double doubleIndex = percent * (values.Count - 1.0);
                int index = (int)Math.Truncate(doubleIndex);
                if (index == currentXNameIndex) continue;
                currentXNameIndex = index;
                xLabelIndexes.Add(x);
            }
            return xLabelIndexes;
        }

        private char[,] CreateChart()
        {
            var chart = new char[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    chart[x, y] = ' ';
                }
            }
            return chart;
        }

        private char[,] RenderChart(List<(string Label, double Value)> values)
        {
            var interpolatedValues = InterpolateValues(values);
            var minValue = interpolatedValues.Min();
            var maxValue = interpolatedValues.Max();
            if (maxValue == 0) maxValue = 1;
            var chart = CreateChart();
            for (int x = 0; x < _width; x++)
            {
                int row = (int)((interpolatedValues[x] - minValue) / (maxValue - minValue) * (_height - 1.0));
                chart[x, row] = '─';
            }
            PostProcessChart(chart);
            return chart;
        }

        private void PrintChart(StructureIO io, char[,] chart, List<double> yLabels)
        {
            for (double y = _height - 1; y >= 0; y--)
            {
                for (double x = -1; x < _width; x++)
                {
                    if (x < 0)
                    {
                        var yLabelString = string.Format(CultureInfo.CurrentCulture.NumberFormat, "{0:0.##}", yLabels[(int)y]);
                        for (int i = 0; i < 10 - yLabelString.Length; i++)
                        {
                            io.WriteNoLine(" ");
                        }
                        io.WriteNoLine(yLabelString);
                        io.WriteNoLine($" ┤");
                    }
                    else
                    {
                        io.WriteNoLine($"{chart[(int)x, (int)y]}");
                    }
                }
                io.Write();
            }
        }

        private List<double> GenerateYLabels(List<(string Label, double Value)> values)
        {
            var yLabels = new List<double>();
            var minValue = values.Min(x => x.Value);
            var maxValue = values.Max(x => x.Value);
            if (maxValue == 0) maxValue = 1;
            for (double y = 0; y < _height; y++)
            {
                var percentAlongAxis = y / (_height - 1.0);
                var range = maxValue - minValue;
                double yValue = minValue + range * percentAlongAxis;
                yLabels.Add(yValue);
            }
            return yLabels;
        }

        private void PrintXAxis(StructureIO io, List<int> xLabelIndexes)
        {
            io.WriteNoLine("           └");
            for (int x = 0; x < _width; x++)
            {
                if (xLabelIndexes.Contains(x))
                {
                    io.WriteNoLine("┴");
                }
                else
                {
                    io.WriteNoLine("─");
                }
            }
            io.Write();
        }

        private void PrintXNameCharacters(StructureIO io, char[,] xNamesCharacters)
        {
            for (int y = 0; y < XLabelRows; y++)
            {
                io.WriteNoLine("            ");
                for (int x = 0; x < _width + XLabelRightPadding; x++)
                {
                    io.WriteNoLine($"{xNamesCharacters[x, y]}");
                }
                io.Write();
            }
        }

        private char[,] CreateXNameCharacterMap()
        {
            var xNamesCharacters = new char[_width + XLabelRightPadding, XLabelRows];
            for (int x = 0; x < _width + XLabelRightPadding; x++)
                for (int y = 0; y < XLabelRows; y++)
                    xNamesCharacters[x, y] = ' ';
            return xNamesCharacters;
        }

        private void PostProcessChart(char[,] chart)
        {
            for (int x = 0; x < _width - 1; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (chart[x, y] == '─')
                    {
                        for (int y2 = 0; y2 < _height; y2++)
                        {
                            if (chart[x + 1, y2] == '─')
                            {
                                PostProcessAtPoint(chart, x, y, y2);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private static void PostProcessAtPoint(char[,] chart, int x, int y, int y2)
        {
            if (y == y2) return;
            int smallerY = Math.Min(y, y2);
            int biggerY = Math.Max(y, y2);
            bool direction = y > y2;
            char upper = direction ? '┐' : '┌';
            char downer = direction ? '└' : '┘';
            chart[x, biggerY] = upper;
            chart[x, smallerY] = downer;
            for (int betweenY = smallerY + 1; betweenY < biggerY; betweenY++) chart[x, betweenY] = '│';
        }
    }
}
