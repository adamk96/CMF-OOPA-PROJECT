using System;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HestonClasses
{
    public class EuropeanOptionsMC
    {
        // again params class better
        //public const int numberParams = 5;
        private const int kappaIndex = 0;
        private const int thetaIndex = 1;
        private const int sigmaIndex = 2;
        private const int rhoIndex = 3;
        private const int vIndex = 4;

        private double r;
       // private double T;
        private double K;
        private double kappaStar;
        private double thetaStar;
        private double sigma;
        private double rho;
        private double v;
        private double S;

        public EuropeanOptionsMC(double r, double K, double kappaStar, double thetaStar, double sigma, double rho, double v, double S)
        {
            if (2 * kappaStar * thetaStar <= sigma * sigma) 
            {
                throw new System.ArgumentException("Feller condition violated.");
            }
            //throw usual exceptions, Feller condition
            this.r = r; 
            this.K = K; this.kappaStar = kappaStar; this.thetaStar = thetaStar;
            this.sigma = sigma; this.rho = rho; this.v = v; this.S = S;
        }

        public EuropeanOptionsMC(double r, double K, double[] paramss, double S)
        {
            this.r = r; 
            this.K = K; kappaStar = paramss[kappaIndex]; thetaStar = paramss[thetaIndex];
            sigma = paramss[sigmaIndex]; rho = paramss[rhoIndex]; v = paramss[vIndex]; this.S = S;
            if (2 * kappaStar * thetaStar <= sigma * sigma)
            {
                throw new System.ArgumentException("Feller condition violated.");
            }
        }

        public double EuropeanCallOptionPriceMC(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            for (int i = 0; i < numberPaths; i++)
            {                
                count += Math.Exp(-r * T) * Math.Max(path.PathGenerator(T, S, numberTimeStepsPerPath) - K, 0);
            }
            return count / numberPaths;
        }

        //maybe do seperate class for
        private void CheckAsianOptionInputs(double[] T, double exerciseT)
        {
            if (T.Length == 0)
                throw new System.ArgumentException("Need at least one monitoring date for Asian option.");

            if (T[0] <= 0)
                throw new System.ArgumentException("The first monitoring date must be positive.");

            for (int i = 1; i < T.Length; ++i)
            {
                if (T[i - 1] >= T[i])
                    throw new System.ArgumentException("Monitoring dates must be increasing");
            }

            if (T[T.Length - 1] > exerciseT)
                throw new System.ArgumentException("Last monitoring time must not be greater than the exercise time");
        }

        public double PriceAsianCallMC(double[] T, double exerciseT, int numberPaths, int numberTimeStepsPerPath) 
        {
            CheckAsianOptionInputs(T, exerciseT);
            int M = T.Length;
            
            double pathCounter = 0;
            for (int i = 0; i < numberPaths; i++)
            {
                double priceCount = 0;
                double holder = S;
                double deltaT = T[0]; 
                MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v); //may need variable N
                for (int j = 0; j < M; j++)
                {
                    if (j > 0)
                        deltaT = T[j] - T[j - 1];
                    int stepNumber = (int)Math.Ceiling(deltaT * numberTimeStepsPerPath / exerciseT);
                    holder = path.PathGenerator(deltaT, holder, stepNumber);
                    priceCount += holder;
                }
                double pathPayoff = Math.Max(priceCount / M - K, 0);
                pathCounter += pathPayoff;
            }
            return Math.Exp(-r * exerciseT) * (pathCounter / numberPaths);
        }

        public double PriceLookbackCallMC(double exerciseT, int numberPaths, int numberTimeStepsPerPath)
        {
            double pathCounter = 0;


            double deltaT = exerciseT / numberTimeStepsPerPath;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v); // think about N=1 here
            for (double i = 0; i < numberPaths; i++)
            {
                double min = S;
                double holder = S;
                for (double j = 0; j <= exerciseT; j += deltaT)
                {
                    holder = path.PathGenerator(deltaT, holder, 1);
                    if (holder < min)
                        min = holder;
                }

                pathCounter += holder - min;
            }
            return Math.Exp(-r * exerciseT) * (pathCounter / numberPaths);
        }
    }

}

