using System;
using System.Collections;

namespace Sudoku.Model
{
	/// <summary>
	/// Summary description for Genome.
	/// </summary>
	public abstract class Genome : IComparable
	{
		public long Length;
		public int  CrossoverPoint;
		public int  MutationIndex;
		public float CurrentFitness = 0.0f;
        
		abstract public float CalculateFitness();
				abstract public string ToString();
		abstract public void	CopyGeneInfo(Genome g);

		
		abstract public int CompareTo(object a);

	}
}
