﻿using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MVCApp.OAuth2.Sample.Models
{
    public class Item
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; }

        [MaxLength(500)]
        public string Value { get; set; }
    }

    public class GoogleAuthContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
    }

    public class EFDataStore : IDataStore
    {
        public async Task ClearAsync()
        {
            using (var context = new GoogleAuthContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                await objectContext.ExecuteStoreCommandAsync("TRUNCATE TABLE [Items]", new object[] {});
            }
        }

        public async Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new GoogleAuthContext())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                var item = context.Items.FirstOrDefault(x => x.Key == generatedKey);
                if (item != null)
                {
                    context.Items.Remove(item);
                    await context.SaveChangesAsync();
                }
            }
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new GoogleAuthContext())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                var item = context.Items.FirstOrDefault(x => x.Key == generatedKey);
                T value = item == null ? default(T) : JsonConvert.DeserializeObject<T>(item.Value);
                return Task.FromResult<T>(value);
            }
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new GoogleAuthContext())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                string json = JsonConvert.SerializeObject(value);

                var item = await context.Items.SingleOrDefaultAsync(x => x.Key == generatedKey);

                if (item == null)
                {
                    context.Items.Add(new Item { Key = generatedKey, Value = json });
                }
                else
                {
                    item.Value = json;
                }

                await context.SaveChangesAsync();
            }
        }

        private static string GenerateStoredKey(string key, Type t)
        {
            return string.Format("{0}-{1}", t.FullName, key);
        }
    }
}

