using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Linq;
using Sudoku.Model;
using Sudoku.GeneticLab;
using System.Collections.Generic;

namespace Sudoku.View
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private GroupBox gbSudokuBoard;
        private GroupBox groupBox2;
        private Label label2;
        private ComboBox cbCrossoverMethod;
        private Button btnPauseContinue;
        private Button btnRun;
        private NumericUpDown npPopulationSize;
        private Label lblSelectionCriteria;
        private Label lbFitnessFunction;
        private Label lblPopSize;
        private ComboBox cbFitnessFunction;
        private ComboBox cbSelectionCriteria;
        private Button btnReset;
        private GroupBox groupBox3;
        private Label label1;
        private Label label4;
        private NumericUpDown npCrossoverProb;
        private Label lblCrossoverProb;
        private NumericUpDown npMutationProb;
        private Label lblMutationProb;
        private Label lblRepMet;
        private ComboBox cbReplacementMethod;
        private Label label3;
        private ComboBox cbMutationMethod;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtFitness;
        private GroupBox gbPuzzle;
        private Button btnGeneratePuzzle;
        private TextBox txtPuzzle;
        private Label lblPuzzle;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn c1;
        private DataGridViewTextBoxColumn c2;
        private DataGridViewTextBoxColumn c3;
        private DataGridViewTextBoxColumn c4;
        private DataGridViewTextBoxColumn c5;
        private DataGridViewTextBoxColumn c6;
        private DataGridViewTextBoxColumn c7;
        private DataGridViewTextBoxColumn c8;
        private DataGridViewTextBoxColumn c9;
        private Button btnNextGen;
        private NumericUpDown npNumberMutations;
        private BackgroundWorker worker;
        private TextBox txtSolution;
        private Label label5;
        bool _threadFlag = false;
		public Form1()
		{
			InitializeComponent();
            InitializeCombos();
		}

        private void InitializeCombos()
        {
            cbSelectionCriteria.DataSource = new List<ComboBoxSource>()
            {
                new ComboBoxSource { Text = "Roulette", Value = (int)SelectionMethod.Roulette},
                new ComboBoxSource { Text = "Tournment", Value = (int)SelectionMethod.Tournment},
            };
            cbSelectionCriteria.DisplayMember = "Text";
            cbSelectionCriteria.ValueMember = "Value";
            cbSelectionCriteria.SelectedValue = 0;

            cbReplacementMethod.DataSource = new List<ComboBoxSource>()
            {
                new ComboBoxSource { Text = "All Population", Value = (int)ReplacementMethod.AllPopulation},
                new ComboBoxSource { Text = "Elitism", Value = (int)ReplacementMethod.Elitism},
            };
            cbReplacementMethod.DisplayMember = "Text";
            cbReplacementMethod.ValueMember = "Value";
            cbReplacementMethod.SelectedValue = 0;

            cbMutationMethod.DataSource = new List<ComboBoxSource>()
            {
                new ComboBoxSource { Text = "Change Numbers", Value = (int)MutationMethod.ChangeSubsquareNumbers},
                new ComboBoxSource { Text = "Change Values", Value = (int)MutationMethod.ChangeIndividualValues},
            };
            cbMutationMethod.DisplayMember = "Text";
            cbMutationMethod.ValueMember = "Value";
            cbMutationMethod.SelectedValue = 0;

            cbCrossoverMethod.DataSource = new List<ComboBoxSource>()
            {
                new ComboBoxSource { Text = "Swap Columns", Value = (int)CrossoverMethod.SwapColumns},
                new ComboBoxSource { Text = "Swap Rows", Value = (int)CrossoverMethod.SwapRows},
                new ComboBoxSource { Text = "Swap Subsquares", Value = (int)CrossoverMethod.SwapSubsquares},
            };
            cbCrossoverMethod.DisplayMember = "Text";
            cbCrossoverMethod.ValueMember = "Value";
            cbCrossoverMethod.SelectedValue = 0;

            cbFitnessFunction.DataSource = new List<ComboBoxSource>()
            {
                new ComboBoxSource { Text = "Row / Line / Subsquare", Value = (int)FitnessFunction.RowLineSubsquareConsistency},
                new ComboBoxSource { Text = "Compare to Solution", Value = (int)FitnessFunction.SolutionComparison},
            };
            cbFitnessFunction.DisplayMember = "Text";
            cbFitnessFunction.ValueMember = "Value";
            cbFitnessFunction.SelectedValue = 0;
        }

        private System.Windows.Forms.StatusBar statusBar1;
               

	    PopulationConfig Population { get; set; }
        public List<ChartPoint> Points { get; set; }

        public SudokuBoardModel BoardModel { get; set; }
        public void UpdateGraphics()
		{
            SudokuGenome g = (SudokuGenome)Population.BestGenome;
            Console.WriteLine("Generation #{0}", Population.CurrentGeneration);
            Console.WriteLine(g.ToString());
            statusBar1.Text = String.Format("Current Fitness = {0} / % Different Genomes = {1}", g.CurrentFitness.ToString("0.000000"), Population.VariancePercentage.ToString("0.000000"));
            this.Text = String.Format("Sudoko Grid - Generation {0}", Population.CurrentGeneration);
            UpdateDataGrid(g);

            Points.Add(new ChartPoint { Average = Population.AverageFitness,
                Generation = Population.CurrentGeneration, Max = Population.MaxFitness, Min = Population.MinFitness });
            chtFitness.DataBind();
            chtFitness.Update();

            if (Population.HasStop)
            {
                Console.WriteLine("Final Solution at Generation {0}", Population.CurrentGeneration);
                statusBar1.Text = String.Format("Final Solution at Generation {0}", Population.CurrentGeneration);
                Console.WriteLine(g.ToString());
            }
        }

        private void UpdateDataGrid(SudokuGenome genome)
        {
            BoardModel.UptadeValues(genome);
            dataGridView1.DataSource = BoardModel.Rows;
            dataGridView1.Refresh();
        }

        private void SetDataSource(SudokuGenome genome)
        {
            BoardModel = new SudokuBoardModel(genome);
            dataGridView1.DataSource = BoardModel.Rows;
            SetFixedStyle(BoardModel);
        }

        private void SetFixedStyle(SudokuBoardModel model)
        {
            DataGridViewCellStyle fixedStyle = new DataGridViewCellStyle();
            fixedStyle.ApplyStyle(dataGridView1.DefaultCellStyle);
            fixedStyle.Font = new Font(fixedStyle.Font, FontStyle.Bold);
            fixedStyle.ForeColor = Color.DarkCyan;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (model.Genome.IsFixed[cell.RowIndex][cell.ColumnIndex])
                        cell.Style = fixedStyle;
                    else
                        cell.Style = dataGridView1.DefaultCellStyle;
                }
            }
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.gbSudokuBoard = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.c1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.npNumberMutations = new System.Windows.Forms.NumericUpDown();
            this.btnNextGen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.npCrossoverProb = new System.Windows.Forms.NumericUpDown();
            this.lblCrossoverProb = new System.Windows.Forms.Label();
            this.npMutationProb = new System.Windows.Forms.NumericUpDown();
            this.lblMutationProb = new System.Windows.Forms.Label();
            this.lblRepMet = new System.Windows.Forms.Label();
            this.cbReplacementMethod = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbMutationMethod = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbCrossoverMethod = new System.Windows.Forms.ComboBox();
            this.btnPauseContinue = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.npPopulationSize = new System.Windows.Forms.NumericUpDown();
            this.lblSelectionCriteria = new System.Windows.Forms.Label();
            this.lbFitnessFunction = new System.Windows.Forms.Label();
            this.lblPopSize = new System.Windows.Forms.Label();
            this.cbFitnessFunction = new System.Windows.Forms.ComboBox();
            this.cbSelectionCriteria = new System.Windows.Forms.ComboBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chtFitness = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.gbPuzzle = new System.Windows.Forms.GroupBox();
            this.btnGeneratePuzzle = new System.Windows.Forms.Button();
            this.txtPuzzle = new System.Windows.Forms.TextBox();
            this.lblPuzzle = new System.Windows.Forms.Label();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.txtSolution = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.gbSudokuBoard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.npNumberMutations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.npCrossoverProb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.npMutationProb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.npPopulationSize)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chtFitness)).BeginInit();
            this.gbPuzzle.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 586);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(1211, 25);
            this.statusBar1.TabIndex = 0;
            this.statusBar1.Text = "Ready...";
            // 
            // gbSudokuBoard
            // 
            this.gbSudokuBoard.Controls.Add(this.dataGridView1);
            this.gbSudokuBoard.Location = new System.Drawing.Point(602, 9);
            this.gbSudokuBoard.Name = "gbSudokuBoard";
            this.gbSudokuBoard.Size = new System.Drawing.Size(603, 571);
            this.gbSudokuBoard.TabIndex = 1;
            this.gbSudokuBoard.TabStop = false;
            this.gbSudokuBoard.Text = "Sudoku Board";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.c1,
            this.c2,
            this.c3,
            this.c4,
            this.c5,
            this.c6,
            this.c7,
            this.c8,
            this.c9});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 18);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 50;
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dataGridView1.Size = new System.Drawing.Size(597, 550);
            this.dataGridView1.TabIndex = 0;
            // 
            // c1
            // 
            this.c1.DataPropertyName = "c1";
            this.c1.HeaderText = "";
            this.c1.Name = "c1";
            this.c1.ReadOnly = true;
            // 
            // c2
            // 
            this.c2.DataPropertyName = "c2";
            this.c2.HeaderText = "";
            this.c2.Name = "c2";
            this.c2.ReadOnly = true;
            // 
            // c3
            // 
            this.c3.DataPropertyName = "c3";
            this.c3.HeaderText = "";
            this.c3.Name = "c3";
            this.c3.ReadOnly = true;
            // 
            // c4
            // 
            this.c4.DataPropertyName = "c4";
            this.c4.HeaderText = "";
            this.c4.Name = "c4";
            this.c4.ReadOnly = true;
            // 
            // c5
            // 
            this.c5.DataPropertyName = "c5";
            this.c5.HeaderText = "";
            this.c5.Name = "c5";
            this.c5.ReadOnly = true;
            // 
            // c6
            // 
            this.c6.DataPropertyName = "c6";
            this.c6.HeaderText = "";
            this.c6.Name = "c6";
            this.c6.ReadOnly = true;
            // 
            // c7
            // 
            this.c7.DataPropertyName = "c7";
            this.c7.HeaderText = "";
            this.c7.Name = "c7";
            this.c7.ReadOnly = true;
            // 
            // c8
            // 
            this.c8.DataPropertyName = "c8";
            this.c8.HeaderText = "";
            this.c8.Name = "c8";
            this.c8.ReadOnly = true;
            // 
            // c9
            // 
            this.c9.DataPropertyName = "c9";
            this.c9.HeaderText = "";
            this.c9.Name = "c9";
            this.c9.ReadOnly = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.npNumberMutations);
            this.groupBox2.Controls.Add(this.btnNextGen);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.npCrossoverProb);
            this.groupBox2.Controls.Add(this.lblCrossoverProb);
            this.groupBox2.Controls.Add(this.npMutationProb);
            this.groupBox2.Controls.Add(this.lblMutationProb);
            this.groupBox2.Controls.Add(this.lblRepMet);
            this.groupBox2.Controls.Add(this.cbReplacementMethod);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cbMutationMethod);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbCrossoverMethod);
            this.groupBox2.Controls.Add(this.btnPauseContinue);
            this.groupBox2.Controls.Add(this.btnRun);
            this.groupBox2.Controls.Add(this.npPopulationSize);
            this.groupBox2.Controls.Add(this.lblSelectionCriteria);
            this.groupBox2.Controls.Add(this.lbFitnessFunction);
            this.groupBox2.Controls.Add(this.lblPopSize);
            this.groupBox2.Controls.Add(this.cbFitnessFunction);
            this.groupBox2.Controls.Add(this.cbSelectionCriteria);
            this.groupBox2.Controls.Add(this.btnReset);
            this.groupBox2.Location = new System.Drawing.Point(20, 108);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(578, 194);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Genetic Algorithm Parameters";
            // 
            // npNumberMutations
            // 
            this.npNumberMutations.Location = new System.Drawing.Point(509, 87);
            this.npNumberMutations.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.npNumberMutations.Name = "npNumberMutations";
            this.npNumberMutations.Size = new System.Drawing.Size(61, 22);
            this.npNumberMutations.TabIndex = 26;
            this.npNumberMutations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnNextGen
            // 
            this.btnNextGen.Location = new System.Drawing.Point(262, 158);
            this.btnNextGen.Name = "btnNextGen";
            this.btnNextGen.Size = new System.Drawing.Size(98, 27);
            this.btnNextGen.TabIndex = 25;
            this.btnNextGen.Text = "Next Gen";
            this.btnNextGen.UseVisualStyleBackColor = true;
            this.btnNextGen.Click += new System.EventHandler(this.btnNextGen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(483, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 17);
            this.label1.TabIndex = 23;
            this.label1.Text = "%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(483, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 17);
            this.label4.TabIndex = 24;
            this.label4.Text = "%";
            // 
            // npCrossoverProb
            // 
            this.npCrossoverProb.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.npCrossoverProb.Location = new System.Drawing.Point(424, 115);
            this.npCrossoverProb.Name = "npCrossoverProb";
            this.npCrossoverProb.Size = new System.Drawing.Size(53, 22);
            this.npCrossoverProb.TabIndex = 22;
            this.npCrossoverProb.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblCrossoverProb
            // 
            this.lblCrossoverProb.AutoSize = true;
            this.lblCrossoverProb.Location = new System.Drawing.Point(302, 118);
            this.lblCrossoverProb.Name = "lblCrossoverProb";
            this.lblCrossoverProb.Size = new System.Drawing.Size(114, 17);
            this.lblCrossoverProb.TabIndex = 21;
            this.lblCrossoverProb.Text = "Crossover Prob.:";
            // 
            // npMutationProb
            // 
            this.npMutationProb.Location = new System.Drawing.Point(424, 87);
            this.npMutationProb.Name = "npMutationProb";
            this.npMutationProb.Size = new System.Drawing.Size(53, 22);
            this.npMutationProb.TabIndex = 20;
            this.npMutationProb.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblMutationProb
            // 
            this.lblMutationProb.AutoSize = true;
            this.lblMutationProb.Location = new System.Drawing.Point(302, 89);
            this.lblMutationProb.Name = "lblMutationProb";
            this.lblMutationProb.Size = new System.Drawing.Size(104, 17);
            this.lblMutationProb.TabIndex = 19;
            this.lblMutationProb.Text = "Mutation Prob.:";
            // 
            // lblRepMet
            // 
            this.lblRepMet.AutoSize = true;
            this.lblRepMet.Location = new System.Drawing.Point(302, 55);
            this.lblRepMet.Name = "lblRepMet";
            this.lblRepMet.Size = new System.Drawing.Size(130, 17);
            this.lblRepMet.TabIndex = 18;
            this.lblRepMet.Text = "Replacemt Method:";
            // 
            // cbReplacementMethod
            // 
            this.cbReplacementMethod.FormattingEnabled = true;
            this.cbReplacementMethod.Items.AddRange(new object[] {
            "AllPopulation",
            "Elitism"});
            this.cbReplacementMethod.Location = new System.Drawing.Point(424, 52);
            this.cbReplacementMethod.Name = "cbReplacementMethod";
            this.cbReplacementMethod.Size = new System.Drawing.Size(146, 24);
            this.cbReplacementMethod.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(302, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 17);
            this.label3.TabIndex = 16;
            this.label3.Text = "Mutation Methods:";
            // 
            // cbMutationMethod
            // 
            this.cbMutationMethod.FormattingEnabled = true;
            this.cbMutationMethod.Items.AddRange(new object[] {
            "Change Numbers",
            "Sum +1"});
            this.cbMutationMethod.Location = new System.Drawing.Point(424, 21);
            this.cbMutationMethod.Name = "cbMutationMethod";
            this.cbMutationMethod.Size = new System.Drawing.Size(146, 24);
            this.cbMutationMethod.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Crossover Method:";
            // 
            // cbCrossoverMethod
            // 
            this.cbCrossoverMethod.FormattingEnabled = true;
            this.cbCrossoverMethod.Items.AddRange(new object[] {
            "Swap Subsquares",
            "Swap Rows",
            "Swap Columns"});
            this.cbCrossoverMethod.Location = new System.Drawing.Point(128, 114);
            this.cbCrossoverMethod.Name = "cbCrossoverMethod";
            this.cbCrossoverMethod.Size = new System.Drawing.Size(147, 24);
            this.cbCrossoverMethod.TabIndex = 13;
            // 
            // btnPauseContinue
            // 
            this.btnPauseContinue.Location = new System.Drawing.Point(367, 158);
            this.btnPauseContinue.Name = "btnPauseContinue";
            this.btnPauseContinue.Size = new System.Drawing.Size(99, 27);
            this.btnPauseContinue.TabIndex = 12;
            this.btnPauseContinue.Text = "Pause";
            this.btnPauseContinue.UseVisualStyleBackColor = true;
            this.btnPauseContinue.Click += new System.EventHandler(this.btnPauseContinue_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(156, 158);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(98, 27);
            this.btnRun.TabIndex = 11;
            this.btnRun.Text = "Run!";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // npPopulationSize
            // 
            this.npPopulationSize.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.npPopulationSize.Location = new System.Drawing.Point(128, 22);
            this.npPopulationSize.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.npPopulationSize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.npPopulationSize.Name = "npPopulationSize";
            this.npPopulationSize.Size = new System.Drawing.Size(74, 22);
            this.npPopulationSize.TabIndex = 10;
            this.npPopulationSize.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // lblSelectionCriteria
            // 
            this.lblSelectionCriteria.AutoSize = true;
            this.lblSelectionCriteria.Location = new System.Drawing.Point(7, 87);
            this.lblSelectionCriteria.Name = "lblSelectionCriteria";
            this.lblSelectionCriteria.Size = new System.Drawing.Size(119, 17);
            this.lblSelectionCriteria.TabIndex = 7;
            this.lblSelectionCriteria.Text = "Selection Criteria:";
            // 
            // lbFitnessFunction
            // 
            this.lbFitnessFunction.AutoSize = true;
            this.lbFitnessFunction.Location = new System.Drawing.Point(7, 55);
            this.lbFitnessFunction.Name = "lbFitnessFunction";
            this.lbFitnessFunction.Size = new System.Drawing.Size(115, 17);
            this.lbFitnessFunction.TabIndex = 5;
            this.lbFitnessFunction.Text = "Fitness Function:";
            // 
            // lblPopSize
            // 
            this.lblPopSize.AutoSize = true;
            this.lblPopSize.Location = new System.Drawing.Point(7, 24);
            this.lblPopSize.Name = "lblPopSize";
            this.lblPopSize.Size = new System.Drawing.Size(110, 17);
            this.lblPopSize.TabIndex = 4;
            this.lblPopSize.Text = "Population Size:";
            // 
            // cbFitnessFunction
            // 
            this.cbFitnessFunction.DisplayMember = "Text";
            this.cbFitnessFunction.FormattingEnabled = true;
            this.cbFitnessFunction.Location = new System.Drawing.Point(128, 52);
            this.cbFitnessFunction.Name = "cbFitnessFunction";
            this.cbFitnessFunction.Size = new System.Drawing.Size(147, 24);
            this.cbFitnessFunction.TabIndex = 2;
            this.cbFitnessFunction.ValueMember = "Value";
            // 
            // cbSelectionCriteria
            // 
            this.cbSelectionCriteria.FormattingEnabled = true;
            this.cbSelectionCriteria.Items.AddRange(new object[] {
            "Tournment",
            "Roulette"});
            this.cbSelectionCriteria.Location = new System.Drawing.Point(128, 83);
            this.cbSelectionCriteria.Name = "cbSelectionCriteria";
            this.cbSelectionCriteria.Size = new System.Drawing.Size(147, 24);
            this.cbSelectionCriteria.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(473, 158);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(98, 27);
            this.btnReset.TabIndex = 0;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chtFitness);
            this.groupBox3.Location = new System.Drawing.Point(17, 308);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(576, 272);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Fitness Analysis";
            // 
            // chtFitness
            // 
            chartArea1.AxisX.Title = "Generations";
            chartArea1.AxisY.Title = "Fitness Value";
            chartArea1.Name = "ChartArea1";
            this.chtFitness.ChartAreas.Add(chartArea1);
            this.chtFitness.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.chtFitness.Legends.Add(legend1);
            this.chtFitness.Location = new System.Drawing.Point(3, 18);
            this.chtFitness.Name = "chtFitness";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "Average";
            series1.XValueMember = "Generation";
            series1.YValueMembers = "Average";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Min";
            series2.XValueMember = "Generation";
            series2.YValueMembers = "Min";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Legend = "Legend1";
            series3.Name = "Max";
            series3.XValueMember = "Generation";
            series3.YValueMembers = "Max";
            this.chtFitness.Series.Add(series1);
            this.chtFitness.Series.Add(series2);
            this.chtFitness.Series.Add(series3);
            this.chtFitness.Size = new System.Drawing.Size(570, 251);
            this.chtFitness.TabIndex = 0;
            this.chtFitness.Text = "chart1";
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title1.Name = "Generation Fitness";
            title1.Text = "Generation Fitness";
            this.chtFitness.Titles.Add(title1);
            // 
            // gbPuzzle
            // 
            this.gbPuzzle.Controls.Add(this.txtSolution);
            this.gbPuzzle.Controls.Add(this.label5);
            this.gbPuzzle.Controls.Add(this.btnGeneratePuzzle);
            this.gbPuzzle.Controls.Add(this.txtPuzzle);
            this.gbPuzzle.Controls.Add(this.lblPuzzle);
            this.gbPuzzle.Location = new System.Drawing.Point(17, 9);
            this.gbPuzzle.Name = "gbPuzzle";
            this.gbPuzzle.Size = new System.Drawing.Size(578, 93);
            this.gbPuzzle.TabIndex = 2;
            this.gbPuzzle.TabStop = false;
            this.gbPuzzle.Text = "Sudoku Board";
            // 
            // btnGeneratePuzzle
            // 
            this.btnGeneratePuzzle.Location = new System.Drawing.Point(473, 17);
            this.btnGeneratePuzzle.Name = "btnGeneratePuzzle";
            this.btnGeneratePuzzle.Size = new System.Drawing.Size(98, 27);
            this.btnGeneratePuzzle.TabIndex = 27;
            this.btnGeneratePuzzle.Text = "Generate";
            this.btnGeneratePuzzle.UseVisualStyleBackColor = true;
            this.btnGeneratePuzzle.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPuzzle
            // 
            this.txtPuzzle.Location = new System.Drawing.Point(128, 20);
            this.txtPuzzle.MaximumSize = new System.Drawing.Size(281, 20);
            this.txtPuzzle.MaxLength = 81;
            this.txtPuzzle.MinimumSize = new System.Drawing.Size(281, 20);
            this.txtPuzzle.Name = "txtPuzzle";
            this.txtPuzzle.Size = new System.Drawing.Size(281, 20);
            this.txtPuzzle.TabIndex = 26;
            // 
            // lblPuzzle
            // 
            this.lblPuzzle.AutoSize = true;
            this.lblPuzzle.Location = new System.Drawing.Point(7, 23);
            this.lblPuzzle.Name = "lblPuzzle";
            this.lblPuzzle.Size = new System.Drawing.Size(104, 17);
            this.lblPuzzle.TabIndex = 25;
            this.lblPuzzle.Text = "Enter a Puzzle:";
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += Worker_DoWork;
            this.worker.ProgressChanged += Worker_ProgressChanged;
            this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            // 
            // txtSolution
            // 
            this.txtSolution.Location = new System.Drawing.Point(128, 46);
            this.txtSolution.MaximumSize = new System.Drawing.Size(281, 20);
            this.txtSolution.MaxLength = 81;
            this.txtSolution.MinimumSize = new System.Drawing.Size(281, 20);
            this.txtSolution.Name = "txtSolution";
            this.txtSolution.Size = new System.Drawing.Size(281, 20);
            this.txtSolution.TabIndex = 29;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 17);
            this.label5.TabIndex = 28;
            this.label5.Text = "Solution:";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(1211, 611);
            this.Controls.Add(this.gbPuzzle);
            this.Controls.Add(this.gbSudokuBoard);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.statusBar1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1229, 658);
            this.MinimumSize = new System.Drawing.Size(1229, 658);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.gbSudokuBoard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.npNumberMutations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.npCrossoverProb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.npMutationProb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.npPopulationSize)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chtFitness)).EndInit();
            this.gbPuzzle.ResumeLayout(false);
            this.gbPuzzle.PerformLayout();
            this.ResumeLayout(false);

		}

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateGraphics();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateGraphics();
        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
        
        		
		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_threadFlag = true;
		}

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (Population == null)
                CreatePopulation();
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (!Population.HasStop)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    Population.NextGeneration();
                    worker.ReportProgress(0);
                }
            }
            worker.ReportProgress(100);
        }

        private void CreatePopulation()
        {
            SelectionMethod selection = (SelectionMethod)cbSelectionCriteria.SelectedValue;
            ReplacementMethod replacement = (ReplacementMethod)cbReplacementMethod.SelectedValue;
            MutationMethod mutation = (MutationMethod)cbMutationMethod.SelectedValue;
            CrossoverMethod crossover = (CrossoverMethod)cbCrossoverMethod.SelectedValue;
            FitnessFunction function = (FitnessFunction)cbFitnessFunction.SelectedValue;
            Population = new PopulationConfig((int)npPopulationSize.Value, selection, replacement, (int)npMutationProb.Value, (int)npNumberMutations.Value, mutation, function, (int)npCrossoverProb.Value,
                crossover, txtPuzzle.Text);
            Points = new List<ChartPoint>();
            txtSolution.Text = Population.Solution;
            Points.Add(new ChartPoint
            {
                Average = Population.AverageFitness,
                Generation = Population.CurrentGeneration,
                Max = Population.MaxFitness,
                Min = Population.MinFitness
            });
            chtFitness.DataSource = Points;
            chtFitness.DataBind();
            SetDataSource(Population.BestGenome);
        }

        private void btnPauseContinue_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
            Population = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtPuzzle.Text = SudokuGenome.CreatePuzzle();
        }

        private void btnNextGen_Click(object sender, EventArgs e)
        {
            if(Population == null)
                CreatePopulation();
            Population.NextGeneration();
            UpdateGraphics();
        }
    }
}
