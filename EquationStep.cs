using System;
using System.Collections.Generic;
using System.Text;
using Jace;
using Jace.Tokenizer;
using Jace.Operations;
using Jace.Execution;
using Jace.Util;
using System.Linq;

namespace SolutionStepParser
{
    class EquationStep : ISolutionStep
    {
        public string LeftExpression;
        public string RightExpression;
        public List<Token> LeftSideTokens;
        public List<Token> RightSideTokens;
        public List<string> InputVariables;
        public List<string> OutputVariable;

        //We could also have the variable to solve included, and have the EquationStep do the algebra.
        //It would let us know if the algebra is impossible, or only approximate.
        //In addition, we could give such a parameter to the Solve() function.  But since that is the interface's method, 
        //it would need to be included in, say, a SolverStep class.  For now, I provide the pre-solved equation.
        public EquationStep(string LeftExpression, string RightExpression)
        {
            this.LeftExpression = LeftExpression;
            this.RightExpression = RightExpression;

            TokenReader tr = new TokenReader();
            this.LeftSideTokens = tr.Read(LeftExpression);
            this.RightSideTokens = tr.Read(RightExpression);

            List<string> temp = GetVariablesFromTokens(this.RightSideTokens);
            InputVariables = temp.Distinct().ToList();
            OutputVariable = GetVariablesFromTokens(this.LeftSideTokens);

        }

        public List<string> GetInputs()
        {
            return InputVariables;
        }

        public List<string> GetOutputs()
        {
            return OutputVariable;
        }


        public void Solve(CalculationEngine ce, Dictionary<string, double> variables)
        {
            SafeAdd(variables, this.LeftExpression, ce.Calculate(this.RightExpression, variables));
        }

        public bool StepHasCorrectSyntax()
        {
            throw new NotImplementedException();
        }

        public bool StepIsValidInContext(FormulaContext context)
        {
            throw new NotImplementedException();

        }

        private List<string> GetVariablesFromTokens(List<Token> tokens)
        {
            List<string> variables = new List<string>();
            if (tokens == null || tokens.Count == 0)
            {
                throw new ArgumentNullException("tokens");
            }
            else
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].TokenType == TokenType.Text)
                    {
                        if (i == tokens.Count - 1)
                        {
                            variables.Add((string)tokens[i].Value);
                        }
                        else if (tokens[i + 1].TokenType != TokenType.LeftBracket)
                        {
                            variables.Add((string)tokens[i].Value);
                        }
                    }
                }
                return variables;


            }
        }

        private bool TokenIsVariable(Token t)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.LeftExpression + " = " + this.RightExpression;
        }

        public bool TranslateVariables(Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                throw new ArgumentNullException("dictionary");
            }
            else
            {
                int count = 0;
                TokenReader tr = new TokenReader();
                int numVarsInstances = GetVariablesFromTokens(this.LeftSideTokens).Count + GetVariablesFromTokens(this.RightSideTokens).Count;

                //do the single-variable left side
                //If equation steps gain algebra-functionality, they won't have to require the input equation to 
                //be solved for the desired quantity.  So this code will have to change.
                if (dictionary.Keys.Contains(this.LeftExpression.ToLower()))
                {
                    this.LeftExpression = new string(dictionary[this.LeftExpression.ToLower()]);
                    count++;
                }

                //do the multi-variable right side.
                List<string> newRightExpression = new List<string>();
                foreach (Token t in this.RightSideTokens)
                {
                    //There are (2) exceptions, currently:
                    //the equality "==" and unary minus "-" symbols (which convert to "=" and "_", respectively)
                    //i.e., the "==" operator in string form is "=" in token form.
                    switch (t.Value.ToString())
                    {
                        case "=": newRightExpression.Add("=="); break;
                        case "_": newRightExpression.Add("-"); break;
                        default:  newRightExpression.Add(t.Value.ToString()); break;
                    }
                }

                for (int i = 0; i < newRightExpression.Count; i++)
                {
                    if (dictionary.Keys.Contains(newRightExpression[i].ToLower()))
                    {
                        newRightExpression[i] = dictionary[newRightExpression[i].ToLower()];
                        count++;
                    }
                }

                this.RightExpression = String.Join("", newRightExpression);


                //Update this class' fields.
                this.LeftSideTokens = tr.Read(LeftExpression);
                this.RightSideTokens = tr.Read(RightExpression);

                List<string> temp = GetVariablesFromTokens(this.RightSideTokens);
                InputVariables = temp.Distinct().ToList();
                OutputVariable = GetVariablesFromTokens(this.LeftSideTokens);

                return count == numVarsInstances;

            }
        }

        //May not be needing this function, but I'll keep it around just in case.
        private string JaceTokensToString(List<Token> tokens)
        {
            if (tokens == null || tokens.Count == 0) return null;
            StringBuilder sb = new StringBuilder();
            foreach (Token t in tokens)
            {
                if (t.Value.ToString() == "=") sb.Append("==");
                else sb.Append(t.Value.ToString());
            }
            return sb.ToString();
        }

        //returns true if the variable was not in the dictionary, and false if a redefinition was made.
        public bool SafeAdd(Dictionary<string, double> dictionary, string key, double value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
                return false;
            }
            return true;
        }

    }
}
