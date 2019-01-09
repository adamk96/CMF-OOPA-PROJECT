using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HestonCalibrationAndPricing
{
    public class CalibrationFailedException : Exception
    {
        public CalibrationFailedException()
        {
        }
        public CalibrationFailedException(string message)
            : base(message)
        {
        }
    }

    public struct MarketData
    {
        public double K;
        public double T;
        public double Price;
    }

    public enum CalibrationOutcome
    {
        NotStarted,
        FinishedOK,
        FailedMaxItReached,
        FailedOtherReason
    };

    public class Calibrator
    {
        private const double defaultAccuracy = 1/1000;
        private const int defaultMaxIts = 1000;
        private double accuracy;
        private int maxIts;

        private List<MarketData> marketList;
        private double r;
        private double S;

        private CalibrationOutcome outcome;

        private double[] calibratedParams;

        public Calibrator()
        {
            accuracy = defaultAccuracy;
            maxIts = defaultMaxIts;
            marketList = new List<MarketData>();
            r = 0;
            S = 0;
        }

        public Calibrator(double r, double S, int maxIts, double accuracy)
        {
            if (r <= 0 || S <= 0 || maxIts <= 0 ) 
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            this.accuracy = accuracy;
            this.maxIts = maxIts;
            marketList = new List<MarketData>();
            this.r = r;
            this.S = S;
        }

        public void SetGuessParameters(double kappaStar, double thetaStar, double sigma, double rho, double v)
        {
            if (sigma <= 0)
            {
                throw new System.ArgumentException("Sigma must be positive");
            }

            Options e = new Options(r, S, kappaStar, thetaStar, sigma, rho, v);
            calibratedParams = e.ParamsAsArray();
        }

        public void AddObservedOption(double K, double T, double Price)
        {
            if (K <= 0 || T <= 0 || Price <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            MarketData observedOption;
            observedOption.K = K;
            observedOption.T = T;
            observedOption.Price = Price;
            marketList.Add(observedOption);
        }

        public double CalculateMeanSquaredErrorBetweenModelAndMarket(Options options)
        {
            double mse = 0;
            foreach (MarketData data in marketList)
            {
                double T = data.T;
                double K = data.K;
                double price = options.EuropeanCallPrice(T, K);
                double diff = price - data.Price;
                mse += diff * diff;
            }
            return mse;
        }

        public void CalibrationObjectiveFunction(double[] paramsarray, ref double func, object obj)
        {
            Options european = new Options(r, S, paramsarray);
            func = CalculateMeanSquaredErrorBetweenModelAndMarket(european);
        }

        public void Calibrate()
        {
            outcome = CalibrationOutcome.NotStarted;

            double[] initialParams = new double[Options.numberParams];
            if (calibratedParams == null)
            {
                throw new System.Exception("Please add an initial guess for parameters");
            }
            calibratedParams.CopyTo(initialParams, 0);  
            double epsg = accuracy;
            double epsf = accuracy; 
            double epsx = accuracy;
            double diffstep = 1.0e-6;
            int maxits = maxIts; 
            double stpmax = 0.05;



            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;
            alglib.minlbfgscreatef(4, initialParams, diffstep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, maxits);
            alglib.minlbfgssetstpmax(state, stpmax);

            alglib.minlbfgsoptimize(state, CalibrationObjectiveFunction, null, null);
            double[] resultParams = new double[Options.numberParams];
            alglib.minlbfgsresults(state, out resultParams, out rep);

            System.Console.WriteLine("Termination type: {0}", rep.terminationtype);
            System.Console.WriteLine("Num iterations {0}", rep.iterationscount);
            System.Console.WriteLine("{0}", alglib.ap.format(resultParams, 5));

            if (rep.terminationtype == 1			
                || rep.terminationtype == 2			
                || rep.terminationtype == 4)
            {    	
                outcome = CalibrationOutcome.FinishedOK;
                calibratedParams = resultParams;
            }
            else if (rep.terminationtype == 5)
            {	
                outcome = CalibrationOutcome.FailedMaxItReached;
                calibratedParams = resultParams;

            }
            else
            {
                outcome = CalibrationOutcome.FailedOtherReason;
                throw new CalibrationFailedException("Heston model calibration failed badly.");
            }
        }

        public void GetCalibrationStatus(ref CalibrationOutcome calibOutcome, ref double pricingError)
        {
            calibOutcome = outcome;
            Options m = new Options(r, S, calibratedParams);
            pricingError = CalculateMeanSquaredErrorBetweenModelAndMarket(m);
        }

        public Options GetCalibratedModel()
        {
            Options m = new Options(r, S, calibratedParams);
            return m;
        }

    }
}