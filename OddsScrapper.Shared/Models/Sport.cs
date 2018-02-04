﻿using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Shared.Models
{
    public class Sport
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
