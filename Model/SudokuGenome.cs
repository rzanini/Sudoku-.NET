using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.Model
{
	/// <summary>
	/// Summary description for SudokuGenome.
	/// </summary>
	public class SudokuGenome : Genome
	{
		protected int[,] SudokuBoard = new int[9,9];
        protected int[] SudokuSequence = new int[81];
        public int[] Puzzle = new int[81];
        public int[] Solution = new int[81];
        protected Random seed = new Random();
        public FitnessFunction FitnessFunction { get; set; }
        public SudokuGenome()
        {
        }

        public SudokuGenome(string puzzle, string solution, FitnessFunction fitness) : this()
        {
            string convertedPuzzle = puzzle.Replace('.', '0');
            Puzzle = ToInt(convertedPuzzle);
            Solution = ToInt(solution);
            SudokuBoard = ToBoard(Puzzle);
            GenerateRandomSequence();
            FitnessFunction = fitness;
        }

        public void UpdateSubsquares(List<int[]> subsquares)
        {
            SudokuBoard = ToBoard(subsquares);
            SudokuSequence = ToSequence(SudokuBoard);
        }

        private void GenerateRandomSequence()
        {
            List<int[]> subsquares = ToSubsquares(ToBoard(Puzzle));
            foreach (var item in subsquares)
            {
                FillSubsquare(item);
            }
            SudokuBoard = ToBoard(subsquares);
            SudokuSequence = ToSequence(SudokuBoard);
        }

        private int[,] ToBoard(List<int[]> subsquares)
        {
            int[,] board = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                int[] subsquare = subsquares[i];
                int baseRowIndex = (i / 3) * 3;
                int baseColumnIndex = (i % 3) * 3;
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        board[baseRowIndex + j, baseColumnIndex + k] = subsquare[j * 3 + k];
                    }
                }
            }
            return board;
        }

        private void FillSubsquare(int[] subsquare)
        {
            List<int> availableNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            List<int> fixedNumbers = subsquare.Where(i => i != 0).ToList();
            availableNumbers.RemoveAll(c => fixedNumbers.Contains(c));
            for (int k = 0; k < 9 && availableNumbers.Count > 0; k++)
            {
                int val = subsquare[k];
                if (val == 0)
                {
                    val = seed.Next(0, availableNumbers.Count);
                    val = availableNumbers.ElementAt(val);
                    subsquare[k] = val;
                    availableNumbers.Remove(val);
                }
            }
        }

        internal void GenerateRandomSubsquare(int index)
        {
            List<int[]> subsquares = ToSubsquares(ToBoard(Puzzle));
            int[] selected = subsquares[index];
            FillSubsquare(selected);
            List<int[]> filledSubsquares = ToSubsquares(SudokuBoard);
            filledSubsquares[index] = selected;
            SudokuBoard = ToBoard(filledSubsquares);
            SudokuSequence = ToSequence(SudokuBoard);
        }

        internal void UpdateValue(int newValue, int i, int j)
        {
            SudokuBoard[i, j] = newValue;
            SudokuSequence = ToSequence(SudokuBoard);
        }

        private int[] ToSequence(int[,] board)
        {
            int size = (int)Math.Sqrt(board.Length);
            int[] output = new int[board.Length];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    output[i * size + j] = board[i, j];
                }
            }
            return output;
        }

        private int[,] ToBoard(int[] sequence)
        {
            int size = (int)Math.Sqrt(sequence.Length);
            int[,] output = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    output[i, j] = sequence[i * size + j];
                }
            }
            return output;
        }

        protected int[] ToInt(string convertedPuzzle)
        {
            return convertedPuzzle.Select(c => int.Parse(c.ToString())).ToArray();
        }

        protected string ToStr(int[] intArray)
        {
            return intArray.Select(i => Convert.ToChar(i)).ToString();
        }

        public string ToStr()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < SudokuSequence.Length; i++)
            {
                builder.Append(SudokuSequence[i].ToString());
            }
            return builder.ToString();
        }

        public int this [int index1, int index2]
		{
			get
			{
				return SudokuBoard[index1, index2];
			}
		}

		public override int CompareTo(object a)
		{
			SudokuGenome Gene1 = this;
			SudokuGenome Gene2 = (SudokuGenome)a;
			return Math.Sign(Gene2.CurrentFitness  -  Gene1.CurrentFitness);
		}
        

        public List<int[]> Rows
        {
            get
            {
                List<int[]> rows = new List<int[]>();
                for (int i = 0; i < 9; i++)
                {
                    int[] row = SudokuSequence.ToList().GetRange(i * 9, 9).ToArray();
                    rows.Add(row);
                }
                return rows;
            }
        }

        public List<int[]> Columns
        {
            get
            {
                List<int[]> columns = new List<int[]>();
                for (int i = 0; i < 9; i++)
                {
                    int[] column = new int[9];
                    for (int j = 0; j < 9; j++)
                    {
                        column[j] = SudokuBoard[j,i];
                    }
                    columns.Add(column);
                }
                return columns;
            }
        }

        public List<int[]> Subsquares
        {
            get
            {
                return ToSubsquares(SudokuBoard);
            }
        }

        public List<int[]> ToSubsquares(int[,] a)
        {
                List<int[]> subsquares = new List<int[]>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var sub = new int[] { a[i * 3, j * 3], a[i * 3, j * 3 + 1], a[i * 3, j * 3 + 2], a[i * 3 + 1, j * 3], a[i * 3 + 1, j * 3 + 1], a[i * 3 + 1, j * 3 + 2], a[i * 3 + 2, j * 3], a[i * 3 + 2, j * 3 + 1], a[i * 3 + 2, j * 3 + 2] };
                        subsquares.Add(sub);
                    }
                }

                return subsquares;
        }

        /// <summary>
        /// Calculate the uniqueness of values within all columns.
        /// The maximum fit is 9 for each column. Each repeated value decreases the fitness value in 1.
        /// If all columns only contain different elements, the fitness will be 100%
        /// </summary>
        /// <returns></returns>
        private float CalculateColumnFitness()
        {
            float columnFitness = 0;
            foreach (var col in Columns)
            {
                columnFitness += col.Distinct().Count();
            }
            return columnFitness / (float)81.0;
        }

        /// <summary>
        /// Calculate the uniqueness of values within all rows.
        /// The maximum fit is 9 for each column. Each repeated value decreases the fitness value in 1.
        /// If all columns only contain different elements, the fitness will be 100%
        /// </summary>
        /// <returns></returns>
        private float CalculateRowFitness()
        {
            float rowFitness = 0;
            foreach (var row in Rows)
            {
                rowFitness += row.Distinct().Count();
            }
            return rowFitness / (float)81.0;
        }

        /// <summary>
        /// Calculate the uniqueness of values within all rows.
        /// The maximum fit is 9 for each column. Each repeated value decreases the fitness value in 1.
        /// If all columns only contain different elements, the fitness will be 100%
        /// </summary>
        /// <returns></returns>
        private float CalculateSubsquareFitness()
        {
            float subFitness = 0;
            List<int[]> subs = Subsquares;
            foreach (var sub in subs)
            {
                subFitness += sub.Distinct().Count();
            }
            return subFitness / (float)81.0;
        }

        /// <summary>
        /// The Calculate Sudoku Fitness uses the uniqueness of columns, rows
        /// and 3x3 squares in the grid to determine a fitness value
        /// </summary>
        /// <returns></returns>
        private float CalculateSudokuFitness()
		{
            //CurrentFitness is the average of column, row and subsquare fitness
            if (FitnessFunction == FitnessFunction.RowLineSubsquareConsistency)
                CurrentFitness = (CalculateColumnFitness() * CalculateRowFitness() * CalculateSubsquareFitness());
            else
            {
                int count = 0;
                for (int i = 0; i < 81; i++)
                {
                    count += SudokuSequence[i] == Solution[i] ? 1 : 0;
                }
                CurrentFitness = (float)count / 81;
            }
			return CurrentFitness;
		}


		public override float CalculateFitness()
		{
			CalculateSudokuFitness();
			return CurrentFitness;
		}

		public override string ToString()
		{
			string strResult = "";
			for (int j = 0; j < 9; j++)
			{
				for (int i = 0; i < 9; i++)
				{
					strResult = strResult + ((int)SudokuBoard[i, j]).ToString() + " ";
				}
				strResult += "\r\n";
			}

			strResult += "-->" + CurrentFitness.ToString();

			return strResult;
		}



		public override void CopyGeneInfo(Genome dest)
		{
			SudokuGenome theGene = (SudokuGenome)dest;
            theGene.Puzzle.CopyTo(Puzzle,0);
            theGene.Solution.CopyTo(Solution, 0);
            FitnessFunction = theGene.FitnessFunction;
            SudokuSequence = ToInt(theGene.ToStr());
            SudokuBoard = ToBoard(SudokuSequence);
		}

        public void CopyGeneInfo(string genome)
        {
            SudokuSequence = ToInt(genome);
            SudokuBoard = ToBoard(SudokuSequence);
        }

        
        internal static string CreatePuzzle()
        {
            return "..3.2.6..9..3.5..1..18.64....81.29..7.......8..67.82....26.95..8..2.3..9..5.1.3..";
        }

        internal bool IsFixedPosition(int i, int j)
        {
            return Puzzle[i * 9 + j] != 0;
        }

        public List<bool[]> IsFixed
        {
            get
            {
                List<bool[]> isFixed = new List<bool[]>();
                for (int i = 0; i < 9; i++)
                {
                    bool[] row = new bool[9];
                    for (int j = 0; j < 9; j++)
                    {
                        row[j] = IsFixedPosition(i, j);
                    }
                    isFixed.Add(row);
                }
                return isFixed;
            }
        }
    }
}
