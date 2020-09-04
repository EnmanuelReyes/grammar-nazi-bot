﻿using Firebase.Database;
using Firebase.Database.Query;
using GrammarNazi.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GrammarNazi.Core.Repositories
{
    public class FirebaseRepository<T> : IRepository<T> where T : class
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseRepository(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public async Task Add(T entity)
        {
            var entityName = typeof(T).Name;

            await _firebaseClient.Child(entityName).PostAsync(Newtonsoft.Json.JsonConvert.SerializeObject(entity));
        }

        public async Task<bool> Any(Expression<Func<T, bool>> filter = null)
        {
            var results = await GetList();

            if (filter == default)
                return results.Any();

            return results.Any(filter.Compile());
        }

        public async Task Delete(T entity)
        {
            var results = await _firebaseClient.Child(typeof(T).Name).OnceAsync<T>();

            var firebaseObject = results.FirstOrDefault(v => v.Object.Equals(entity));

            if (firebaseObject != default)
            {
                await _firebaseClient.Child(typeof(T).Name).Child(firebaseObject.Key).DeleteAsync();
            }
        }

        public async Task<T> GetFirst(Expression<Func<T, bool>> filter)
        {
            var results = await GetList();

            return results.FirstOrDefault(filter.Compile());
        }

        public async Task<TResult> Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            var results = await GetList();

            return results.Max(selector.Compile());
        }

        private async Task<IEnumerable<T>> GetList() => (await _firebaseClient.Child(typeof(T).Name).OnceAsync<T>()).Select(v => v.Object);
    }
}