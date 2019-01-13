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

    /// <summary>
    /// This class is used to calibrate the parameters of the Heston Model to real world data
    /// </summary>
    public class Calibrator
    {
        private const double defaultAccuracy = 1.0e-15;
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

        /// <summary>
        /// Sets the parameters which will be used as a starting point by the calibrator
        /// </summary>
        public void SetGuessParameters(double kappaStar, double thetaStar, double sigma, double rho, double v)
        {
            if (sigma <= 0 || v <= 0)
            {
                throw new System.ArgumentException("Sigma, v must be positive");
            }

            Options e = new Options(r, S, kappaStar, thetaStar, sigma, rho, v);
            calibratedParams = e.ParamsAsArray();
        }

        /// <summary>
        /// Adds the details of a real world option to the list marketList of data which will be used for calibration.
        /// </summary>
        /// <param name="K">Observed option's strike price.</param>
        /// <param name="T">Observed option's maturity time.</param>
        /// <param name="Price">Observed options price.</param>
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

        /// <summary>
        /// Calculates the mean squared error between the European call prices of an instance, options,
        /// of the class Options and the market prices found in marketList
        /// </summary>
        /// <param name="options">An instance of the class Options.</param>
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

         /// <summary>
         /// This is the function which will be used by the calibrator
         /// </summary>
        public void CalibrationObjectiveFunction(double[] paramsarray, ref double func, object obj)
        {
            Options european = new Options(r, S, paramsarray);
            func = CalculateMeanSquaredErrorBetweenModelAndMarket(european);
        }

         /// <summary>
         /// Calibrates the model parameters to fit the market data as closely as possible
         /// </summary>
        public void Calibrate()
        {

            // set up for calibration
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
            alglib.minlbfgscreatef(5, initialParams, diffstep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, maxits);
            alglib.minlbfgssetstpmax(state, stpmax);

            // calibrate and return outcome, error
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

         /// <summary>
         /// Obtains the calibration status of the model, as well as the models pricing error.
         /// </summary>
        public void GetCalibrationStatus(ref CalibrationOutcome calibOutcome, ref double pricingError)
        {
            calibOutcome = outcome;
            Options m = new Options(r, S, calibratedParams);
            pricingError = CalculateMeanSquaredErrorBetweenModelAndMarket(m);
        }

         /// <summary>
         /// Creates an instance of the class Options with the calibrated parameters.
         /// </summary>
         /// <returns>Calibrated model.</returns>
        public Options GetCalibratedModel()
        {
            Options m = new Options(r, S, calibratedParams);
            return m;
        }

    }
}