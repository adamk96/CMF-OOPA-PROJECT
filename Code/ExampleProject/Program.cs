using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel;
using HestonModel.Interfaces;
using HestonModel.InterfaceImplement;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // Price an Asian call option with maturity 1 year, monitoring times 0.75 and 1, strike price 100
            // parameters as in (2) in project tasks, using 10000 trials and 365 timesteps a year.
            double[] T = { 0.75, 1.0 };

            InterfaceFill asianOption = new InterfaceFill(1, 2, 0.06, 0.4, 0.5, 0.04, 10000, 365, 0, T, 100, 0.01);
            IAsianOption a = asianOption;
            IMonteCarloSettings settings = asianOption;

            OtherInterfaceFill pars = new OtherInterfaceFill(1, 2, 0.06, 0.4, 0.5, 0.04, 1000, 1000, 100, 0.1, 0, 0.01);
            IHestonModelParameters parameters = pars;

            Console.WriteLine(HestonModel.Heston.HestonAsianOptionPriceMC(parameters, a, settings));
            Console.ReadKey();

        }
    }
}

