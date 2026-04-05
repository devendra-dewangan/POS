using POS.Entity;
using POS.Repos;

namespace POS.Services
{
    public class BuyerService : IBuyerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BuyerService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public async Task<Buyer> AddBuyerAsync(string name)
        {
            var buyer = new Buyer
            {
                Name = name
            };

            await _unitOfWork.Buyers.AddAsync(buyer);
            await _unitOfWork.CommitAsync();
            return buyer;
        }

        public async Task<Buyer?> GetBuyerByNameAsync(string name)
        {
            var buyers = await _unitOfWork.Buyers.GetByNameAsync(name);

            return buyers?.FirstOrDefault();
        }

        public async Task<IEnumerable<Buyer>> GetAllBuyersAsync()
        {
            return await _unitOfWork.Buyers.GetAllAsync() ?? [];
        }
    }
}