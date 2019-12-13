// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.MOQ
{

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.Dtos;
    using IES.Common;
    public static class Parser
    {
        // Functions as they must be typed into a MOQ Equation by the user
        private const string REGEX_FunctionLogarithm = "log10\\(";
        private const string REGEX_FunctionNaturalLogarithm = "ln\\(";
        private const string REGEX_FunctionSquareRoot = "sqrt\\(";

        // Pulls out operands when there is a units designator between the operand and the next operator
        private const string REGEX_UnsignedNumericalOperator = "[\\d\\.,%]*\\d[\\d\\.,%]*";
        private const string REGEX_SignedNumericalOperator = "[\\+\\-]*" + REGEX_UnsignedNumericalOperator;

        // Regex validator for characters allowed in a variable name
        private const string REGEX_ValidVariable = "^[" + REGEX_VariableCharacters + "]*$";
        private const string REGEX_VariableCharacters = " a-zA-Z\\d";

        // Regex validator for misplaced decimal points
        private const string REGEX_InvalidDecimalPointPlacement = "([\\.][{0}])|([\\.]$)";

        // Regex validator for misplaced commas
        private const string REGEX_ValidCommaPlacement = "((?<=\\d),(?=(\\d\\d\\d[\\.,])))|((?<=\\d),(?=(\\d\\d\\d$)))";
        private const string REGEX_CommaAfterDecimalPlacement = "\\..*,";

        // REGEX used for pulling out units designators during validation
        private const string REGEX_OperandWithUnitsForValidation = "(" + REGEX_UnsignedNumericalOperator + ")[ ]+(.*[" + REGEX_VariableCharacters + " ]+.*)$";

        // REGEX used for removing units designators before calculation
        private const string REGEX_OperandWithUnitsForCalculation = "(" + REGEX_UnsignedNumericalOperator + ")[ ]+([" + REGEX_VariableCharacters + " ]+)";

        // Regex searches used for marking variable names with special characters for ease of parsing
        // on the client side for dynamic UI updates
        private const string REGEX_MarkedVariableReplacement = "$1<$2>$3";
        private const string FORMAT_MarkedVariableReplacement = "<{0}>";
        private const string REGEX_LoneVariableSearch = "(^[ ]*)({0})([ ]*$)";
        private const string REGEX_UnmarkedVariableBeginningSearch = "(^[ ]*)({1})([ ]*[{0}])";
        private const string REGEX_UnmarkedVariableMiddleSearch = "([{0}][ ]*)({1})([ ]*[{0}])";
        private const string REGEX_UnmarkedVariableEndingSearch = "([{0}][ ]*)({1})([ ]*$)";

        // Regex searches for replacing X(Y with X*(Y and X)Y with X)*Y
        private const string REGEX_ParenthesisMultiplicationReplacement = "$1*$3";
        private const string REGEX_OpenParenthesisMultiplication = "([\\da-zA-Z\\.])([ ]*)(\\()";
        private const string REGEX_CloseParenthesisMultiplication = "(\\))([ ]*)([\\da-zA-Z\\.])";
        private const string REGEX_PairedParenthesisMultiplication = "(\\))([ ]*)(\\()";
 
        // Regex searches for workspace and ordinary variables
        public const string REGEX_WorkspaceVariableTagReplacement = "<WSVAR:{0}>";
        
        private const string FORMAT_Decimal = "0.############################################################";

        // Maximum length of variable names
        private const int CONSTANT_VariableMaximumLength = 40;

        private static string _invalidEndOperatorsString;
        private static string _operatorsRegexString;
        private static string _invalidStartOperatorsString;
        private static string _allValidNonDigitCharactersRegexString;

        // Create static Regex objects.
        private static Regex regexReplaceSpacesInPercentages = new Regex("([ ]{1,}%)", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexBeginsWithAlphabetic = new Regex("[a-zA-Z]", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexBeginsWithNumber = new Regex("[\\d\\.,]", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexHasUnitsDesignatorAndSpace = new Regex("^" + REGEX_UnsignedNumericalOperator + "[a-zA-Z]+", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexHasUnitsDesignatorNoSpace = new Regex(REGEX_UnsignedNumericalOperator, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexOperandWithUnits = new Regex(REGEX_OperandWithUnitsForValidation, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexValidCharacters = new Regex("^[" + AllValidCharactersRegexString + "]*$", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexValidCharactersReplace = new Regex("[" + AllValidCharactersRegexString + "]*", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexInvalidStartOperator = new Regex("^[" + InvalidStartOperatorsString + "]", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexInvalidEndOperator = new Regex("[" + InvalidEndOperatorsString + "]$", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexDecimalPointPlacement = new Regex(String.Format(REGEX_InvalidDecimalPointPlacement, AllValidNonDigitCharactersRegexString), RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexFindPeriods = new Regex("\\.", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexFindCommas = new Regex("\\,", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexCommaAfterDecimalPlacement = new Regex(REGEX_CommaAfterDecimalPlacement, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexValidCommaPlacement = new Regex(REGEX_ValidCommaPlacement, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexValidVariable = new Regex(REGEX_ValidVariable, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexOperandWithUnitsForCalculation = new Regex(REGEX_OperandWithUnitsForCalculation, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexFunctionLogarithm = new Regex(REGEX_FunctionLogarithm, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
        private static Regex regexFunctionNaturalLogarithm = new Regex(REGEX_FunctionNaturalLogarithm, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
        private static Regex regexFunctionSquareRoot = new Regex(REGEX_FunctionSquareRoot, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
        private static Regex regexOpenParenthesisMultiplication = new Regex(REGEX_OpenParenthesisMultiplication, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexCloseParenthesisMultiplication = new Regex(REGEX_CloseParenthesisMultiplication, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexPairedParenthesisMultiplication = new Regex(REGEX_PairedParenthesisMultiplication, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexSignedNumericalOperator = new Regex(REGEX_SignedNumericalOperator, RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexSignedNumericalOperatorRightToLeft = new Regex(REGEX_SignedNumericalOperator, RegexOptions.RightToLeft, Constants.REGEX_TIMEOUT);
        private static Regex regexFirstCharacterOperator = new Regex("[" + OperatorsRegexString.Replace("\\)", "") + "]", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexEndOfNextOperand = new Regex(REGEX_SignedNumericalOperator + ".*?[" + OperatorsRegexString + "]", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexPositiveSigns = new Regex("\\+", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexNegativeSigns = new Regex("\\-", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex regexUnsignedNumericalOperator = new Regex(REGEX_UnsignedNumericalOperator, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Primary calculation function for MOQ Equations. Prepares the string and then calls the
        /// recursive _Calculate function. Variables must already be replaced with values in the given
        /// input equation.
        /// </summary>
        /// <param name="inputEquation">The MOQ Equation to calculate</param>
        /// <param name="ws">Workspace, used for adjusting precision (rounding) of the result</param>
        /// <returns>The resulting value from the equation</returns>
        public static string Calculate(string inputEquation, WorkspaceDTO ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            
            string toReturn = "0";

            if (inputEquation != null && inputEquation.Trim().Length > 0)
            {
                inputEquation = RemoveSpacesInPercentages(inputEquation);
                inputEquation = RemoveUnitsDesignators(inputEquation);
                inputEquation = inputEquation.Replace(" ", "");
                inputEquation = ReformatPercentages(inputEquation);
                inputEquation = ReformatSpecialFunctions(inputEquation);

                toReturn = _Calculate(inputEquation);
            }

            // Round the result to precision specified in the workspace
            decimal result;
            if (!Decimal.TryParse(toReturn, out result))
            {
                throw new GeneralMOQCalculationException("Calculation resulted in a number too large for processing.");
            }
            result = Utilities.AdjustPrecision(result, ws.ResourceDecimalPrecision);

            // Return the result
            return result.ToString(Utilities.PrecisionFormattingStringNoComma(ws.ResourceDecimalPrecision));
        }

        /// <summary>
        /// Removes the spaces in numbers with percentages -> 100% is the same as 100 %
        /// </summary>
        /// <param name="inputEquation">Original Equation</param>
        /// <returns>New equation</returns>
        public static string RemoveSpacesInPercentages(string inputEquation)
        {
            inputEquation = regexReplaceSpacesInPercentages.Replace(inputEquation, "%");

            return inputEquation;
        }

        /// <summary>
        /// Full back-end calculation function takes raw MOQ Equation, Task Ordinary Variables and Workspace
        /// Variables and generates the resulting calculation.
        /// </summary>
        /// <param name="inputEquation">The raw MOQ equation as saved by the user</param>
        /// <param name="ordinaryVariables">List of all of the task's ordinary variables</param>
        /// <param name="workspaceVariables">List of all of the workspace's workspace variables</param>
        /// <param name="variableSelectBOEtoSumCalculation">The variable select bo eto sum calculation.</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <param name="ws">The workspace.</param>
        /// <returns>
        /// The resulting value from the equation
        /// </returns>
        /// <exception cref="System.ArgumentNullException">variableSelectBOEtoSumCalculation</exception>
        public static string Calculate(
            string inputEquation,
            ICollection<OrdinaryVariableDto> ordinaryVariables,
            ICollection<WorkspaceVariableDTO> workspaceVariables,
            IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation, 
            DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc, WorkspaceDTO ws)
        {
            if (variableSelectBOEtoSumCalculation == null)
            {
                throw new ArgumentNullException(nameof(variableSelectBOEtoSumCalculation));
            }

            if (dataForSumOfBoeCalc == null)
            {
                dataForSumOfBoeCalc = new DataClassForSumOfBOEsCalculation();
            }

            string toReturn = "0";

            if (inputEquation != null && inputEquation.Trim().Length > 0)
            {
                Collection<MOQVariable> otherVariables = null;

                // Untag any tagged workspace variables to convert them to variable names from IDs
                inputEquation = UntagVariables(inputEquation, workspaceVariables, out otherVariables);

                // Call validate to format any variables found in the equation, making them easier
                // to find and replace with their values
                ICollection<string> validationResults = Validate(inputEquation);

                // Set the equation to the first result returned from Validate, which is the
                // re-formatted input equation
                String equation = validationResults.First();

                // If there were variables found in the equation, we'll replace them with their values
                if (validationResults.Count > 1)
                {
                    // Iterate over all ordinary variables and replace any occurrences of those variables
                    // in the equation with the corresponding value
                    if (ordinaryVariables != null)
                    {
                        foreach (OrdinaryVariableDto variable in ordinaryVariables)
                        {
                            String variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, variable.OrdinaryVariableName);
                            if (Regex.IsMatch(equation, variableReplacementRegex, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT))
                            {
                                variable.OrdinaryVariableValue = variableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(variable, dataForSumOfBoeCalc);
                                equation = Regex.Replace(equation, variableReplacementRegex, variable.OrdinaryVariableValue.ToString(), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                            }
                        }
                    }

                    // Iterate over all workspace variables and replace any occurrences of those variables
                    // in the equation with the corresponding value
                    if (workspaceVariables != null)
                    {
                        foreach (WorkspaceVariableDTO var in workspaceVariables)
                        {
                            String variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, var.WorkspaceVariableName);
                            if (Regex.IsMatch(equation, variableReplacementRegex, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT))
                            {
                                decimal workspaceVariableValue;
                                // This seems to already have a value, and the calculation seems to yield the wrong result.
                                if ((workspaceVariableValue = var.WorkspaceVariableValue) == 0m)
                                {
                                    workspaceVariableValue = variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(var, dataForSumOfBoeCalc);
                                }
                                equation = Regex.Replace(equation, variableReplacementRegex, workspaceVariableValue.ToString(), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                            }
                        }
                    }

                    // Iterate over all other variables and replace any occurrences of those variables
                    // in the equation with the corresponding value
                    if (otherVariables != null)
                    {
                        foreach (MOQVariable var in otherVariables)
                        {
                            String variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, var.VariableDisplayName);
                            if (Regex.IsMatch(equation, variableReplacementRegex, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT))
                            {
                                equation = Regex.Replace(equation, variableReplacementRegex, var.VariableValue.ToString(), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                            }
                        }
                    }
                }

                toReturn = Calculate(equation, ws);
            }

            return toReturn;
        }

        /// <summary>
        /// Recursive function to perform MOQ Equation calculation. The algorithm is as follows:
        ///   1. Get lowest precedence parentheses block
        ///   2. Recursive self-call using contents of lowest precedence parentheses block as input
        ///   3. Replace lowest precedence parentheses block with result from recursive call
        ///   4. Go back to step 1 until no blocks remain. This handles multiple sibling blocks like (2*5)+(5*5)
        ///   5. Get the operation of highest precedence in the equation
        ///   6. Process the operation
        ///   7. Go back to step 5 until no operations remain.
        ///   8. Return the result
        /// </summary>
        /// <param name="inputEquation"></param>
        /// <returns>The resulting value from the given subequation</returns>
        private static string _Calculate(string inputEquation)
        {
            // Get lowest precedence parentheses block
            ParenthesisPair lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);

            // While there are still unprocessed parentheses blocks
            while (lowestPrecedenceParentheses != null)
            {
                // Call this _Calculate function on the contents of the parentheses block
                string recursiveInputEquation = _Calculate(inputEquation.Substring(lowestPrecedenceParentheses.OpenIndex + 1, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex - 1));

                // Replace the parentheses block with the result of the recursive _Calculate call
                inputEquation = inputEquation.Remove(lowestPrecedenceParentheses.OpenIndex, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex + 1);
                inputEquation = inputEquation.Insert(lowestPrecedenceParentheses.OpenIndex, recursiveInputEquation);

                // Get the next lowest remaining parentheses block
                lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);
            }

            // Get the operation of highest precedence in the equation
            Int32 nextOperationIndex = FindNextOperationIndex(inputEquation);

            // Process all operations in the equation until none remain
            while (nextOperationIndex >= 0)
            {
                // Process the selected operation
                inputEquation = DoOperationAtIndex(inputEquation, nextOperationIndex);

                // Get the next operation of highest precedence in the equation
                nextOperationIndex = FindNextOperationIndex(inputEquation);
            }

            return inputEquation;
        }

        /// <summary>
        /// Validates the equation and gets a list of unique variable names from the
        /// input equation. Variables are those operands that start with a alphabetic character.
        /// </summary>
        /// <param name="inputEquation">The MOQ equation to search for variables</param>
        /// <returns>An Enumerable collection of unique upper-case variable names</returns>
        public static ICollection<string> Validate(string inputEquation)
        {
            if (inputEquation == null)
            {
                throw new ArgumentNullException(nameof(inputEquation));
            }

            List<string> equationVariables = new List<string> { "" };

            if (inputEquation.Trim().Length > 0)
            {
                // Remove spaces if percentage is inputted
                inputEquation = RemoveSpacesInPercentages(inputEquation);

                // Verify that there are no invalid characters in the entire equation
                ValidateAllCharacters(inputEquation);

                // Checks parentheses in the input equation for proper balance.
                ValidateEquationParentheses(inputEquation);

                // Format special functions to make detection of variables easier
                inputEquation = ReformatSpecialFunctions(inputEquation);

                // Check for invalid operators on the ends of the equation
                ValidateEquationEnds(inputEquation);

                // Check for invalid use of number decorator '.'
                ValidateNumberDecorators(inputEquation);

                // Validate the equation and get a list of variables found
                equationVariables = _Validate(inputEquation);

                // Mark the equation's variables and add the marked equation as the
                // first element in the return collection
                equationVariables.Insert(0, MarkVariablesInEquation(inputEquation, equationVariables.Select(variable => variable.ToUpper()).Distinct().ToList()));
            }

            return equationVariables;
        }

        /// <summary>
        /// Validates the equation and gets a list of unique variable names from the
        /// input equation. Variables are those operands that start with a alphabetic character.
        /// </summary>
        /// <param name="inputEquation">The MOQ equation to search for variables</param>
        /// <returns>An Enumerable collection of unique upper-case variable names</returns>
        private static List<string> _Validate(string inputEquation)
        {
            List<string> variables = new List<string>();

            // Set a value to indicate whether we dove from this recursion into a deeper parenthesis block.
            // If this parenthesis level has no other operands, but we did dive into a nested parenthesis block
            // then we won't need to throw an exception indicating no operands (since the nested block creates one).
            bool recursed = false;

            // Get lowest precedence parentheses block
            ParenthesisPair lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);

            // While there are still unprocessed parentheses blocks
            while (lowestPrecedenceParentheses != null)
            {
                // Set a flag to indicate that we recursed into a nested parenthesis block
                if (!recursed)
                {
                    recursed = true;
                }

                // Call this _Validate function on the contents of the parentheses block
                variables.AddRange(_Validate(inputEquation.Substring(lowestPrecedenceParentheses.OpenIndex + 1, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex - 1)));

                // Remove the parentheses block
                inputEquation = inputEquation.Remove(lowestPrecedenceParentheses.OpenIndex, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex + 1);

                // Get the next lowest remaining parentheses block
                lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);
            }

            Collection<string> constants = new Collection<string>();
            Collection<string> unitsDesignators = new Collection<string>();

            // Get all operands in this equation portion
            var allOperands = GetAllOperands(inputEquation);

            // Check for no operands in this portion of the equation. If any portion of the equation has
            // no operands, then we'll thow an error. However, if we are in a portion of the equation that
            // has nested parenthesis blocks, we can skip this exception, since the contents of that parenthesis
            // block will effectively create an operand.
            if (allOperands.None() && !recursed)
            {
                throw new GeneralMOQParsingException("A portion of the equation has no operands defined. Please check for parenthesis blocks containing no operators or variables. For example, '(60)', '(60 Hours)' and '(Hours)' are all valid, but '()' and '(+)' are not.");
            }

            // Get a list of all operands in the equation and iterate over them
            foreach (string operand in allOperands)
            {
                // If a given operand begins with an alphabetic character, it is a variable
                if (regexBeginsWithAlphabetic.IsMatch(operand.Substring(0, 1)))
                {
                    // So add it to the return collection
                    variables.Add(operand);
                }
                // If it begins with a number, it is a constant, possibly with units
                else if (regexBeginsWithNumber.IsMatch(operand.Substring(0, 1)))
                {
                    // If there is a units designator, verify that it and the constant are separated by at least one space
                    if (regexHasUnitsDesignatorAndSpace.IsMatch(operand))
                    {
                        throw new GeneralMOQParsingException(
                            string.Format(
                                "One or more spaces must appear between a constant value and its units designator. Please add a space before '{0}'.",
                                regexHasUnitsDesignatorNoSpace.Replace(operand, string.Empty)));
                    }
                    // The constant is valid. We'll pull out the value and any units for validation on both
                    else
                    {
                        // If there is a units designator, we'll collect it for validation with variables
                        Match match = regexOperandWithUnits.Match(operand);
                        if (match.Success)
                        {
                            string matchString = match.Groups[2].Value.Trim();

                            if (matchString.Length > 0)
                            {
                                unitsDesignators.Add(matchString);
                            }

                            constants.Add(match.Groups[1].Value.Trim());
                        }
                        // It's a constant without units
                        else
                        {
                            constants.Add(operand.Trim());
                        }
                    }
                }
            }

            // Checks constants for validity
            ValidateConstantValues(constants);

            // Validate Variable Whitespace Removed by Task 1984 -------------

            // Checks to ensure that variables contain no white space
            //ValidateVariableWhitespace(variables);

            // Checks to ensure that units designators contain no white space
            //ValidateUnitsWhitespace(unitsDesignators);

            // ---------------------------------------------------------------

            // Checks for variable length requirements
            ValidateVariableLength(variables);

            // Checks for units designator length requirements
            ValidateUnitsLength(unitsDesignators);

            // Checks variables for invalid characters
            ValidateVariableCharacters(variables);

            // Checks units designators for invalid characters
            ValidateUnitsCharacters(unitsDesignators);

            // Checks variables for invalid first characters
            ValidateVariableFirstCharacter(variables);

            // Checks units designators for invalid first characters
            ValidateUnitsFirstCharacter(unitsDesignators);

            // Return the collection of variables and the marked input equation
            return variables.Select(variable => variable.ToUpper()).Distinct().ToList();
        }

        /// <summary>
        /// Replaces workspace variables in the MOQ equation with their values.
        /// </summary>
        /// <param name="equationFromDB">MOQ eqation</param>
        /// <param name="workspaceVariables">workspace variables</param>
        /// <returns>
        /// MOQ equations with workspace variable references replaced with workspace variable values
        /// </returns>
        public static String UntagVariables(
            string equationFromDB,
            ICollection<WorkspaceVariableDTO> workspaceVariables)
        {
            Collection<MOQVariable> moqVariables = null;
            
            return UntagVariables(equationFromDB, workspaceVariables, out moqVariables);
        }

        /// <summary>
        /// Untags the variables.
        /// </summary>
        /// <param name="equationFromDB">The equation from database.</param>
        /// <param name="workspaceVariables">The workspace variables.</param>
        /// <param name="untaggedVariables">The untagged variables.</param>
        /// <returns></returns>
        public static String UntagVariables(
            string equationFromDB,
            ICollection<WorkspaceVariableDTO> workspaceVariables,
            out Collection<MOQVariable> untaggedVariables)
        {
            untaggedVariables = new Collection<MOQVariable>();

            if (string.IsNullOrEmpty(equationFromDB))
            {
                return string.Empty;
            }

            // Iterate over all workspace variables and replace any occurences of those variables
            // IDs in the equation with the cooresponding variable name
            if (workspaceVariables != null)
            {
                foreach (WorkspaceVariableDTO var in workspaceVariables)
                {
                    String variableReplacementRegex = string.Format(REGEX_WorkspaceVariableTagReplacement, var.Id);
                    equationFromDB = Regex.Replace(equationFromDB, variableReplacementRegex, var.WorkspaceVariableName.ToString(), RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                }
            }

            return equationFromDB;
        }

        /// <summary>
        /// Tags the variables.
        /// </summary>
        /// <param name="inputEquation">The input equation.</param>
        /// <param name="workspaceVariables">The workspace variables.</param>
        /// <returns></returns>
        public static String TagVariables(string inputEquation,
            IReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables)
        {
            if (string.IsNullOrEmpty(inputEquation))
            {
                return string.Empty;
            }

            // Iterate over all workspace variables and replace any occurences of those variables
            // IDs in the equation with the cooresponding variable name
            if (workspaceVariables != null)
            {
                inputEquation = MarkVariablesInEquation(inputEquation, workspaceVariables.Select(w => w.WorkspaceVariableName).ToList());

                foreach (WorkspaceVariableDTO var in workspaceVariables)
                {
                    String variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, var.WorkspaceVariableName);
                    String variableReplacement = string.Format(REGEX_WorkspaceVariableTagReplacement, var.Id);
                    inputEquation = Regex.Replace(inputEquation, variableReplacementRegex, variableReplacement, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                }
            }

            return inputEquation;
        }

        /// <summary>
        /// Validate all characters in the equation, and alert the user if there are invalid entries found
        /// </summary>
        /// <param name="inputEquation">The equation to validate</param>
        private static void ValidateAllCharacters(string inputEquation)
        {
            if (!regexValidCharacters.IsMatch(inputEquation))
            {
                // Throw an exception with a specific message if an invalid character is present
                throw new GeneralMOQParsingException(
                    "Equation contains invalid characters. Please remove the following special characters and try again: " +
                    regexValidCharactersReplace.Replace(inputEquation, string.Empty)
                );
            }
        }

        /// <summary>
        /// Checks parentheses in the input equation for proper balance.
        /// </summary>
        /// <param name="inputEquation">The MOQ equation to check for parentheses balance</param>
        private static void ValidateEquationParentheses(string inputEquation)
        {
            // Varaiable stores the +/- difference in parentheses found in the equation
            int parenthesesDifference = 0;

            // Step through each character in the equation and increment/decrement the
            // difference count for each parentheses found
            foreach (char equationCharacter in inputEquation)
            {
                // Open parens will increment the count
                if (equationCharacter == '(')
                {
                    parenthesesDifference++;
                }
                // Close parens will decrement the count
                else if (equationCharacter == ')')
                {
                    parenthesesDifference--;
                }

                // If a close parenthesis is found without a corresponding preceding open parentheses,
                // then the difference count will fall below 0 and we'll throw an exception
                if (parenthesesDifference < 0)
                {
                    throw new GeneralMOQParsingException("Equation parentheses must be properly balanced.");
                }
            }

            // If we scan all characters in the equation and have a difference number other than 1, then
            // the parentheses weren't properly balanced
            if (parenthesesDifference != 0)
            {
                throw new GeneralMOQParsingException("Equation parentheses must be properly balanced.");
            }
        }

        /// <summary>
        /// Validate the characters on the beginning and end of the equation
        /// </summary>
        /// <param name="inputEquation"></param>
        private static void ValidateEquationEnds(string inputEquation)
        {
            inputEquation = inputEquation.Trim();

            if (regexInvalidStartOperator.IsMatch(inputEquation))
            {
                // Throw an exception with a specific message if an invalid character is present
                throw new GeneralMOQParsingException(
                    "Equation must not start with multiplication, division or exponentiation. Please remove these operators or add another operand at the beginning of the equation before saving."
                );
            }
            else if (regexInvalidEndOperator.IsMatch(inputEquation))
            {
                // Throw an exception with a specific message if an invalid character is present
                throw new GeneralMOQParsingException(
                    "Equation must not end with an operator. Please remove these operators or add another operand at the end of the equation before saving."
                );
            }
        }

        /// <summary>
        /// Validate that any decimal points used in the equation are between two digits
        /// </summary>
        /// <param name="inputEquation"></param>
        private static void ValidateNumberDecorators(string inputEquation)
        {
            inputEquation = inputEquation.Trim();

            if (regexDecimalPointPlacement.IsMatch(inputEquation))
            {
                // Throw an exception with a specific message if an invalid decimal point is present
                throw new GeneralMOQParsingException(
                    "Decimal points must always be followed by another digit."
                );
            }
        }

        /// <summary>
        /// Validate that all constant values only contain zero or one decimal points and that all commas
        /// a preceded by one digit and followed by three.
        /// </summary>
        /// <param name="constants">The constants.</param>
        /// <exception cref="GeneralMOQParsingException">
        /// </exception>
        private static void ValidateConstantValues(Collection<string> constants)
        {
            // For each constant, we'll check to make sure that all characters are valid
            foreach (string toValidate in constants)
            {
                if (regexFindPeriods.Matches(toValidate).Count > 1)
                {
                    // Throw an exception with a specific message if more than one decimal point is present
                    throw new GeneralMOQParsingException(
                        string.Format(
                            "A numeric value must not contain more than one decimal point. Please correct '{0}'.",
                            toValidate));
                }

                var commaCount = regexFindCommas.Matches(toValidate).Count;

                if (commaCount > 0)
                {
                    if (regexCommaAfterDecimalPlacement.IsMatch(toValidate))
                    {
                        // Throw an exception with a specific message if invalid commas are present
                        throw new GeneralMOQParsingException(
                            String.Format(
                                "A comma must not appear following a decimal point. Please correct '{0}'.",
                                toValidate));
                    }

                    var matches = regexValidCommaPlacement.Matches(toValidate);
                    if (matches.Count != commaCount)
                    {
                        // Throw an exception with a specific message if invalid commas are present
                        throw new GeneralMOQParsingException(
                            string.Format(
                                "Each comma must be preceded by at least one digit and followed by exactly three consecutive digits. Please correct '{0}'.",
                                toValidate));
                    }
                }
            }
        }

        /// <summary>
        /// Checks variables for invalid first character
        /// </summary>
        /// <param name="variables">List of variables in the equation</param>
        private static void ValidateVariableFirstCharacter(ICollection<string> variables)
        {
            GenericValidateFirstCharacter(variables, "Variable names");
        }

        /// <summary>
        /// Checks units designators for invalid first character
        /// </summary>
        /// <param name="unitsDesignators">The units designators.</param>
        private static void ValidateUnitsFirstCharacter(ICollection<string> unitsDesignators)
        {
            GenericValidateFirstCharacter(unitsDesignators, "Units designators");
        }

        /// <summary>
        /// Checks a generic collection of strings for invalid first character
        /// </summary>
        /// <param name="collectionToValidate">The collection to validate.</param>
        /// <param name="exceptionPrepend">The exception prepend.</param>
        /// <exception cref="GeneralMOQParsingException"></exception>
        private static void GenericValidateFirstCharacter(ICollection<string> collectionToValidate, String exceptionPrepend)
        {
            // For each variable, we'll check to make sure that all characters are valid
            foreach (string toValidate in collectionToValidate)
            {
                if (!regexBeginsWithAlphabetic.IsMatch(toValidate.Substring(0, 1)))
                {
                    // Throw an exception with a specific message if an invalid character is present
                    throw new GeneralMOQParsingException(
                        String.Format(
                            exceptionPrepend + " must begin with an alphabetic character. Please correct '{0}'. ",
                            toValidate));
                }
            }
        }

        /// <summary>
        /// Checks variables for invalid characters
        /// </summary>
        /// <param name="variables">List of variables in the equation</param>
        private static void ValidateVariableCharacters(ICollection<string> variables)
        {
            GenericValidateCharacters(variables, "Variable names");
        }

        /// <summary>
        /// Checks units designators for invalid characters
        /// </summary>
        /// <param name="unitsDesignators">The units designators.</param>
        private static void ValidateUnitsCharacters(ICollection<string> unitsDesignators)
        {
            GenericValidateCharacters(unitsDesignators, "Units designators");
        }

        /// <summary>
        /// Checks a generic collection of strings for invalid characters
        /// </summary>
        /// <param name="collectionToValidate">The collection to validate.</param>
        /// <param name="exceptionPrepend">The exception prepend.</param>
        /// <exception cref="GeneralMOQParsingException"></exception>
        private static void GenericValidateCharacters(ICollection<string> collectionToValidate, String exceptionPrepend)
        {
            // For each variable, we'll check to make sure that all characters are valid
            foreach (string toValidate in collectionToValidate)
            {
                if (!regexValidVariable.IsMatch(toValidate))
                {
                    // Throw an exception with a specific message if an invalid character is present
                    throw new GeneralMOQParsingException(
                        string.Format(
                            exceptionPrepend + " can only contain alphanumeric characters. Please correct '{0}'. ",
                            toValidate));
                }
            }
        }

        /// <summary>
        /// Checks variables for length requirements
        /// </summary>
        /// <param name="variables">List of variables in the equation</param>
        private static void ValidateVariableLength(ICollection<string> variables)
        {
            GenericValidateLength(variables, "Variable names");
        }

        /// <summary>
        /// Checks units designators for length requirements
        /// </summary>
        /// <param name="unitsDesignators">The units designators.</param>
        private static void ValidateUnitsLength(ICollection<string> unitsDesignators)
        {
            GenericValidateLength(unitsDesignators, "Units designators");
        }

        /// <summary>
        /// Checks a generic collection of strings for length requirements
        /// </summary>
        /// <param name="collectionToValidate">The collection to validate.</param>
        /// <param name="exceptionPrepend">The exception prepend.</param>
        /// <exception cref="GeneralMOQParsingException"></exception>
        private static void GenericValidateLength(ICollection<string> collectionToValidate, String exceptionPrepend)
        {
            // For each variable, we'll check to make sure that length does not exceed 20 characters
            foreach (string toValidate in collectionToValidate)
            {
                if (toValidate.Length > CONSTANT_VariableMaximumLength)
                {
                    // Throw an exception with a specific message if length is exceeded
                    throw new GeneralMOQParsingException(
                        String.Format(
                            exceptionPrepend + " must be between 1 and {0} characters in length. Please shorten '{1}'.",
                            CONSTANT_VariableMaximumLength,
                            toValidate));
                }
            }
        }

        /// <summary>
        /// Marks the given equations variables (given in the filteredVariables collection) with
        /// brackets for ease of variable replacement later.
        /// </summary>
        /// <param name="inputEquation">The input equation containing unmarked varaibles</param>
        /// <param name="filteredVariables">A collection of distinct variables in the equation</param>
        /// <returns>Equation with variables marked with brackets</returns>
        private static string MarkVariablesInEquation(string inputEquation, ICollection<string> filteredVariables)
        {
            // Declare Regex strings
            string beginSearch = string.Format(REGEX_UnmarkedVariableBeginningSearch, OperatorsRegexString, "{0}");
            string middleSearch = string.Format(REGEX_UnmarkedVariableMiddleSearch, OperatorsRegexString, "{0}");
            string endSearch = string.Format(REGEX_UnmarkedVariableEndingSearch, OperatorsRegexString, "{0}");

            // For each distinct variable, mark that variable's location in the equation
            // as "<VARIABLENAME>". The brackets will assist the client side in replacing these
            // variables with values when it comes time to calculate. Uses Regex Grouping for
            // replacement operations.
            foreach (string variable in filteredVariables)
            {
                inputEquation = Regex.Replace(
                    inputEquation,
                    string.Format(REGEX_LoneVariableSearch, variable),
                    REGEX_MarkedVariableReplacement,
                    RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

                inputEquation = Regex.Replace(
                    inputEquation,
                    string.Format(beginSearch, variable),
                    REGEX_MarkedVariableReplacement,
                    RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

                // Had to add this while statement because the Replace function wasn't catching two consecutive
                // matches of the same variable name.
                string middleVariableRegEx = string.Format(middleSearch, variable);
                while (Regex.IsMatch(inputEquation, middleVariableRegEx, RegexOptions.IgnoreCase))
                {
                    inputEquation = Regex.Replace(
                        inputEquation,
                        middleVariableRegEx,
                        REGEX_MarkedVariableReplacement,
                        RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                }

                inputEquation = Regex.Replace(
                    inputEquation,
                    string.Format(endSearch, variable),
                    REGEX_MarkedVariableReplacement,
                    RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
            }

            return inputEquation;
        }

        /// <summary>
        /// Gets every operand in the equation. Stacks the equation similar to the Calculate funtion
        /// and then moves through the stack getting operands recursively.
        /// </summary>
        /// <param name="inputEquation">The original MOQ equation</param>
        /// <returns>A collection of every operand in the given equation</returns>
        private static ICollection<string> GetAllOperands(string inputEquation)
        {
            List<string> toReturn = new List<string>();

            // Get lowest precedence parentheses block
            ParenthesisPair lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);

            // While there are still unprocessed parentheses blocks
            while (lowestPrecedenceParentheses != null)
            {
                // Call this GetAllOperands function on the contents of the parentheses block
                toReturn.AddRange(GetAllOperands(inputEquation.Substring(lowestPrecedenceParentheses.OpenIndex + 1, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex - 1)));

                // Remove the parentheses block after it has been processed recursively
                inputEquation = inputEquation.Remove(lowestPrecedenceParentheses.OpenIndex, lowestPrecedenceParentheses.CloseIndex - lowestPrecedenceParentheses.OpenIndex + 1);

                // Get the next lowest remaining parentheses block
                lowestPrecedenceParentheses = GetLowestPrecedenceParentheses(inputEquation);
            }

            // Split the inputEquation on the occurence of any of the supported MathOperators
            string[] operands = inputEquation.Split(((MathOperators[])Enum.GetValues(typeof(MathOperators))).Select(enumValue => (Char)enumValue).ToArray());

            // Add the operands to the return collection, filtering out blank values
            toReturn.AddRange(operands.Select(operand => operand.Trim()).Where(operand => operand.Length > 0));

            return toReturn;
        }

        /// <summary>
        /// Removes units from operands with units designators. For example '10 hours' is reduced to '10'.
        /// </summary>
        /// <param name="inputEquation">The equation to remove units from.</param>
        /// <returns>The equation with units removed.</returns>
        private static string RemoveUnitsDesignators(string inputEquation)
        {
            inputEquation = regexOperandWithUnitsForCalculation.Replace(inputEquation, "$1");
            return inputEquation;
        }

        /// <summary>
        /// Changes XX% to XX/100. For example, 35.5% becomes .355. Works for 0% up to very large percentages
        /// (i.e. 1000000.05% will become 10000.0005)
        /// </summary>
        /// <param name="inputEquation">Equation to reformat</param>
        /// <returns>Reformatted Equation String</returns>
        private static string ReformatPercentages(string inputEquation)
        {
            while (inputEquation.IndexOf((Char)NumberDecorators.Percentage) != -1)
            {
                // Get index of first '%'
                int ndxFirstPercent = inputEquation.IndexOf((Char)NumberDecorators.Percentage);

                // Get index of last non-digit or decimal point before '%'
                int ndxFirstCharOfPercentage = FindStartOfPreviousOperand(inputEquation, ndxFirstPercent);

                // Get percentage string and Remove '%'
                string percentage = inputEquation.Substring(ndxFirstCharOfPercentage, ndxFirstPercent - ndxFirstCharOfPercentage);

                // Divide by 100 and format string
                percentage = (decimal.Parse(percentage) / 100).ToString();

                // Replace original percentage string with reformatted value
                inputEquation = inputEquation.Remove(ndxFirstCharOfPercentage, ndxFirstPercent - ndxFirstCharOfPercentage + 1);
                inputEquation = inputEquation.Insert(ndxFirstCharOfPercentage, percentage);
            }

            return inputEquation;
        }

        /// <summary>
        /// Reformats special functions in an equation to make them more easily understood by the parser.
        /// sqrt(X) is converted to #(X), log(X) is converted to ∟(X), Y(X) is converted to Y*(X) and (X)Y
        /// is converted to (X)*Y. This standardizes all operations to have a single operator with operands
        /// on each side (^, *, /, +, -) or an operand to the right (log(), sqrt()).
        /// </summary>
        /// <param name="inputEquation">Equation to reformat</param>
        /// <returns>Reformatted Equation String</returns>
        private static string ReformatSpecialFunctions(string inputEquation)
        {
            // Replace log10( with a ∟ to denote logarithms
            inputEquation = regexFunctionLogarithm.Replace(inputEquation, string.Empty + (Char)MathOperators.Logarithm + (Char)Parenthesis.Open);

            // Replace ln( with a ⌐ to denote natural logarithms
            inputEquation = regexFunctionNaturalLogarithm.Replace(inputEquation, string.Empty + (Char)MathOperators.NaturalLogarithm + (Char)Parenthesis.Open);

            // Replace sqrt( with a # to denote square roots
            inputEquation = regexFunctionSquareRoot.Replace(inputEquation, string.Empty + (char)MathOperators.SquareRoot + (char)Parenthesis.Open);

            // Add multiplication sign to convert Y(X) --> Y*(X), (X)Y --> (X)*Y, (X)(Y) --> (X)*(Y)
            inputEquation = regexOpenParenthesisMultiplication.Replace(inputEquation, REGEX_ParenthesisMultiplicationReplacement);
            inputEquation = regexCloseParenthesisMultiplication.Replace(inputEquation, REGEX_ParenthesisMultiplicationReplacement);
            inputEquation = regexPairedParenthesisMultiplication.Replace(inputEquation, REGEX_ParenthesisMultiplicationReplacement);

            return inputEquation;
        }

        /// <summary>
        /// Finds the set of parentheses in the equation that should be executed last. This can be used
        /// for recursion by passing in smaller and smaller chunks of the equation until null is returned from
        /// this function.
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <returns>The lowest precedence parentheses pair, or null if no parentheses exist</returns>
        private static ParenthesisPair GetLowestPrecedenceParentheses(string inputEquation)
        {
            ParenthesisPair toReturn = null;

            // Get a list of all pairs of parentheses in the equation
            List<ParenthesisPair> allPairs = GetParenthesisPairs(inputEquation);

            if (allPairs.Count > 0)
            {
                // Return the last pair in the list, which was the last one popped from the stack
                toReturn = allPairs[allPairs.Count - 1];
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a list of all parenthesis pairs in the equation, ordered from highest precendence to lowest.
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <returns>List of all parentheses pairs in the equation, or an empty list if no parentheses exist</returns>
        private static List<ParenthesisPair> GetParenthesisPairs(string inputEquation)
        {
            List<ParenthesisPair> toReturn = new List<ParenthesisPair>();

            Stack<Int32> parenthesisIndexes = new Stack<int>();

            // Iterate over each character in the equation
            for (int ndx = 0; ndx < inputEquation.Length; ndx++)
            {
                if (inputEquation[ndx] == (Char)Parenthesis.Open)
                {
                    // If the current character is a '(', push this index onto the stack
                    parenthesisIndexes.Push(ndx);
                }
                else if (inputEquation[ndx] == (Char)Parenthesis.Close)
                {
                    // If the current character is a ')', pop the last '(' index from the stack and
                    // add a new parentheses pair to the return list
                    toReturn.Add(new ParenthesisPair(parenthesisIndexes.Pop(), ndx));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Finds the index of the next most important operation in the equation. Ignores parentheses, as
        /// parentheses precedent should be handled by recursion in the top-level Calculate function. Precendence
        /// is as follows: 1.] log()  2.] sqrt() or ^  3.] * or /  4.] + or -
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <returns>The index of the next operation that should be processed, or -1 if no operations exist.</returns>
        private static int FindNextOperationIndex(string inputEquation)
        {
            int toReturn = -1;

            // Calls ReturnIndexOfFirstOperation with operators in order of precedence. If any index comes back greater
            // than -1, the remaining "else if" statements are not executed and the operator index is returned. If toReturn
            // remains -1, then no operators were found in the equation.
            if ((toReturn = ReturnIndexOfFirstOperation(inputEquation, MathOperators.Logarithm, MathOperators.NaturalLogarithm)) >= 0) {}
            else if ((toReturn = ReturnIndexOfFirstOperation(inputEquation, MathOperators.SquareRoot, MathOperators.Exponentiation)) >= 0) { }
            else if ((toReturn = ReturnIndexOfFirstOperation(inputEquation, MathOperators.Multiplication, MathOperators.Division)) >= 0) { }
            else if ((toReturn = ReturnIndexOfFirstOperation(inputEquation, MathOperators.Addition, MathOperators.Subtraction)) >= 0) { }

            return toReturn;
        }

        /// <summary>
        /// Takes two operators of equal precedence and returns the index of the first occurence of either in
        /// the given equation. When two operators of equal precedence are in a equation, they are executed in left-to-right
        /// order. The second operator can be null if the search should only look for one operator type.
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <param name="mathOperator1">The first operator to search for</param>
        /// <param name="mathOperator2">The second operator to search for, or null if the search should only look for mathOperator1</param>
        /// <returns></returns>
        private static Int32 ReturnIndexOfFirstOperation(string inputEquation, MathOperators mathOperator1, MathOperators? mathOperator2)
        {
            Int32 toReturn = -1;

            // Get the index of the first operator
            Int32 operator1Index = inputEquation.IndexOf((Char)mathOperator1, GetOperatorSearchStartIndex(mathOperator1));

            // Get the index of the second operator, or -1 if it is null
            Int32 operator2Index = (mathOperator2 != null) ? inputEquation.IndexOf((Char)mathOperator2, GetOperatorSearchStartIndex(mathOperator2)) : -1;

            // If nither operator is found, we'll return -1
            if (operator1Index >= 0 || operator2Index >= 0)
            {
                // If both operators are found, return the index of the first one in the string
                if (operator1Index >= 0 && operator2Index >= 0)
                {
                    toReturn = Math.Min(operator1Index, operator2Index);
                }
                // If only the first operator is found, return its index
                else if (operator1Index >= 0)
                {
                    toReturn = operator1Index;
                }
                // If only the second operator is found, return its index
                else
                {
                    toReturn = operator2Index;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Function return the index where a search for a particular operator should start. Logs and Square Roots are
        /// the only operators that can start an equation (including sub-equations in parentheses). So, searches for other operators
        /// should begin from index 1 instead of index 0. This also avoids seeing the '-' in (-5*10) as a subtraction operation.
        /// </summary>
        /// <param name="mathOperator">The operator that will be searched for</param>
        /// <returns>The index of an equation where the search for this operator should begin</returns>
        private static Int32 GetOperatorSearchStartIndex(MathOperators? mathOperator)
        {
            Int32 toReturn = 0;

            if (mathOperator != null)
            {
                switch (mathOperator)
                {
                    case MathOperators.Exponentiation:
                    case MathOperators.Multiplication:
                    case MathOperators.Division:
                    case MathOperators.Addition:
                    case MathOperators.Subtraction:
                        toReturn = 1;
                        break;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Performs actual calculations for an operator at the index specified. For function operators like log() and
        /// sqrt(), the function will find the operand on the right and perform the calculation. For other operations, the
        /// function will find the operands on right AND left and perform the operation on them. The equation is then modified
        /// with the resulting value and returned.
        /// </summary>
        /// <param name="inputEquation">Equation to process</param>
        /// <param name="indexOfOperator">Index of the operator to process</param>
        /// <returns>Equation with selected operation processed</returns>
        private static string DoOperationAtIndex(string inputEquation, Int32 indexOfOperator)
        {
            if (indexOfOperator >= 0)
            {
                int ndxNumberAfterOperator;
                string stringAfterOperator;
                decimal numberAfterOperator, resultOfOperation = 0;

                // Get the operator at the specified index. Enum.Parse wants a string name of the enum value OR an string
                // of the Int value represented by the enum value, so we'll give it the latter by converting the operator
                // character to an int and then to a string
                MathOperators mathOperator = (MathOperators)Enum.Parse(typeof(MathOperators), ((Int32)inputEquation[indexOfOperator]).ToString());
                
                // Outer switch allows different operations to get different numbers of operands (only right, or  both right and left)
                switch (mathOperator)
                {
                    // These operations only need the right side operand
                    case MathOperators.SquareRoot:
                    case MathOperators.Logarithm:
                    case MathOperators.NaturalLogarithm:

                        // Get the right side operand
                        ndxNumberAfterOperator = FindEndOfNextOperand(inputEquation, indexOfOperator);
                        stringAfterOperator = inputEquation.Substring(indexOfOperator + 1, ndxNumberAfterOperator - indexOfOperator).Trim();

                        // Strip any units designators, collapse multiple signs if present (i.e. --5 to 5)
                        // and then parse the result as a Double. This is our numeric operand.
                        if(!Decimal.TryParse(CollapseMultipleSigns(regexSignedNumericalOperator.Match(stringAfterOperator).Value), out numberAfterOperator))
                        {
                            throw new GeneralMOQCalculationException("A value in the MOQ Equation is too large for processing.");
                        }
                            
                        // Inner switch to perform the operation on the right side operand
                        switch (mathOperator)
                        {
                            case MathOperators.SquareRoot:
                                if (numberAfterOperator < 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to take the square root of a negative number.");
                                }

                                resultOfOperation = TryConvertToDecimal(Math.Sqrt(Convert.ToDouble(numberAfterOperator)));

                                break;

                            case MathOperators.Logarithm:
                                if (numberAfterOperator == 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to take the log of zero.");
                                }

                                if (numberAfterOperator < 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to take the log of a negative number.");
                                }

                                resultOfOperation = TryConvertToDecimal(Math.Log10(Convert.ToDouble(numberAfterOperator)));

                                break;

                            case MathOperators.NaturalLogarithm:
                                if (numberAfterOperator == 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to take the natural log of zero.");
                                }

                                if (numberAfterOperator < 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to take the natural log of a negative number.");
                                }

                                resultOfOperation = TryConvertToDecimal(Math.Log(Convert.ToDouble(numberAfterOperator)));

                                break;
                        }

                        // Modify the original equation by replacing the operator and its operand with the operation result
                        inputEquation = inputEquation.Remove(indexOfOperator, ndxNumberAfterOperator - indexOfOperator + 1);
                        inputEquation = inputEquation.Insert(indexOfOperator, resultOfOperation.ToString());

                        break;

                    // These operations need both the left and right side operands
                    case MathOperators.Exponentiation:
                    case MathOperators.Multiplication:
                    case MathOperators.Division:
                    case MathOperators.Addition:
                    case MathOperators.Subtraction:

                        // Get both the right and left side operands
                        int ndxNumberBeforeOperator = FindStartOfPreviousOperand(inputEquation, indexOfOperator);
                        ndxNumberAfterOperator = FindEndOfNextOperand(inputEquation, indexOfOperator);

                        string stringBeforeOperator = inputEquation.Substring(ndxNumberBeforeOperator, indexOfOperator - ndxNumberBeforeOperator).Trim();
                        stringAfterOperator = inputEquation.Substring(indexOfOperator + 1, ndxNumberAfterOperator - indexOfOperator).Trim();

                        // Strip any units designators, collapse multiple signs if present (i.e. --5 to 5)
                        // and then parse the results as Doubles. These are our numeric  left and right 
                        // side operands.
                        decimal numberBeforeOperator;
                        if(!Decimal.TryParse(CollapseMultipleSigns(regexSignedNumericalOperator.Match(stringBeforeOperator).Value), out numberBeforeOperator) 
                            || !Decimal.TryParse(CollapseMultipleSigns(regexSignedNumericalOperator.Match(stringAfterOperator).Value), out numberAfterOperator))
                        {
                            throw new GeneralMOQCalculationException("A value in the MOQ Equation is too large for processing.");
                        }

                        // Inner switch to perform the operation on the right and left side operands
                        switch (mathOperator)
                        {
                            case MathOperators.Exponentiation:
                                if (numberBeforeOperator < 0 && (numberAfterOperator - Math.Truncate(numberAfterOperator)) > 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation attempted to raise a negative number to a power that is not a whole number.");
                                }

                                resultOfOperation = TryConvertToDecimal(Math.Pow(Convert.ToDouble(numberBeforeOperator), Convert.ToDouble(numberAfterOperator)));

                                break;

                            case MathOperators.Multiplication:
                                resultOfOperation = TryConvertToDecimal(Convert.ToDouble(numberBeforeOperator) * Convert.ToDouble(numberAfterOperator));

                                break;

                            case MathOperators.Division:
                                if (numberAfterOperator == 0)
                                {
                                    throw new GeneralMOQCalculationException("Calculation resulted in a division by zero.");
                                }

                                resultOfOperation = TryConvertToDecimal(Convert.ToDouble(numberBeforeOperator) / Convert.ToDouble(numberAfterOperator));

                                break;

                            case MathOperators.Addition:
                                resultOfOperation = TryConvertToDecimal(Convert.ToDouble(numberBeforeOperator) + Convert.ToDouble(numberAfterOperator));

                                break;

                            case MathOperators.Subtraction:
                                resultOfOperation = TryConvertToDecimal(Convert.ToDouble(numberBeforeOperator) - Convert.ToDouble(numberAfterOperator));

                                break;
                        }

                        // Modify the original equation by replacing the operator and its operands with the operation result
                        inputEquation = inputEquation.Remove(ndxNumberBeforeOperator, ndxNumberAfterOperator - ndxNumberBeforeOperator + 1);
                        inputEquation = inputEquation.Insert(ndxNumberBeforeOperator, resultOfOperation.ToString(FORMAT_Decimal));

                        break;
                }
            }

            // Return the modified original equation
            return inputEquation;
        }

        /// <summary>
        /// Checks that the double result isn't greater or less than the Decimal min/max values. If it is, it throws an exception. Otherwise, it converts to decimal
        /// </summary>
        /// <param name="result">Double to check</param>
        /// <returns>Converted decimal</returns>
        private static decimal TryConvertToDecimal(double result)
        {
            if (result >= Convert.ToDouble(Decimal.MaxValue) || result <= Convert.ToDouble(Decimal.MinValue))
            {
                throw new GeneralMOQCalculationException("Calculation resulted in a number too large for processing.");
            }

            return Convert.ToDecimal(result);
        }

        /// <summary>
        /// Takes an operator index and returns the beginning index of the operand to the left of it. Honors
        /// + and - signs on numbers by including them in the returned value (represented by the index).
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <param name="operatorIndex">Index of the operator to search to the left of</param>
        /// <returns>The beginning index of the operand to the left of the specified operator.</returns>
        private static Int32 FindStartOfPreviousOperand(string inputEquation, Int32 operatorIndex)
        {
            // Start the return value at zero. If there are no operators before our operatorIndex
            // then the entire string to the left of that index is the previous operand
            Int32 toReturn = 0;

            // Search right to left and find the index where the previous <sign(s)><number> begins
            Match match = regexSignedNumericalOperatorRightToLeft.Match(inputEquation.Substring(0, operatorIndex));

            // If we found a match, we need to check to make sure that the first sign in
            // that match isn't actually an operator
            if (match.Success)
            {
                // Start by setting the index to the beginning of the <sign(s)><number> found
                toReturn = match.Index;

                // If we're not already at the beginning of the input equation
                if (match.Index != 0)
                {
                    // If the character before the first sign in the match isn't an operator (excluding ')')
                    // then our first matched character is actually a + or - operator
                    if (!regexFirstCharacterOperator.IsMatch(inputEquation[match.Index - 1].ToString()))
                    {
                        // So we'll bump the index forward one so that we don't return an operator
                        // as part of the operand
                        toReturn = toReturn + 1;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Takes an operator index and returns the ending index of the operand to the right of it. Honors
        /// + and - signs on numbers by including them in the returned value (represented by the index).
        /// </summary>
        /// <param name="inputEquation">Equation to search</param>
        /// <param name="operatorIndex">Index of the operator to search to the right of</param>
        /// <returns>The end index of the operand to the right of the specified operator.</returns>
        private static Int32 FindEndOfNextOperand(string inputEquation, Int32 operatorIndex)
        {
            // Set the return value to the end of the equation. If no operators are found
            // then the entire string after the operatorIndex is the next operand
            Int32 toReturn = inputEquation.Length - 1;

            // Match the next instance of <sign(s)><number><any text><operator> after the given
            // operator index.
            Match match = regexEndOfNextOperand.Match(inputEquation.Substring(operatorIndex + 1));

            // If matched, the <sign(s><number><any text> part is our next operand to
            // return, so we'll return the index of the last character in that string.
            if (match.Success)
            {
                toReturn = operatorIndex + match.Length - 1;
            }

            return toReturn;
        }

        /// <summary>
        /// Takes a number with multiple +/- signs on the front and collapses those signs into
        /// a single sign. An odd number or -'s means that the number is negative, while an
        /// even number, or zero, -'s means that it is positive.
        /// </summary>
        /// <param name="operand">The full operand with signs included</param>
        /// <returns>The operand with a single '-' sign or no sign for a positive number</returns>
        private static string CollapseMultipleSigns(string operand)
        {
            // If the operand only has a single sign, or no signs, bomb out and return
            // the original operand string
            if ((regexPositiveSigns.Matches(operand).Count + regexNegativeSigns.Matches(operand).Count) <= 1)
            {
                return operand;
            }

            String toReturn = "";

            // If the operand has an odd number of -'s, then we'll append a minus
            // sign to the result string.
            if (regexNegativeSigns.Matches(operand).Count % 2 == 1)
            {
                toReturn += "-";
            }

            // Append the number portion of the operand to the result string
            toReturn += regexUnsignedNumericalOperator.Match(operand).Value;

            return toReturn;
        }

        /// <summary>
        /// Creates a REGEX string to match all valid characters in an equation
        /// </summary>
        /// <returns>A regex string containing all valid characters</returns>
        private static string AllValidCharactersRegexString
        {
            get
            {
                return "\\d" + AllValidNonDigitCharactersRegexString;
            }
        }

        /// <summary>
        /// Creates a REGEX string to match all valid characters in an equation, excluding digits
        /// </summary>
        /// <returns>A regex string containing all valid non-numeric characters</returns>
        private static string AllValidNonDigitCharactersRegexString
        {
            get
            {
                if (string.IsNullOrEmpty(_allValidNonDigitCharactersRegexString))
                {
                    // Adds nonoperational valid characters
                    string toReturn = 
                        " " +
                        "a-z" +
                        "A-Z";

                    // Add number decorators
                    toReturn +=
                    "\\" + (char)NumberDecorators.Decimal +
                    (char)NumberDecorators.Percentage +
                        (char)NumberDecorators.Comma;

                    toReturn += OperatorsRegexString;

                    _allValidNonDigitCharactersRegexString = toReturn;
                }

                return _allValidNonDigitCharactersRegexString;
            }
        }

        /// <summary>
        /// Creates a REGEX string to match all operators that cannot be located at the start of
        /// the equation
        /// </summary>
        /// <returns>A regex string containing all MOQ equation operators that cannot start
        /// an equation</returns>
        private static string InvalidStartOperatorsString
        {
            get
            {
                if (string.IsNullOrEmpty(_invalidStartOperatorsString))
                {
                    // Adds operators to the return string
                    string toReturn = ((Char)MathOperators.Exponentiation).ToString() +
                            ((Char)MathOperators.Multiplication).ToString() +
                            ((Char)MathOperators.Division).ToString();

                    // Escapes operators that are used as special characters in regex to ensure that
                    // they are used as literals in our regex functions.
                    toReturn = toReturn.Replace(((Char)MathOperators.Multiplication).ToString(), "\\" + ((Char)MathOperators.Multiplication).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Exponentiation).ToString(), "\\" + ((Char)MathOperators.Exponentiation).ToString());
                    _invalidStartOperatorsString = toReturn;
                }

                return _invalidStartOperatorsString;
            }
        }
        
        /// <summary>
        /// Creates a REGEX string to match all operators that cannot be located on the end of
        /// the equation
        /// </summary>
        /// <returns>A regex string containing all MOQ equation operators that cannot end
        /// an equation</returns>
        private static string InvalidEndOperatorsString
        {
            get
            {
                if (string.IsNullOrEmpty(_invalidEndOperatorsString))
                {
                    // Adds operators and parenthesis to the return string
                    string toReturn = string.Join("", ((MathOperators[])Enum.GetValues(typeof(MathOperators))).Select(enumValue => (Char)enumValue).ToArray());

                    // Escapes operators that are used as special characters in regex to ensure that
                    // they are used as literals in our regex functions.
                    toReturn = toReturn.Replace(((Char)MathOperators.Addition).ToString(), "\\" + ((Char)MathOperators.Addition).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Subtraction).ToString(), "\\" + ((Char)MathOperators.Subtraction).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Multiplication).ToString(), "\\" + ((Char)MathOperators.Multiplication).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Exponentiation).ToString(), "\\" + ((Char)MathOperators.Exponentiation).ToString());

                    _invalidEndOperatorsString = toReturn;
                }

                return _invalidEndOperatorsString;
            }
        }

        /// <summary>
        /// Creates a REGEX string to match all operators that can be located on either side of an
        /// operand in a MOQ equation. Escapes operators that are used in regex to ensure that
        /// they are used as literals in regex functions.
        /// </summary>
        /// <returns>A regex string containing all MOQ equation operators</returns>
        private static string OperatorsRegexString
        {
            get
            {
                if (string.IsNullOrEmpty(_operatorsRegexString))
                {
                    // Adds operators and parenthesis to the return string
                    string toReturn =
                        string.Join("", ((MathOperators[])Enum.GetValues(typeof(MathOperators))).Select(enumValue => (Char)enumValue).ToArray()) +
                        string.Join("", ((Parenthesis[])Enum.GetValues(typeof(Parenthesis))).Select(enumValue => (Char)enumValue).ToArray());

                    // Escapes operators that are used as special characters in regex to ensure that
                    // they are used as literals in our regex functions.
                    toReturn = toReturn.Replace(((Char)MathOperators.Addition).ToString(), "\\" + ((Char)MathOperators.Addition).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Subtraction).ToString(), "\\" + ((Char)MathOperators.Subtraction).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Multiplication).ToString(), "\\" + ((Char)MathOperators.Multiplication).ToString());
                    toReturn = toReturn.Replace(((Char)MathOperators.Exponentiation).ToString(), "\\" + ((Char)MathOperators.Exponentiation).ToString());
                    toReturn = toReturn.Replace(((Char)Parenthesis.Open).ToString(), "\\" + ((Char)Parenthesis.Open).ToString());
                    toReturn = toReturn.Replace(((Char)Parenthesis.Close).ToString(), "\\" + ((Char)Parenthesis.Close).ToString());

                    _operatorsRegexString = toReturn;
                }
                return _operatorsRegexString;
            }
        }
    }

    /// <summary>
    /// Collection of operators. Doing a (Char)value on one of these values will give you the
    /// single character representation of that operation.
    /// </summary>
    public enum MathOperators
    {
        Logarithm = '∟',        // Custom log character for ease of parsing
        NaturalLogarithm = '⌐', // Custom natural log character for ease of parsing
        SquareRoot = '#',
        Exponentiation = '^',
        Multiplication = '*',
        Division = '/',
        Addition = '+',
        Subtraction = '-'
    }

    /// <summary>
    /// Values used to decorate a number, such as decimal point and percent sign. Doing a
    /// (Char)value on one of these values will give you the character representation itself.
    /// single character representation
    /// </summary>
    public enum NumberDecorators
    {
        Decimal = '.',
        Percentage = '%',
        Comma = ','
    }

    /// <summary>
    /// Signs that indicate positive or negative values. Doing a (Char)value on one of these
    /// values will give you the character representation itself.
    /// </summary>
    public enum NumberSigns
    {
        Positive = '+',
        Negative = '-'
    }

    /// <summary>
    /// Parentheses values.  Doing a (Char)value on one of these values will give you the
    /// character representation iteself.
    /// </summary>
    public enum Parenthesis
    {
        Open = '(',
        Close = ')'
    }

    /// <summary>
    /// Simple class to represent a parentheses block in an equation with indexes for where
    /// the block opens and closes.
    /// </summary>
    public class ParenthesisPair
    {
        /// <summary>
        /// Index of the opening parentheses
        /// </summary>
        public int OpenIndex { get; set; }

        /// <summary>
        /// Index of the closing parentheses
        /// </summary>
        public int CloseIndex { get; set; }

        /// <summary>
        /// Constructs a ParenthesesPair by taking open and closed indexes for the parentheses
        /// </summary>
        /// <param name="openIndex">Index of the opening parentheses</param>
        /// <param name="closeIndex">Index of the closing parentheses</param>
        public ParenthesisPair(int openIndex, int closeIndex)
        {
            this.OpenIndex = openIndex;
            this.CloseIndex = closeIndex;
        }

    }
}
