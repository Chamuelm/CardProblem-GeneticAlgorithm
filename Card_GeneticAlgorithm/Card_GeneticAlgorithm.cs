using System;
using System.Collections.Generic;
using System.Text;

namespace Card_GeneticAlgorithm
{
    public class CardGA
    {
        int[] CARDS = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // Available cards
        private int POP = 30; // Population size, default is 30
        private int LEN = 5; // Num of genes
        private int WINSCORE = 42; // Minimum score of individual for win
        private double MUT = 0.1; // Mutation rate
        private double END = 1000; // How many tournaments should be played

        private int SUMTARG = 36; // Target result for the sum pile
        private int MAX_DIFF_FROM_SUM = 21; // 36 - (1+2+3+4+5)
        private int PRODTARG = 360; // Target result for the prod pile
        private int MAX_DIFF_FROM_PROD = 29880; // (10*9*8*7*6*5)-360
        private double DIV_FACTOR = 29880 / 21; // MAX_DIFF_FROM_PROD / MAX_DIFF_FROM_SUM, to get balanced weights


        private int[,] gene; // The genes matrix
        private int[,] geneCombined; // Temp matrix for creating next generation
        private int[] eval; // Current generation evaluation

        Random rnd = new Random();  //used to create randomness

        // Constructor, create population data structures and initialize it
        public CardGA(int pop)
        {
            this.POP = pop;
            gene = new int[POP, LEN];
            eval = new int[POP];

            init_pop(); // Initialise the population (randomly)
            geneCombined = new int[POP, LEN]; // Init temp matrix
        }

        // Runs the GA to solve the problem domain
        // Where the problem domain is specified as follows
        //
        // You have 10 cards numbered 1 to 10.
        // You have to divide them into 2 even piles so that:
        //
        // The sum of the first pile is as close as possible to 36
        // And the product of all in second pile is as close as poss to 360
        //
        // Each individual is charcterized by 5 cards numbers which are the product members
        // The rest of the cards are the sum members
        public void run()
        {
            int a, b; // Declare pop member a,b
            bool foundWin = false;
            int splitLocation;

            // Start a tournament
            for (int tournamentNo = 0; (tournamentNo < END) && !foundWin; tournamentNo++)
            {
                // Display
                Console.WriteLine("==============================");
                Console.WriteLine("========== Round {0,2} ==========", tournamentNo);

                // Calculate evaluation for current generation population
                evaluateGeneration();
                displayCurrentPopulation();
                Console.WriteLine("");
                // Check for win result
                for (int i = 0; i < POP; i++)
                {
                    if (eval[i] >= WINSCORE)
                    {
                        display(tournamentNo, i);
                        foundWin = true;
                    }
                }

                if (foundWin)
                    break;

                // For each next generation individual - select 2 random individuals (by their weights)
                // and combine them to create next gen individual
                for (int i = 0; i < POP; i++)
                {
                    //Display
                    Console.WriteLine("**** Creating individual number {0:D2} ****", i);

                    // Pull 2 population members by their weighted avarage
                    a = getRandomIndividual();
                    b = getRandomIndividual();
                    Console.WriteLine("Parents:");
                    displayIndividual(a);
                    displayIndividual(b);

                    // Get random location to split individuals
                    splitLocation = rnd.Next(1, 5);
                    Console.WriteLine("Location to split: {0}", splitLocation);

                    // Recombine a and b
                    recombine(a, b, splitLocation, i);

                    // Maybe do some mutation
                    if (rnd.NextDouble() < MUT)
                    {
                        mutate(i);
                        Console.WriteLine("Mutation was done");
                    }

                    Console.Write("Recombined: ");
                    displayIndividual(i, geneCombined);

                    // Test to see if the new population member is a winner
                    if (evaluate(i) >= WINSCORE)
                    {
                        display(tournamentNo, i);
                        foundWin = true;
                    }

                    //Console.WriteLine("Press any key to continue");
                    //Console.ReadKey();
                    Console.WriteLine("*****************************************", i);
                }

                // After finished creating all new generation copy them to main matrix
                copyCombined();
                
                if (tournamentNo == 999)
                {
                    int maxValue = 0, maxIndex = 0;
                    for (int i = 0; i < POP; i++)
                    {
                        if (eval[i] > maxValue)
                        {
                            maxValue = eval[i];
                            maxIndex = i;
                        }
                    }
                    display(tournamentNo, maxIndex);
                }
            }
        }

        // Evaluate score for each individual in main matrix
        private void evaluateGeneration()
        {
            for (int i = 0; i < POP; i++)
            {
                eval[i] = (int)evaluate(i);
            }
        }

        // Returns weighted random individual from main matrix
        private int getRandomIndividual()
        {
            int sumOfWeights = 0;
            int currSum;

            foreach (int weight in eval)
                sumOfWeights += weight;

            int chosen = rnd.Next(sumOfWeights);

            currSum = 0;
            for (int i = 0; i < POP; i++)
            {
                currSum += eval[i];
                if (chosen < currSum)
                    return i;
            }

            throw new IndexOutOfRangeException("Cannot find chosen element");
        }

