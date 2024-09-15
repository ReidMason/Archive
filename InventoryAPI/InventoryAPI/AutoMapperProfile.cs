using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryAPI.Dtos.InventoryItem;
using InventoryAPI.Models;

namespace InventoryAPI
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<InventoryItem, GetInventoryItemDto>();
            CreateMap<AddInventoryItemDto, InventoryItem>();
        }
    }
}
