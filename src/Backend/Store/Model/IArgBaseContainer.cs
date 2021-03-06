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

using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// An object that contains <see cref="ArgBase"/>s.
    /// </summary>
    public interface IArgBaseContainer
    {
        /// <summary>
        /// A list of command-line arguments to be passed to an executable.
        /// </summary>
        [XmlElement(typeof(Arg)), XmlElement(typeof(ForEachArgs))]
        [NotNull]
        List<ArgBase> Arguments { get; }
    }
}
