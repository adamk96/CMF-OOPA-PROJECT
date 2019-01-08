using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HestonModel.Interfaces;

namespace HestonModel.InterfaceImplement
{
    public class FinalInterfaceFill : IOptionMarketData<IEuropeanOption>
    {
        double price;
        IEuropeanOption option;

        public FinalInterfaceFill(double price, double T, PayoffType p, double K)
        {
            this.price = price;
            double[] TT = { 0 };
            InterfaceFill fill = new InterfaceFill(T, 0, 0, 0, 0, 0, 0, 0, p, TT, K, 0);
            option = fill;
        }

        IEuropeanOption IOptionMarketData<IEuropeanOption>.Option => option;

        double IOptionMarketData<IEuropeanOption>.Price => price;
    }
}
