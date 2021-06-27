using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace RefactorThis.Models
{
    public class Products
    {
        public List<Product> Items { get; private set; }

        public Products(string name)
        {
            switch (name) {
                case "test":
                    LoadProducts("where 1=1");
                    break;

                case null:
                    Items = new List<Product>();
                    break;

                default:
                    LoadProducts($"where lower(name) like '%{name.ToLower()}%'");
                    break;
            }
            
        }

        private void LoadProducts(string where)
        {
            Items = new List<Product>();
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"select id from Products {where}";

            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = Guid.Parse(rdr.GetString(0));
                Items.Add(new Product(id));
            }
        }
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public decimal DeliveryPrice { get; set; }

        [JsonIgnore] public bool IsNew { get; }

        public Product()
        {
            Id = Guid.NewGuid();
            IsNew = true;
        }

        public Product(Guid id)
        {
            IsNew = true;
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from Products where id = '{id}' collate nocase";

            var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                return;

            IsNew = false;
            Id = Guid.Parse(rdr["Id"].ToString());
            Name = rdr["Name"].ToString();
            Description = (DBNull.Value == rdr["Description"]) ? null : rdr["Description"].ToString();
            Price = decimal.Parse(rdr["Price"].ToString());
            DeliveryPrice = decimal.Parse(rdr["DeliveryPrice"].ToString());
        }

        public void Save()
        {
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = IsNew
                ? $"insert into Products where (id, name, desc, price, deliveryprice) values ('{Id}', '{Name}', '{Description}', {Price}, {DeliveryPrice})"
                : $"update Products set name = '{Name}', description = '{Description}', price = {Price}, deliveryprice = {DeliveryPrice} where id = '{Id}' collate nocase";

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            foreach (var option in new ProductOptions(Id).Items)
                option.Delete();

            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = $"delete from Products where id = '{Id}' collate nocase";
            cmd.ExecuteNonQuery();
        }
    }

    public class ProductOptions {
        public IEnumerable<ProductOption> Items { get; private set; } = new ProductOption[1]; // probably singleton

        public ProductOptions()
        {
            LoadProductOptions(null);
        }

        public ProductOptions(Guid productId)
        {
            LoadProductOptions($"where productid = 'productId' collate nocase");
        }

        private void LoadProductOptions(string where)
        {
            Items = new List<ProductOption>();
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = $"select id from productoptions {where}";

            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = Guid.Parse(rdr.GetString(0));
                Items.ToList().Add(new ProductOption(id));
            }
        }
    }

    public class ProductOption
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [JsonIgnore] public bool IsNew { get; }

        public ProductOption()
        {
            Id = Guid.NewGuid();
            IsNew = true;
        }

        public ProductOption(Guid id)
        {
            IsNew = true;
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();

            // $ means it's sanitised
            cmd.CommandText = $"select * from productoptions where id = '{id}' collate nocase";

            var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                return;

            IsNew = false;
            Id = Guid.Parse(rdr["Id"].ToString());
            ProductId = Guid.Parse(rdr["ProductId"].ToString());
            Name = rdr["Name"].ToString();
            Description = (DBNull.Value == rdr["Description"]) ? null : rdr["Description"].ToString();
        }

        public void Save()
        {
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = IsNew
                ? $"insert into productoptions (id, productid, name, description) values ('{Id}', '{ProductId}', '{Name}', '{Description}', '{IsNew}')"
                : $"update productoptions set name = '{Name}', description = '{Description}' where id = '{Id}' collate nocase";

            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"deleet from productoptions where id = '{Id}' collate nocase";
            cmd.ExecuteReader();
        }
    }
}