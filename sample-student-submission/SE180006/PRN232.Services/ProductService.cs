
using System;
using System.Collections.Generic;
using PRN232.Repositories;
namespace PRN232.Services {
    public class ProductService {
        private readonly ProductRepo _repo = new();
        public List<Product> GetAll() => _repo.GetAll();
        public Product Create(string name, decimal price) => _repo.Add(new Product { Id = Guid.NewGuid(), Name = name, Price = price });
    }
}
