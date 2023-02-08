using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("行數:");
            int rows = Convert.ToInt32(Console.ReadLine());
            for (int i = 1; i <= rows; i++) {

                for (int j = 1; j <= rows-i; j++) {
                    Console.Write(" ");
                }

                for (int k = 1; k <= 2*i-1; k++) {
                    Console.Write("*");
                }
                Console.Write("\n");
            }

    


            Console.Read();


        }
    }
}
