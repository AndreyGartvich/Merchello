﻿namespace Merchello.Web.Search
{
    using System;
    using System.Linq;

    using Merchello.Core;
    using Merchello.Core.Persistence.Querying;
    using Merchello.Web.Models;
    using Merchello.Web.Models.VirtualContent;

    /// <summary>
    /// Represents a product filter query.
    /// </summary>
    internal class ProductContentQuery : ICmsContentQuery<IProductContent>
    {
        /// <summary>
        /// The <see cref="ICachedProductQuery"/>.
        /// </summary>
        private readonly ICachedProductQuery _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductContentQuery"/> class.
        /// </summary>
        /// <param name="query">
        /// The <see cref="ICachedProductQuery"/>.
        /// </param>
        public ProductContentQuery(ICachedProductQuery query)
        {
            Ensure.ParameterNotNull(query, "The CachedProductQuery cannot be null");
            _query = query;
        }

        /// <summary>
        /// Gets a value indicating whether has the query has a search term.
        /// </summary>
        public bool HasSearchTerm
        {
            get
            {
                return !this.SearchTerm.IsNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public long Page { get; set; }

        /// <summary>
        /// Gets or sets the items per page.
        /// </summary>
        public long ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the sort by field.
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        public SortDirection SortDirection { get; set; }

        /// <summary>
        /// Gets or sets the collection keys.
        /// </summary>
        public Guid[] CollectionKeys { get; set; }

        /// <summary>
        /// Gets or sets a setting for specifying how the query should treat collection clusivity in specified collections and filters.
        /// </summary>
        public CollectionClusivity CollectionClusivity { get; set; }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <returns>
        /// The <see cref="PagedCollection"/>.
        /// </returns>
        public PagedCollection<IProductContent> Execute()
        {
            var hasCollections = CollectionKeys != null && CollectionKeys.Any();


            if (!hasCollections)
            {
                return HasSearchTerm
                  ? _query.TypedProductContentSearchPaged(SearchTerm, Page, ItemsPerPage, SortBy, SortDirection)
                  : _query.TypedProductContentSearchPaged(Page, ItemsPerPage, SortBy, SortDirection);
            }

            switch (this.CollectionClusivity)
            {
                case CollectionClusivity.DoesNotExistInAnyCollectionsAndFilters:

                    return HasSearchTerm ?
                        _query.TypedProductContentPageThatNotInAnyCollections(CollectionKeys, SearchTerm, Page, ItemsPerPage, SortBy, SortDirection) :
                        _query.TypedProductContentPageThatNotInAnyCollections(CollectionKeys, Page, ItemsPerPage, SortBy, SortDirection);

                case CollectionClusivity.ExistsInAnyCollectionOrFilter:

                    return HasSearchTerm ?
                        _query.TypedProductContentPageThatExistsInAnyCollections(CollectionKeys, SearchTerm, Page, ItemsPerPage, SortBy, SortDirection) :
                        _query.TypedProductContentPageThatExistsInAnyCollections(CollectionKeys, Page, ItemsPerPage, SortBy, SortDirection);

                case CollectionClusivity.ExistsInAllCollectionsAndFilters:
                default:
                    return HasSearchTerm ?
                        _query.TypedProductContentPageThatExistInAllCollections(CollectionKeys, SearchTerm, Page, ItemsPerPage, SortBy, SortDirection) :
                        _query.TypedProductContentPageThatExistInAllCollections(CollectionKeys, Page, ItemsPerPage, SortBy, SortDirection);
            }
        }
    }
}