using BOM.DAL;
using BOM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace BOM
{

    [ApiController]
    [Route("Order")]
    public class OrderController : ControllerBase
    {
        ApplicationContext db;
        public OrderController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> Get()
        {
            return await db.Order.ToListAsync();
        }


        [HttpGet("{Id}")]
        public async Task<ActionResult<LinkedList<Order>>> GetOrder(int id)
        {
            Order order = await db.Order.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null)
                return NotFound();
            return new ObjectResult(order);
        }



        [HttpPost]
        public async Task<ActionResult<Order>> Post(Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            db.Order.Add(order);
            await db.SaveChangesAsync();
            return Ok(order);
        }


        [HttpPut("{Id}")]
        public async Task<ActionResult<Order>> Put(Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }
            if (!db.Order.Any(x => x.Id == order.Id))
            {
                return NotFound();
            }

            db.Update(order);
            await db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult<Order>> Delete([Required] int Id)
        {
            var order = await db.Order.FirstOrDefaultAsync(x => Id == x.Id);
            if (order == null)
            {
                return NotFound();
            }
            db.Order.Remove(order);
            await db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpGet("GetConnectedNodes")]
        public async Task<ActionResult<List<Specification>>> GetConnectedNodes(string dateDecomposition)
        {
            var orders = await db.Order.Where(x => DateTime.Parse(dateDecomposition) >= x.OrderDate).ToListAsync();
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
                await GetLeafNodes(id.Value, connectedNodes, totalNodes, 1, (double)order.Quantity);

                result.Nodes.AddRange(connectedNodes);
                result.TotalNodes += totalNodes;
                result.TotalWeight += totalWeight;
                connectedNodes.Clear();
            }
            return Ok(result);
        }

        private async Task GetLeafNodes(int parentId, List<Specification> connectedNodes, int totalNodes, int parentQuantity, double quantity)
        {
            var children = await db.Specification.Where(s => s.ParentsId == parentId).ToListAsync();
            if (children.Count == 0)
            {
                var node = new Specification
                {
                    PositionId = parentId,
                    Description = "Нижний узел",
                    QuantityPerParent = (int?)(parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1) * quantity)
                };
                connectedNodes.Add(node);
                totalNodes++;
            }
            else
            {
                foreach (var child in children)
                {
                    await GetLeafNodes(child.PositionId, connectedNodes, totalNodes, parentQuantity * (db.Specification.FirstOrDefault(s => s.PositionId == parentId)?.QuantityPerParent ?? 1), quantity); // Умножаем на QuantityPerParent родительского узла
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