﻿using Aiursoft.BaGet.Core.Metadata;
using Aiursoft.BaGet.Protocol.Models;

namespace Aiursoft.BaGet.Core.Search
{
    public interface ISearchResponseBuilder
    {
        SearchResponse BuildSearch(IReadOnlyList<PackageRegistration> results);
        AutocompleteResponse BuildAutocomplete(IReadOnlyList<string> data);
        DependentsResponse BuildDependents(IReadOnlyList<PackageDependent> results);
    }
}
