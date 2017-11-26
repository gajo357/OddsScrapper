using Microsoft.AspNetCore.Http;
using OddsScrapper.Shared.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OddsScrapper.Mvc.ViewModels
{
    public class ResultsViewModel
    {
        public ResultsViewModel()
        {

        }

        public IList<GameDto> Games { get; } = new List<GameDto>();

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Save to file")]
        public bool SaveGamesToLocalFile { get; set; }
        [Display(Name = "Select local file"), FileExtensions(Extensions = "csv", ErrorMessage = "Specify a CSV file. (Comma-separated values)")]
        public IFormFile File { get; set; }

        public bool IsDownloading { get; set; }

        public async Task DownloadResultsAsync()
        {
            IsDownloading = true;
            await Task.Delay(1000);
            IsDownloading = false;
        }
    }
}
