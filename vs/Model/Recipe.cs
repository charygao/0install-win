﻿/*
 * Copyright 2010 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A recipe is a list of <see cref="RecipeStep"/>s used to create an <see cref="Implementation"/> directory.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("recipe", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Recipe : RetrievalMethod, IEquatable<Recipe>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<RecipeStep> _steps = new C5.ArrayList<RecipeStep>();
        /// <summary>
        /// An ordered list of <see cref="RecipeStep"/>s to execute.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlElement("archive", typeof(Archive))] // Note: explicit naming of XML tag can be removed once other RecipeStep types have been added
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<RecipeStep> Steps { get { return _steps; } }
        #endregion
        
        //--------------------//

        #region Simplify
        /// <summary>
        /// Call <see cref="ISimplifyable.Simplify"/> on all contained <see cref="RecipeStep"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Feed"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            foreach (var step in Steps) step.Simplify();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "Archive: Location (MimeType, Size + StartOffset) => Extract". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Recipe: {0} Archives", Steps.Count);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Recipe"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Recipe"/>.</returns>
        public override RetrievalMethod CloneRetrievalMethod()
        {
            var recipe = new Recipe();
            foreach (var step in Steps)
                recipe.Steps.Add(step.CloneRecipeStep());

            return recipe;
        }
        #endregion
        
        #region Equality
        public bool Equals(Recipe other)
        {
            if (ReferenceEquals(null, other)) return false;

            return base.Equals(other) && Steps.SequencedEquals(other.Steps);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Recipe) && Equals((Recipe)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Steps.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
