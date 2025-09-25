using app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace app.Repository
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(string id);
        Task<Post> CreateAsync(Post post);
        Task<Post?> UpdateAsync(Post post);
        Task<Post?> DeleteAsync(string id);
    }
}