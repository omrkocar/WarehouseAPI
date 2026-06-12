using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Models.DTOs;

public class UpdateOrderStatusDto
{
    public required OrderStatus Status { get; set; }
}