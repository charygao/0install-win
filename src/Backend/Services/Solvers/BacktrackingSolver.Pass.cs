﻿/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    partial class BacktrackingSolver
    {
        /// <summary>
        /// Holds state during a single pass of the <see cref="BacktrackingSolver"/>.
        /// </summary>
        private class Pass
        {
            #region Depdendencies
            private CancellationToken _cancellationToken;
            private readonly SelectionCandidateProvider _candidateProvider;

            /// <summary>
            /// Creates a new backtracking solver run.
            /// </summary>
            /// <param name="requirements">The top-level requirements the solver should try to meet.</param>
            /// <param name="cancellationToken">Used to signal when the user wishes to cancel the solver run.</param>
            /// <param name="candidateProvider">Generates <see cref="SelectionCandidate"/>s for the solver to choose among.</param>
            public Pass([NotNull] Requirements requirements, CancellationToken cancellationToken, [NotNull] SelectionCandidateProvider candidateProvider)
            {
                #region Sanity checks
                if (requirements == null) throw new ArgumentNullException(nameof(requirements));
                #endregion

                _cancellationToken = cancellationToken;
                _candidateProvider = candidateProvider;

                _topLevelRequirements = requirements;
                Selections.InterfaceUri = requirements.InterfaceUri;
                Selections.Command = requirements.Command;
            }
            #endregion

            private readonly Requirements _topLevelRequirements;

            /// <summary>
            /// The implementations selected by the solver run.
            /// </summary>
            [NotNull]
            public Selections Selections { get; } = new Selections();

            /// <summary>
            /// Try to satisfy the <see cref="_topLevelRequirements"/>. If successful the result can be retrieved from <see cref="Selections"/>.
            /// </summary>
            /// <returns><c>true</c> if a solution was found; <c>false</c> otherwise.</returns>
            public bool TryToSolve()
            {
                return TryToSolve(_topLevelRequirements);
            }

            /// <summary>
            /// Try to satisfy a set of <paramref name="requirements"/>, respecting any existing <see cref="Selections"/>.
            /// </summary>
            private bool TryToSolve([NotNull] Requirements requirements)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var allCandidates = _candidateProvider.GetSortedCandidates(requirements);
                var suitableCandidates = FilterSuitableCandidates(allCandidates, requirements.InterfaceUri);
                var existingSelection = Selections.GetImplementation(requirements.InterfaceUri);

                if (existingSelection == null) return TryToSelectCandidate(suitableCandidates, requirements, allCandidates);
                else if (TryToUseExistingCandidate(requirements, suitableCandidates, existingSelection)) return true;
                else throw new SolverException("Dependency graph too complex");
            }

            /// <summary>
            /// A running list of <see cref="Restriction"/>s from all <see cref="SelectionCandidate"/>s added to <see cref="Selections"/> so far.
            /// </summary>
            private readonly List<Restriction> _restrictions = new List<Restriction>();

            private IEnumerable<SelectionCandidate> FilterSuitableCandidates([NotNull, ItemNotNull] IEnumerable<SelectionCandidate> candidates, [NotNull] FeedUri interfaceUri)
            {
                return candidates.Where(candidate =>
                    candidate.IsSuitable &&
                    !ConflictsWithExistingRestrictions(candidate, interfaceUri) &&
                    !ConflictsWithExistingSelections(candidate));
            }

            private bool ConflictsWithExistingRestrictions([NotNull] SelectionCandidate candidate, [NotNull] FeedUri interfaceUri)
            {
                var nativeImplementation = candidate.Implementation as ExternalImplementation;

                foreach (var restriction in _restrictions.Where(x => x.InterfaceUri == interfaceUri))
                {
                    if (restriction.Versions != null && !restriction.Versions.Match(candidate.Version)) return true;
                    if (nativeImplementation != null && !restriction.Distributions.ContainsOrEmpty(nativeImplementation.Distribution)) return true;
                }
                return false;
            }

            private bool ConflictsWithExistingSelections([NotNull] SelectionCandidate candidate)
            {
                var nativeImplementation = candidate.Implementation as ExternalImplementation;

                foreach (var restriction in candidate.Implementation.Restrictions)
                {
                    var existingSelection = Selections.GetImplementation(restriction.InterfaceUri);
                    if (existingSelection != null)
                    {
                        if (restriction.Versions != null && !restriction.Versions.Match(existingSelection.Version)) return true;
                        if (nativeImplementation != null && !restriction.Distributions.ContainsOrEmpty(nativeImplementation.Distribution)) return true;
                    }
                }

                return false;
            }

            private bool TryToUseExistingCandidate([NotNull] Requirements requirements, [NotNull, ItemNotNull] IEnumerable<SelectionCandidate> suitableCandidates, [NotNull] ImplementationSelection selection)
            {
                if (!suitableCandidates.Contains(selection)) return false;
                if (selection.ContainsCommand(requirements.Command)) return true;

                var command = selection.AddCommand(requirements, from: _candidateProvider.LookupOriginalImplementation(selection));
                return TryToSolveCommand(command, requirements);
            }

            private bool TryToSelectCandidate([NotNull, ItemNotNull] IEnumerable<SelectionCandidate> candidates, [NotNull] Requirements requirements, [NotNull, ItemNotNull] IList<SelectionCandidate> allCandidates)
            {
                foreach (var candidate in candidates)
                {
                    var selection = AddToSelections(candidate, requirements, allCandidates);

                    Debug.Assert(requirements.Command != null);
                    var command = selection[requirements.Command];
                    if (TryToSolveDependencies(selection) && TryToSolveCommand(command, requirements) && TryToSolveBindingRequirements(selection)) return true;
                    else RemoveLastFromSelections();
                }
                return false;
            }

            private bool TryToSolveBindingRequirements([NotNull] IInterfaceUriBindingContainer selection)
            {
                return selection.ToBindingRequirements(selection.InterfaceUri).All(TryToSolve);
            }

            private bool TryToSolveDependencies([NotNull] IDependencyContainer dependencyContainer)
            {
                var dependencies = dependencyContainer.Dependencies;

                var essentialDependencies = new List<Dependency>();
                var recommendedDependencies = new List<Dependency>();
                dependencies.Bucketize(x => x.Importance)
                    .Add(Importance.Essential, essentialDependencies)
                    .Add(Importance.Recommended, recommendedDependencies)
                    .Run();

                foreach (var dependency in essentialDependencies)
                    if (!TryToSolveDependency(dependency)) return false;
                foreach (var dependency in recommendedDependencies)
                    if (!TryToSolveDependency(dependency)) dependencies.Remove(dependency);
                return true;
            }

            private bool TryToSolveDependency([NotNull] Dependency dependency)
            {
                return TryToSolve(dependency.ToRequirements(_topLevelRequirements)) && TryToSolveBindingRequirements(dependency);
            }

            private bool TryToSolveCommand([CanBeNull] Command command, [NotNull] Requirements requirements)
            {
                if (command == null) return true;

                if (command.Bindings.OfType<ExecutableInBinding>().Any()) throw new SolverException("<executable-in-*> not supported in <command>");

                if (command.Runner != null)
                    if (!TryToSolve(command.Runner.ToRequirements(_topLevelRequirements))) return false;

                return command.ToBindingRequirements(requirements.InterfaceUri).All(TryToSolve) && TryToSolveDependencies(command);
            }

            private ImplementationSelection AddToSelections([NotNull] SelectionCandidate candidate, [NotNull] Requirements requirements, [NotNull, ItemNotNull] IEnumerable<SelectionCandidate> allCandidates)
            {
                var selection = candidate.ToSelection(allCandidates, requirements);
                Selections.Implementations.Add(selection);
                _restrictions.AddRange(selection.Restrictions);
                return selection;
            }

            private void RemoveLastFromSelections()
            {
                _restrictions.RemoveLast(Selections.Implementations.Last().Restrictions.Count);
                Selections.Implementations.RemoveLast();
            }
        }
    }
}
