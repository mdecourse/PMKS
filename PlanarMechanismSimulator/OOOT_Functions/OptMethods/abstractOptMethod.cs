﻿/*************************************************************************
 *     This file & class is part of the Object-Oriented Optimization
 *     Toolbox (or OOOT) Project
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     OOOT is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General internal License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     OOOT is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General internal License for more details.
 *  
 *     You should have received a copy of the GNU General internal License
 *     along with OOOT.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on OOOT
 *     at http://ooot.codeplex.com/.
 *************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using StarMathLib;

namespace OptimizationToolbox
{
    /// <summary>
    /// The main class that all optimization methods inherit from.
    /// </summary>
    internal abstract partial class abstractOptMethod
    {
        #region Fields

        private const double sameTolerance = 0.00001;
        private const double defaultFiniteDifferenceStepSize = 0.1;
        private const differentiate defaultFiniteDifferenceMode = differentiate.Central2;

        /* I usually object to such simple names for variables, but this 
         * follows the convention used in my course - ME392C at UT Austin. */
        /// <summary>
        /// Gets or sets the number of decision variables.
        /// </summary>
        /// <value>The number of decision variables, n.</value>
        internal int n { get; private set; }/* the total number of design variables - the length of x. */

        /// <summary>
        /// Gets the iteration count (can be set only by an optimization method).
        /// </summary>
        /// <value>The iteration count, k.</value>
        internal int k { get; set; }

        /// <summary>
        /// Gets the value of the optimum for single objective problems (can be set only by an optimization method).
        /// </summary>
        /// <value>The value of the optimum, fstar.</value>
        internal double fStar { get; set; } /*fStar is the optimum that is returned at the end of run. */
        /* 'active' is the set of Active Constraints. For simplicity all equality constraints 
         * are assumed to be active, and any additional g's that come and go in this active
         * set strategy. More importantly we want the gradient of A which is a m by n matrix. 
         * m is the # of active constraints and n is the # of variables. */
        /// <summary>
        /// Gets the search dir method.
        /// Such objects must inherit from the abstractSearchDirection class.
        /// </summary>
        /// <value>The search dir method.</value>
        internal abstractSearchDirection searchDirMethod { get; private set; }
        /// <summary>
        /// Gets the line search method.
        /// Such objects must inherit from the abstractLineSearch class.
        /// </summary>
        /// <value>The line search method.</value>
        internal abstractLineSearch lineSearchMethod { get; private set; }
        /// <summary>
        /// Gets the list of convergence methods.
        /// All objects in list inherit from the abstractConvergence class.
        /// </summary>
        /// <value>The convergence methods.</value>
        internal List<abstractConvergence> ConvergenceMethods { get; private set; }

        /* The following Booleans should be set in the constructor of every optimization method. 
         * Even if it seems redundant to do so, it is better to have them clearly indicated for each
         * method. */
        /// <summary>
        /// Gets or sets a value indicating whether [requires objective function].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires objective function]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresObjectiveFunction { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [constraints solved with penalties].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [constraints solved with penalties]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean ConstraintsSolvedWithPenalties { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires merit function].
        /// 
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires merit function]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresMeritFunction
        {
            get
            {
                return (ConstraintsSolvedWithPenalties || _requiresMeritFunction);
            }
            set
            {
                _requiresMeritFunction = value;
            }
        }
        /* In most cases RequiresMeritFunction is the same as ConstraintsSolvedWithPenalties, so 
         * RequiresMeritFunction is rarely set. The only time it is needed is when the method
         * sometimes uses the penalty and sometimes does not. This is only the case in SQP and GRG.*/
        private Boolean _requiresMeritFunction;
        /// <summary>
        /// Gets or sets a value indicating whether [inequalities converted to equalities].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [inequalities converted to equalities]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean InequalitiesConvertedToEqualities { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires search direction method].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires search direction method]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresSearchDirectionMethod { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires line search method].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires line search method]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresLineSearchMethod { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires an initial point].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires an initial point]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresAnInitialPoint { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires convergence criteria].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires convergence criteria]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresConvergenceCriteria { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires feasible start point].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires feasible start point]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresFeasibleStartPoint { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [requires discrete space descriptor].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [requires discrete space descriptor]; otherwise, <c>false</c>.
        /// </value>
        internal Boolean RequiresDiscreteSpaceDescriptor { get; set; }

        /// <summary>
        /// Gets the merit/penalty function method used by the optimization.
        /// This object inherits from the abstractMeritFunction class.
        /// </summary>
        /// <value>The merit function.</value>
        internal abstractMeritFunction meritFunction { get; private set; }
        /// <summary>
        /// Gets the initial or start x candidate.
        /// </summary>
        /// <value>The x start.</value>
        internal double[] xStart { get; set; }
        /// <summary>
        /// Gets the running value of x.
        /// </summary>
        /// <value>The x.</value>
        internal double[] x { get; set; }

        /// <summary>
        /// Gets or sets the feasible outer loop max.
        /// </summary>
        /// <value>The feasible outer loop max.</value>
        internal int feasibleOuterLoopMax { get; set; }
        /// <summary>
        /// Gets or sets the feasible inner loop max.
        /// </summary>
        /// <value>The feasible inner loop max.</value>
        internal int feasibleInnerLoopMax { get; set; }

        #endregion

        #region Set-up function, Add.

        /// <summary>
        /// Initializes a new instance of the <see cref="abstractOptMethod"/> class.
        /// </summary>
        protected abstractOptMethod()
        {
            fStar = double.PositiveInfinity;
            ConvergenceMethods = new List<abstractConvergence>();
            g = new List<IInequality>();
            h = new List<IEquality>();
            active = new List<IConstraint>();
            f = new List<IObjectiveFunction>();
        }

        /// <summary>
        /// Adds the specified object to the optimization routine.
        /// </summary>
        /// <param name="function">The object, function.</param>
        internal virtual void Add(object function)
        {

            if (function is IOptFunction)
            {
                if (function is IInequality)
                    g.Add((IInequality)function);
                else if (function is IEquality)
                    h.Add((IEquality)function);
                else if (function is IObjectiveFunction)
                    f.Add((IObjectiveFunction)function);
            }
            else if (function is IDependentAnalysis)
                dependentAnalysis = (IDependentAnalysis)function;
            else if (function is abstractLineSearch)
            {
                lineSearchMethod = (abstractLineSearch)function;
                lineSearchMethod.SetOptimizationDetails(this);
            }
            else if (function is abstractSearchDirection)
                searchDirMethod = (abstractSearchDirection)function;
            else if (function is abstractMeritFunction)
                meritFunction = (abstractMeritFunction)function;
            else if (function is abstractConvergence)
                if (ConvergenceMethods.Any(a => a.GetType() == function.GetType()))
                    throw new Exception("You cannot add a convergence method of type " + function.GetType() +
                                        "to the optimization method since one already exists of this same type.");
                else ConvergenceMethods.Add((abstractConvergence)function);
            else if (function is double[])
                xStart = (double[])function;

            else
                throw (new Exception("Function, " + function + ", not of known type (needs "
                                     + "to inherit from inequality, equality, objectiveFunction, abstractLineSearch, " +
                                     "or abstractSearchDirection)."));
        }
        #endregion

        #region Initialize and Run funtions

        /// <summary>
        /// Runs the optimization process and returns the optimal as xStar 
        /// and the value of fStar is return by the function.
        /// </summary>
        /// <param name="xStar">The optimizer, xStar.</param>
        /// <returns>optimal value, fStar</returns>
        internal double Run(out double[] xStar)
        {
            if (xStart != null) return Run(out xStar, xStart);
            return run(out xStar, null);
        }

        /// <summary>
        /// Runs the optimization process from the specified xInit and
        /// returns the optimal as xStar and the value of fStar is return by the function.
        /// </summary>
        /// <param name="xStar">The optimizer, xStar.</param>
        /// <param name="xInit">The initial or start point, xInit.</param>
        /// <returns>optimal value, fStar</returns>
        internal double Run(out double[] xStar, double[] xInit)
        {
            n = xInit.GetLength(0);
            return run(out xStar, xInit);
        }

        /// <summary>
        /// Runs the optimization process with the specified number of variables and
        /// returns the optimal as xStar and the value of fStar is return by the function.
        /// </summary>
        /// <param name="xStar">The optimizer, xStar.</param>
        /// <param name="NumberOfVariables">The number of variables.</param>
        /// <returns>optimal value, fStar</returns>
        internal double Run(out double[] xStar, int NumberOfVariables)
        {
            n = NumberOfVariables;
            return run(out xStar, null);
        }

        private double run(out double[] xStar, double[] xInit)
        {
            xStar = null;
            fStar = double.PositiveInfinity;
            // k = 0 --> iteration counter
            k = 0;

            if (RequiresObjectiveFunction && (f.Count == 0))
            {
                SearchIO.output("No objective function specified.", 0);
                return fStar;
            }
            if (RequiresSearchDirectionMethod && (searchDirMethod == null))
            {
                SearchIO.output("No search direction specified.", 0);
                return fStar;
            }
            if (RequiresLineSearchMethod && (lineSearchMethod == null))
            {
                SearchIO.output("No line search method specified.", 0);
                return fStar;
            }
            if (RequiresConvergenceCriteria && ConvergenceMethods.Count == 0)
            {
                SearchIO.output("No convergence method specified.", 0);
                return fStar;
            }
            if (RequiresMeritFunction && (g.Count + h.Count > 0))
            {
                if (meritFunction == null)
                {
                    SearchIO.output("No merit function specified.", 0);
                    return fStar;
                }
                else SearchIO.output("Constraints will be solved with penalty function.", 4);
            }
            if (g.Count == 0) SearchIO.output("No inequalities specified.", 4);
            if (h.Count == 0) SearchIO.output("No equalities specified.", 4);
            if (InequalitiesConvertedToEqualities && (g.Count > 0))
                SearchIO.output(g.Count + " inequality constsraints will be converted to equality" +
                                " constraints with the addition of " + g.Count + " slack variables.", 4);

            if (RequiresAnInitialPoint)
            {
                if (xInit != null)
                {
                    xStart = (double[])xInit.Clone();
                    x = (double[])xInit.Clone();
                }
                else if (xStart != null) x = (double[])xStart.Clone();
                else
                {
                    // no? need a random start
                    x = new double[n];
                    var randy = new Random();
                    for (var i = 0; i < n; i++)
                        x[i] = 100.0 * randy.NextDouble();

                }
                if (RequiresFeasibleStartPoint && !feasible(x))
                    if (!findFeasibleStartPoint()) return fStar;
            }
            p = h.Count;
            q = g.Count;
            m = p;

            if (n <= m)
            {
                if (n == m)
                    SearchIO.output("There are as many equality constraints as design variables " +
                                    "(m = size). Consider another approach. Optimization is not needed.");
                else
                    SearchIO.output("There are more equality constraints than design variables " +
                                    "(m > size). Therefore the problem is overconstrained.");
                return fStar;
            }

            return run(out xStar);
        }


        /// <summary>
        /// Runs the specified optimization method. This includes the details
        /// of the optimization method.
        /// </summary>
        /// <param name="xStar">The x star.</param>
        /// <returns></returns>
        protected abstract double run(out double[] xStar);


        private Boolean findFeasibleStartPoint()
        {
            var average = StarMath.norm1(x) / x.GetLength(0);
            var randNum = new Random();
            // n-m variables can be changed

            //double[,] gradH = calc_h_gradient(x);

            for (var outerK = 0; outerK < feasibleOuterLoopMax; outerK++)
            {
                SearchIO.output("looking for feasible start point (attempt #" + outerK, 4);
                for (var innerK = 0; innerK < feasibleOuterLoopMax; innerK++)
                {
                    // gradA = calc_h_gradient(x, varsToChange);
                    // invGradH = StarMath.inverse(gradA);
                    //x = StarMath.subtract(x, StarMath.multiply(invGradH, calc_h_vector(x)));
                    if (feasible(x)) return true;
                }
                for (var i = 0; i < n; i++)
                    x[i] += 2 * average * (randNum.NextDouble() - 0.5);

                //gradA = calc_h_gradient(x);
                //invGradH = StarMath.inverse(gradA);
                if (feasible(x)) return true;
            }
            return false;
        }

        #endregion

