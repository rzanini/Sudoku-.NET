using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Model
{

    public enum CrossoverMethod
    {
        SwapSubsquares = 0,
        SwapRows = 1,
        SwapColumns = 2
    }
    public class SexMachine
    {
        public CrossoverMethod Method { get; set; }
        public double CrossoverProb { get; set; }

        private Random seed;

        public SexMachine(CrossoverMethod method, double crossoverProb)
        {
            Method = method;
            seed = new Random();
            CrossoverProb = CrossoverProb;
        }

        public SudokuGenome[] GenerateChildren(SudokuGenome dad, SudokuGenome mom)
        {
            double pick = seed.NextDouble();
            if (pick < CrossoverProb)
            {
                if (Method == CrossoverMethod.SwapRows)
                {
                    return SwapRows(dad, mom);
                }
                if (Method == CrossoverMethod.SwapColumns)
                {
                    return SwapColumns(dad, mom);
                }
                return SwapSubsquares(dad, mom);
            }
            return CreateChildren(dad, mom);
        }

        private SudokuGenome[] SwapColumns(SudokuGenome dad, SudokuGenome mom)
        {
            SudokuGenome[] children = CreateChildren(dad, mom);
            int firstSwap = seed.Next(1, 8);
            List<int[]> tempColumn = children[0].Columns;
            List<int[]> tempColumn2 = children[1].Columns;
            for (int j = firstSwap; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    children[0].UpdateValue(tempColumn2[j][i], i, j);
                    children[1].UpdateValue(tempColumn[j][i], i, j);
                }
            }            
            return children;
        }

        private SudokuGenome[] SwapSubsquares(SudokuGenome dad, SudokuGenome mom)
        {
            SudokuGenome[] children = CreateChildren(dad,mom);
            int k, j;
            k = seed.Next(0, 2);
            j = seed.Next(0, 2);
            SwapSubsquares(children[0], children[1], k, j);
            return children;
        }

        private SudokuGenome[] CreateChildren(SudokuGenome dad, SudokuGenome mom)
        {
            SudokuGenome[] children = new SudokuGenome[2];
            children[0] = new SudokuGenome();
            children[0].CopyGeneInfo(dad);

            children[1] = new SudokuGenome();
            children[1].CopyGeneInfo(mom);

            return children;
        }

        private void SwapSubsquares(SudokuGenome sudokuGenome1, SudokuGenome sudokuGenome2, int rowIndex, int colIndex)
        {
            int[,] temp = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    temp[i, j] = sudokuGenome1[rowIndex * 3 + i, colIndex * 3 + j];
                    sudokuGenome1.UpdateValue(sudokuGenome2[rowIndex * 3 + i, colIndex * 3 + j], rowIndex * 3 + i, colIndex * 3 + j);
                    sudokuGenome2.UpdateValue(temp[i,j], rowIndex * 3 + i, colIndex * 3 + j);
                }
            }
        }

        private SudokuGenome[] SwapRows(SudokuGenome dad, SudokuGenome mom)
        {
            SudokuGenome[] children = CreateChildren(dad, mom);
            int crossOverPoint = seed.Next(1, 8);
            List<int[]> firstSubsquares = children[0].Subsquares;
            List<int[]> secondSubsquares = children[1].Subsquares;
            List<int[]> temp = firstSubsquares.GetRange(crossOverPoint, 9 - crossOverPoint);
            List<int[]> temp2 = secondSubsquares.GetRange(crossOverPoint, 9 - crossOverPoint);
            firstSubsquares.RemoveRange(crossOverPoint, 9 - crossOverPoint);
            firstSubsquares.AddRange(temp2);
            secondSubsquares.RemoveRange(crossOverPoint, 9 - crossOverPoint);
            secondSubsquares.AddRange(temp);
            children[0].UpdateSubsquares(firstSubsquares);
            children[1].UpdateSubsquares(secondSubsquares);
            return children;
        }
    }
}
