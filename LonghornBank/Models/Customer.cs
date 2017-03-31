﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LonghornBank.Models
{
    public class Customer
    {
        public Int32 CustomerID { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "A First Name is Required")]
        public String FName { get; set; }

        [Required(ErrorMessage = "A Last Name is Required")]
        [Display(Name = "Last Name")]
        public String LName { get; set; }

        [Display(Name = "Middle Initial")]
        public Char? MiddleInitial { get; set; }

        [Required(ErrorMessage = "A Street Address is Required")]
        [Display(Name = "Street Address")]
        public String StreetAddress { get; set; }
        [Required(ErrorMessage = "City is Required")]
        [Display(Name = "City")]
        public String City { get; set; }
        [Required(ErrorMessage = "State is Required")]
        [Display(Name = "State")]
        public String State { get; set; }

        [Display(Name = "Zip Code")]
        [Required(ErrorMessage = "Zip Code is Required")]
        public String Zip { get; set; }

        [Required(ErrorMessage = "Email Address is Required")]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public String EmailAddress { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [Required(ErrorMessage = "Phone Number is Required")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public String PhoneNumber { get; set; }

        [Required(ErrorMessage = "DOB is Required")]
        [Display(Name = "Birthdate")]
        public DateTime DOB { get; set; }

        
        [Display(Name = "Active Status")]
        [Required(ErrorMessage = "An Active Status is Required")]
        public Boolean ActiveStatus { get; set; }
    }
}