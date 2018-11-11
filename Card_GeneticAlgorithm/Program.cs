using System;
using System.Collections.Generic;
using System.Text;

namespace Card_GeneticAlgorithm
{
    class Program
    {
        //main access point
        static void Main(string[] args)
        {
            //create a new Microbial GA
            CardGA GA = new CardGA(30);
            GA.run();
            //read a line, to stop the Console window closing
            Console.ReadLine();
        }
    }
}
