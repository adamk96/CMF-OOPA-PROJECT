﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel.Interfaces;

namespace HestonModel.InterfaceImplement
{
    public class OtherInterfaceFill : IHestonModelParameters, ICalibrationResult
    {
        double S;
        double r;
        IVarianceProcessParameters paramss;
        CalibrationOutcome c;
        double error;


        //note opposite order to normal
        public OtherInterfaceFill(double T, double kappa, double theta, double sigma, double rho, double v, int numberTrials, int numberTimeSteps, double S, double r, CalibrationOutcome c, double error)
        {
            this.S = S; this.r = r;
            double[] TT = { 0 };
            InterfaceFill fill = new InterfaceFill(T, kappa, theta, sigma, rho, v, numberTrials, numberTimeSteps, 0, TT, 100, 0);
            paramss = fill;
            this.c = c;
        }

        double IHestonModelParameters.InitialStockPrice => S;

        double IHestonModelParameters.RiskFreeRate => r;

        IVarianceProcessParameters IHestonModelParameters.VarianceParameters => paramss;

        CalibrationOutcome ICalibrationResult.MinimizerStatus => c;

        double ICalibrationResult.PricingError => error;

    }
}
