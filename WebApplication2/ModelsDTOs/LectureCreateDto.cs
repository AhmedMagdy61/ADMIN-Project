﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication2.ModelsDTOs
{
    public class LectureCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Lecture Loction is required.")]

        public IFormFile LecturePDF { get; set; } 
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }
        [Required(ErrorMessage = "Admin ID is required.")]
        public int AdminId { get; set; }
    }
}
