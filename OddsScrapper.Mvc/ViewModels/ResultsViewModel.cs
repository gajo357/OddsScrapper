using OddsScrapper.Shared.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OddsScrapper.Mvc.ViewModels
{
    public class ResultsViewModel
    {
        private static ResultsViewModel _instance;
        public static ResultsViewModel Instance => _instance ?? (_instance = new ResultsViewModel());

        private ResultsViewModel()
        {

        }

        public IList<GameDto> Games { get; } = new List<GameDto>();

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        public bool SaveGamesToLocalFile { get; set; }

        public string FileName { get; set; }
        
        public void SelectFileForSave()
        {

        }

        public async Task DownloadResultsAsync()
        {
            await Task.Delay(1000);
        }
    }
}
