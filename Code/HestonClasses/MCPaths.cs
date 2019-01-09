using System;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HestonCalibrationAndPricing
{
    public class MCPaths
    {
        private double r;
        private double rho;
        private double kappaStar;
        private double thetaStar;
        private double sigma;
        private double v;

        public MCPaths(double r, double kappaStar, double thetaStar, double sigma, double rho, double v)
        {
            if (r <= 0 || sigma <= 0)
            {
                throw new System.ArgumentException("r, sigma must be positive");
            }

            this.r = r; this.rho = rho; this.kappaStar = kappaStar;
            this.thetaStar = thetaStar; this.sigma = sigma; this.v = v;
        }



        public double PathGenerator(double T, double S, int numberTimeStepsPerPath)
        {
            if (T <= 0 || S <= 0 || numberTimeStepsPerPath <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double tau = T / numberTimeStepsPerPath;
            double sqrtTau = Math.Sqrt(tau);
            double sqrtOneMinusRhoSquared = Math.Sqrt(1 - rho * rho);
            double[] x1 = new double[numberTimeStepsPerPath];
            Normal.Samples(x1, 0, 1);
            double[] x2 = new double[numberTimeStepsPerPath];
            Normal.Samples(x2, 0, 1);

            double alpha = (4 * kappaStar * thetaStar - sigma * sigma) / 8.0;
            double beta = -kappaStar / 2.0;
            double gamma = sigma / 2.0;
            double y = Math.Sqrt(v);
            double s = S;

            for (int i = 0; i < numberTimeStepsPerPath; i++)
            {
                double deltaZ1 = sqrtTau * x1[i];
                double deltaZ2 = sqrtTau * (rho * x1[i] + sqrtOneMinusRhoSquared * x2[i]);
                s = s + r * s * tau + y * s * deltaZ1;
                double a = (y + gamma * deltaZ2) / (2 * (1 - beta * tau));
                y = a + Math.Sqrt(a * a + alpha * tau / (1 - beta * tau));
            }

            return s;
        }

        public double[] PathGeneratorAnithetic(double T, double S, int numberTimeStepsPerPath)
        {
            if (T <= 0 || S <= 0 || numberTimeStepsPerPath <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double tau = T / numberTimeStepsPerPath;
            double sqrtTau = Math.Sqrt(tau);
            double sqrtOneMinusRhoSquared = Math.Sqrt(1 - rho * rho);
            double[] x1 = new double[numberTimeStepsPerPath];
            double[] x2 = new double[numberTimeStepsPerPath];
            Normal.Samples(x1, 0, 1);
            Normal.Samples(x2, 0, 1);

            double alpha = (4 * kappaStar * thetaStar - sigma * sigma) / 8.0;
            double beta = -kappaStar / 2.0;
            double gamma = sigma / 2.0;
            double y = Math.Sqrt(v);
            double y1 = Math.Sqrt(v);
            double s = S;
            double s1 = S;

            for (int i = 0; i < numberTimeStepsPerPath; i++)
            {
                double deltaZ1 = sqrtTau * x1[i];
                double deltaZ2 = sqrtTau * (rho * x1[i] + sqrtOneMinusRhoSquared * x2[i]);
                //double deltaZ2min = sqrtTau * (-rho * x1[i] + sqrtOneMinusRhoSquared * minusx2[i]);
                s = s + r * s * tau + y * s * deltaZ1;
                s1 = s1 + r * s1 * tau - y * s1 * deltaZ1;
                double a = (y + gamma * deltaZ2) / (2 * (1 - beta * tau));
                double aa = -(y1 + gamma * deltaZ2) / (2 * (1 - beta * tau));
                y = a + Math.Sqrt(a * a + alpha * tau / (1 - beta * tau));
                y1 = aa + Math.Sqrt(aa * aa + alpha * tau / (1 - beta * tau));
            }

            double[] sArray = { s, s1 };
            return sArray;
        }

    }
}
