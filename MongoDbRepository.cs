using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
//using game_server.Players;

namespace Pelijuttujentaustat
{
    public class MongoDbRepository : IRepository
    {
        private readonly IMongoCollection<Player> _collection;
        private readonly IMongoCollection<BsonDocument> _bsonDocumentCollection;

        public MongoDbRepository()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = mongoClient.GetDatabase("Game");
            _collection = database.GetCollection<Player>("players");
            _bsonDocumentCollection = database.GetCollection<BsonDocument>("players");
        }

        public async Task<Player> Create(Player player)
        {
            await _collection.InsertOneAsync(player);
            return player;
        }

        public async Task<Player[]> GetAll()
        {
            List<Player> players = await _collection.Find(new BsonDocument()).ToListAsync();
            return players.ToArray();
        }

        public Task<Player> Get(Guid id)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", id);
            return _collection.Find(filter).FirstAsync();
        }

        public async Task<Player[]> GetBetweenLevelsAsync(int minLevel, int maxLevel)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Gte("Level", 18) & Builders<Player>.Filter.Lte("Level", 30);
            List<Player> players = await _collection.Find(filter).ToListAsync();
            return players.ToArray();
        }


        public Task<Player> IncreasePlayerScoreAndRemoveItem(Guid playerId, Guid itemId, int score)
        {
            var pull = Builders<Player>.Update.PullFilter(p => p.Items, i => i.ItemId == itemId);
            var inc = Builders<Player>.Update.Inc(p => p.Score, score);
            var update = Builders<Player>.Update.Combine(pull, inc);
            var filter = Builders<Player>.Filter.Eq(p => p.Id, playerId);

            return _collection.FindOneAndUpdateAsync(filter, update);
        }

        public async Task<Player> Modify(Guid id, ModifiedPlayer modifiedPlayer)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", id);
            Player player = await _collection.Find(filter).FirstAsync();
            player.Score = modifiedPlayer.Score;
            await _collection.ReplaceOneAsync(filter, player);
            return player;
        }

        public async Task<Player[]> GetAllSortedByScoreDescending()
        {
            SortDefinition<Player> sortDef = Builders<Player>.Sort.Descending(p => p.Score);
            List<Player> players = await _collection.Find(new BsonDocument()).Sort(sortDef).ToListAsync();
            return players.ToArray();
        }

        public async Task<Player> IncrementPlayerScore(string id, int increment)
        {
            var filter = Builders<Player>.Filter.Eq("_id", id);
            var incrementScoreUpdate = Builders<Player>.Update.Inc(p => p.Score, increment);
            var options = new FindOneAndUpdateOptions<Player>()
            {
                ReturnDocument = ReturnDocument.After
            };
            Player player = await _collection.FindOneAndUpdateAsync(filter, incrementScoreUpdate, options);
            return player;
        }

        public async Task<Player> Delete(Guid playerId)
        {
            var filter = Builders<Player>.Filter.Eq("_id", playerId);
            Player player = await _collection.FindOneAndDeleteAsync(filter);
            return player;
        }

        public async Task<Item> CreateItem(Guid playerId, Item item, DateTime creationdate)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", playerId);
            Player player = await _collection.Find(filter).FirstAsync();
            if (player.Level >= item.ItemLevel) {
                player.Items.Add(item);
                await _collection.ReplaceOneAsync(filter, player);
                return item;
            } else {
                throw new LevelRequirementException("Too low level!");
            }
        }

        public async Task<Item> GetItem(Guid playerId, Guid itemId)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", playerId);
            Player player = await _collection.Find(filter).FirstAsync();
            for (int i = 0; i < player.Items.Count; i++) {
                if (player.Items[i].ItemId == itemId) {
                    return player.Items[i];
                }
            }
            return null;
        }

        public async Task<Item[]> GetItems(Guid playerId)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", playerId);
            Player player = await _collection.Find(filter).FirstAsync();
            return player.Items.ToArray();
        }

        public async Task<Item> ModifyItem(Guid id, ModifiedItem item, Guid itemid)
        {
            //var filter = Builders<Player>.Filter.Eq("_id", id);
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", id);
            Player player = await _collection.Find(filter).FirstAsync();
            for (int i = 0; i < player.Items.Count; i++) {
                if (player.Items[i].ItemId == itemid) {
                    player.Items[i].ItemLevel = item.ItemLevel;
                    await _collection.ReplaceOneAsync(filter, player);
                    return player.Items[i];
                }
            }
            return null;
        }

        public async Task<Item> DeleteItem(Guid id, Guid itemid)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("_id", id);
            Player player = await _collection.Find(filter).FirstAsync();
            for (int i = 0; i < player.Items.Count; i++) {
                if (player.Items[i].ItemId == itemid) {
                    player.Items.Remove(player.Items[i]);
                    await _collection.ReplaceOneAsync(filter, player);
                    return player.Items[i];
                }
            }
            return null;
        }
    }
}
