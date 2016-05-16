using Sudoku.GeneticLab;
using Sudoku.Model;
using SudokuSolver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    public enum SelectionMethod
    {
        Tournment = 0,
        Roulette = 1
    }

    public enum ReplacementMethod
    {
        AllPopulation = 0,
        Elitism = 1
    }

    public enum FitnessFunction
    {
        RowLineSubsquareConsistency = 0,
        SolutionComparison = 1
    }

    /// <summary>
    /// Summary description for Population.
    /// </summary>
    public class PopulationConfig
	{

		protected int kLength = 9;
		protected int kCrossover = 0;
        protected int tournmentSize = 2;
        protected int kInitialPopulation = 1000;
		protected int kPopulationLimit = 50;
		protected int kMax = 1000;
		protected double  kMutationFrequency = 0.33f;
        protected double kMutationPoints = 2;
        protected double kCrossoverFrequency = 0.33f;
        protected double  kDeathFitness = -1.00f;
		protected double  kReproductionFitness = 0.0f;
        protected SexMachine SexMachine;
        protected MutantAgent MutantAgent;
        protected ReplacementMethod ReplacementMethod;
        protected SelectionMethod SelectionMethod;
        protected FitnessFunction FitnessFunction;

        protected List<SudokuGenome> Genomes;
        protected List<SudokuGenome> GenomeReproducers;
        protected List<SudokuGenome> GenomeResults;

        protected Random seed;

        protected int CurrentPopulation;
		protected bool	  Best2 = true;
        protected string Puzzle = string.Empty;
        public string Solution
        {
            get; set;
        }

        public float MinFitness { get; set; }
        public float MaxFitness { get; set; }
        public float AverageFitness { get; set; }
        public SudokuGenome BestGenome { get; set; }

        public PopulationConfig(int populationSize, SelectionMethod selection, ReplacementMethod replacement, 
            int mutationProb, int mutationPoints, MutationMethod mutation, FitnessFunction fitness,
            int crossoverProb, CrossoverMethod crossover, string puzzle)
		{
            Genomes = new List<SudokuGenome>();
            GenomeReproducers = new List<SudokuGenome>();
            GenomeResults = new List<SudokuGenome>();

            seed = new Random();

            Puzzle = puzzle;
            kInitialPopulation = populationSize;
            kMutationFrequency = mutationProb / 100.0;
            kCrossoverFrequency = crossoverProb / 100.0;
            kMutationPoints = mutationPoints;

            ReplacementMethod = replacement;
            SelectionMethod = selection;
            SexMachine = new SexMachine(crossover, kCrossoverFrequency);
            MutantAgent = new SudokuMutator(mutation, mutationPoints);
            FitnessFunction = fitness;

            CalculateSolution();

            InitializePopulation();

            SetHighestScoreGenome();

            SetVariance();
        }

        private void CalculateSolution()
        {
            var initialCells = Puzzle.Replace('.','0').StringToCells();
            var resultCells = new SudokuSolver.SudokuSolver().Solve(initialCells);
            Solution = resultCells.CellsToString();
        }

        public bool HasStop
        {
            get
            {
                return BestGenome.CurrentFitness == 1;
            }
        }
        public int CurrentGeneration { get; internal set; }

        public float VariancePercentage { get; set; }

        public void InitializePopulation()
        {
            CurrentGeneration = 1;
            for (int i = 0; i < kInitialPopulation; i++)
            {
                SudokuGenome aGenome = new SudokuGenome(Puzzle, Solution, FitnessFunction);
                aGenome.CalculateFitness();
                Genomes.Add(aGenome);
            }
        }

		private void Mutate(SudokuGenome aGene)
		{
			if (seed.NextDouble() <= kMutationFrequency)
			{
                MutantAgent.Mutate(aGene);
			}
		}

		public void NextGeneration()
		{
			CurrentGeneration++;

            GenomeReproducers.Clear();
            GenomeResults.Clear();

            SelectParents();

            CreateNextGeneration();

            MutateAndCalculateFitness();

            ReplaceGeneration();

            MinFitness = Genomes.Min(c => c.CalculateFitness());
            MaxFitness = Genomes.Max(c => c.CalculateFitness());
            AverageFitness = Genomes.Average(c => c.CalculateFitness());

            CurrentPopulation = Genomes.Count;
            SetHighestScoreGenome();
            SetVariance();
        }

        private void SetVariance()
        {
            VariancePercentage = (float)Genomes.Select(c => c.ToStr()).Distinct().Count() / Genomes.Count();
        }

        private void MutateAndCalculateFitness()
        {
            foreach (var item in GenomeResults)
            {
                Mutate(item);
                float itemFitness = item.CalculateFitness();
            }
        }

        private void CreateNextGeneration()
        {
            for (int i = 0; i < kInitialPopulation / 2; i++)
            {
                SudokuGenome mom = GenomeReproducers[seed.Next(0, GenomeReproducers.Count)];
                SudokuGenome dad = GenomeReproducers[seed.Next(0, GenomeReproducers.Count)];
                SudokuGenome[] children = SexMachine.GenerateChildren(dad, mom);
                GenomeResults.Add(children[0]);
                GenomeResults.Add(children[1]);
            }

        }

        private void ReplaceGeneration()
        {
            if(ReplacementMethod == ReplacementMethod.AllPopulation)
            {
                //Allpopulation
                Genomes = GenomeResults.ToList();
            }
            else if(ReplacementMethod == ReplacementMethod.Elitism)
            {
                //Elitism
                GenomeResults.Sort();
                GenomeResults.RemoveAt(GenomeResults.Count - 1);
                Genomes.Sort();
                GenomeResults.Add(Genomes[0]);
                Genomes = GenomeResults.ToList();
            }
        }

        private void SelectParents()
        {
            Genomes.Sort();
            float fitnessSum = Genomes.Sum(g => g.CurrentFitness);
            for (int i = 0; i < kInitialPopulation / 2; i++)
            {
                if (SelectionMethod == SelectionMethod.Tournment)
                {
                    List<SudokuGenome> tournmentList = PickRandomGenomes();
                    tournmentList.Sort();
                    GenomeReproducers.Add(tournmentList[0]);
                }
                else if(SelectionMethod == SelectionMethod.Roulette)
                {
                    float randomFitness = (float)seed.NextDouble()*fitnessSum;
                    float sumK = 0;
                    for (int j = 0; j < Genomes.Count; j++)
                    {
                        sumK += Genomes[j].CurrentFitness;
                        if(sumK > randomFitness)
                        {
                            GenomeReproducers.Add(Genomes[j]);
                        }
                    }
                }
            }
        }

        private List<SudokuGenome> PickRandomGenomes()
        {
            List<SudokuGenome> list = new List<SudokuGenome>();
            for (int i = 0; i < tournmentSize; i++)
            {
                var gen = Genomes.ElementAt(seed.Next(Genomes.Count));
                list.Add(gen);
            }
            return list;
        }
        
		private Genome SetHighestScoreGenome()
		{
			Genomes.Sort();
            BestGenome = Genomes[0];
            return (Genome)BestGenome;
        }
    }
}
