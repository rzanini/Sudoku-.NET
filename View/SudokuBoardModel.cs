using Sudoku.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.View
{
    public class SudokuBoardModel
    {
        public class SudokuRow
        {
            public int c1 { get; set; }
            public int c2 { get; set; }
            public int c3 { get; set; }
            public int c4 { get; set; }
            public int c5 { get; set; }
            public int c6 { get; set; }
            public int c7 { get; set; }
            public int c8 { get; set; }
            public int c9 { get; set; }
        }

        public List<SudokuRow> Rows { get; set; }

        public SudokuGenome Genome { get; set; }

        public SudokuBoardModel(SudokuGenome genome)
        {
            Rows = new List<SudokuRow>();
            Rows = genome.Rows.Select(c => new SudokuRow { c1 = c[0], c2 = c[1], c3 = c[2], c4 = c[3], c5 = c[4], c6 = c[5], c7 = c[6], c8 = c[7], c9 = c[8] }).ToList();
            Genome = genome;
        }

        public void UptadeValues(SudokuGenome genome)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                Rows[i].c1 = genome[i, 0];
                Rows[i].c2 = genome[i, 1];
                Rows[i].c3 = genome[i, 2];
                Rows[i].c4 = genome[i, 3];
                Rows[i].c5 = genome[i, 4];
                Rows[i].c6 = genome[i, 5];
                Rows[i].c7 = genome[i, 6];
                Rows[i].c8 = genome[i, 7];
                Rows[i].c9 = genome[i, 8];
            }
        }

    }
}
