﻿/*
 * Copyright 2010-2013 Bastian Eicher
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

using System.Collections.Generic;
using System.Linq;
using Common.Collections;
using Common.Tasks;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Solvers.Backtracking
{
    /// <summary>
    /// Holds state during a single <see cref="BacktrackingSolver.Solve"/> run.
    /// </summary>
    internal sealed class BacktrackingRun : SolverRun
    {
        /// <inheritdoc/>
        public BacktrackingRun(Requirements requirements, CancellationToken cancellationToken, Config config, IFeedManager feedManager, IStore store) : base(requirements, cancellationToken, config, feedManager, store)
        {}

        /// <inheritdoc/>
        public override bool TryToSolve()
        {
            return TryToSolve(TopLevelRequirements);
        }

        /// <summary>
        /// Try to satisfy a set of <paramref name="requirements"/>, respecting any existing <see cref="SolverRun.Selections"/>.
        /// </summary>
        private bool TryToSolve(Requirements requirements)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var allCandidates = GetSortedCandidates(requirements);
            var suitableCandidates = FilterSuitableCandidates(allCandidates, requirements.InterfaceID);

            // Stop if specific implementation already selected elsewhere in tree
            if (Selections.ContainsImplementation(requirements.InterfaceID))
                return suitableCandidates.Contains(Selections[requirements.InterfaceID].ID);

            return TryToSelectCandidate(suitableCandidates, requirements, allCandidates);
        }

        /// <summary>
        /// A running list of <see cref="Restriction"/>s from all <see cref="SelectionCandidate"/>s added to <see cref="Selections"/> so far.
        /// </summary>
        private readonly List<Restriction> _restrictions = new List<Restriction>();

        private IEnumerable<SelectionCandidate> FilterSuitableCandidates(IEnumerable<SelectionCandidate> candidates, string interfaceID)
        {
            return candidates.Where(candidate =>
                candidate.IsSuitable &&
                !ConflictsWithExistingRestrictions(interfaceID, candidate) &&
                !ConflictsWithExistingSelections(candidate));
        }

        private bool ConflictsWithExistingRestrictions(string interfaceID, SelectionCandidate candidate)
        {
            return _restrictions.Any(restriction =>
                restriction.Interface == interfaceID && !restriction.EffectiveVersions.Match(candidate.Version));
        }

        private bool ConflictsWithExistingSelections(SelectionCandidate candidate)
        {
            return candidate.Implementation.Restrictions.Any(restriction =>
                Selections.ContainsImplementation(restriction.Interface) && !restriction.EffectiveVersions.Match(Selections[restriction.Interface].Version));
        }

        private bool TryToSelectCandidate(IEnumerable<SelectionCandidate> candidates, Requirements requirements, IList<SelectionCandidate> allCandidates)
        {
            foreach (var candidate in candidates)
            {
                AddToSelections(candidate, requirements, allCandidates);
                if (TryToSolveCommand(candidate.Implementation.GetCommand(requirements.Command)) &&
                    TryToSolveDependencies(candidate.Implementation.Dependencies))
                    return true;
                else RemoveFromSelections(candidate);
            }
            return false;
        }

        private bool TryToSolveDependencies(IEnumerable<Dependency> dependencies)
        {
            var dependencyRequirements = dependencies.Select(dependency => dependency.ToRequirements(TopLevelRequirements));
            return dependencyRequirements.All(TryToSolve);
        }

        private bool TryToSolveCommand(Command command)
        {
            if (command == null) return true;
            if (command.Runner != null)
                if (!TryToSolve(command.Runner.ToRequirements(TopLevelRequirements))) return false;

            return TryToSolveDependencies(command.Dependencies);
        }

        private void AddToSelections(SelectionCandidate candidate, Requirements requirements, IEnumerable<SelectionCandidate> allCandidates)
        {
            Selections.Implementations.Add(candidate.ToSelection(allCandidates, requirements));
            _restrictions.AddRange(candidate.Implementation.Restrictions);
        }

        private void RemoveFromSelections(SelectionCandidate candidate)
        {
            Selections.Implementations.RemoveLast();
            _restrictions.RemoveLast(candidate.Implementation.Restrictions.Count);
        }
    }
}