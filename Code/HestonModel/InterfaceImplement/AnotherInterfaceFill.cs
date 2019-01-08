using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel.Interfaces;

namespace HestonModel.InterfaceImplement
{
    public class AnotherInterfaceFill : IHestonCalibrationResult
    {
        IHestonModelParameters paramms;
        double error;
        CalibrationOutcome c;

        public AnotherInterfaceFill(double T, double kappa, double theta, double sigma, double rho, double v, int numberTrials, int numberTimeSteps, double S, double r, CalibrationOutcome c, double error)
        {
            this.c = c; this.error = error;
            OtherInterfaceFill fill = new OtherInterfaceFill(T, kappa, theta, sigma, rho, v, numberTrials, numberTimeSteps, S, r, c, error);
            paramms = fill;
        }

        IHestonModelParameters IHestonCalibrationResult.Parameters => paramms;

        CalibrationOutcome ICalibrationResult.MinimizerStatus => c;

        double ICalibrationResult.PricingError => error;
    }
}
