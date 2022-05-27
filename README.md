# SolutionStepParser
A command line solver that parses and executes user-defined linear and nonlinear solution steps, that is 'equations' and 'systems of equations', returning the values of all outputted variables as a solution. It was created to form the backend of a GUI-based solution program.

INPUT
A set of files each containing a set of equations, a file defining input variables, and an optional dictionary file giving alternate names to variables for clarification. Equations that are not within a system together are treated as their own solution step (held within an EquationStep class). Systems of equations are grouped within their own file, and a custom class is created that implements the ISolutionStep interface and solves the system. 

STEP ORDERING AND EXECUTION
SolutionSteps are contained within a Solution class. The steps are ordered according to their inputs. The equations that will be ordered at the top of the execution queue are dependent on input variables only. As those initial equations are added to the queue, their output variables ("intermediate terms"), are available to the "intermediate steps". This continues either until all SolutionSteps are deemed solvable (i.e. their inputs are available) or if any equation is found unsolvable.

Execution is a matter of popping the solution step queue and running the Solve() method on each until the queue is empty. Every SolutionStep output is written to a file along with its name (or alias, if provided by the dictionary) for examination.

The parsing and calculation of equations is aided by the JACE computation engine, complements of Pieter Derycke. 

CUSTOMIZATION AND FLEXIBILITY
The solver allows you to edit equations via text files without changing the program. While systems of equations currently require the programming of a custom class to solve them, the equations within the system can be edited as long as it does not invalidate their custom solution method. 

Custom functions can be added to the JACE engine, and are written in C# code. This allows for vastly complex functions, such as calls to library functions, system utilities, and synchronous HTTP requests.

FEATURE WISHLIST
1) The ability to provide a single file of equations and allow the program to determine the 'cyclical' components (i.e. system of equations (SoE) that reference each other's variables), determines the appropriate approximation method (if any) to resolve them, and then creates a SolutionStep for you. It would prompt you to complete certain parameters of the step, such as the error tolerance.
2) Visualization, where the solution set is represented as a directed graph. SoE should be identified as circular node paths.
3) Create a scoring function that guides the user or an AI search function in adjusting the inputs to achieve a desired result. The scoring function can include any variable in the solution (inputs, intermediate terms, outputs). For automated search, the program can be told to minimize or maximize this score, in addition to discovering a variety of local minima or maxima for user consideration.
