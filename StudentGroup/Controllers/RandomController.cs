using Microsoft.AspNetCore.Mvc;

namespace StudentGroup.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RandomController: Controller
    {
        [HttpGet]
        public string GetRandomStudent()
        {
            var students = new List<string> { "Alice", "Bob", "Charlie", "David", "Eve" };
            var random = new Random();
            int index = random.Next(students.Count);
            return students[index];
        }
    }
}
