﻿using System.Collections.Generic;

namespace OddsWebsite.Models
{
    public interface IArchiveDataRepository
    {
        IEnumerable<League> GetAllLeagues();
    }
}