#if withProblemDefinition
        #region from/to Problem Definition

        private void readInProblemDefinition(ProblemDefinition pd)
        {
            foreach (var f0 in pd.f) Add(f0);
            foreach (var ineq in pd.g) Add(ineq);
            foreach (var eq in pd.h) Add(eq);
            NumConvergeCriteriaNeeded = pd.NumConvergeCriteriaNeeded;
            if (pd.ConvergenceMethods != null)
                foreach (var cM in pd.ConvergenceMethods)
                    Add(cM);
            if (pd.SpaceDescriptor != null) Add(pd.SpaceDescriptor);
            if ((pd.xStart != null) && (pd.xStart.GetLength(0) > 0))
            {
                xStart = (double[])pd.xStart.Clone();
                n = xStart.GetLength(0);
            }
        }

        /// <summary>
        /// Creates a problem definition object from the details loaded in the
        /// optimization routine. For use in saving the data to XML, etc.
        /// </summary>
        /// <returns>the problem definition</returns>
        internal ProblemDefinition createProblemDefinition()
        {
            var pd = new ProblemDefinition
                         {
                             ConvergenceMethods = ConvergenceMethods,
                             xStart = xStart,
                             NumConvergeCriteriaNeeded = NumConvergeCriteriaNeeded,
                         };
            foreach (IObjectiveFunction f0 in f)
                pd.f.Add(f0);
            foreach (IEquality eq in h)
                pd.h.Add(eq);
            foreach (IInequality ineq in g)
                if (ineq.GetType() == typeof(lessThanConstant))
                {
                    var ub = ((lessThanConstant)ineq).constant;
                    var varIndex = ((lessThanConstant)ineq).index;
                    pd.SpaceDescriptor[varIndex].UpperBound = ub;
                }
                else if (ineq.GetType() == typeof(greaterThanConstant))
                {
                    var lb = ((greaterThanConstant)ineq).constant;
                    var varIndex = ((greaterThanConstant)ineq).index;
                    pd.SpaceDescriptor[varIndex].UpperBound = lb;
                }
                else pd.g.Add(ineq);
            return pd;
        }

        #endregion
