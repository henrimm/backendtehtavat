using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pelijuttujentaustat
{
     public interface IRepository
    {
        //Players
        Task<Player> Get(Guid id);
        Task<Player[]> GetAll();
        Task<Player> Create(Player player);
        Task<Player> Modify(Guid id, ModifiedPlayer player);
        Task<Player> Delete(Guid id);
        //Items
        Task<Item[]> GetItems(Guid id);
        Task<Item> CreateItem(Guid id, Item item, DateTime creationtime);
        Task<Item> ModifyItem(Guid id, ModifiedItem item, Guid itemid);
        Task<Item> DeleteItem(Guid id, Guid itemid);

    }
}