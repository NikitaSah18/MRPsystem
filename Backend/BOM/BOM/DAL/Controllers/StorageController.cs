using BOM.DAL;
using BOM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BOM
{

    [ApiController]
    [Route("Storage")]
    public class StorageController : ControllerBase
    {
        ApplicationContext db;
        public StorageController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Storage>>> Get()
        {
            return await db.Storage.ToListAsync();
        }


        [HttpGet("{Id}")]
        public async Task<ActionResult<LinkedList<Storage>>> GetSpecStorage(int id)
        {
            Storage specStorage = await db.Storage.FirstOrDefaultAsync(x => x.Id == id);
            if (specStorage == null)
                return NotFound();
            return new ObjectResult(specStorage);
        }



        [HttpPost]
        public async Task<ActionResult<Storage>> Post(Storage specStorage)
        {
            if (specStorage == null)
            {
                return BadRequest();
            }

            db.Storage.Add(specStorage);
            await db.SaveChangesAsync();
            return Ok(specStorage);
        }


        [HttpPut("{Id}")]
        public async Task<ActionResult<Storage>> Put(Storage specStorage)
        {
            if (specStorage == null)
            {
                return BadRequest();
            }
            if (!db.Storage.Any(x => x.Id == specStorage.Id))
            {
                return NotFound();
            }

            db.Update(specStorage);
            await db.SaveChangesAsync();
            return Ok(specStorage);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Storage>> Delete([Required] int Id)
        {
            var order = await db.Storage.FirstOrDefaultAsync(x => Id == x.Id);
            if (order == null)
            {
                return NotFound();
            }
            db.Storage.Remove(order);
            await db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpGet("GetTotalCountByDate")]
        public async Task<string> GetTotalCountByDate(DateTime date)
        {
            try
            {
                var storageItems = await db.Storage
                    .Where(x => x.StorageDate.Date <= date.Date)
                    .GroupBy(x => x.SpecificationId)
                    .Select(g => new
                    {
                        SpecificationId = g.Key,
                        TotalCount = g.Sum(x => x.Count ?? 0),
                        Description = db.Specification
                            .Where(s => s.PositionId == g.Key)
                            .Select(s => s.Description)
                            .FirstOrDefault()
                    })
                    .ToListAsync();
                string json = JsonConvert.SerializeObject(new
                {
                    StorageItems = storageItems
                });

                return json;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("GetConnectedNodes")]
        public async Task<ActionResult<List<Specification>>> GetConnectedNodes(string dateDecomposition)
        {
            var orders = await db.Storage.Where(x => DateTime.Parse(dateDecomposition) >= x.StorageDate).ToListAsync();
            if (orders == null) return new List<Specification>();

            var connectedNodes = new List<Specification>();
            var totalNodes = 0;
            var result = new SpecificationDto
            {
                Nodes = new List<Specification>(),
                TotalNodes = 0,
                TotalWeight = 0
            };

            foreach (var order in orders)
            {
                var id = order.SpecificationId;
                var totalWeight = await CalculateWeight(id.Value);
                await GetLeafNodes(id.Value, connectedNodes, totalNodes, 1, (double)order.Count); 

                result.Nodes.AddRange(connectedNodes);
                result.TotalNodes += totalNodes;
                result.TotalWeight += totalWeight;
                connectedNodes.Clear();
            }

            return Ok(result);
        }

        private async Task GetLeafNodes(int parentId, List<Specification> connectedNodes, int totalNodes, int parentQuantity, double count)
        {
            var children = await db.Specification.Where(s => s.ParentsId == parentId).ToListAsync();
            if (children.Count == 0)
            {
                var node = new Specification
                {
                    PositionId = parentId,
                    Description = "Нижний узел",
                    QuantityPerParent = (int?)(parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1) * count)
                };
                connectedNodes.Add(node);
                totalNodes++;
            }
            else
            {
                foreach (var child in children)
                {
                    await GetLeafNodes(child.PositionId, connectedNodes, totalNodes, parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1), count); // Pass count here
                }
            }
        }

        private async Task<int> CalculateWeight(int nodeId)
        {
            var children = await db.Specification.Where(s => s.ParentsId == nodeId).ToListAsync();
            int weight = 1;

            foreach (var child in children)
            {
                weight += Math.Max(1, child.QuantityPerParent ?? 1);
                weight += await CalculateWeight(child.PositionId);
            }

            return weight;
        }

    }
}

        
  