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

using System.Collections.Generic;
using System.Linq;
using Akka.Persistence.Journal;
using Akkatecture.Core;
using Akkatecture.Extensions;
using JetBrains.Annotations;
using Tauron;

namespace Akkatecture.Events
{
    [PublicAPI]
    public class AggregateEventTagger : IWriteEventAdapter
    {
        public string Manifest(object evt) => string.Empty;

        public object ToJournal(object evt)
        {
            try
            {
                var aggregateName = evt
                   .GetType()
                   .GetCommittedEventAggregateRootName();

                var eventDefinitionService = new EventDefinitionService(null);

                var aggregateEventType = evt
                   .GetType()
                   .GetCommittedEventAggregateEventType();

                eventDefinitionService.Load(aggregateEventType);
                var eventDefinition = eventDefinitionService.GetDefinition(aggregateEventType);

                var tags = new HashSet<string> {aggregateName.Value, eventDefinition.Name};
                evt.GetType().GetAllCustomAttributes<TagAttribute>().Select(a => a.Name).Foreach(s => tags.Add(s));

                return new Tagged(evt, tags);
            }
            catch
            {
                return evt;
            }
        }
    }
}