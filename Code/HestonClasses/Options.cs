using System;
using MathNet.Numerics.Integration;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HestonCalibrationAndPricing
{
    /// <summary>
    /// This class prices options within the Heston model
    /// </summary>
    public class Options
    {
        public const int numberParams = 5;
        public const int kappaIndex = 0;
        public const int thetaIndex = 1;
        public const int sigmaIndex = 2;
        public const int rhoIndex = 3;
        public const int vIndex = 4;

        private double r;
        private double kappaStar;
        private double thetaStar;
        private double sigma;
        private double rho;
        private double v;
        private double S;
        

        public Options(double r, double S, double kappaStar, double thetaStar, double sigma, double rho, double v)
        {
            if (r <= 0 || S <= 0 || sigma <= 0 || v <= 0)                 //check what other params need be positive
            {
                throw new System.ArgumentException("r, S, sigma, v must be positive");
            }
            this.r = r; this.S = S; this.kappaStar = kappaStar; this.thetaStar = thetaStar;
            this.sigma = sigma; this.rho = rho; this.v = v;
        }

        public Options(double r, double S, double[] parameters)
        {
            this.r = r; this.S = S;
            kappaStar = parameters[kappaIndex];
            thetaStar = parameters[thetaIndex];
            sigma = parameters[sigmaIndex];
            rho = parameters[rhoIndex];
            v = parameters[vIndex];
            if (r <= 0 || S <= 0 || sigma <= 0 || v <= 0)                 //check what other params need be positive
            {
                throw new System.ArgumentException("r, S, sigma, v must be positive");
            }
            
        }

         /// <summary>
         /// Prices a European call option within the Heston model.
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "K">The options' strike price.</param>
         /// <returns>Option price.</returns>
        public double EuropeanCallPrice(double T, double K)
        {
            if (T < 0 || K < 0)
            {
                throw new System.ArgumentException("T, K must be non-negative");
            }

            double[] b = { kappaStar - rho * sigma, kappaStar };
            double[] u = { 0.5, -0.5 };

            double a = kappaStar * thetaStar;
            Complex i = new Complex(0, 1);

            Func<int, double, double> RealP = (j, phi) =>
            {
                // j must be 0 or 1.

                Complex temp1 = new Complex(-b[j], rho * sigma * phi);
                Complex tempp1 = new Complex(-phi * phi, 2 * u[j] * phi);
                Complex d = Complex.Pow(temp1 * temp1 - sigma * sigma * tempp1, 0.5);
                Complex g = (b[j] - rho * sigma * phi * i - d) * Complex.Pow(b[j] - rho * sigma * phi * i + d, -1);

                Complex c = r * phi * T * i + (a / (sigma * sigma)) * ((b[j] - rho * sigma * phi * i - d) * T - 2 * Complex.Log((1 - g * Complex.Exp(-T * d)) / (1 - g)));
                Complex bigD = ((b[j] - rho * sigma * phi * i - d) / (sigma * sigma)) * ((1 - Complex.Exp(-T * d)) / (1 - g * Complex.Exp(-T * d)));
                Complex littlePhi = Complex.Exp(c + bigD * v + phi * i * Math.Log(S));
                Complex value = Complex.Exp(-i * phi * Math.Log(K)) * littlePhi / (i * phi);
                return value.Real;
            };

            double[] P = new double[2];
            
            //decide on these
            P[0] = 0.5 + (1.0 / Math.PI) * SimpsonRule.IntegrateComposite(x => RealP(0, x), 0.000001, 50, 100);
            P[1] = 0.5 + (1.0 / Math.PI) * SimpsonRule.IntegrateComposite(x => RealP(1, x), 0.000001, 50, 100);
            return S * P[0] - K * Math.Exp(-r * T) * P[1];
        }

         /// <summary>
         /// Prices a European put option within the Heston model.
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "K">The options' strike price.</param>
         /// <returns>Option price.</returns>
        public double EuropeanPutPrice(double T, double K)
        {
            if (T < 0 || K < 0)
            {
                throw new System.ArgumentException("T, K must be non-negative");
            }
            return EuropeanCallPrice(T, K) - S + K * Math.Exp(-r * T);
        }

         /// <summary>
         /// Forms an array of the model parameters.
         /// </summary>
         /// <returns>Model parameters.</returns>
        public double[] ParamsAsArray()
        {
            double[] paramsArray = new double[Options.numberParams];
            paramsArray[kappaIndex] = kappaStar;
            paramsArray[thetaIndex] = thetaStar;
            paramsArray[sigmaIndex] = sigma;
            paramsArray[rhoIndex] = rho;
            paramsArray[vIndex] = v;
            return paramsArray;
        }

    }
}
