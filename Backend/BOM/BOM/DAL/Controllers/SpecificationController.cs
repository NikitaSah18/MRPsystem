using BOM.DAL;
using BOM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace BOM
{
    [ApiController]
    [Route("Specification")]
    public class SpecificationController : ControllerBase
    {
        ApplicationContext db;
        public SpecificationController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Specification>>> Get()
        {
            return await db.Specification.ToListAsync();
        }


        [HttpGet("{PositionId}")]
        public async Task<ActionResult<LinkedList<Specification>>> GetClient(int id)
        {
            Specification Specification = await db.Specification.FirstOrDefaultAsync(x => x.PositionId == id);
            if (Specification == null)
                return NotFound();
            return new ObjectResult(Specification);
        }



        [HttpPost]
        public async Task<ActionResult<Specification>> Post(Specification specification)
        {
            if (specification == null)
            {
                return BadRequest();
            }

            db.Specification.Add(specification);
            await db.SaveChangesAsync();
            return Ok(specification);
        }


        [HttpPut("{positionId}")]
        public async Task<ActionResult<Specification>> Put(Specification specification)
        {
            if (specification == null)
            {
                return BadRequest();
            }
            if (!db.Specification.Any(x => x.PositionId == specification.PositionId))
            {
                return NotFound();
            }

            db.Update(specification);
            await db.SaveChangesAsync();
            return Ok(specification);
        }

        [HttpDelete("{Id}")]
        public async Task Delete([Required] int Id)
        {

            {
                {
                    {
                        Specification? specification = db.Specification.Find(Id);

                        if (specification != null)
                        {
                            List<List<int>> layers = new List<List<int>>();

                            List<int> currentLayer = new List<int>();
                            List<int> nextLayer = new List<int>();
                            currentLayer.Add(specification.PositionId);

                            while (currentLayer.Count > 0)
                            {
                                foreach (int currentId in currentLayer)
                                {
                                    List<Specification> children = db.Specification.Where(c => (c.ParentsId == currentId)).ToList();
                                    foreach (var child in children)
                                    {
                                        nextLayer.Add(child.PositionId);
                                    }
                                }
                                layers.Add(currentLayer);
                                currentLayer = nextLayer;
                                nextLayer = new List<int>();
                            }

                            while (layers.Count > 0)
                            {
                                List<int> lastLayer = layers.Last();
                                foreach (int currentId in lastLayer)
                                {
                                    Specification? s = db.Specification.Find(currentId);
                                    db.Specification.Remove(s);
                                    db.SaveChanges();
                                }
                                layers.RemoveAt(layers.Count - 1);
                            }
                        }


                     }
                }
            }
        }
        [HttpGet("GetConnectedNodes")]
        public async Task<ActionResult<List<Specification>>> GetConnectedNodes(int id)
        {
            var connectedNodes = new List<Specification>();
            var totalNodes = 0;
            var totalWeight = await CalculateWeight(id);
            await GetLeafNodes(id, connectedNodes, totalNodes, 1);

            var result = new
            {
                Nodes = connectedNodes,
                TotalNodes = totalNodes,
                TotalWeight = totalWeight
            };

            return Ok(result);
        }

        private async Task GetLeafNodes(int parentId, List<Specification> connectedNodes, int totalNodes, int parentQuantity)
        {
            var children = await db.Specification.Where(s => s.ParentsId == parentId).ToListAsync();
            if (children.Count == 0)
            {
                var node = new Specification
                {
                    PositionId = parentId,
                    Description = "Нижний узел",
                    QuantityPerParent = parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1)
                };
                connectedNodes.Add(node);
                totalNodes++;
            }
            else
            {
                foreach (var child in children)
                {
                    await GetLeafNodes(child.PositionId, connectedNodes, totalNodes, parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1)); // Умножаем на QuantityPerParent родительского узла
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