using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hairdresser.Api.Models.ViewModels;

public class ServiceViewModel
{
    public int Id { get; set; }

    [Required]
    public string ServiceName { get; set; } = null!;

    [Required]
    public int DurationMinutes { get; set; }

    [Required]
    public decimal Price { get; set; }

    // Çoklu seçilebilecek çalışanlar
    public List<int> SelectedWorkerIds { get; set; } = new();

    public IEnumerable<SelectListItem> AllWorkers { get; set; } = new List<SelectListItem>();
}