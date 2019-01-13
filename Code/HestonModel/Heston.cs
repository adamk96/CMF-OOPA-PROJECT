using System;
using System.Collections.Generic;
using HestonModel.Interfaces;
using HestonCalibrationAndPricing;
using System.Linq;
using HestonModel.InterfaceImplement;

namespace HestonModel
{

    /// <summary> 
    /// This class will be used for grading. 
    /// Don't remove any of the methods and don't modify their signatures. Don't change the namespace. 
    /// Your code should be implemented in other classes (or even projects if you wish), and the relevant functionality should only be called here and outputs returned.
    /// You don't need to implement the interfaces that have been provided if you don't want to.
    /// </summary>
    public static class Heston
    {
        /// <summary>
        /// Method for calibrating the heston model.
        /// </summary>
        /// <param name="guessModelParameters">Object implementing IHestonModelParameters interface containing the risk-free rate, initial stock price
        /// and initial guess parameters to be used in the calibration.</param>
        /// <param name="referenceData">A collection of objects implementing IOptionMarketData<IEuropeanOption> interface. These should contain the reference data used for calibration.</param>
        /// <param name="calibrationSettings">An object implementing ICalibrationSettings interface.</param>
        /// <returns>Object implementing IHestonCalibrationResult interface which contains calibrated model parameters and additional diagnostic information</returns>
        public static IHestonCalibrationResult CalibrateHestonParameters(IHestonModelParameters guessModelParameters, IEnumerable<IOptionMarketData<IEuropeanOption>> referenceData, ICalibrationSettings calibrationSettings)
        {
            // set up calibrator
            Calibrator cal = new Calibrator(guessModelParameters.RiskFreeRate, guessModelParameters.InitialStockPrice, calibrationSettings.MaximumNumberOfIterations, calibrationSettings.Accuracy);
            cal.SetGuessParameters(guessModelParameters.VarianceParameters.Kappa, guessModelParameters.VarianceParameters.Theta,
                guessModelParameters.VarianceParameters.Sigma, guessModelParameters.VarianceParameters.Rho, guessModelParameters.VarianceParameters.V0);

            // add market data
            foreach (IOptionMarketData<IEuropeanOption> data in referenceData)
            {
                cal.AddObservedOption(data.Option.StrikePrice, data.Option.Maturity, data.Price);
            }

            // calibrate, get error, outcome, parameters
            cal.Calibrate();
            double error = 0;
            HestonCalibrationAndPricing.CalibrationOutcome outcome = HestonCalibrationAndPricing.CalibrationOutcome.NotStarted;
            cal.GetCalibrationStatus(ref outcome, ref error);
           
            Options e = cal.GetCalibratedModel();
            double[] paramArray = e.ParamsAsArray();
            CalibrationOutcome outcome1 = (CalibrationOutcome)outcome;
            
            // implement and return IHestonCalibrationResult
            AnotherInterfaceFill fill = new AnotherInterfaceFill(0, paramArray[Options.kappaIndex], paramArray[Options.thetaIndex], paramArray[Options.sigmaIndex], paramArray[Options.rhoIndex], paramArray[Options.vIndex], 0, 0, 0, 0, outcome1, error);
            return fill;
        }

        /// <summary>
        /// Price a European option in the Heston model using the Heston formula. This should be accurate to 5 decimal places
        /// </summary>
        /// <param name="parameters">Object implementing IHestonModelParameters interface, containing model parameters.</param>
        /// <param name="europeanOption">Object implementing IEuropeanOption interface, containing the option parameters.</param>
        /// <returns>Option price</returns>
        public static double HestonEuropeanOptionPrice(IHestonModelParameters parameters, IEuropeanOption europeanOption)
        {
            Options eur = new Options(parameters.RiskFreeRate, parameters.InitialStockPrice,
               parameters.VarianceParameters.Kappa, parameters.VarianceParameters.Theta, parameters.VarianceParameters.Sigma,
               parameters.VarianceParameters.Rho, parameters.VarianceParameters.V0);

            if (europeanOption.Type == 0)
            {
                return eur.EuropeanCallPrice(europeanOption.Maturity, europeanOption.StrikePrice);
            }
            else
                return eur.EuropeanPutPrice(europeanOption.Maturity, europeanOption.StrikePrice);
        }

