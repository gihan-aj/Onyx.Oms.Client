using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Products;

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
