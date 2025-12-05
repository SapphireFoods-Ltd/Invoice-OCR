using Vendor_OCR.Repositories;

namespace Vendor_OCR.Services
{
    public interface IUserService
    {
        Task<bool> AuthenticateUserAsync(string userId, string password);
    }

    public class UserService : IUserService
    {
        private readonly VendorRepository _vendorRepository;

        public UserService(VendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public async Task<bool> AuthenticateUserAsync(string userId, string password)
        {

            return await _vendorRepository.ValidateUserAsync(userId, password);
        }
    }
}
