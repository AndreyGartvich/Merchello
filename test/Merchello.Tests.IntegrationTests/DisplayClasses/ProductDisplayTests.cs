﻿using System;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using Merchello.Core.Models;
using Merchello.Examine.Providers;
using Merchello.Tests.Base.DataMakers;
using Merchello.Tests.IntegrationTests.Services;
using Merchello.Web.Models.ContentEditing;
using NUnit.Framework;

namespace Merchello.Tests.IntegrationTests.DisplayClasses
{
    [TestFixture]
    public class ProductDisplayTests : ServiceIntegrationTestBase
    {
        private Guid _productKey;
        private Guid _productVariantKey;
        private int _examineId;
        private IWarehouse _warehouse;

        [SetUp]
        public void Init()
        {

            var warehouseService = PreTestDataWorker.WarehouseService;
            _warehouse = warehouseService.GetDefaultWarehouse();

            var productVariantService = PreTestDataWorker.ProductVariantService;
            var productService = PreTestDataWorker.ProductService;

            var product = MockProductDataMaker.MockProductCollectionForInserting(1).First();
            product.ProductOptions.Add(new ProductOption("Color"));
            product.ProductOptions.First(x => x.Name == "Color").Choices.Add(new ProductAttribute("Blue", "Blue"));
            product.ProductOptions.First(x => x.Name == "Color").Choices.Add(new ProductAttribute("Red", "Red"));
            product.ProductOptions.First(x => x.Name == "Color").Choices.Add(new ProductAttribute("Green", "Green"));
            product.ProductOptions.Add(new ProductOption("Size"));
            product.ProductOptions.First(x => x.Name == "Size").Choices.Add(new ProductAttribute("Small", "Sm"));
            product.ProductOptions.First(x => x.Name == "Size").Choices.Add(new ProductAttribute("Medium", "Med"));
            product.ProductOptions.First(x => x.Name == "Size").Choices.Add(new ProductAttribute("Large", "Lg"));
            product.ProductOptions.First(x => x.Name == "Size").Choices.Add(new ProductAttribute("X-Large", "XL"));
            product.Height = 11M;
            product.Width = 11M;
            product.Length = 11M;
            product.CostOfGoods = 15M;
            product.OnSale = true;
            product.SalePrice = 18M;
            productService.Save(product);

            _productKey = product.Key;

            var attributes = new ProductAttributeCollection()
            {
                product.ProductOptions.First(x => x.Name == "Color").Choices.First(x => x.Sku == "Blue"),
                product.ProductOptions.First(x => x.Name == "Size").Choices.First(x => x.Sku == "XL" )
            };

            var variant = productVariantService.CreateProductVariantWithKey(product, attributes);
            variant.AddToCatalogInventory(_warehouse.DefaultCatalog());
            productVariantService.Save(variant);
            _productVariantKey = variant.Key;
            _examineId = ((ProductVariant) variant).ExamineId;

            var provider = (ProductIndexer)ExamineManager.Instance.IndexProviderCollection["MerchelloProductIndexer"];
            provider.AddProductToIndex(product);
        }

        [Test]
        public void Can_Build_ProductVariantDisplay_From_Indexed_ProductVariant()
        {
            //// Arrange
            var searcher = ExamineManager.Instance.SearchProviderCollection["MerchelloProductSearcher"];
            var criteria = searcher.CreateSearchCriteria("productvariant", BooleanOperation.And);
            criteria.Id(_examineId);
            
            var result = searcher.Search(criteria).FirstOrDefault();

            //// Act
            var productVariantDisplay = result.ToProductVariantDisplay();

            //// Assert
            Assert.NotNull(productVariantDisplay);
            Assert.IsTrue(2 == productVariantDisplay.Attributes.Count());
            Assert.IsTrue(productVariantDisplay.CatalogInventories.Any());
        }

        [Test]
        public void Can_Build_ProductDisplay_From_Indexed_Product()
        {
            //// Arrange
            var searcher = ExamineManager.Instance.SearchProviderCollection["MerchelloProductSearcher"];
            var criteria = searcher.CreateSearchCriteria("productvariant", BooleanOperation.And);
            criteria.Field("productKey", _productKey.ToString()).And().Field("master", "True");

            var result = searcher.Search(criteria).FirstOrDefault();

            //// Act
            var product = result.ToProductDisplay();

            //// Assert
            Assert.NotNull(product);
        }


    }
}