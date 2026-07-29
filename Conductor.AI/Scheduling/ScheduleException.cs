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

namespace Conductor.AI.Scheduling;

public class ScheduleException : Exception
{
    public ScheduleException(string message) : base(message) { }
    public ScheduleException(string message, Exception inner) : base(message, inner) { }
}

public sealed class ScheduleNameConflict : ScheduleException
{
    public ScheduleNameConflict(string message) : base(message) { }
}

public sealed class ScheduleNotFound : ScheduleException
{
    public ScheduleNotFound(string message) : base(message) { }
}

public sealed class InvalidCronExpression : ScheduleException
{
    public InvalidCronExpression(string message) : base(message) { }
}