#endif
        #region Convergence Main Function

        private int numConvergeCriteriaNeeded = 1;

        /// <summary>
        /// Gets or sets the num convergence criteria needed to stop the process.
        /// </summary>
        /// <value>The num converge criteria needed.</value>
        internal int NumConvergeCriteriaNeeded
        {
            get { return Math.Min(ConvergenceMethods.Count, numConvergeCriteriaNeeded); }
            set { numConvergeCriteriaNeeded = value; }
        }

        /// <summary>
        /// Gets the criteria that declared convergence.
        /// </summary>
        /// <value>The convergence declared by.</value>
        internal List<abstractConvergence> ConvergenceDeclaredBy { get; private set; }
        /// <summary>
        /// Gets the convergence methods as a single (CSV) string of types.
        /// </summary>
        /// <value>The convergence declared by type string.</value>
        internal string ConvergenceDeclaredByTypeString
        {
            get
            {
                if (ConvergenceDeclaredBy == null) return "";
                string result = ConvergenceDeclaredBy.Aggregate("", (current, p) => current + (", " + p.GetType().ToString()));
                result = result.Remove(0, 2);
                return result.Replace("OptimizationToolbox.", "");
            }
        }

        /// <summary>
        /// Returns true is the process has not converged.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="numFnEvals">The num fn evals.</param>
        /// <param name="fBest">The f best.</param>
        /// <param name="xBest">The x best.</param>
        /// <param name="population">The population.</param>
        /// <param name="gradF">The grad F.</param>
        /// <returns></returns>
        protected Boolean notConverged(long iteration = -1, long numFnEvals = -1, double fBest = double.NaN,
                                          IList<double> xBest = null, IList<double[]> population = null,
            IList<double> gradF = null)
        {
            var trueIndices = new List<int>();
            int i = 0;
            while (trueIndices.Count < NumConvergeCriteriaNeeded)
            {
                if (i == ConvergenceMethods.Count) return true;
                if (ConvergenceMethods[i].converged(iteration, numFnEvals, fBest,
                                                      xBest, population, gradF))
                    trueIndices.Add(i);
                i++;
            }
            ConvergenceDeclaredBy = new List<abstractConvergence>
                    (trueIndices.Select(index => ConvergenceMethods[index]));
            return false;
        }

        #endregion

    }
}