        // Recombine 2 individuals a,b from main matrix and save result in recombineGene
        // with given index popIndex and given splitLocation
        private void recombine(int a, int b, int splitLocation, int popIndex)
        {
            int i;
            List<int> possible = new List<int>(CARDS);


            // Updating first part of new individual
            for (i = 0; i < splitLocation; i++)
            {
                geneCombined[popIndex, i] = gene[a, i];
                possible.Remove(gene[a, i]);
            }

            // Updating possible b genes in case of repeated genes after splitLocation
            List<int> bPossible = new List<int>();
            for (i = 0; i < splitLocation; i++)
            {
                if (possible.Contains(gene[b, i]))
                    bPossible.Add(gene[b, i]);
            }

            // Updating second part of new individual
            for (i = splitLocation; i < LEN; i++)
            {
                if (possible.Contains(gene[b, i]))
                {   // If not repeated number
                    geneCombined[popIndex, i] = gene[b, i];
                    possible.Remove(gene[b, i]);
                }
                else
                {   // Take from first part of b id card i is repeated
                    int bIndex = rnd.Next(bPossible.Count);
                    geneCombined[popIndex, i] = bPossible[bIndex];
                    bPossible.RemoveAt(bIndex);
                }
            }
        }

        // Mutate the i-th item
        // @param i : individual's index to mutate
        private void mutate(int i)
        {
            List<int> possible = new List<int>(CARDS);
            for (int j = 0; j < LEN; j++)
                possible.Remove(geneCombined[i, j]);

            int indexToMutate = rnd.Next(LEN);
            int newCardToPush = rnd.Next(possible.Count);
            geneCombined[i, indexToMutate] = possible[newCardToPush];
        }

        //Display the results. Only called for good GA which has solved
        //the problem domain
        //@param tournaments : the current tournament loop number
        //@param n : the nth member of the population. 
        private void display(int tournaments, int n)
        {
            List<int> possible = new List<int>(CARDS);

            Console.WriteLine("\r\n==============================\r\n");
            Console.WriteLine("After " + tournaments + " tournaments, Solution prod pile (should be 360) cards are : ");
            for (int i = 0; i < LEN; i++)
            {
                Console.WriteLine(gene[n, i]);
                possible.Remove(gene[n, i]);
            }

            Console.WriteLine("\r\nAnd sum pile (should be 36)  cards are : ");
            foreach (int card in possible)
            {
                Console.WriteLine(card);
            }
        }

        // Display population of current generation
        private void displayCurrentPopulation()
        {
            for (int i = 0; i < POP; i++)
            {
                displayIndividual(i);
            }
        }


        // Display individual i from current main matrix
        private void displayIndividual(int i)
        {
            displayIndividual(i, gene);
        }

        // Display individual i from given matrix
        private void displayIndividual(int i, int[,] matrix)
        {
            List<int> possible = new List<int>(CARDS);
            String prodStr = "";
            String sumStr = "";

            for (int j = 0; j < LEN; j++)
            {
                prodStr += " " + matrix[i, j];
                if (j < LEN - 1)
                    prodStr += ",";
                possible.Remove(matrix[i, j]);
            }

            for (int j = 0; j < possible.Count; j++)
            {
                sumStr += " " + possible[j];
                if (j < possible.Count - 1)
                    sumStr += ",";
            }
            Console.Write("{0:D2} - PROD: {1,-15}     SUM: {2,-15}", i, prodStr, sumStr);
            int evalNew;
            if (ReferenceEquals(matrix, gene))
                evalNew = eval[i];
            else
                evalNew = (int)evaluate(i, geneCombined);
            Console.WriteLine("    Evaluation: {0}.", evalNew);
        }

        //evaluate the the nth member of the population
        //@param n : the nth member of the population
        //@return : the score for this member of the population.
        //If score is ~ 42, then we have a good GA which has solved
        //the problem domain
        private double evaluate(int n)
        {
            return evaluate(n, gene);
        }

        private double evaluate(int n, int[,] matrix)
        {
            // Initialise field values
            int sum = 0, prod = 1;
            List<int> possible = new List<int>(CARDS);
            int calculatedSum, calculatedProd;

            // Loop though all genes for this population member
            for (int i = 0; i < LEN; i++)
            {
                int cardNum = matrix[n, i];
                possible.Remove(cardNum);
                prod *= cardNum;
            }

            // Loop through cards in sum deck and calculate sum
            foreach (int card in possible)
            {
                sum += card;
            }

            // As the sum is close to target, the diff is lower and therefore 
            // calculatedSum is higher (best is MAX_DIIF_FROM_SUM = 21)
            calculatedSum = MAX_DIFF_FROM_SUM - Math.Abs(SUMTARG - sum);

            // As the prod is close to target, the diff is lower and therefore 
            // calculatedProd is higher (best is MAX_DIIF_FROM_PROD/DIV_FACTOR ~= 21)
            calculatedProd = (int)((MAX_DIFF_FROM_PROD - Math.Abs(PRODTARG - prod)) / DIV_FACTOR);

            return calculatedProd + calculatedSum;
        }

        // initialise population
        private void init_pop()
        {
            //for entire population
            for (int i = 0; i < POP; i++)
            {
                // Get cards randomlt without repetition

                List<int> possible = new List<int>(CARDS);
                for (int j = 0; j < LEN; j++)
                {
                    int index = rnd.Next(0, possible.Count);
                    gene[i, j] = possible[index];
                    possible.RemoveAt(index);
                }
            }
        }

        // Copy combined matrix to main matrix
        private void copyCombined()
        {
            for (int i = 0; i < POP; i++)
            {
                for (int j = 0; j < LEN; j++)
                    gene[i, j] = geneCombined[i, j];
            }
        }
    }
}
