using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FilterDesign {

    internal class Filter { 
        public List<DraggableMarkerPlot> zeros = new List<DraggableMarkerPlot>();
        public List<DraggableMarkerPlot> poles = new List<DraggableMarkerPlot>();
        public double gainFactor = 1;
        public List<Filter> allPassFilters = new List<Filter>();
        
        List<Complex> numerator;
        List<Complex> denominator;

        Complex[] inputDelay;
        Complex[] outputDelay;
        double calculateGainFactor(Complex[] zeros, Complex[] poles) {
            Complex numerator = Complex.One;
            Complex denominator = Complex.One;

            foreach (Complex zero in zeros) {
                numerator *= zero;
            }
            numerator *= Math.Pow(-1,zeros.Length);
            foreach (Complex pole in poles) {
                denominator *= pole;
            }

            denominator *= Math.Pow(-1, poles.Length);
            Complex gainFactor = numerator / denominator;

            return gainFactor.Magnitude;
        }
        public void initFilter() {
            List<Complex> zerosArr = new List<Complex>();
            List<Complex> polesArr = new List<Complex>();
            for (int i = 0; i < zeros.Count; i++) 
                zerosArr.Add(new Complex(zeros[i].X, zeros[i].Y));
            for (int i = 0; i < poles.Count; i++) 
                polesArr.Add(new Complex(poles[i].X, poles[i].Y));
            foreach (var f in allPassFilters) {
                for (int i = 0; i < f.zeros.Count; i++)
                    zerosArr.Add(new Complex(f.zeros[i].X, f.zeros[i].Y));
                for (int i = 0; i < f.poles.Count; i++)
                    polesArr.Add(new Complex(f.poles[i].X, f.poles[i].Y));
            }
            gainFactor = calculateGainFactor(zerosArr.ToArray(), polesArr.ToArray());
            if(numerator != null) numerator.Clear();
            if (denominator != null) denominator.Clear();
            numerator = calculatePolynoial(zerosArr.ToArray());
            denominator = calculatePolynoial(polesArr.ToArray());
            inputDelay = new Complex[numerator.Count];
            outputDelay = new Complex[denominator.Count];
            for (int i = 0; i < inputDelay.Length; i++) inputDelay[i] = 0;
            for (int i = 0; i < outputDelay.Length; i++) outputDelay[i] = 0;
        }



        public String filterEquation(int start = 0) {
            initFilter();
            String eqn = $"Gain Factor(K) = {gainFactor.ToString("0.#####")} {System.Environment.NewLine}y[n] = ";
            
            if (numerator.Count == 0 && denominator.Count == 0) return "y[n] = 0";
            
            for(int i = 0; i < numerator.Count; i++) {
                if (numerator[i].Real != 0) {
                    String sign = numerator[i].Real >= 0 ? " +" : " ";
                    String x = $"x[n{((i+start) == 0 ? "" : " - " + (i+start).ToString())}]";
                    eqn += $"{sign}{numerator[i].Real.ToString("0.###")} * {x}";
                }
            }

            for (int i = 0; i < denominator.Count; i++) {
                if (denominator[i].Real != 0) {
                    String sign = (-denominator[i].Real) >= 0 ? " + " : " ";
                    String x = $"y[n - {i + start + 1}]";
                    eqn += $"{sign}{(-denominator[i].Real).ToString("0.###")} * {x}";
                }
            }
            for (int i = 0; i < allPassFilters.Count; i++) {
                eqn += allPassFilters[i].filterEquation(i + 1);
            }
            return eqn;
        }
        public bool removeZero(DraggableMarkerPlot zero) {
            bool removedMainFilter = zeros.Remove(zero);
            if (!removedMainFilter) {
                foreach (Filter f in allPassFilters) {
                    if (f.zeros.Remove(zero)) {
                        removedMainFilter = true;
                        break;
                    }
                }
            }
            return removedMainFilter;
        }
        public bool removePole(DraggableMarkerPlot pole) {
            bool removedMainFilter = poles.Remove(pole);
            if (!removedMainFilter) {
                foreach (Filter f in allPassFilters) {
                    if (f.poles.Remove(pole)) {
                        removedMainFilter = true;
                        break;
                    }
                }
            }
            return removedMainFilter;
        }


        /*double xn1 = 0;
        double xn2 = 0;

        double yn1 = 0;
        double yn2 = 0;*/

        public double ApplyFilter(double input) {
            /*double output = input + 1.969 * xn1 + xn2 - 1.969 * yn1 - 0.9692 * yn2;
            xn2 = xn1;
            xn1 = input;

            yn2 = yn1;
            yn1 = output;
            return output;*/

            

            // Compute the filtered output
            double outputValue = 0.0;
            for (int j = 0; j < numerator.Count; j++) {
                outputValue += numerator[j].Real * inputDelay[j].Real;
            }
            for (int j = 1; j < denominator.Count; j++) {
                outputValue -= denominator[j].Real * outputDelay[j].Real;
            }

            // Update the delay lines
            for (int j = numerator.Count - 1; j > 0; j--) {
                inputDelay[j] = inputDelay[j - 1];
            }
            for (int j = denominator.Count - 1; j > 0; j--) {
                outputDelay[j] = outputDelay[j - 1];
            }
            if(inputDelay.Length > 0)
                inputDelay[0] = input;
            if(outputDelay.Length > 0)
                outputDelay[0] = outputValue;

            return outputValue;

            /*Complex output = 0.0;
            if(inputDelay.Length != 0) {
                for (int i = 0; i < inputDelay.Length - 1; i++)
                    inputDelay[i + 1] = inputDelay[i];

                inputDelay[0] = input;
            }

            for (int i=0;i < numerator.Count; i++) {
                output += numerator[i] * inputDelay[i];
            }
            
            output *= gainFactor;
            for (int i = 0; i < denominator.Count; i++) {
                output -= denominator[i] * outputDelay[i];
            }

            if(outputDelay.Length != 0) {
                for (int i = 0; i < outputDelay.Length - 1; i++)
                    outputDelay[i + 1] = outputDelay[i];
                outputDelay[0] = output;
            }
            return output.Real;*/

        }







        public void clearAll() {
            removeAllPoles();
            removeAllZeros();
        }
        public void removeAllZeros() {
            zeros.Clear();
            foreach (var f in allPassFilters) {
                f.removeAllZeros();
            }
        }
        public void removeAllPoles() {
            poles.Clear();
            foreach (var f in allPassFilters) {
                f.removeAllPoles();
            }
        }

        List<Complex> muliplyBrackets(List<Complex> b1, List<Complex> b2) {
            Complex[] res = new Complex[Math.Max(b1.Count, b2.Count) + 1];

            for (int i = 0; i < b1.Count; i++) {
                for (int j = 0; j < b2.Count; j++) {
                    res[i + j] += b1[i] * b2[j];
                }
            }

            return res.ToList();
        }

        List<Complex> calculatePolynoial(Complex[] roots) {
            if (roots.Length == 0) return roots.ToList();
            Queue<List<Complex>> brackets = new Queue<List<Complex>>();
            for (int i = 0; i < roots.Length; i++) {
                brackets.Enqueue(new List<Complex> { 1, -roots[i] });
            }
            List<Complex> res = brackets.Dequeue();
            while (brackets.Count > 0) {
                res = muliplyBrackets(res, brackets.Dequeue());
            }
            return res;
        }
    }
}
