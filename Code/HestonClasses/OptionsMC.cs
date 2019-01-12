using System;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HestonCalibrationAndPricing
{
    /// <summary>
    /// This class prices options within the Heston model using Monte Carlo methods.
    /// </summary>
    public class OptionsMC
    {
        public const int kappaIndex = 0;
        public const int thetaIndex = 1;
        public const int sigmaIndex = 2;
        public const int rhoIndex = 3;
        public const int vIndex = 4;

        private double r;
        private double K;
        private double kappaStar;
        private double thetaStar;
        private double sigma;
        private double rho;
        private double v;
        private double S;

        public OptionsMC(double r, double K, double kappaStar, double thetaStar, double sigma, double rho, double v, double S)
        {
            if (2 * kappaStar * thetaStar <= sigma * sigma) 
            {
                throw new System.ArgumentException("Feller condition violated.");
            }

            if (r <= 0 || K <= 0 || sigma <= 0 || S <= 0 || v <= 0)
            {
                throw new System.ArgumentException("r, K, sigma, S must be positive");
            }
                    
            this.r = r; 
            this.K = K; this.kappaStar = kappaStar; this.thetaStar = thetaStar;
            this.sigma = sigma; this.rho = rho; this.v = v; this.S = S;
        }

        public OptionsMC(double r, double kappaStar, double thetaStar, double sigma, double rho, double v, double S)
        {
            if (2 * kappaStar * thetaStar <= sigma * sigma)
            {
                throw new System.ArgumentException("Feller condition violated.");
            }
            if (r <= 0 || K <= 0 || sigma <= 0 || S <= 0 || v <= 0)
            {
                throw new System.ArgumentException("r, K, sigma, S must be positive");
            }
            this.r = r;
            this.kappaStar = kappaStar; this.thetaStar = thetaStar;
            this.sigma = sigma; this.rho = rho; this.v = v; this.S = S;
        }

        public OptionsMC(double r, double K, double[] paramss, double S)
        {
            this.r = r; 
            this.K = K; kappaStar = paramss[kappaIndex]; thetaStar = paramss[thetaIndex];
            sigma = paramss[sigmaIndex]; rho = paramss[rhoIndex]; v = paramss[vIndex]; this.S = S;
            if (2 * kappaStar * thetaStar <= sigma * sigma)
            {
                throw new System.ArgumentException("Feller condition violated.");
            }
            if (r <= 0 || K <= 0 || sigma <= 0 || S <= 0 || v <= 0)
            {
                throw new System.ArgumentException("r, K, sigma, S must be positive");
            }
        }

         /// <summary>
         /// Prices a European call option within the Heston model using Monte Carlo methods.
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <returns>Option price.</returns>
        public double EuropeanCallOptionPriceMC(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            for (int i = 0; i < numberPaths; i++)
            {                
                count += Math.Max(path.PathGenerator(T, S, numberTimeStepsPerPath) - K, 0);
            }
            return Math.Exp(-r * T) * count / numberPaths;
        }

         /// <summary>
         /// Prices a European put option within the Heston model using Monte Carlo methods.
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <returns>Option price.</returns>
        public double EuropeanPutOptionPriceMC(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            for (int i = 0; i < numberPaths; i++)
            {                
                count += Math.Max(K - path.PathGenerator(T, S, numberTimeStepsPerPath), 0);
            }
            return Math.Exp(-r * T) * count / numberPaths;
        }

         /// <summary>
         /// Prices a European call option within the Heston model using Monte Carlo methods with parallelisation.
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <returns>Option price.</returns>
        public double EuropeanCallOptionPriceMCParallel(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);

            Parallel.For(0, numberPaths, (i) =>
            {
                double pathAdd = Math.Max(path.PathGenerator(T, S, numberTimeStepsPerPath) - K, 0);
                Interlocked.Exchange(ref count, count + pathAdd);
            });

            return Math.Exp(-r * T) * count / numberPaths;
        }

         /// <summary>
         /// Prices a European call option within the Heston model using Monte Carlo methods using anithetic sampling .
         /// </summary>
         /// <param name = "T">The maturity date of the option in years.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <returns>Option price.</returns>
        public double EuropeanCallOptionPriceMCAnithetic(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            for (int i = 0; i < numberPaths; i++)
            {
                count += Math.Max(path.PathGeneratorAnithetic(T, S, numberTimeStepsPerPath)[0] - K, 0) + Math.Max(path.PathGeneratorAnithetic(T, S, numberTimeStepsPerPath)[1] - K, 0);
            }
            return Math.Exp(-r * T) * count / (2 * numberPaths);
        }

        /// <summary>
        /// Prices a European call option within the Heston model using Monte Carlo methods using both parallelisation and anithetic sampling .
        /// </summary>
        /// <param name = "T">The maturity date of the option in years.</param>
        /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
        /// <param name = "numberPaths">The number of simulations we wish to run.</param>
        /// <returns>Option price.</returns>
        public double EuropeanCallOptionPriceMCAnitheticParallel(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            int halfNumPaths = (int)Math.Ceiling(numberPaths / 2.0);
            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);

            Parallel.For(0, halfNumPaths, (i) =>
            {
                double[] paths = path.PathGeneratorAnithetic(T, S, numberTimeStepsPerPath);
                double pathAdd = Math.Max(paths[0] - K, 0) + Math.Max(paths[1] - K, 0);
                Interlocked.Exchange(ref count, count + pathAdd);
            });

            return Math.Exp(-r * T) * count / (2.0 * halfNumPaths);
        }

        /// <summary>
        /// Prices a European put option within the Heston model using Monte Carlo methods using both parallelisation and anithetic sampling .
        /// </summary>
        /// <param name = "T">The maturity date of the option in years.</param>
        /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time T.</param>
        /// <param name = "numberPaths">The number of simulations we wish to run.</param>
        /// <returns>Option price.</returns>
        public double EuropeanPutOptionPriceMCAnitheticParallel(double T, int numberTimeStepsPerPath, int numberPaths)
        {
            if (T <= 0 || numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            int halfNumPaths = (int)Math.Ceiling(numberPaths / 2.0);
            double count = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);

            Parallel.For(0, halfNumPaths, (i) =>
            {
                double[] paths = path.PathGeneratorAnithetic(T, S, numberTimeStepsPerPath);
                double pathAdd = Math.Max(K - paths[0], 0) + Math.Max(K - paths[1], 0);
                Interlocked.Exchange(ref count, count + pathAdd);
            });

            return Math.Exp(-r * T) * count / (2.0 * halfNumPaths);
        }


        /// <summary>
        /// Checks that the times used for pricing Asian options make sense.
        /// </summary>
        /// <param name = "T">An array containing the onservation times of the Asian option.</param>
        /// <param name = "exerciseT">The Asian option's exercise time.</param>
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

         /// <summary>
         /// Prices an Asian call option within the Heston model using Monte Carlo methods.
         /// </summary>
         /// <param name = "T">An array containing the onservation times of the Asian option.</param>
         /// <param name = "exerciseT">The Asian option's exercise time.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time exerciseT.</param>
         /// <returns>Option price.</returns>
        public double PriceAsianCallMC(double[] T, double exerciseT, int numberPaths, int numberTimeStepsPerPath) 
        {
            if (numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Monte Carlo settings must be positive");
            }
            CheckAsianOptionInputs(T, exerciseT);
            int M = T.Length;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            double pathCounter = 0;
            for (int i = 0; i < numberPaths; i++)
            {
                double priceCount = 0;
                double holder = S;
                double deltaT = T[0]; 
                
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

        /// <summary>
         /// Prices an Asian put option within the Heston model using Monte Carlo methods.
         /// </summary>
         /// <param name = "T">An array containing the onservation times of the Asian option.</param>
         /// <param name = "exerciseT">The Asian option's exercise time.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time exerciseT.</param>
         /// <returns>Option price.</returns>
        public double PriceAsianPutMC(double[] T, double exerciseT, int numberPaths, int numberTimeStepsPerPath) 
        {
            if (numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Monte Carlo settings must be positive");
            }
            CheckAsianOptionInputs(T, exerciseT);
            int M = T.Length;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);
            double pathCounter = 0;
            for (int i = 0; i < numberPaths; i++)
            {
                double priceCount = 0;
                double holder = S;
                double deltaT = T[0]; 
                
                for (int j = 0; j < M; j++)
                {
                    if (j > 0)
                        deltaT = T[j] - T[j - 1];
                    int stepNumber = (int)Math.Ceiling(deltaT * numberTimeStepsPerPath / exerciseT); 
                    holder = path.PathGenerator(deltaT, holder, stepNumber);
                    priceCount += holder;
                }
                double pathPayoff = Math.Max(K - priceCount / M, 0);
                pathCounter += pathPayoff;
            }
            return Math.Exp(-r * exerciseT) * (pathCounter / numberPaths);
        }

         /// <summary>
         /// Prices an Asian call option within the Heston model using Monte Carlo methods with parallelisation.
         /// </summary>
         /// <param name = "T">An array containing the onservation times of the Asian option.</param>
         /// <param name = "exerciseT">The Asian option's exercise time.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time exerciseT.</param>
         /// <returns>Option price.</returns>
        public double PriceAsianCallMCParallel(double[] T, double exerciseT, int numberPaths, int numberTimeStepsPerPath) 
        {
            if (numberTimeStepsPerPath <= 0 || numberPaths <= 0)
            {
                throw new System.ArgumentException("Monte Carlo settings must be positive");
            }

            CheckAsianOptionInputs(T, exerciseT);
            int M = T.Length;
            double pathCounter = 0;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v);  

            Parallel.For(0, numberPaths, (i) =>
            {
                double priceCount = 0;
                double holder = S;
                double deltaT = T[0];
                Parallel.For(0, M, (j) =>
                {
                    if (j > 0)
                        deltaT = T[j] - T[j - 1];
                    int stepNumber = (int)Math.Ceiling(deltaT * numberTimeStepsPerPath / exerciseT);
                    holder = path.PathGenerator(deltaT, holder, stepNumber);
                    Interlocked.Exchange(ref priceCount, priceCount + holder);
                });
                double pathPayoff = Math.Max((priceCount / M) - K, 0);
                Interlocked.Exchange(ref pathCounter, pathCounter + pathPayoff);
            });
            return Math.Exp(-r * exerciseT) * (pathCounter / numberPaths);
        }
        
         /// <summary>
         /// Prices a Lookback option within the Heston model using Monte Carlo methods.
         /// </summary>
         /// <param name = "exerciseT">The Lookback option's exercise time.</param>
         /// <param name = "numberPaths">The number of simulations we wish to run.</param>
         /// <param name = "numberTimeStepsPerPath">The number of steps we wish our path generator to take to reach time exerciseT.</param>
         /// <returns>Option price.</returns>
        public double PriceLookbackCallMC(double exerciseT, int numberPaths, int numberTimeStepsPerPath)
        {
            if(exerciseT <= 0 || numberPaths <=0 || numberTimeStepsPerPath <= 0)
            {
                throw new System.ArgumentException("Parameters must be positive");
            }

            double pathCounter = 0;
            double deltaT = exerciseT / numberTimeStepsPerPath;
            MCPaths path = new MCPaths(r, kappaStar, thetaStar, sigma, rho, v); 

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

