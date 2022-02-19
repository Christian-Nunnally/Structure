using Structure.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Structure.Graphing
{
    public class ConsoleGraph
    {
        private readonly int _totalColumns;
        private readonly int _totalRows;
        private const int XLabelRightPadding = 10;
        private const int XLabelRows = 6;

        public ConsoleGraph(int width, int height)
        {
            _totalColumns = width;
            _totalRows = height;
        }

        public void Print(StructureIO io, List<List<(string Label, double Value)>> listOfValues)
        {
            if (io is null || listOfValues is null || !listOfValues.Any()) return;

            var firstValues = listOfValues.First();
            var allValues = listOfValues.SelectMany(x => x).Select(x => x.Value);
            var maxValue = allValues.Any() ? allValues.Max() : 0;
            var minValue = allValues.Any() ? allValues.Min() : 0;

            var chart = CreateChart();
            foreach (var values in listOfValues)
            {
                chart = CombineCharts(chart, RenderChart(values, minValue, maxValue));
            }
            Print(io, firstValues, chart);
        }

        private void Print(StructureIO io, List<(string Label, double Value)> labelInfo, char[,] chart)
        {
            var yLabels = GenerateYLabels(labelInfo);
            PrintChart(io, chart, yLabels);
            var xLabelIndexes = GetXLabelIndexes(labelInfo);
            PrintXAxis(io, xLabelIndexes);
            var xNamesCharacters = InitializeXNameCharacterMap(labelInfo);
            PrintXNameCharacters(io, xNamesCharacters);
        }

        private double[] InterpolateValues(List<(string Label, double Value)> values)
        {
            var interpolatedValues = new double[_totalColumns];
            if (values.Count == 0) return interpolatedValues;
            for (int column = 0; column < _totalColumns - 1; column++)
            {
                interpolatedValues[column] = InterpolateValue(values, column);
            }
            interpolatedValues[^1] = values[^1].Value;
            return interpolatedValues;
        }

        private double InterpolateValue(List<(string Label, double Value)> values, int indexOfInterpolatedNumber)
        {
            var percent = indexOfInterpolatedNumber / (_totalColumns - 1.0);
            var doubleIndex = percent * (values.Count - 1.0);
            var previousValue = values[(int)doubleIndex].Value;
            var nextValue = (values.Count > 1)
                ? values[(int)doubleIndex + 1].Value
                : 0;
            var percentDistanceToNextIndex = doubleIndex - (int)doubleIndex;
            var percentDistanceFromPreviousIndex = 1 - percentDistanceToNextIndex;
            var contributionFromPreviousPoint = previousValue * percentDistanceFromPreviousIndex;
            var contributionFromNextPoint = nextValue * percentDistanceToNextIndex;
            return contributionFromPreviousPoint + contributionFromNextPoint;
        }

        private char[,] InitializeXNameCharacterMap(List<(string Label, double Value)> values)
        {
            var xNamesCharacters = CreateXNameCharacterMap();
            int currentXNameIndex = -1;
            for (int x = 0; x < _totalColumns; x++)
            {
                double percent = x / (_totalColumns - 1.0);
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
                        if (x + i < _totalColumns + XLabelRightPadding && y < XLabelRows)
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
            for (int x = 0; x < _totalColumns; x++)
            {
                double percent = x / (_totalColumns - 1.0);
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
            var chart = new char[_totalColumns, _totalRows];
            for (int y = 0; y < _totalRows; y++)
            {
                for (int x = 0; x < _totalColumns; x++)
                {
                    chart[x, y] = ' ';
                }
            }
            return chart;
        }

        private char[,] RenderChart(List<(string Label, double Value)> values, double yAxisMin, double yAxisMax)
        {
            var interpolatedValues = InterpolateValues(values);
            if (yAxisMax < yAxisMin) yAxisMax = yAxisMin + 1;
            var chart = CreateChart();
            for (int column = 0; column < _totalColumns; column++)
            {
                var scaler = _totalRows - 1.0;
                var numerator = interpolatedValues[column] - yAxisMin;
                var denominator = yAxisMax - yAxisMin;
                var row = SafeDivision(numerator, denominator) * scaler;
                MarkPointOnChart(chart, column, (int)row);
            }
            PostProcessChart(chart);
            return chart;
        }

        private void MarkPointOnChart(char[,] chart, int column, int row)
        {
            if (column >= 0 && row >= 0 && column < _totalColumns && row < _totalRows) 
                chart[column, row] = '─';
        }

        private char[,] CombineCharts(char[,] chartA, char[,] chartB)
        {
            var chart = CreateChart();
            for (int x = 0; x < _totalColumns - 1; x++)
            {
                for (int y = 0; y < _totalRows - 1; y++)
                {
                    var isChartAEmpty = char.IsWhiteSpace(chartA[x, y]);
                    var isChartBEmpty = char.IsWhiteSpace(chartB[x, y]);
                    chart[x, y] = chartA[x, y];
                    if (isChartAEmpty && !isChartBEmpty)
                    {
                        chart[x, y] = chartB[x, y];
                    }
                }
            }
            return chart;
        }

        private void PrintChart(StructureIO io, char[,] chart, List<double> yLabels)
        {
            for (double y = _totalRows - 1; y >= 0; y--)
            {
                PrintYLabelString(io, yLabels[(int)y]);
                io.WriteNoLine($" ┤");
                PrintRow(io, chart, y);
                io.Write();
            }
        }

        private static void PrintYLabelString(StructureIO io, double yLabel)
        {
            var yLabelString = string.Format(CultureInfo.CurrentCulture.NumberFormat, "{0:0.##}", yLabel);
            for (int i = 0; i < 10 - yLabelString.Length; i++) io.WriteNoLine(" ");
            io.WriteNoLine(yLabelString);
        }

        private void PrintRow(StructureIO io, char[,] chart, double row)
        {
            for (double x = 0; x < _totalColumns; x++)
            {
                io.WriteNoLine($"{chart[(int)x, (int)row]}");
            }
        }

        private List<double> GenerateYLabels(List<(string Label, double Value)> values)
        {
            var yLabels = new List<double>();
            if (!values.Any()) return yLabels;
            var minValue = values.Min(x => x.Value);
            var maxValue = values.Max(x => x.Value);
            if (maxValue == 0) maxValue = 1;
            for (double y = 0; y < _totalRows; y++)
            {
                var percentAlongAxis = y / (_totalRows - 1.0);
                var range = maxValue - minValue;
                double yValue = minValue + range * percentAlongAxis;
                yLabels.Add(yValue);
            }
            return yLabels;
        }

        private void PrintXAxis(StructureIO io, List<int> xLabelIndexes)
        {
            io.WriteNoLine("           └");
            for (int x = 0; x < _totalColumns; x++)
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
                for (int x = 0; x < _totalColumns + XLabelRightPadding; x++)
                {
                    io.WriteNoLine($"{xNamesCharacters[x, y]}");
                }
                io.Write();
            }
        }

        private char[,] CreateXNameCharacterMap()
        {
            var xNamesCharacters = new char[_totalColumns + XLabelRightPadding, XLabelRows];
            for (int x = 0; x < _totalColumns + XLabelRightPadding; x++)
                for (int y = 0; y < XLabelRows; y++)
                    xNamesCharacters[x, y] = ' ';
            return xNamesCharacters;
        }

        private void PostProcessChart(char[,] chart)
        {
            for (int x = 0; x < _totalColumns - 1; x++)
            {
                PostProcessColumn(chart, x);
            }
        }

        private void PostProcessColumn(char[,] chart, int column)
        {
            var index = FindCharacterIndexInColumn(chart, '─', column);
            if (index == -1) return;
            var nextIndex = FindCharacterIndexInColumn(chart, '─', column + 1);
            PostProcessAtPoint(chart, column, index, nextIndex);
        }

        private int FindCharacterIndexInColumn(char[,] chart, char character, int column)
        {
            for (int y = 0; y < _totalRows; y++)
            {
                if (chart[column, y] == character) return y;
            }
            return -1;
        }

        private static void PostProcessAtPoint(char[,] chart, int x, int y, int y2)
        {
            if (y == y2 || y == -1 || y2 == -1) return;
            int smallerY = Math.Min(y, y2);
            int biggerY = Math.Max(y, y2);
            bool direction = y > y2;
            char upper = direction ? '┐' : '┌';
            char downer = direction ? '└' : '┘';
            chart[x, biggerY] = upper;
            chart[x, smallerY] = downer;
            for (int betweenY = smallerY + 1; betweenY < biggerY; betweenY++) chart[x, betweenY] = '│';
        }

        private double SafeDivision(double numerator, double denominator) => denominator != 0 ? numerator / denominator : 0;
    }
}
