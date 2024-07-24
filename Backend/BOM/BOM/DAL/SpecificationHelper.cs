using BOM.DAL;
using BOM.Models;
using Microsoft.EntityFrameworkCore;

namespace KISMisha.DAL
{
    public class SpecificationHelper
    {
        private readonly ApplicationContext _db;
        public SpecificationHelper(ApplicationContext context)
        {
            _db = context;
        }

        public async Task<List<Specification>> GetParentsTree(int? id, bool isDelete)
        {
            var itemsDal = new List<Specification>();
            if (id == null)
            {
                itemsDal = await _db.Specification.ToListAsync();
            }
            else
            {
                itemsDal = await _db.Specification.Where(x => x.PositionId == id).ToListAsync();
            }

            var needItems = new List<Specification>();
            while (itemsDal.Count != 0)
            {
                var copyParents = new List<Specification>(itemsDal);
                itemsDal.Clear();
                foreach (var parentItem in copyParents)
                {
                    var childItem = await _db.Specification.FirstOrDefaultAsync(x => x.PositionId == parentItem.ParentsId);
                    if (childItem == null) continue;
                    itemsDal.Add(childItem);
                    needItems.Add(childItem);
                }
            }

            if (id != null && isDelete)
            {
                var mainItem = await _db.Specification.FirstAsync(x => x.PositionId == needItems[0].ParentsId);
                needItems.Insert(0, mainItem);
                needItems.Reverse();
                foreach (var item in needItems)
                {
                    var deletedItem = await _db.Specification.FirstOrDefaultAsync(x => x.PositionId == item.PositionId);
                    _db.Specification.Remove(deletedItem);
                    await _db.SaveChangesAsync();
                }
                _db.Specification.Remove(mainItem);
                await _db.SaveChangesAsync();
                return needItems;
            }


            return needItems;
        }
    }

}
