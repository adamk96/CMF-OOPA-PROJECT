using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel;
using HestonCalibrationAndPricing;
using HestonModel.Interfaces;
using HestonModel.InterfaceImplement;

namespace MyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine(1.0 / 1000 <= 0);
            //Calibrator calibrator = new Calibrator(0.025, 100, 1000, 1.0 / 10000000000000);
            //calibrator.SetGuessParameters(1.5768, 0.0398, 0.5751, -0.5711, 0.0175);

            //calibrator.AddObservedOption(80, 1, 25.72);
            //calibrator.AddObservedOption(90, 1, 18.93);
            //calibrator.AddObservedOption(80, 2, 30.49);
            //calibrator.AddObservedOption(100, 2, 19.36);
            //calibrator.AddObservedOption(100, 1.5, 16.58);
            //calibrator.Calibrate(); 
            //double error = 0;
            //HestonCalibrationAndPricing.CalibrationOutcome outcome = HestonCalibrationAndPricing.CalibrationOutcome.NotStarted;
            //calibrator.GetCalibrationStatus(ref outcome, ref error);
            //Console.WriteLine("Calibration outcome: {0} and error: {1}", outcome, error);
            //Options e = calibrator.GetCalibratedModel();
            //Console.WriteLine();
            
            //Console.ReadKey();

            //double[] TT = { 0 };
            //InterfaceFill fill = new InterfaceFill(1, 2, 0.06, 0.4, 0.5, 0.04, 100000, 365, 0, TT, 100, 1 / 1000);
            //OtherInterfaceFill filler = new OtherInterfaceFill(0, 1.5768, 0.0398, 0.5751, -0.5711, 0.0175, 0, 0, 100, 0.025, 0, 0);
            //IHestonModelParameters calibPar;
            //calibPar = filler;
            //FinalInterfaceFill num1 = new FinalInterfaceFill(25.72, 1, 0, 80);
            //IOptionMarketData<IEuropeanOption> numm1 = num1;
            //FinalInterfaceFill num2 = new FinalInterfaceFill(18.93, 1, 0, 90);
            //IOptionMarketData<IEuropeanOption> numm2 = num2;
            //FinalInterfaceFill num3 = new FinalInterfaceFill(30.49, 2, 0, 80);
            //IOptionMarketData<IEuropeanOption> numm3 = num3;
            //FinalInterfaceFill num4 = new FinalInterfaceFill(19.36, 2, 0, 100);
            //IOptionMarketData<IEuropeanOption> numm4 = num4;
            //FinalInterfaceFill num5 = new FinalInterfaceFill(16.58, 1.5, 0, 100);
            //IOptionMarketData<IEuropeanOption> numm5 = num5;
            //IEnumerable<IOptionMarketData<IEuropeanOption>> enume = new IOptionMarketData<IEuropeanOption>[] { numm1 };
            //enume = enume.Concat(new IOptionMarketData<IEuropeanOption>[] { numm2 });
            //enume = enume.Concat(new IOptionMarketData<IEuropeanOption>[] { numm3 });
            //enume = enume.Concat(new IOptionMarketData<IEuropeanOption>[] { numm4 });
            //enume = enume.Concat(new IOptionMarketData<IEuropeanOption>[] { numm5 });
            //ICalibrationSettings set = fill;
            //IHestonCalibrationResult result = Heston.CalibrateHestonParameters(calibPar, enume, set);
            //Console.WriteLine(result.MinimizerStatus);
            //Console.WriteLine(result.PricingError);
            //Console.WriteLine(result.Parameters.VarianceParameters.Kappa);

            OptionsMC options = new OptionsMC(0.1, 100, 2, 0.06, 0.4, 0.5, 0.04, 100);
            Options options1 =  new Options(0.1, 100, 2, 0.06, 0.4, 0.5, 0.04);
            Console.WriteLine(options.EuropeanCallOptionPriceMCAnithetic(1, 365, 100000));
            Console.WriteLine(options.EuropeanCallOptionPriceMC(1, 365, 100000));
            Console.WriteLine(options.EuropeanCallOptionPriceMCAnitheticParallel(1, 365, 100000));
            Console.WriteLine(options1.EuropeanCallPrice(1, 100));

            Console.WriteLine(options.EuropeanPutOptionPriceMCAnitheticParallel(1, 365, 100000));
            Console.WriteLine(options1.EuropeanPutPrice(1, 100));

            double[] TT = { 0.5, 1 };
            Console.WriteLine(options.PriceAsianCallMC(TT, 1, 100000, 365));
            Console.WriteLine(options.PriceAsianCallMCParallel(TT, 1, 100000, 365));
            Console.ReadKey();
        }
    }
}