        /// <summary>
        /// Price a European option in the Heston model using the Monte-Carlo method. Accuracy will depend on number of time steps and samples
        /// </summary>
        /// <param name="parameters">Object implementing IHestonModelParameters interface, containing model parameters.</param>
        /// <param name="europeanOption">Object implementing IEuropeanOption interface, containing the option parameters.</param>
        /// <param name="monteCarloSimulationSettings">An object implementing IMonteCarloSettings object and containing simulation settings.</param>
        /// <returns>Option price</returns>
        public static double HestonEuropeanOptionPriceMC(IHestonModelParameters parameters, IEuropeanOption europeanOption, IMonteCarloSettings monteCarloSimulationSettings)
        {
            OptionsMC option = new OptionsMC(parameters.RiskFreeRate, europeanOption.StrikePrice, parameters.VarianceParameters.Kappa,
                parameters.VarianceParameters.Theta, parameters.VarianceParameters.Sigma, parameters.VarianceParameters.Rho,
                parameters.VarianceParameters.V0, parameters.InitialStockPrice);

            if(europeanOption.Type == 0)
            {
                return option.EuropeanCallOptionPriceMCAnitheticParallel(europeanOption.Maturity, monteCarloSimulationSettings.NumberOfTimeSteps, monteCarloSimulationSettings.NumberOfTrials);
            }
            else
                return option.EuropeanPutOptionPriceMCAnitheticParallel(europeanOption.Maturity, monteCarloSimulationSettings.NumberOfTimeSteps, monteCarloSimulationSettings.NumberOfTrials);
        }

        /// <summary>
        /// Price a Asian option in the Heston model using the 
        /// Monte-Carlo method. Accuracy will depend on number of time steps and samples</summary>
        /// <param name="parameters">Object implementing IHestonModelParameters interface, containing model parameters.</param>
        /// <param name="asianOption">Object implementing IAsian interface, containing the option parameters.</param>
        /// <param name="monteCarloSimulationSettings">An object implementing IMonteCarloSettings object and containing simulation settings.</param>
        /// <returns>Option price</returns>
        public static double HestonAsianOptionPriceMC(IHestonModelParameters parameters, IAsianOption asianOption, IMonteCarloSettings monteCarloSimulationSettings)
        {
            OptionsMC asian = new OptionsMC(parameters.RiskFreeRate, asianOption.StrikePrice, parameters.VarianceParameters.Kappa,
               parameters.VarianceParameters.Theta, parameters.VarianceParameters.Sigma, parameters.VarianceParameters.Rho,
               parameters.VarianceParameters.V0, parameters.InitialStockPrice);

            if (asianOption.Type == 0)
            {
                return asian.PriceAsianCallMC(asianOption.MonitoringTimes.ToArray(), asianOption.Maturity,
                monteCarloSimulationSettings.NumberOfTrials, monteCarloSimulationSettings.NumberOfTimeSteps);
            }
            else
                return asian.PriceAsianPutMC(asianOption.MonitoringTimes.ToArray(), asianOption.Maturity,
                monteCarloSimulationSettings.NumberOfTrials, monteCarloSimulationSettings.NumberOfTimeSteps);
        }

        /// <summary>
        /// Price a lookback option in the Heston model using the  
        /// a Monte-Carlo method. Accuracy will depend on number of time steps and samples </summary>
        /// <param name="parameters">Object implementing IHestonModelParameters interface, containing model parameters.</param>
        /// <param name="maturity">An object implementing IOption interface and containing option's maturity</param>
        /// <param name="monteCarloSimulationSettings">An object implementing IMonteCarloSettings object and containing simulation settings.</param>
        /// <returns>Option price</returns>
        public static double HestonLookbackOptionPriceMC(IHestonModelParameters parameters, IOption maturity, IMonteCarloSettings monteCarloSimulationSettings)
        {
            OptionsMC lookback = new OptionsMC(parameters.RiskFreeRate, parameters.VarianceParameters.Kappa,
                            parameters.VarianceParameters.Theta, parameters.VarianceParameters.Sigma, parameters.VarianceParameters.Rho,
                            parameters.VarianceParameters.V0, parameters.InitialStockPrice);

            return lookback.PriceLookbackCallMC(maturity.Maturity, monteCarloSimulationSettings.NumberOfTrials, monteCarloSimulationSettings.NumberOfTimeSteps);
        }       
    }
}
