using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.AccessControl
{
    public class UpdateUserAccessRequest
    {
        [Required(ErrorMessage = "Поле role обязательно.")]
        [RegularExpression("^(user|manager|admin)$", ErrorMessage =
      "Роль должна быть одной из: user, manager, admin.")]
        public required string Role { get; set; }

        public bool IsActive { get; set; }
    }
}