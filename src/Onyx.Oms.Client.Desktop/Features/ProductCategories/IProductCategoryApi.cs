using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

// DTOs matching the API Reference docs
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

public class ProductCategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Using IEnumerable or List for nested children
    public List<ProductCategoryTreeDto> SubCategories { get; set; } = new();

    // UI Helper properties that could be populated in ViewModel if needed
    // However, since TreeView binds directly, having them here or wrapping them is a choice.
    // For simplicity, we add standard UI helpers.
    public bool CanEdit { get; set; }
    public bool CanAddSubcategory => Level < 3; // Business rule max depth 2
    public bool CanDelete { get; set; }
    public bool CanToggleStatus { get; set; }
    
    public string IconGlyph 
    {
        get
        {
            if (string.IsNullOrEmpty(IconUrl)) return "\xF07B"; // Default to FontAwesome Folder icon
            
            // Handle if user typed "&#xE8D5;"
            if (IconUrl.StartsWith("&#x", StringComparison.OrdinalIgnoreCase) && IconUrl.EndsWith(";"))
            {
                var hex = IconUrl.Substring(3, IconUrl.Length - 4);
                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var codePoint))
                {
                    return char.ConvertFromUtf32(codePoint);
                }
            }
            
            // Handle if user typed "\uE8D5"
            if (IconUrl.StartsWith("\\u", StringComparison.OrdinalIgnoreCase) && IconUrl.Length >= 6)
            {
                var hex = IconUrl.Substring(2, 4);
                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var codePoint))
                {
                    return char.ConvertFromUtf32(codePoint);
                }
            }
            
            // Handle if they just typed the hex code (e.g. "E8D5")
            if (IconUrl.Length == 4 && int.TryParse(IconUrl, System.Globalization.NumberStyles.HexNumber, null, out var cp))
            {
                return char.ConvertFromUtf32(cp);
            }

            return IconUrl;
        }
    }
    public Microsoft.UI.Xaml.Media.SolidColorBrush IconColorBrush 
    {
        get
        {
            if (!string.IsNullOrEmpty(Color) && Color.StartsWith("#") && (Color.Length == 7 || Color.Length == 9))
            {
                try
                {
                    // Convert Hex to Brush. If it fails, fallback.
                    var a = (byte)255;
                    var offset = 1;
                    if (Color.Length == 9)
                    {
                        a = Convert.ToByte(Color.Substring(1, 2), 16);
                        offset = 3;
                    }
                    var r = Convert.ToByte(Color.Substring(offset, 2), 16);
                    var g = Convert.ToByte(Color.Substring(offset + 2, 2), 16);
                    var b = Convert.ToByte(Color.Substring(offset + 4, 2), 16);
                    return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
                }
                catch { }
            }
            // Fallback to Accent/System color or just Gray if invalid
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
        }
    }
    
    public string NamePath { get; set; } = string.Empty; 
}

public class CreateProductCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public List<SpecDefinition> Specifications { get; set; } = new();
}

public class UpdateProductCategoryDto : CreateProductCategoryDto
{
    public Guid Id { get; set; }
}

public enum SpecType
{
    Text = 0,
    Number = 1,
    Select = 2,
    MultiSelect = 3,
    Toggle = 4,
    Date = 5
}

public class SpecDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public SpecType Type { get; set; } = SpecType.Text;
    public bool IsRequired { get; set; } = false;
    public List<string> Options { get; set; } = new();
}

public record ProductCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int Level,
    string Path,
    string NamePath,
    bool IsActive,
    int DisplayOrder,
    string? IconUrl,
    string? Color,
    bool HasProducts,
    List<SpecDefinition> Specifications
);

public interface IProductCategoryApi
{
    [Get("/api/v1/product-categories")]
    Task<List<ProductCategoryDto>> GetCategories([AliasAs("onlyLeaves")] bool? onlyLeaves = null, [AliasAs("isActive")] bool? isActive = null);

    [Get("/api/v1/product-categories/search")]
    Task<PagedResult<ProductCategoryDto>> SearchCategories(
        int page, 
        int pageSize, 
        [AliasAs("searchTerm")] string? searchTerm = null, 
        [AliasAs("isActive")] bool? isActive = null,
        [AliasAs("sortColumn")] string? sortColumn = null,
        [AliasAs("sortOrder")] string? sortOrder = null,
        [AliasAs("isValidParent")] bool? isValidParent = null,
        [AliasAs("isLeafOnly")] bool? isLeafOnly = null);

    [Get("/api/v1/product-categories/tree")]
    Task<List<ProductCategoryTreeDto>> GetCategoryTree([AliasAs("isActive")] bool? isActive = null);

    [Get("/api/v1/product-categories/{id}")]
    Task<ProductCategoryResponse> GetCategoryById(Guid id);

    [Post("/api/v1/product-categories")]
    Task<Guid> CreateCategory([Body] CreateProductCategoryDto dto);

    [Put("/api/v1/product-categories/{id}")]
    Task UpdateCategory(Guid id, [Body] UpdateProductCategoryDto dto);

    [Delete("/api/v1/product-categories/{id}")]
    Task DeleteCategory(Guid id);

    [Put("/api/v1/product-categories/{id}/activate")]
    Task ActivateCategory(Guid id);

    [Put("/api/v1/product-categories/{id}/deactivate")]
    Task DeactivateCategory(Guid id);
}
