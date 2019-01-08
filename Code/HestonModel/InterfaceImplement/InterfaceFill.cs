using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel.Interfaces;

namespace HestonModel.InterfaceImplement
{
    public class InterfaceFill : IOption, IVarianceProcessParameters, IMonteCarloSettings, IAsianOption, IEuropeanOption, ICalibrationSettings
    {
        double T;
        double kappa;
        double theta;
        double sigma;
        double v;
        double rho;
        int numberTrials;
        int numberTimeSteps;
        PayoffType p;
        IEnumerable<double> timeList;
        double K;
        double accuracy;



        public InterfaceFill(double T, double kappa, double theta, double sigma, double rho, double v, int numberTrials, int numberTimeSteps, PayoffType p, double[] timeArray, double K, double accuracy)
        {
            this.T = T; this.kappa = kappa; this.theta = theta; this.sigma = sigma;
            this.v = v; this.rho = rho; this.numberTrials = numberTrials; this.numberTimeSteps = numberTimeSteps;
            this.p = p; timeList = timeArray.AsEnumerable(); this.K = K;
            this.accuracy = accuracy;
        }



        double IOption.Maturity => T;

        double IVarianceProcessParameters.Kappa => kappa;

        double IVarianceProcessParameters.Theta => theta;

        double IVarianceProcessParameters.Sigma => sigma;

        double IVarianceProcessParameters.V0 => v;

        double IVarianceProcessParameters.Rho => rho;

        int IMonteCarloSettings.NumberOfTrials => numberTrials;

        int IMonteCarloSettings.NumberOfTimeSteps => numberTimeSteps;

        IEnumerable<double> IAsianOption.MonitoringTimes => timeList;

        PayoffType IEuropeanOption.Type => p;

        double IEuropeanOption.StrikePrice => K;

        double ICalibrationSettings.Accuracy => accuracy;

        int ICalibrationSettings.MaximumNumberOfIterations => 1000;
    }
}
