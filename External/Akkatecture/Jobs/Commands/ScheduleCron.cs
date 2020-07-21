// The MIT License (MIT)
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture 
// 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using Cronos;
using JetBrains.Annotations;

namespace Akkatecture.Jobs.Commands
{
    [PublicAPI]
    public sealed class ScheduleCron<TJob, TIdentity> : Schedule<TJob, TIdentity>
        where TJob : IJob
        where TIdentity : IJobId
    {
        private readonly CronExpression _expression;

        public string CronExpression { get; }

        public ScheduleCron(
            TIdentity jobId,
            TJob job,
            string cronExpression,
            DateTime triggerDate,
            object? ack = null,
            object? nack = null)
            : base(jobId, job, triggerDate, ack, nack)
        {
            if (string.IsNullOrWhiteSpace(cronExpression)) throw new ArgumentNullException(nameof(cronExpression));

            CronExpression = cronExpression;
            _expression = Cronos.CronExpression.Parse(cronExpression);
        }

        public override Schedule<TJob, TIdentity>? WithNextTriggerDate(DateTime utcDate)
        {
            var next = _expression.GetNextOccurrence(utcDate);
            return next.HasValue ? new ScheduleCron<TJob, TIdentity>(JobId, Job, CronExpression, next.Value) : null;
        }

        public override Schedule<TJob, TIdentity> WithAck(object ack) => new ScheduleCron<TJob, TIdentity>(JobId, Job, CronExpression, TriggerDate, ack, Nack);

        public override Schedule<TJob, TIdentity> WithNack(object nack) => new ScheduleCron<TJob, TIdentity>(JobId, Job, CronExpression, TriggerDate, Ack, nack);
    }
}