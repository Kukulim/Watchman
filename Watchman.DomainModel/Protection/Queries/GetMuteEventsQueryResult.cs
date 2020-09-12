﻿using System.Collections.Generic;
using Watchman.Cqrs;

namespace Watchman.DomainModel.Protection.Queries
{
    public class GetMuteEventsQueryResult : IQueryResult
    {
        public IEnumerable<MuteEvent> MuteEvents { get; }

        public GetMuteEventsQueryResult(IEnumerable<MuteEvent> muteEvents)
        {
            this.MuteEvents = muteEvents;
        }
    }
}