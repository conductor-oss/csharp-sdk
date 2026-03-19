/*
 * Copyright 2024 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using System;

namespace Conductor.Client.Worker
{
    /// <summary>
    /// Throw this from a worker's Execute method to mark the task as FAILED_WITH_TERMINAL_ERROR,
    /// preventing any further retries. Use for unrecoverable failures such as invalid input,
    /// missing configuration, or business rule violations that cannot succeed on retry.
    /// </summary>
    public class NonRetryableException : Exception
    {
        public NonRetryableException(string message) : base(message) { }
        public NonRetryableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
