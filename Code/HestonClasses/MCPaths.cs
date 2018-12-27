using System;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HestonClasses
{
    public class MCPaths
    {
        private double r;
        // private double T; Gonna move in cos think may need for asian
        private int N; // prob change to number time steps
        private double rho;
        private double kappaStar;
        private double thetaStar;
        private double sigma;
        // private double S; Gonna move in cos think may need for asian
        private double v;


        // Check feller condition, dont think sigma should have star in notes
        //  better to do param class
        public MCPaths(double r, int N, double kappaStar, double thetaStar, double sigma, double rho, double v)
        {
            this.r = r; this.N = N; this.rho = rho; this.kappaStar = kappaStar;
            this.thetaStar = thetaStar; this.sigma = sigma; this.v = v;
        }



        public double PathGenerator(double T, double S)
        {   // N is (int) rounded upT*365
            //maybe do as many paths as need at once
            double tau = T / N;
            double sqrtTau = Math.Sqrt(tau);
            double sqrtOneMinusRhoSquared = Math.Sqrt(1 - rho * rho);
            //could prob do these in 1 go 
            double[] x1 = new double[N];
            Normal.Samples(x1, 0, 1);
            double[] x2 = new double[N];
            Normal.Samples(x2, 0, 1);

            double alpha = (4 * kappaStar * thetaStar - sigma * sigma) / 8.0;
            double beta = -kappaStar / 2.0;
            double gamma = sigma / 2.0;
            double y = Math.Sqrt(v);
            double s = S;

            for (int i = 0; i < N; i++)
            {
                double deltaZ1 = sqrtTau * x1[i];
                double deltaZ2 = sqrtTau * (rho * x1[i] + sqrtOneMinusRhoSquared * x2[i]);
                s = s + r * s * tau + y * s * deltaZ1;
                double a = (y + gamma * deltaZ2) / (2 * (1 - beta * tau));
                y = a + Math.Sqrt(a * a + alpha * tau / (1 - beta * tau));
            }

            return s;
        }

       
    }
}
