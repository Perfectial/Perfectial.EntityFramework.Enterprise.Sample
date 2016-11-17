namespace Perfectial.Application.Model
{
    using System.Collections.Generic;

    public class GetUsersOutput : IOutputDto
    {
        public List<UserDto> Users { get; set; }
    }
}