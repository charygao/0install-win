﻿/*
 * Copyright 2006-2011 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Threading;
using Common.Tasks;

namespace Common
{
    /// <summary>
    /// Pretends to perform a task for testing purposes
    /// </summary>
    public class MockTask : TaskBase
    {
        #region Variables
        /// <summary>Indicates that <see cref="MockStateComplete"/> has been called.</summary>
        private readonly EventWaitHandle _joinWait = new EventWaitHandle(false, EventResetMode.AutoReset);
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "MockTask"; } }

        /// <inheritdoc/>
        public override bool CanCancel { get { return false; } }
        #endregion

        #region Control
        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Started"/> and <see cref="ITask.BytesProcessed"/> to <code>128</code>.
        /// </summary>
        public override void Start()
        {
            State = TaskState.Started;
            BytesTotal = 128;
        }

        /// <summary>
        /// Blocks until <see cref="MockStateComplete"/> is called.
        /// </summary>
        public override void Join()
        {
            _joinWait.WaitOne();
        }

        /// <inheritdoc/>
        public override void Cancel()
        {
            throw new NotSupportedException("Task can not be canceled.");
        }
        #endregion

        //--------------------//

        #region Mock state
        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Data"/> and <see cref="ITask.BytesProcessed"/> to <code>64</code>.
        /// </summary>
        public void MockStateData()
        {
            State = TaskState.Data;
            BytesProcessed = 64;
        }

        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Complete"/> and unlocks <see cref="Join"/>.
        /// </summary>
        public void MockStateComplete()
        {
            State = TaskState.Complete;
            _joinWait.Set();
        }
        #endregion
    }
}
