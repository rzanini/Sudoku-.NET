using Sudoku.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.GeneticLab
{

    public enum MutationMethod
    {
        ChangeSubsquareNumbers = 0,
        ChangeIndividualValues = 1
    }
    public class SudokuMutator : MutantAgent
    {
        private Random seed;
        public MutationMethod Method { get; set; }
        private int NumberOfPoints = 2;

        public SudokuMutator(MutationMethod method, int point)
        {
            seed = new Random();
            Method = method;
            NumberOfPoints = point;
        }

        public override void Mutate(SudokuGenome original)
        {
            SudokuGenome sudokuGen = original as SudokuGenome;
            for (int i = 0; i < NumberOfPoints; i++)
            {
                if (Method == MutationMethod.ChangeSubsquareNumbers)
                {
                    int subsquareIndex = seed.Next(0, 9);
                    original.GenerateRandomSubsquare(subsquareIndex);
                }
                else
                {
                    int row = seed.Next(0, 9);
                    int col = seed.Next(0, 9);
                    int value = seed.Next(1, 10);
                    while(original.IsFixedPosition(row, col))
                    {
                        row = seed.Next(0, 9);
                        col = seed.Next(0, 9);
                    }
                    original.UpdateValue(value, row, col);
                }
            }
        }
    }
}
