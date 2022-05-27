using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Jace;
using Jace.Execution;
using Jace.Util;



namespace SolutionStepParser
{
    class Solution
    {
        public Queue<ISolutionStep> Steps;
        public List<ISolutionStep> UnsolvedSteps;
        public List<ISolutionStep> UnorderedSteps;
        public Dictionary<string, double> Parameters;
        public Dictionary<string, double> SolvedVariables;

        public CalculationEngine engine;

        //TODO: create AddSteps() and AddParameters() methods.
        public Solution()
        {
            this.Steps = new Queue<ISolutionStep>();
            this.UnsolvedSteps = new List<ISolutionStep>();
            this.UnorderedSteps = new List<ISolutionStep>();
            this.Parameters = new Dictionary<string, double>();
            this.SolvedVariables = new Dictionary<string, double>();
            this.engine = new CalculationEngine();
            AddTechWareFunctions(engine);
            AddIAPWSFunctions(engine);
        }

        public Solution(List<ISolutionStep> InitialSteps, Dictionary<string,double> InitialParameters)
        {
            this.Steps = new Queue<ISolutionStep>();
            this.UnsolvedSteps = new List<ISolutionStep>(InitialSteps);
            this.UnorderedSteps = new List<ISolutionStep>(InitialSteps);
            this.Parameters = new Dictionary<string,double>(InitialParameters);
            this.SolvedVariables = new Dictionary<string, double>();
            this.engine = new CalculationEngine();
            AddTechWareFunctions(engine);
            AddIAPWSFunctions(engine);
        }

        public void OrderSolutionSteps()
         {
            //Reset our Solution as unsolved and unordered
            //NotModifedSinceSorting = false;
            this.Steps = new Queue<ISolutionStep>();
            this.SolvedVariables = new Dictionary<string, double>();

            //We will use this to line up Equations that are ready to be added to the solution queue; i.e. Equations
            //for which all of their inputs (right side variables) are known.
            Queue<ISolutionStep> ZeroUnknowns = new Queue<ISolutionStep>();

            //A dictionary is probably not the best way to do this.
            //TODO find a better data structure.
            Dictionary<ISolutionStep, int> SortedByUnknownCount = new Dictionary<ISolutionStep, int>();

            //Initial Sort.  The number of unknowns are counted for each step.
            //This corresponds to the number of variables left after removing the variables from the 
            //list returned by GetInputs() that also exist in the Parameters list.
            foreach (ISolutionStep eq in this.UnorderedSteps)
            {
                List<string> unknowns = eq.GetInputs().Except(this.Parameters.Keys).ToList();
                if (unknowns.Count == 0)
                {
                    ZeroUnknowns.Enqueue(eq);
                }
                else
                {
                    SortedByUnknownCount.Add(eq, unknowns.Count);
                }
            }
            foreach (ISolutionStep eq in ZeroUnknowns)
            {
                UnsolvedSteps.Remove(eq);
            }

            //Incorporate solved equations into steps queue and increment the unknown count of 
            //the equations in the SortedByUnknownCount list that contain the newly solved variable.
            while (ZeroUnknowns.Count() > 0)
            {
                ISolutionStep eq = ZeroUnknowns.Dequeue();
                Steps.Enqueue(eq);
                List<string> newKnownVariables = eq.GetOutputs();
                //may not need to initialize, especially if each step is using the SafeAdd function during their solve methods.
                foreach(string variable in newKnownVariables)
                {
                    this.SolvedVariables.TryAdd(variable,0);
                }

                List<ISolutionStep> entriesToDecrement = new List<ISolutionStep>();

                //See if any other equations contain this newVariable as input.
                foreach (ISolutionStep e in SortedByUnknownCount.Keys)
                {
                    try
                    {
                        foreach(string variable in newKnownVariables)
                        {
                            if (e.GetInputs().Contains(variable))
                            {
                                entriesToDecrement.Add(e);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }

                //NOW we can safely remove equations from the collection?
                if (entriesToDecrement.Count > 0)
                {
                    foreach (ISolutionStep e in entriesToDecrement)
                    {
                        SortedByUnknownCount[e] -= 1;
                        if (SortedByUnknownCount[e] == 0)
                        {
                            ZeroUnknowns.Enqueue(e);
                            SortedByUnknownCount.Remove(e);
                            UnsolvedSteps.Remove(e);
                        }

                    }
                    entriesToDecrement.Clear();
                }
                
            }

        }

        private void AddDefaultFunctions(CalculationEngine engine)
        {
            engine.AddFunction("sin", (Func<double, double>)((a) => Math.Sin(a)));
            engine.AddFunction("cos", (Func<double, double>)((a) => Math.Cos(a)));
            engine.AddFunction("csc", (Func<double, double>)((a) => MathUtil.Csc(a)));
            engine.AddFunction("sec", (Func<double, double>)((a) => MathUtil.Sec(a)));
            engine.AddFunction("asin", (Func<double, double>)((a) => Math.Asin(a)));
            engine.AddFunction("acos", (Func<double, double>)((a) => Math.Acos(a)));
            engine.AddFunction("tan", (Func<double, double>)((a) => Math.Tan(a)));
            engine.AddFunction("cot", (Func<double, double>)((a) => MathUtil.Cot(a)));
            engine.AddFunction("atan", (Func<double, double>)((a) => Math.Atan(a)));
            engine.AddFunction("acot", (Func<double, double>)((a) => MathUtil.Acot(a)));
            engine.AddFunction("loge", (Func<double, double>)((a) => Math.Log(a)));
            engine.AddFunction("log10", (Func<double, double>)((a) => Math.Log10(a)));
            engine.AddFunction("logn", (Func<double, double, double>)((a, b) => Math.Log(a, b)));
            engine.AddFunction("sqrt", (Func<double, double>)((a) => Math.Sqrt(a)));
            engine.AddFunction("abs", (Func<double, double>)((a) => Math.Abs(a)));
            engine.AddFunction("if", (Func<double, double, double, double>)((a, b, c) => (a != 0.0 ? b : c)));
            engine.AddFunction("ifless", (Func<double, double, double, double, double>)((a, b, c, d) => (a < b ? c : d)));
            engine.AddFunction("ifmore", (Func<double, double, double, double, double>)((a, b, c, d) => (a > b ? c : d)));
            engine.AddFunction("ifequal", (Func<double, double, double, double, double>)((a, b, c, d) => (a == b ? c : d)));
            engine.AddFunction("ceiling", (Func<double, double>)((a) => Math.Ceiling(a)));
            engine.AddFunction("floor", (Func<double, double>)((a) => Math.Floor(a)));
            engine.AddFunction("truncate", (Func<double, double>)((a) => Math.Truncate(a)));
            engine.AddFunction("round", (Func<double, double>)((a) => Math.Round(a)));
        }


        public bool TranslateParameters(Dictionary<string, string> dictionary)
        {
            List<String> varNames = this.Parameters.Keys.ToList();
            int count = 0;
            int numParams = this.Parameters.Count;

            foreach (string name in varNames)
            {
                if (dictionary.Keys.Contains(name.ToLower()))
                {
                    this.Parameters.Add(dictionary[name.ToLower()], this.Parameters[name]);
                    this.Parameters.Remove(name);
                    count++;
                }
            }

            return count == numParams;
        }

    }
}
