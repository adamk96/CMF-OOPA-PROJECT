using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompositeIntegrator
{
    // note prob no need for full seperate namespace, also could prob use alglib
    public class Integrator
    {
        private int newtonCotesOrder;
        const int maxOrder = 4;
        const int maxOrderLength = 5;

        private static double[,] weights = new double[maxOrder, maxOrderLength]
        {
            {0.5, 0.5, 0, 0, 0 },
            {1.0/6.0, 4.0/6.0, 1.0/6.0, 0, 0 },
            {1.0/8.0, 3.0/8.0, 3.0/8.0, 1.0/8.0, 0 },
            {7.0/90.0, 32.0/90.0, 12.0/90.0, 32.0/90.0, 7.0/90.0 }
        }; //reason this gets semicolon at end is that its all one line defining an array.

        private double[] quadraturePoints;
        private double[] quadraturePointsFVal;

        public Integrator()
        {
            newtonCotesOrder = 1;
            quadraturePoints = new double[newtonCotesOrder + 1];
            quadraturePointsFVal = new double[newtonCotesOrder + 1];
        }

        public Integrator(Integrator integrator)
        {
            newtonCotesOrder = integrator.newtonCotesOrder;
            quadraturePoints = new double[newtonCotesOrder + 1];
            quadraturePointsFVal = new double[newtonCotesOrder + 1];
        }

        public Integrator(int newtonCotesOrder)
        {
            if (newtonCotesOrder <= 0 || newtonCotesOrder > 4)
            {
                throw new System.ArgumentException("Newton Cotes order must be between 1 and 4");
            }
            this.newtonCotesOrder = newtonCotesOrder;
            quadraturePoints = new double[newtonCotesOrder + 1];
            quadraturePointsFVal = new double[newtonCotesOrder + 1];
        }

        // update to allow int entry
        private void UpdateQuadraturePointsAndFvals(double a, double h, int j, Func<int, double, double> f)
        {
            double delta = h / newtonCotesOrder;
            for (int i = 0; i <= newtonCotesOrder; i++)
            {
                quadraturePoints[i] = a + i * delta;
                quadraturePointsFVal[i] = f(j, quadraturePoints[i]);
            }
        }

        // update to allow int entry
        public double Integrate(Func<int, double, double> f, double a, double b, int j, int N)
        {
            //change to continue while integrand bigger than a certain value or something
            if (N <= 0)
            {
                throw new System.ArgumentException("Partition size must be positive");
            }
            if (a > b)
            {
                throw new System.ArgumentException("Starting point cannot be greater than end point");
            }
            double integral = 0;
            double h = (b - a) / N;
            for (int i = 0; i < N; i++)
            {
                UpdateQuadraturePointsAndFvals(a + i * h, h, j, f);
                double stepIncrement = 0.0;
                for (int jj = 0; jj < newtonCotesOrder; jj++)
                {
                    stepIncrement += weights[newtonCotesOrder - 1, jj] * quadraturePointsFVal[jj];
                }
                integral += stepIncrement * h;
            }
            return integral;
        }
    }
}
