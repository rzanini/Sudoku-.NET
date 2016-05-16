using Sudoku.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.GeneticLab
{
    public abstract class MutantAgent
    {
        public abstract void Mutate(SudokuGenome original);
    }
}
