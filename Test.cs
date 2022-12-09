using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxCalculatorInterviewTests
{
    internal class Test
    {
        static void Main(string[] args)
        {
            TaxCalculator taxCalculator = new TaxCalculator();
            Console.WriteLine(taxCalculator.GetStandardTaxRate(Commodity.FoodServices));
            taxCalculator.SetCustomTaxRate(Commodity.FoodServices, 15);
            Console.WriteLine(taxCalculator.GetStandardTaxRate(Commodity.FoodServices));

            Console.WriteLine(taxCalculator.GetTaxRateForDateTime(Commodity.FoodServices,DateTime.UtcNow));

            Console.WriteLine(taxCalculator.GetTaxRateForDateTime(Commodity.FoodServices, DateTime.UtcNow.AddMinutes(-1)));


            Console.ReadLine();
        }
    }
}
