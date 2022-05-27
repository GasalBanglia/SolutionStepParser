using System;
using System.Collections.Generic;
using System.Text;
using Jace;
using Jace.Tokenizer;
using Jace.Operations;
using Jace.Execution;
using Jace.Util;

namespace SolutionStepParser
{
    interface ISolutionStep
    {
        /*
         * Step receives a calculation engine and a variable dictionary to complete its calculations and  
         * add newly solved variables to the dictionary.
         */
        void Solve(CalculationEngine ce, Dictionary<string, double> variables); 
        /*
         * Step provides the variables it requires to be known prior to its calculations.
         */
        List<string> GetInputs();
        /*
         * Step provides the variables it will add to the variable dictionary throughout and after its calculations.
         */
        List<string> GetOutputs();
        /*
         * This method verifies that the step's calculation routine contains valid JACE statements, i.e. string expressions that
         * are intended to be calculated and parsed by a JACE CalculationEngine object.
         */
        bool StepHasCorrectSyntax();
        /*
         * Verifies that the JACE statements of a step's calculation routine are valid within the provided JACE FormulaContext, i.e.
         * that it contains only function names and constant names that were added and defined in the ConstantRegistry and FunctionRegistry objects.
         */
        bool StepIsValidInContext(FormulaContext context);
        /*
         * This method substitutes text in all Jace expressions of a step
         * with new text as specified in the provided string-string dictionary parameter
         * 
         * Perhaps later there will be text that is not a Jace expression; there may be translate functions 
         * that apply to those, too, and that apply only to functions or variables.
         * 
         * Returns true if all variables were substituted, and false otherwise.
         * 
         */
        bool TranslateVariables(Dictionary<string, string> dictionary);
        /*
         * A handy function that eliminates exceptions that may occur if a key is already in the dictionary.
         * Returns true if the entry was added, and false if the entry had to be redefined.
         */
        bool SafeAdd(Dictionary<string, double> dictionary, string key, double value);
    }
}
