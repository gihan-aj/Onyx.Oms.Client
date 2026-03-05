using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IProductCategoryLookupApi
{
    [Get("/api/v1/product-categories/search")]
    Task<PagedResult<ProductCategoryDto>> SearchCategories(
        [AliasAs("Page")] int page, 
        [AliasAs("PageSize")] int pageSize, 
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null, 
        [AliasAs("SortOrder")] string? sortOrder = null,
        [AliasAs("IsActive")] bool? isActive = null,
        [AliasAs("IsValidParent")] bool? isValidParent = null,
        [AliasAs("IsLeafOnly")] bool? isLeafOnly = null);

    [Get("/api/v1/product-categories/{id}")]
    Task<ProductCategoryResponse> GetCategoryById(Guid id, [AliasAs("includeParentSpecs")] bool includeParentSpecs = true);
}

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public string NamePath { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ProductCategoryResponse : ProductCategoryDto
{
    public List<SpecDefinition> Specifications { get; set; } = new();
    public List<SpecDefinition> AllSpecifications { get; set; } = new();
}

public class SpecDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public SpecType Type { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
}

public enum SpecType
{
    Text,
    Number,
    Select,
    MultiSelect,
    Toggle,
    Date
}